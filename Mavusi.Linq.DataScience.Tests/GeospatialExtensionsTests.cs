namespace Mavusi.Linq.DataScience.Tests;

public class GeospatialExtensionsTests
{
    [Fact]
    public void HaversineDistance_BetweenKnownLocations_CalculatesCorrectly()
    {
        // Arrange - New York to Los Angeles
        var newYork = new GeoCoordinate(40.7128, -74.0060);
        var losAngeles = new GeoCoordinate(34.0522, -118.2437);

        // Act
        var distance = newYork.HaversineDistance(losAngeles);

        // Assert - approximately 3944 km
        Assert.InRange(distance, 3935, 3955);
    }

    [Fact]
    public void HaversineDistanceMiles_CalculatesCorrectly()
    {
        // Arrange - London to Paris
        var london = new GeoCoordinate(51.5074, -0.1278);
        var paris = new GeoCoordinate(48.8566, 2.3522);

        // Act
        var distanceMiles = london.HaversineDistanceMiles(paris);

        // Assert - approximately 214 miles
        Assert.InRange(distanceMiles, 210, 218);
    }

    [Fact]
    public void HaversineDistance_SameLocation_ReturnsZero()
    {
        // Arrange
        var location = new GeoCoordinate(40.7128, -74.0060);

        // Act
        var distance = location.HaversineDistance(location);

        // Assert
        Assert.Equal(0, distance, precision: 5);
    }

    [Fact]
    public void WithinRadius_FiltersCorrectly()
    {
        // Arrange - Cities in Europe
        var cities = new[]
        {
            new { Name = "London", Coord = new GeoCoordinate(51.5074, -0.1278) },
            new { Name = "Paris", Coord = new GeoCoordinate(48.8566, 2.3522) },
            new { Name = "Brussels", Coord = new GeoCoordinate(50.8503, 4.3517) },
            new { Name = "Amsterdam", Coord = new GeoCoordinate(52.3676, 4.9041) },
            new { Name = "Berlin", Coord = new GeoCoordinate(52.5200, 13.4050) }
        };

        var center = new GeoCoordinate(50.8503, 4.3517); // Brussels

        // Act - Find cities within 300 km of Brussels
        var nearbyCities = cities.WithinRadius(c => c.Coord, center, 300).ToList();

        // Assert
        Assert.Contains(nearbyCities, c => c.Name == "Brussels");
        Assert.Contains(nearbyCities, c => c.Name == "Paris");
        Assert.Contains(nearbyCities, c => c.Name == "Amsterdam");
        Assert.DoesNotContain(nearbyCities, c => c.Name == "Berlin");
    }

    [Fact]
    public void Nearest_FindsClosestItem()
    {
        // Arrange
        var cities = new[]
        {
            new { Name = "Chicago", Coord = new GeoCoordinate(41.8781, -87.6298) },
            new { Name = "Milwaukee", Coord = new GeoCoordinate(43.0389, -87.9065) },
            new { Name = "Indianapolis", Coord = new GeoCoordinate(39.7684, -86.1581) }
        };

        var myLocation = new GeoCoordinate(43.1, -88.0); // Near Milwaukee

        // Act
        var nearest = cities.Nearest(c => c.Coord, myLocation);

        // Assert
        Assert.Equal("Milwaukee", nearest.Name);
    }

    [Fact]
    public void NearestN_ReturnsCorrectCount()
    {
        // Arrange
        var cities = new[]
        {
            new { Name = "A", Coord = new GeoCoordinate(40.0, -74.0) },
            new { Name = "B", Coord = new GeoCoordinate(40.1, -74.0) },
            new { Name = "C", Coord = new GeoCoordinate(40.2, -74.0) },
            new { Name = "D", Coord = new GeoCoordinate(40.3, -74.0) },
            new { Name = "E", Coord = new GeoCoordinate(40.4, -74.0) }
        };

        var target = new GeoCoordinate(40.0, -74.0);

        // Act
        var nearest3 = cities.NearestN(c => c.Coord, target, 3).ToList();

        // Assert
        Assert.Equal(3, nearest3.Count);
        Assert.Equal("A", nearest3[0].Name);
        Assert.Equal("B", nearest3[1].Name);
        Assert.Equal("C", nearest3[2].Name);
    }

    [Fact]
    public void GeographicCenter_CalculatesCorrectCenter()
    {
        // Arrange - Triangle of points
        var coordinates = new[]
        {
            new GeoCoordinate(0, 0),
            new GeoCoordinate(0, 10),
            new GeoCoordinate(10, 5)
        };

        // Act
        var center = coordinates.GeographicCenter();

        // Assert
        Assert.InRange(center.Latitude, 3, 4);
        Assert.InRange(center.Longitude, 4, 6);
    }

    [Fact]
    public void GeographicCenter_WithSelector_WorksCorrectly()
    {
        // Arrange
        var locations = new[]
        {
            new { Name = "A", Coord = new GeoCoordinate(40.0, -74.0) },
            new { Name = "B", Coord = new GeoCoordinate(40.5, -74.0) },
            new { Name = "C", Coord = new GeoCoordinate(40.25, -73.5) }
        };

        // Act
        var center = locations.GeographicCenter(l => l.Coord);

        // Assert
        Assert.NotNull(center);
        Assert.InRange(center.Latitude, 40.0, 40.5);
        Assert.InRange(center.Longitude, -74.0, -73.5);
    }

    [Fact]
    public void BoundingBox_CalculatesCorrectBounds()
    {
        // Arrange
        var coordinates = new[]
        {
            new GeoCoordinate(40.0, -74.0),
            new GeoCoordinate(41.0, -73.0),
            new GeoCoordinate(39.5, -74.5),
            new GeoCoordinate(40.5, -73.5)
        };

        // Act
        var bounds = coordinates.BoundingBox();

        // Assert
        Assert.Equal(39.5, bounds.MinLatitude);
        Assert.Equal(41.0, bounds.MaxLatitude);
        Assert.Equal(-74.5, bounds.MinLongitude);
        Assert.Equal(-73.0, bounds.MaxLongitude);
    }

    [Fact]
    public void BoundingBox_Center_CalculatesCorrectly()
    {
        // Arrange
        var coordinates = new[]
        {
            new GeoCoordinate(40.0, -74.0),
            new GeoCoordinate(42.0, -72.0)
        };

        var bounds = coordinates.BoundingBox();

        // Act
        var center = bounds.Center;

        // Assert
        Assert.Equal(41.0, center.Latitude);
        Assert.Equal(-73.0, center.Longitude);
    }

    [Fact]
    public void GeoBounds_Contains_WorksCorrectly()
    {
        // Arrange
        var bounds = new GeoBounds(40.0, 42.0, -75.0, -73.0);
        var inside = new GeoCoordinate(41.0, -74.0);
        var outside = new GeoCoordinate(43.0, -74.0);

        // Assert
        Assert.True(bounds.Contains(inside));
        Assert.False(bounds.Contains(outside));
    }

    [Fact]
    public void WithinBounds_FiltersCorrectly()
    {
        // Arrange
        var locations = new[]
        {
            new { Name = "Inside1", Coord = new GeoCoordinate(41.0, -74.0) },
            new { Name = "Inside2", Coord = new GeoCoordinate(40.5, -73.5) },
            new { Name = "Outside", Coord = new GeoCoordinate(43.0, -74.0) }
        };

        var bounds = new GeoBounds(40.0, 42.0, -75.0, -73.0);

        // Act
        var filtered = locations.WithinBounds(l => l.Coord, bounds).ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
        Assert.Contains(filtered, l => l.Name == "Inside1");
        Assert.Contains(filtered, l => l.Name == "Inside2");
        Assert.DoesNotContain(filtered, l => l.Name == "Outside");
    }

    [Fact]
    public void PairwiseDistances_CalculatesAllPairs()
    {
        // Arrange
        var coordinates = new[]
        {
            new GeoCoordinate(40.0, -74.0),
            new GeoCoordinate(41.0, -73.0),
            new GeoCoordinate(42.0, -72.0)
        };

        // Act
        var distances = coordinates.PairwiseDistances().ToList();

        // Assert
        Assert.Equal(3, distances.Count); // 3 choose 2 = 3 pairs
        Assert.All(distances, d => Assert.True(d.DistanceKm > 0));
        Assert.All(distances, d => Assert.True(d.DistanceMiles > 0));
    }

    [Fact]
    public void ConsecutiveDistances_CalculatesRouteSegments()
    {
        // Arrange - A simple route
        var route = new[]
        {
            new GeoCoordinate(40.0, -74.0),
            new GeoCoordinate(40.1, -74.0),
            new GeoCoordinate(40.2, -74.0),
            new GeoCoordinate(40.3, -74.0)
        };

        // Act
        var segments = route.ConsecutiveDistances().ToList();

        // Assert
        Assert.Equal(3, segments.Count); // 4 points = 3 segments
        Assert.All(segments, s => Assert.True(s.DistanceKm > 0));
    }

    [Fact]
    public void TotalDistance_CalculatesRouteLength()
    {
        // Arrange
        var route = new[]
        {
            new GeoCoordinate(40.0, -74.0),
            new GeoCoordinate(40.1, -74.0),
            new GeoCoordinate(40.2, -74.0)
        };

        // Act
        var totalDistance = route.TotalDistance();

        // Assert
        Assert.True(totalDistance > 0);

        // Verify it's the sum of consecutive distances
        var manualSum = route.ConsecutiveDistances().Sum(d => d.DistanceKm);
        Assert.Equal(manualSum, totalDistance);
    }

    [Fact]
    public void GroupByProximity_ClustersNearbyPoints()
    {
        // Arrange - Two clusters of points
        var points = new[]
        {
            new { Id = 1, Coord = new GeoCoordinate(40.0, -74.0) },
            new { Id = 2, Coord = new GeoCoordinate(40.01, -74.0) }, // Close to point 1
            new { Id = 3, Coord = new GeoCoordinate(41.0, -73.0) },  // Far from others
            new { Id = 4, Coord = new GeoCoordinate(41.01, -73.0) }  // Close to point 3
        };

        // Act - Group with 10km threshold (points within each pair are ~1km apart)
        var groups = points.GroupByProximity(p => p.Coord, 10).ToList();

        // Assert
        Assert.Equal(2, groups.Count); // Two clusters
        Assert.All(groups, g => Assert.Equal(2, g.Count()));
    }

    [Fact]
    public void HaversineDistance_ThrowsOnNullArguments()
    {
        // Arrange
        var coord = new GeoCoordinate(40.0, -74.0);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((GeoCoordinate)null).HaversineDistance(coord));
        Assert.Throws<ArgumentNullException>(() => coord.HaversineDistance(null));
    }

    [Fact]
    public void WithinRadius_InvalidRadius_ThrowsException()
    {
        // Arrange
        var data = new[] { new GeoCoordinate(40.0, -74.0) };
        var center = new GeoCoordinate(40.0, -74.0);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            data.WithinRadius(x => x, center, 0).ToList());
        Assert.Throws<ArgumentException>(() => 
            data.WithinRadius(x => x, center, -1).ToList());
    }

    [Fact]
    public void Nearest_EmptySequence_ThrowsException()
    {
        // Arrange
        var empty = Enumerable.Empty<GeoCoordinate>();
        var target = new GeoCoordinate(40.0, -74.0);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            empty.Nearest(x => x, target));
    }

    [Fact]
    public void RealWorldExample_FindNearestRestaurants()
    {
        // Arrange - Restaurant locations
        var restaurants = new[]
        {
            new { Name = "Pizza Place", Location = new GeoCoordinate(40.748817, -73.985428), Rating = 4.5 },
            new { Name = "Burger Joint", Location = new GeoCoordinate(40.750580, -73.993584), Rating = 4.2 },
            new { Name = "Sushi Bar", Location = new GeoCoordinate(40.752726, -73.977229), Rating = 4.8 },
            new { Name = "Taco Stand", Location = new GeoCoordinate(40.755083, -73.987821), Rating = 4.0 },
            new { Name = "Steakhouse", Location = new GeoCoordinate(40.745255, -73.982345), Rating = 4.7 }
        };

        var myLocation = new GeoCoordinate(40.750, -73.985); // Near Times Square

        // Act - Find 3 nearest restaurants
        var nearest = restaurants
            .NearestN(r => r.Location, myLocation, 3)
            .ToList();

        // Assert
        Assert.Equal(3, nearest.Count);
        Assert.All(nearest, r => Assert.NotNull(r.Name));
    }

    [Fact]
    public void RealWorldExample_DeliveryRadius()
    {
        // Arrange - Customer locations
        var customers = new[]
        {
            new { Id = 1, Name = "John", Location = new GeoCoordinate(40.748, -73.985) },
            new { Id = 2, Name = "Jane", Location = new GeoCoordinate(40.752, -73.977) },
            new { Id = 3, Name = "Bob", Location = new GeoCoordinate(40.780, -73.975) }  // Too far
        };

        var storeLocation = new GeoCoordinate(40.750, -73.985);
        var deliveryRadiusKm = 2.0;

        // Act - Find customers within delivery radius
        var inRange = customers
            .WithinRadius(c => c.Location, storeLocation, deliveryRadiusKm)
            .ToList();

        // Assert
        Assert.Contains(inRange, c => c.Name == "John");
        Assert.Contains(inRange, c => c.Name == "Jane");
        Assert.DoesNotContain(inRange, c => c.Name == "Bob");
    }

    [Fact]
    public void RealWorldExample_TripPlanning()
    {
        // Arrange - Road trip waypoints
        var roadTrip = new[]
        {
            new GeoCoordinate(34.0522, -118.2437), // Los Angeles
            new GeoCoordinate(36.7783, -119.4179), // Fresno
            new GeoCoordinate(37.7749, -122.4194), // San Francisco
            new GeoCoordinate(38.5816, -121.4944)  // Sacramento
        };

        // Act
        var totalDistance = roadTrip.TotalDistance();
        var bounds = roadTrip.BoundingBox();
        var center = roadTrip.GeographicCenter();

        // Assert
        Assert.True(totalDistance > 500); // Total trip > 500 km
        Assert.True(bounds.Contains(center));
        Assert.InRange(center.Latitude, 34, 39);
        Assert.InRange(center.Longitude, -123, -118);
    }

    [Fact]
    public void BoundingBox_WithSelector_WorksCorrectly()
    {
        // Arrange
        var locations = new[]
        {
            new { City = "A", Coord = new GeoCoordinate(40.0, -74.0) },
            new { City = "B", Coord = new GeoCoordinate(42.0, -72.0) }
        };

        // Act
        var bounds = locations.BoundingBox(l => l.Coord);

        // Assert
        Assert.Equal(40.0, bounds.MinLatitude);
        Assert.Equal(42.0, bounds.MaxLatitude);
        Assert.Equal(-74.0, bounds.MinLongitude);
        Assert.Equal(-72.0, bounds.MaxLongitude);
    }
}
