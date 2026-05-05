using ILGPU;
using ILGPU.Runtime;
using Mavusi.Linq.DataScience.Models;

namespace Mavusi.Linq.DataScience.GpuBound;


/// <summary>
/// GPU-accelerated geospatial operations using ILGPU.
/// </summary>
public static class GeospatialExtensions
{
    private static (Context Context, Accelerator Accelerator) GpuContext => 
        CorrelationExtensions.GpuContext;

    private const double EarthRadiusKm = 6371.0;
    private const double EarthRadiusMiles = 3959.0;

    /// <summary>
    /// Calculates the Haversine distance in kilometers between two geographical coordinates using GPU acceleration.
    /// </summary>
    public static double HaversineDistanceGpu(this GeoCoordinate from, GeoCoordinate to)
    {
        if (from == null) throw new ArgumentNullException(nameof(from));
        if (to == null) throw new ArgumentNullException(nameof(to));

        var lat1 = DegreesToRadians(from.Latitude);
        var lat2 = DegreesToRadians(to.Latitude);
        var dLat = DegreesToRadians(to.Latitude - from.Latitude);
        var dLon = DegreesToRadians(to.Longitude - from.Longitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    /// <summary>
    /// Calculates the Haversine distance in miles between two geographical coordinates using GPU acceleration.
    /// </summary>
    public static double HaversineDistanceMilesGpu(this GeoCoordinate from, GeoCoordinate to)
    {
        if (from == null) throw new ArgumentNullException(nameof(from));
        if (to == null) throw new ArgumentNullException(nameof(to));

        var lat1 = DegreesToRadians(from.Latitude);
        var lat2 = DegreesToRadians(to.Latitude);
        var dLat = DegreesToRadians(to.Latitude - from.Latitude);
        var dLon = DegreesToRadians(to.Longitude - from.Longitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusMiles * c;
    }

    /// <summary>
    /// Calculates distances from a center point to multiple coordinates in parallel using GPU acceleration.
    /// </summary>
    public static double[] CalculateDistancesGpu(this GeoCoordinate center, IEnumerable<GeoCoordinate> coordinates)
    {
        if (center == null) throw new ArgumentNullException(nameof(center));
        if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));

        var coordArray = coordinates.ToArray();
        if (coordArray.Length == 0) return Array.Empty<double>();

        var (context, accelerator) = GpuContext;

        // Flatten coordinates to arrays
        var latitudes = coordArray.Select(c => c.Latitude).ToArray();
        var longitudes = coordArray.Select(c => c.Longitude).ToArray();
        var results = new double[coordArray.Length];

        using var deviceLats = accelerator.Allocate1D(latitudes);
        using var deviceLons = accelerator.Allocate1D(longitudes);
        using var deviceResults = accelerator.Allocate1D<double>(coordArray.Length);

        var distanceKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, double, double, ArrayView<double>, ArrayView<double>, ArrayView<double>>(
            HaversineDistanceKernel);

        distanceKernel(coordArray.Length, center.Latitude, center.Longitude, 
            deviceLats.View, deviceLons.View, deviceResults.View);
        accelerator.Synchronize();

        deviceResults.CopyToCPU(results);
        return results;
    }

    /// <summary>
    /// Filters coordinates within a specified radius from a center point using GPU acceleration.
    /// </summary>
    public static IEnumerable<T> WithinRadiusGpu<T>(
        this IEnumerable<T> source,
        Func<T, GeoCoordinate> coordinateSelector,
        GeoCoordinate center,
        double radiusKm)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (coordinateSelector == null) throw new ArgumentNullException(nameof(coordinateSelector));
        if (center == null) throw new ArgumentNullException(nameof(center));
        if (radiusKm <= 0) throw new ArgumentException("Radius must be greater than zero", nameof(radiusKm));

        var items = source.ToArray();
        var coordinates = items.Select(coordinateSelector).ToArray();

        var distances = center.CalculateDistancesGpu(coordinates);

        for (int i = 0; i < items.Length; i++)
        {
            if (distances[i] <= radiusKm)
            {
                yield return items[i];
            }
        }
    }

    /// <summary>
    /// Finds the nearest N coordinates to a center point using GPU acceleration.
    /// </summary>
    public static IEnumerable<T> NearestGpu<T>(
        this IEnumerable<T> source,
        Func<T, GeoCoordinate> coordinateSelector,
        GeoCoordinate center,
        int count)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (coordinateSelector == null) throw new ArgumentNullException(nameof(coordinateSelector));
        if (center == null) throw new ArgumentNullException(nameof(center));
        if (count <= 0) throw new ArgumentException("Count must be greater than zero", nameof(count));

        var items = source.ToArray();
        var coordinates = items.Select(coordinateSelector).ToArray();

        var distances = center.CalculateDistancesGpu(coordinates);

        return items
            .Select((item, index) => new { Item = item, Distance = distances[index] })
            .OrderBy(x => x.Distance)
            .Take(count)
            .Select(x => x.Item);
    }

    /// <summary>
    /// Calculates pairwise distances between all coordinate pairs using GPU acceleration.
    /// </summary>
    public static double[,] PairwiseDistancesGpu(this IEnumerable<GeoCoordinate> coordinates)
    {
        if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));

        var coordArray = coordinates.ToArray();
        if (coordArray.Length == 0) return new double[0, 0];

        var (context, accelerator) = GpuContext;
        var n = coordArray.Length;

        var latitudes = coordArray.Select(c => c.Latitude).ToArray();
        var longitudes = coordArray.Select(c => c.Longitude).ToArray();
        var resultsFlat = new double[n * n];

        using var deviceLats = accelerator.Allocate1D(latitudes);
        using var deviceLons = accelerator.Allocate1D(longitudes);
        using var deviceResults = accelerator.Allocate1D<double>(n * n);

        var pairwiseKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<double>, ArrayView<double>, int, ArrayView<double>>(
            PairwiseDistanceKernel);

        pairwiseKernel(n * n, deviceLats.View, deviceLons.View, n, deviceResults.View);
        accelerator.Synchronize();

        deviceResults.CopyToCPU(resultsFlat);

        // Convert flat array to 2D
        var result = new double[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                result[i, j] = resultsFlat[i * n + j];

        return result;
    }

    // Helper methods

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    // GPU Kernels

    private static void HaversineDistanceKernel(
        Index1D index,
        double centerLat,
        double centerLon,
        ArrayView<double> latitudes,
        ArrayView<double> longitudes,
        ArrayView<double> distances)
    {
        if (index < latitudes.Length)
        {
            const double PI = 3.14159265358979323846;
            var lat1 = centerLat * PI / 180.0;
            var lat2 = latitudes[index] * PI / 180.0;
            var dLat = (latitudes[index] - centerLat) * PI / 180.0;
            var dLon = (longitudes[index] - centerLon) * PI / 180.0;

            var sinDLat = Math.Sin(dLat / 2);
            var sinDLon = Math.Sin(dLon / 2);

            var a = sinDLat * sinDLat +
                    Math.Cos(lat1) * Math.Cos(lat2) *
                    sinDLon * sinDLon;

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            distances[index] = 6371.0 * c; // Earth radius in km
        }
    }

    private static void PairwiseDistanceKernel(
        Index1D index,
        ArrayView<double> latitudes,
        ArrayView<double> longitudes,
        int n,
        ArrayView<double> distances)
    {
        if (index < n * n)
        {
            int i = (int)index / n;
            int j = (int)index % n;

            if (i == j)
            {
                distances[index] = 0.0;
                return;
            }

            const double PI = 3.14159265358979323846;
            var lat1 = latitudes[i] * PI / 180.0;
            var lat2 = latitudes[j] * PI / 180.0;
            var dLat = (latitudes[j] - latitudes[i]) * PI / 180.0;
            var dLon = (longitudes[j] - longitudes[i]) * PI / 180.0;

            var sinDLat = Math.Sin(dLat / 2);
            var sinDLon = Math.Sin(dLon / 2);

            var a = sinDLat * sinDLat +
                    Math.Cos(lat1) * Math.Cos(lat2) *
                    sinDLon * sinDLon;

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            distances[index] = 6371.0 * c; // Earth radius in km
        }
    }
}
