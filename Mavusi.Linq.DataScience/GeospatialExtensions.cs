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
    private const double KmToMiles = EarthRadiusMiles / EarthRadiusKm;

    /// <summary>
    /// Calculates the Haversine distance in kilometers between two geographical coordinates.
    /// </summary>
    public static double HaversineDistance(this GeoCoordinate from, GeoCoordinate to)
    {
        if (from == null) throw new ArgumentNullException(nameof(from));
        if (to == null) throw new ArgumentNullException(nameof(to));

        return EarthRadiusKm * ComputeAngularDistanceRadians(from, to);
    }

    /// <summary>
    /// Calculates the Haversine distance in miles between two geographical coordinates.
    /// </summary>
    public static double HaversineDistanceMiles(this GeoCoordinate from, GeoCoordinate to)
    {
        if (from == null) throw new ArgumentNullException(nameof(from));
        if (to == null) throw new ArgumentNullException(nameof(to));

        return EarthRadiusMiles * ComputeAngularDistanceRadians(from, to);
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

        var x = 0.0;
        var y = 0.0;
        var z = 0.0;
        var count = 0;

        foreach (var coord in source)
        {
            var lat = DegreesToRadians(coord.Latitude);
            var lon = DegreesToRadians(coord.Longitude);

            x += Math.Cos(lat) * Math.Cos(lon);
            y += Math.Cos(lat) * Math.Sin(lon);
            z += Math.Sin(lat);
            count++;
        }

        if (count == 0) throw new InvalidOperationException("Sequence contains no elements");

        var total = count;
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

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext()) throw new InvalidOperationException("Sequence contains no elements");

        var first = enumerator.Current;
        var minLat = first.Latitude;
        var maxLat = first.Latitude;
        var minLon = first.Longitude;
        var maxLon = first.Longitude;

        while (enumerator.MoveNext())
        {
            var coord = enumerator.Current;
            if (coord.Latitude < minLat) minLat = coord.Latitude;
            if (coord.Latitude > maxLat) maxLat = coord.Latitude;
            if (coord.Longitude < minLon) minLon = coord.Longitude;
            if (coord.Longitude > maxLon) maxLon = coord.Longitude;
        }

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
                var distanceMiles = distanceKm * KmToMiles;

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
            var distanceMiles = distanceKm * KmToMiles;

            yield return new GeoDistance(from, to, distanceKm, distanceMiles);
        }
    }

    /// <summary>
    /// Calculates the total distance of a route (sum of consecutive distances).
    /// </summary>
    public static double TotalDistance(this IEnumerable<GeoCoordinate> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext()) throw new InvalidOperationException("Sequence must contain at least two elements");

        var previous = enumerator.Current;
        var total = 0.0;
        var count = 1;

        while (enumerator.MoveNext())
        {
            var current = enumerator.Current;
            total += previous.HaversineDistance(current);
            previous = current;
            count++;
        }

        if (count < 2) throw new InvalidOperationException("Sequence must contain at least two elements");

        return total;
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
        var coordinates = items.Select(coordinateSelector).ToList();
        var clusters = new Dictionary<int, List<T>>();
        var assigned = new bool[items.Count];
        var nextClusterId = 0;

        for (var i = 0; i < items.Count; i++)
        {
            if (assigned[i])
                continue;

            var currentCluster = nextClusterId++;
            clusters[currentCluster] = new List<T> { items[i] };
            assigned[i] = true;

            var coord1 = coordinates[i];

            for (var j = i + 1; j < items.Count; j++)
            {
                if (assigned[j])
                    continue;

                var coord2 = coordinates[j];
                var distance = coord1.HaversineDistance(coord2);

                if (distance <= thresholdKm)
                {
                    clusters[currentCluster].Add(items[j]);
                    assigned[j] = true;
                }
            }
        }

        return clusters.Select(kvp => 
            new Grouping<int, T>(kvp.Key, kvp.Value));
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
    private static double RadiansToDegrees(double radians) => radians * 180.0 / Math.PI;

    private static double ComputeAngularDistanceRadians(GeoCoordinate from, GeoCoordinate to)
    {
        var lat1 = DegreesToRadians(from.Latitude);
        var lat2 = DegreesToRadians(to.Latitude);
        var dLat = DegreesToRadians(to.Latitude - from.Latitude);
        var dLon = DegreesToRadians(to.Longitude - from.Longitude);

        var sinHalfDLat = Math.Sin(dLat * 0.5);
        var sinHalfDLon = Math.Sin(dLon * 0.5);

        var a = sinHalfDLat * sinHalfDLat +
                Math.Cos(lat1) * Math.Cos(lat2) * sinHalfDLon * sinHalfDLon;

        // Clamp to [0, 1] to protect against slight floating-point drift.
        a = Math.Clamp(a, 0.0, 1.0);

        return 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
    }

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
