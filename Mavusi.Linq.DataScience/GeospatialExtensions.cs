using Mavusi.Linq.DataScience.Models;

namespace Mavusi.Linq.DataScience;

/// <summary>
/// Represents a geographical bounding box.
/// </summary>
public record GeoBounds(double MinLatitude, double MaxLatitude, double MinLongitude, double MaxLongitude)
{
    public GeoCoordinate Center => new(
        (MinLatitude + MaxLatitude) / 2,
        (MinLongitude + MaxLongitude) / 2
    );

    public bool Contains(GeoCoordinate coordinate)
    {
        return coordinate.Latitude >= MinLatitude &&
               coordinate.Latitude <= MaxLatitude &&
               coordinate.Longitude >= MinLongitude &&
               coordinate.Longitude <= MaxLongitude;
    }
}

public static class GeospatialExtensions
{
    private const double EarthRadiusKm = 6371.0;
    private const double EarthRadiusMiles = 3959.0;

    /// <summary>
    /// Calculates the Haversine distance in kilometers between two geographical coordinates.
    /// </summary>
    public static double HaversineDistance(this GeoCoordinate from, GeoCoordinate to)
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
    /// Calculates the Haversine distance in miles between two geographical coordinates.
    /// </summary>
    public static double HaversineDistanceMiles(this GeoCoordinate from, GeoCoordinate to)
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
    /// Filters coordinates within a specified radius from a center point.
    /// </summary>
    public static IEnumerable<T> WithinRadius<T>(
        this IEnumerable<T> source,
        Func<T, GeoCoordinate> coordinateSelector,
        GeoCoordinate center,
        double radiusKm)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (coordinateSelector == null) throw new ArgumentNullException(nameof(coordinateSelector));
        if (center == null) throw new ArgumentNullException(nameof(center));
        if (radiusKm <= 0) throw new ArgumentException("Radius must be greater than zero", nameof(radiusKm));

        return source.Where(item =>
        {
            var coord = coordinateSelector(item);
            return center.HaversineDistance(coord) <= radiusKm;
        });
    }

    /// <summary>
    /// Finds the nearest item to a specified coordinate.
    /// </summary>
    public static T Nearest<T>(
        this IEnumerable<T> source,
        Func<T, GeoCoordinate> coordinateSelector,
        GeoCoordinate target)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (coordinateSelector == null) throw new ArgumentNullException(nameof(coordinateSelector));
        if (target == null) throw new ArgumentNullException(nameof(target));

        return source.MinBy(item => target.HaversineDistance(coordinateSelector(item)))
            ?? throw new InvalidOperationException("Sequence contains no elements");
    }

    /// <summary>
    /// Finds the N nearest items to a specified coordinate.
    /// </summary>
    public static IEnumerable<T> NearestN<T>(
        this IEnumerable<T> source,
        Func<T, GeoCoordinate> coordinateSelector,
        GeoCoordinate target,
        int count)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (coordinateSelector == null) throw new ArgumentNullException(nameof(coordinateSelector));
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (count <= 0) throw new ArgumentException("Count must be greater than zero", nameof(count));

        return source
            .OrderBy(item => target.HaversineDistance(coordinateSelector(item)))
            .Take(count);
    }

    /// <summary>
    /// Calculates the geographical center (centroid) of a collection of coordinates.
    /// </summary>
    public static GeoCoordinate GeographicCenter(this IEnumerable<GeoCoordinate> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var coordinates = source.ToList();
        if (coordinates.Count == 0) throw new InvalidOperationException("Sequence contains no elements");

        var x = 0.0;
        var y = 0.0;
        var z = 0.0;

        foreach (var coord in coordinates)
        {
            var lat = DegreesToRadians(coord.Latitude);
            var lon = DegreesToRadians(coord.Longitude);

            x += Math.Cos(lat) * Math.Cos(lon);
            y += Math.Cos(lat) * Math.Sin(lon);
            z += Math.Sin(lat);
        }

        var total = coordinates.Count;
        x /= total;
        y /= total;
        z /= total;

        var centralLongitude = Math.Atan2(y, x);
        var centralSquareRoot = Math.Sqrt(x * x + y * y);
        var centralLatitude = Math.Atan2(z, centralSquareRoot);

        return new GeoCoordinate(
            RadiansToDegrees(centralLatitude),
            RadiansToDegrees(centralLongitude)
        );
    }

    /// <summary>
    /// Calculates the geographical center using a selector function.
    /// </summary>
    public static GeoCoordinate GeographicCenter<T>(
        this IEnumerable<T> source,
        Func<T, GeoCoordinate> coordinateSelector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (coordinateSelector == null) throw new ArgumentNullException(nameof(coordinateSelector));

        return source.Select(coordinateSelector).GeographicCenter();
    }

    /// <summary>
    /// Calculates the bounding box that contains all coordinates in the sequence.
    /// </summary>
    public static GeoBounds BoundingBox(this IEnumerable<GeoCoordinate> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var coordinates = source.ToList();
        if (coordinates.Count == 0) throw new InvalidOperationException("Sequence contains no elements");

        var minLat = coordinates.Min(c => c.Latitude);
        var maxLat = coordinates.Max(c => c.Latitude);
        var minLon = coordinates.Min(c => c.Longitude);
        var maxLon = coordinates.Max(c => c.Longitude);

        return new GeoBounds(minLat, maxLat, minLon, maxLon);
    }

    /// <summary>
    /// Calculates the bounding box using a selector function.
    /// </summary>
    public static GeoBounds BoundingBox<T>(
        this IEnumerable<T> source,
        Func<T, GeoCoordinate> coordinateSelector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (coordinateSelector == null) throw new ArgumentNullException(nameof(coordinateSelector));

        return source.Select(coordinateSelector).BoundingBox();
    }

    /// <summary>
    /// Calculates all pairwise distances between coordinates in a sequence.
    /// </summary>
    public static IEnumerable<GeoDistance> PairwiseDistances(this IEnumerable<GeoCoordinate> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var coordinates = source.ToList();
        if (coordinates.Count < 2) throw new InvalidOperationException("Sequence must contain at least two elements");

        for (var i = 0; i < coordinates.Count - 1; i++)
        {
            for (var j = i + 1; j < coordinates.Count; j++)
            {
                var from = coordinates[i];
                var to = coordinates[j];
                var distanceKm = from.HaversineDistance(to);
                var distanceMiles = from.HaversineDistanceMiles(to);

                yield return new GeoDistance(from, to, distanceKm, distanceMiles);
            }
        }
    }

    /// <summary>
    /// Calculates consecutive distances between coordinates in a sequence (route distance).
    /// </summary>
    public static IEnumerable<GeoDistance> ConsecutiveDistances(this IEnumerable<GeoCoordinate> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var coordinates = source.ToList();
        if (coordinates.Count < 2) throw new InvalidOperationException("Sequence must contain at least two elements");

        for (var i = 0; i < coordinates.Count - 1; i++)
        {
            var from = coordinates[i];
            var to = coordinates[i + 1];
            var distanceKm = from.HaversineDistance(to);
            var distanceMiles = from.HaversineDistanceMiles(to);

            yield return new GeoDistance(from, to, distanceKm, distanceMiles);
        }
    }

    /// <summary>
    /// Calculates the total distance of a route (sum of consecutive distances).
    /// </summary>
    public static double TotalDistance(this IEnumerable<GeoCoordinate> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        return source.ConsecutiveDistances().Sum(d => d.DistanceKm);
    }

    /// <summary>
    /// Filters coordinates within a bounding box.
    /// </summary>
    public static IEnumerable<T> WithinBounds<T>(
        this IEnumerable<T> source,
        Func<T, GeoCoordinate> coordinateSelector,
        GeoBounds bounds)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (coordinateSelector == null) throw new ArgumentNullException(nameof(coordinateSelector));
        if (bounds == null) throw new ArgumentNullException(nameof(bounds));

        return source.Where(item => bounds.Contains(coordinateSelector(item)));
    }

    /// <summary>
    /// Groups coordinates by proximity using a specified distance threshold.
    /// </summary>
    public static IEnumerable<IGrouping<int, T>> GroupByProximity<T>(
        this IEnumerable<T> source,
        Func<T, GeoCoordinate> coordinateSelector,
        double thresholdKm)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (coordinateSelector == null) throw new ArgumentNullException(nameof(coordinateSelector));
        if (thresholdKm <= 0) throw new ArgumentException("Threshold must be greater than zero", nameof(thresholdKm));

        var items = source.ToList();
        var clusters = new Dictionary<int, List<T>>();
        var clusterAssignments = new Dictionary<int, int>();
        var nextClusterId = 0;

        for (var i = 0; i < items.Count; i++)
        {
            if (clusterAssignments.ContainsKey(i))
                continue;

            var currentCluster = nextClusterId++;
            clusters[currentCluster] = new List<T> { items[i] };
            clusterAssignments[i] = currentCluster;

            var coord1 = coordinateSelector(items[i]);

            for (var j = i + 1; j < items.Count; j++)
            {
                if (clusterAssignments.ContainsKey(j))
                    continue;

                var coord2 = coordinateSelector(items[j]);
                var distance = coord1.HaversineDistance(coord2);

                if (distance <= thresholdKm)
                {
                    clusters[currentCluster].Add(items[j]);
                    clusterAssignments[j] = currentCluster;
                }
            }
        }

        return clusters.Select(kvp => 
            new Grouping<int, T>(kvp.Key, kvp.Value));
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
    private static double RadiansToDegrees(double radians) => radians * 180.0 / Math.PI;

    private class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        private readonly IEnumerable<TElement> _elements;

        public Grouping(TKey key, IEnumerable<TElement> elements)
        {
            Key = key;
            _elements = elements;
        }

        public TKey Key { get; }

        public IEnumerator<TElement> GetEnumerator() => _elements.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
