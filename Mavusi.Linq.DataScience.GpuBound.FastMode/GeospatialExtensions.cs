using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using Mavusi.Linq.DataScience.Models;
using System.Runtime.CompilerServices;

namespace Mavusi.Linq.DataScience.GpuBound;


/// <summary>
/// GPU-accelerated geospatial operations using ILGPU with optimized 32-bit precision.
/// </summary>
public static class GeospatialExtensions
{
    private static (Context Context, Accelerator Accelerator) GpuContext => 
        GpuContextBase.GpuContext;

    private const float EarthRadiusKm = 6371.0f;
    private const float EarthRadiusMiles = 3959.0f;
    private const float PI = 3.14159265f;
    private const float DEG_TO_RAD = PI / 180.0f;

    // Cached kernel delegates to avoid recompilation
    private static Action<Index1D, float, float, ArrayView<float>, ArrayView<float>, ArrayView<float>>? _distanceKernelCache;
    private static Action<Index1D, ArrayView<float>, ArrayView<float>, int, ArrayView<float>>? _pairwiseKernelCache;
    private static readonly object _kernelLock = new();

    /// <summary>
    /// Calculates the Haversine distance in kilometers between two geographical coordinates.
    /// Note: This method runs on CPU. For batch operations, use CalculateDistancesGpu for true GPU acceleration.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float HaversineDistance(this GeoCoordinate from, GeoCoordinate to)
    {
        if (from == null) throw new ArgumentNullException(nameof(from));
        if (to == null) throw new ArgumentNullException(nameof(to));

        var lat1 = (float)from.Latitude * DEG_TO_RAD;
        var lat2 = (float)to.Latitude * DEG_TO_RAD;
        var dLat = (float)(to.Latitude - from.Latitude) * DEG_TO_RAD;
        var dLon = (float)(to.Longitude - from.Longitude) * DEG_TO_RAD;

        var sinDLat = MathF.Sin(dLat * 0.5f);
        var sinDLon = MathF.Sin(dLon * 0.5f);

        var a = sinDLat * sinDLat +
                MathF.Cos(lat1) * MathF.Cos(lat2) *
                sinDLon * sinDLon;

        var c = 2.0f * MathF.Atan2(MathF.Sqrt(a), MathF.Sqrt(1.0f - a));

        return EarthRadiusKm * c;
    }

    /// <summary>
    /// Calculates the Haversine distance in miles between two geographical coordinates.
    /// Note: This method runs on CPU. For batch operations, use CalculateDistancesGpu for true GPU acceleration.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float HaversineDistanceMiles(this GeoCoordinate from, GeoCoordinate to)
    {
        if (from == null) throw new ArgumentNullException(nameof(from));
        if (to == null) throw new ArgumentNullException(nameof(to));

        var lat1 = (float)from.Latitude * DEG_TO_RAD;
        var lat2 = (float)to.Latitude * DEG_TO_RAD;
        var dLat = (float)(to.Latitude - from.Latitude) * DEG_TO_RAD;
        var dLon = (float)(to.Longitude - from.Longitude) * DEG_TO_RAD;

        var sinDLat = MathF.Sin(dLat * 0.5f);
        var sinDLon = MathF.Sin(dLon * 0.5f);

        var a = sinDLat * sinDLat +
                MathF.Cos(lat1) * MathF.Cos(lat2) *
                sinDLon * sinDLon;

        var c = 2.0f * MathF.Atan2(MathF.Sqrt(a), MathF.Sqrt(1.0f - a));

        return EarthRadiusMiles * c;
    }

    /// <summary>
    /// Calculates distances from a center point to multiple coordinates in parallel using GPU acceleration.
    /// Optimized for 32-bit precision and maximum throughput.
    /// </summary>
    public static float[] CalculateDistancesGpu(this GeoCoordinate center, IEnumerable<GeoCoordinate> coordinates)
    {
        if (center == null) throw new ArgumentNullException(nameof(center));
        if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));

        var coordArray = coordinates.ToArray();
        if (coordArray.Length == 0) return Array.Empty<float>();

        var (context, accelerator) = GpuContext;

        // Convert to float arrays for optimal GPU memory bandwidth (32-bit vs 64-bit)
        var latitudes = new float[coordArray.Length];
        var longitudes = new float[coordArray.Length];
        for (int i = 0; i < coordArray.Length; i++)
        {
            latitudes[i] = (float)coordArray[i].Latitude;
            longitudes[i] = (float)coordArray[i].Longitude;
        }
        var results = new float[coordArray.Length];

        using var deviceLats = accelerator.Allocate1D(latitudes);
        using var deviceLons = accelerator.Allocate1D(longitudes);
        using var deviceResults = accelerator.Allocate1D<float>(coordArray.Length);

        // Use cached kernel to avoid recompilation overhead
        var distanceKernel = GetOrCreateDistanceKernel(accelerator);

        distanceKernel(coordArray.Length, (float)center.Latitude, (float)center.Longitude, 
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
        float radiusKm)
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
    /// Optimized for 32-bit precision and maximum throughput.
    /// </summary>
    public static float[,] PairwiseDistancesGpu(this IEnumerable<GeoCoordinate> coordinates)
    {
        if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));

        var coordArray = coordinates.ToArray();
        if (coordArray.Length == 0) return new float[0, 0];

        var (context, accelerator) = GpuContext;
        var n = coordArray.Length;

        // Convert to float arrays for optimal GPU memory bandwidth (32-bit vs 64-bit)
        var latitudes = new float[n];
        var longitudes = new float[n];
        for (int i = 0; i < n; i++)
        {
            latitudes[i] = (float)coordArray[i].Latitude;
            longitudes[i] = (float)coordArray[i].Longitude;
        }
        var resultsFlat = new float[n * n];

        using var deviceLats = accelerator.Allocate1D(latitudes);
        using var deviceLons = accelerator.Allocate1D(longitudes);
        using var deviceResults = accelerator.Allocate1D<float>(n * n);

        // Use cached kernel to avoid recompilation overhead
        var pairwiseKernel = GetOrCreatePairwiseKernel(accelerator);

        pairwiseKernel(n * n, deviceLats.View, deviceLons.View, n, deviceResults.View);
        accelerator.Synchronize();

        deviceResults.CopyToCPU(resultsFlat);

        // Convert flat array to 2D
        var result = new float[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                result[i, j] = resultsFlat[i * n + j];

        return result;
    }

    // Helper methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Action<Index1D, float, float, ArrayView<float>, ArrayView<float>, ArrayView<float>> GetOrCreateDistanceKernel(Accelerator accelerator)
    {
        if (_distanceKernelCache == null)
        {
            lock (_kernelLock)
            {
                if (_distanceKernelCache == null)
                {
                    _distanceKernelCache = accelerator.LoadAutoGroupedStreamKernel<
                        Index1D, float, float, ArrayView<float>, ArrayView<float>, ArrayView<float>>(
                        HaversineDistanceKernel);
                }
            }
        }
        return _distanceKernelCache;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Action<Index1D, ArrayView<float>, ArrayView<float>, int, ArrayView<float>> GetOrCreatePairwiseKernel(Accelerator accelerator)
    {
        if (_pairwiseKernelCache == null)
        {
            lock (_kernelLock)
            {
                if (_pairwiseKernelCache == null)
                {
                    _pairwiseKernelCache = accelerator.LoadAutoGroupedStreamKernel<
                        Index1D, ArrayView<float>, ArrayView<float>, int, ArrayView<float>>(
                        PairwiseDistanceKernel);
                }
            }
        }
        return _pairwiseKernelCache;
    }

    // GPU Kernels - Optimized for 32-bit precision and GPU execution

    private static void HaversineDistanceKernel(
        Index1D index,
        float centerLat,
        float centerLon,
        ArrayView<float> latitudes,
        ArrayView<float> longitudes,
        ArrayView<float> distances)
    {
        if (index < latitudes.Length)
        {
            // Use 32-bit precision throughout for optimal GPU performance
            const float DEG_TO_RAD = 0.0174532925f; // PI / 180
            var lat1 = centerLat * DEG_TO_RAD;
            var lat2 = latitudes[index] * DEG_TO_RAD;
            var dLat = (latitudes[index] - centerLat) * DEG_TO_RAD;
            var dLon = (longitudes[index] - centerLon) * DEG_TO_RAD;

            // ILGPU automatically converts Math.* to GPU intrinsics for single precision
            var halfDLat = dLat * 0.5f;
            var halfDLon = dLon * 0.5f;
            var sinDLat = (float)Math.Sin(halfDLat);
            var sinDLon = (float)Math.Sin(halfDLon);

            var a = sinDLat * sinDLat +
                    (float)Math.Cos(lat1) * (float)Math.Cos(lat2) *
                    sinDLon * sinDLon;

            var sqrtA = (float)Math.Sqrt(a);
            var sqrtOneMinusA = (float)Math.Sqrt(1.0f - a);
            var c = 2.0f * (float)Math.Atan2(sqrtA, sqrtOneMinusA);

            distances[index] = 6371.0f * c; // Earth radius in km
        }
    }

    private static void PairwiseDistanceKernel(
        Index1D index,
        ArrayView<float> latitudes,
        ArrayView<float> longitudes,
        int n,
        ArrayView<float> distances)
    {
        if (index < n * n)
        {
            int i = (int)index / n;
            int j = (int)index % n;

            if (i == j)
            {
                distances[index] = 0.0f;
                return;
            }

            // Use 32-bit precision throughout for optimal GPU performance
            const float DEG_TO_RAD = 0.0174532925f; // PI / 180
            var lat1 = latitudes[i] * DEG_TO_RAD;
            var lat2 = latitudes[j] * DEG_TO_RAD;
            var dLat = (latitudes[j] - latitudes[i]) * DEG_TO_RAD;
            var dLon = (longitudes[j] - longitudes[i]) * DEG_TO_RAD;

            // ILGPU automatically converts Math.* to GPU intrinsics for single precision
            var halfDLat = dLat * 0.5f;
            var halfDLon = dLon * 0.5f;
            var sinDLat = (float)Math.Sin(halfDLat);
            var sinDLon = (float)Math.Sin(halfDLon);

            var a = sinDLat * sinDLat +
                    (float)Math.Cos(lat1) * (float)Math.Cos(lat2) *
                    sinDLon * sinDLon;

            var sqrtA = (float)Math.Sqrt(a);
            var sqrtOneMinusA = (float)Math.Sqrt(1.0f - a);
            var c = 2.0f * (float)Math.Atan2(sqrtA, sqrtOneMinusA);

            distances[index] = 6371.0f * c; // Earth radius in km
        }
    }
}
