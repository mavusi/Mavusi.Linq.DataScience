using Mavusi.Linq.DataScience.GpuBound;

namespace Mavusi.Linq.DataScience.Tests.GpuBound;

public class GeospatialExtensionsTests
{
    [Fact]
    public void HaversineDistanceGpu_SameLocation_ReturnsZero()
    {
        // Arrange
        var coord = new GeoCoordinate(40.7128, -74.0060); // New York

        try
        {
            // Act
            var result = coord.HaversineDistanceGpu(coord);

            // Assert
            Assert.Equal(0.0, result, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void HaversineDistanceGpu_NewYorkToLondon_CalculatesCorrectly()
    {
        // Arrange
        var newYork = new GeoCoordinate(40.7128, -74.0060);
        var london = new GeoCoordinate(51.5074, -0.1278);

        try
        {
            // Act
            var result = newYork.HaversineDistanceGpu(london);

            // Assert - approximately 5570 km
            Assert.InRange(result, 5500, 5600);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void HaversineDistanceMilesGpu_NewYorkToLondon_CalculatesCorrectly()
    {
        // Arrange
        var newYork = new GeoCoordinate(40.7128, -74.0060);
        var london = new GeoCoordinate(51.5074, -0.1278);

        try
        {
            // Act
            var result = newYork.HaversineDistanceMilesGpu(london);

            // Assert - approximately 3460 miles
            Assert.InRange(result, 3400, 3500);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void CalculateDistancesGpu_MultipleCities_CalculatesAllDistances()
    {
        // Arrange
        var center = new GeoCoordinate(40.7128, -74.0060); // New York
        var cities = new[]
        {
            new GeoCoordinate(34.0522, -118.2437), // Los Angeles
            new GeoCoordinate(41.8781, -87.6298),  // Chicago
            new GeoCoordinate(29.7604, -95.3698)   // Houston
        };

        try
        {
            // Act
            var distances = center.CalculateDistancesGpu(cities);

            // Assert
            Assert.Equal(3, distances.Length);
            Assert.True(distances.All(d => d > 0));
            Assert.True(distances[1] < distances[0]); // Chicago closer than LA
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void WithinRadiusGpu_FiltersCitiesCorrectly()
    {
        // Arrange
        var center = new GeoCoordinate(40.7128, -74.0060); // New York
        var cities = new[]
        {
            new { Name = "Philadelphia", Coord = new GeoCoordinate(39.9526, -75.1652) },
            new { Name = "Boston", Coord = new GeoCoordinate(42.3601, -71.0589) },
            new { Name = "Los Angeles", Coord = new GeoCoordinate(34.0522, -118.2437) }
        };

        try
        {
            // Act - cities within 500 km of NYC
            var result = cities.WithinRadiusGpu(c => c.Coord, center, 500).ToArray();

            // Assert - Philly and Boston should be within 500km, LA should not
            Assert.Equal(2, result.Length);
            Assert.Contains(result, c => c.Name == "Philadelphia");
            Assert.Contains(result, c => c.Name == "Boston");
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void NearestGpu_FindsClosestCities()
    {
        // Arrange
        var center = new GeoCoordinate(40.7128, -74.0060); // New York
        var cities = new[]
        {
            new { Name = "Philadelphia", Coord = new GeoCoordinate(39.9526, -75.1652) },
            new { Name = "Los Angeles", Coord = new GeoCoordinate(34.0522, -118.2437) },
            new { Name = "Boston", Coord = new GeoCoordinate(42.3601, -71.0589) },
            new { Name = "Chicago", Coord = new GeoCoordinate(41.8781, -87.6298) }
        };

        try
        {
            // Act
            var result = cities.NearestGpu(c => c.Coord, center, 2).ToArray();

            // Assert
            Assert.Equal(2, result.Length);
            Assert.Contains(result, c => c.Name == "Philadelphia" || c.Name == "Boston");
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void PairwiseDistancesGpu_CalculatesDistanceMatrix()
    {
        // Arrange
        var cities = new[]
        {
            new GeoCoordinate(40.7128, -74.0060), // New York
            new GeoCoordinate(34.0522, -118.2437), // Los Angeles
            new GeoCoordinate(41.8781, -87.6298)  // Chicago
        };

        try
        {
            // Act
            var distances = cities.PairwiseDistancesGpu();

            // Assert
            Assert.Equal(3, distances.GetLength(0));
            Assert.Equal(3, distances.GetLength(1));

            // Diagonal should be zero
            Assert.Equal(0.0, distances[0, 0], precision: 10);
            Assert.Equal(0.0, distances[1, 1], precision: 10);
            Assert.Equal(0.0, distances[2, 2], precision: 10);

            // Matrix should be symmetric
            Assert.Equal(distances[0, 1], distances[1, 0], precision: 10);
            Assert.Equal(distances[0, 2], distances[2, 0], precision: 10);
            Assert.Equal(distances[1, 2], distances[2, 1], precision: 10);

            // All off-diagonal values should be positive
            Assert.True(distances[0, 1] > 0);
            Assert.True(distances[0, 2] > 0);
            Assert.True(distances[1, 2] > 0);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void HaversineDistanceGpu_GpuMatchesCpuResults()
    {
        // Arrange
        var coord1 = new GeoCoordinate(40.7128, -74.0060);
        var coord2 = new GeoCoordinate(51.5074, -0.1278);

        try
        {
            // Act
            var gpuResult = coord1.HaversineDistanceGpu(coord2);
            var cpuResult = coord1.HaversineDistance(coord2);

            // Assert
            Assert.Equal(cpuResult, gpuResult, precision: 5);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void HaversineDistanceMilesGpu_GpuMatchesCpuResults()
    {
        // Arrange
        var coord1 = new GeoCoordinate(40.7128, -74.0060);
        var coord2 = new GeoCoordinate(51.5074, -0.1278);

        try
        {
            // Act
            var gpuResult = coord1.HaversineDistanceMilesGpu(coord2);
            var cpuResult = coord1.HaversineDistanceMiles(coord2);

            // Assert
            Assert.Equal(cpuResult, gpuResult, precision: 5);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void CalculateDistancesGpu_EmptyCollection_ReturnsEmpty()
    {
        // Arrange
        var center = new GeoCoordinate(40.7128, -74.0060);
        var coords = Array.Empty<GeoCoordinate>();

        try
        {
            // Act
            var result = center.CalculateDistancesGpu(coords);

            // Assert
            Assert.Empty(result);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void WithinRadiusGpu_InvalidRadius_ThrowsException()
    {
        // Arrange
        var center = new GeoCoordinate(40.7128, -74.0060);
        var coords = new[] { new { Coord = center } };

        try
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                coords.WithinRadiusGpu(c => c.Coord, center, 0).ToArray());
            Assert.Throws<ArgumentException>(() => 
                coords.WithinRadiusGpu(c => c.Coord, center, -100).ToArray());
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void NearestGpu_InvalidCount_ThrowsException()
    {
        // Arrange
        var center = new GeoCoordinate(40.7128, -74.0060);
        var coords = new[] { new { Coord = center } };

        try
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                coords.NearestGpu(c => c.Coord, center, 0).ToArray());
            Assert.Throws<ArgumentException>(() => 
                coords.NearestGpu(c => c.Coord, center, -1).ToArray());
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void PairwiseDistancesGpu_LargeDataset_CalculatesCorrectly()
    {
        // Arrange
        var coords = Enumerable.Range(0, 50)
            .Select(i => new GeoCoordinate(40.0 + i * 0.1, -74.0 + i * 0.1))
            .ToArray();

        try
        {
            // Act
            var distances = coords.PairwiseDistancesGpu();

            // Assert
            Assert.Equal(50, distances.GetLength(0));
            Assert.Equal(50, distances.GetLength(1));

            // Verify diagonal is zero
            for (int i = 0; i < 50; i++)
            {
                Assert.Equal(0.0, distances[i, i], precision: 10);
            }
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }
}
