using Mavusi.Linq.DataScience.GpuBound.FastMode;
using Mavusi.Linq.DataScience.Models;

namespace Mavusi.Linq.DataScience.GpuBound.FastMode.Tests;

public class GeospatialExtensionsTests
{
    [Fact]
    public void HaversineDistance_SamePoint_ReturnsZero()
    {
        // Arrange
        var coord1 = new GeoCoordinate(40.7128, -74.0060); // New York
        var coord2 = new GeoCoordinate(40.7128, -74.0060);

        try
        {
            // Act
            var result = coord1.HaversineDistance(coord2);

            // Assert
            Assert.Equal(0.0f, result, precision: 4);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void HaversineDistance_KnownDistance_CalculatesCorrectly()
    {
        // Arrange
        var newYork = new GeoCoordinate(40.7128, -74.0060);
        var london = new GeoCoordinate(51.5074, -0.1278);

        try
        {
            // Act
            var result = newYork.HaversineDistance(london);

            // Assert - Distance should be approximately 5570 km
            Assert.InRange(result, 5500f, 5600f);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void HaversineDistanceMiles_KnownDistance_CalculatesCorrectly()
    {
        // Arrange
        var newYork = new GeoCoordinate(40.7128, -74.0060);
        var losAngeles = new GeoCoordinate(34.0522, -118.2437);

        try
        {
            // Act
            var result = newYork.HaversineDistanceMiles(losAngeles);

            // Assert - Distance should be approximately 2450 miles
            Assert.InRange(result, 2400f, 2500f);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void CalculateDistancesGpu_MultiplePoints_CalculatesCorrectly()
    {
        // Arrange
        var origin = new GeoCoordinate(0.0, 0.0);
        var destinations = new[]
        {
            new GeoCoordinate(0.0, 0.0),
            new GeoCoordinate(1.0, 0.0),
            new GeoCoordinate(0.0, 1.0)
        };

        try
        {
            // Act
            var result = origin.CalculateDistancesGpu(destinations).ToArray();

            // Assert
            Assert.Equal(3, result.Length);
            Assert.Equal(0.0f, result[0], precision: 4); // Same point
            Assert.True(result[1] > 0); // Different points
            Assert.True(result[2] > 0); // Different points
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void WithinRadiusGpu_FiltersCorrectly()
    {
        // Arrange
        var center = new GeoCoordinate(40.7128, -74.0060); // New York
        var locations = new[]
        {
            new GeoCoordinate(40.7128, -74.0060), // Same location
            new GeoCoordinate(40.7589, -73.9851), // Times Square (~7 km)
            new GeoCoordinate(51.5074, -0.1278)   // London (~5570 km)
        };
        var radiusKm = 10.0;

        try
        {
            // Act
            var result = locations.WithinRadiusGpu(c => c, center, (float)radiusKm).ToArray();

            // Assert - Should include New York and Times Square, but not London
            Assert.Equal(2, result.Length);
            Assert.Contains(result, c => Math.Abs(c.Latitude - 40.7128) < 0.0001 && Math.Abs(c.Longitude - (-74.0060)) < 0.0001);
            Assert.Contains(result, c => Math.Abs(c.Latitude - 40.7589) < 0.0001);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void NearestGpu_FindsClosestPoint()
    {
        // Arrange
        var target = new GeoCoordinate(40.7128, -74.0060); // New York
        var candidates = new[]
        {
            new GeoCoordinate(51.5074, -0.1278),   // London
            new GeoCoordinate(40.7589, -73.9851),  // Times Square (closest)
            new GeoCoordinate(34.0522, -118.2437)  // Los Angeles
        };

        try
        {
            // Act
            var result = candidates.NearestGpu(c => c, target, 1).First();

            // Assert - Should return Times Square
            Assert.Equal(40.7589, result.Latitude, precision: 4);
            Assert.Equal(-73.9851, result.Longitude, precision: 4);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void PairwiseDistancesGpu_CalculatesAllDistances()
    {
        // Arrange
        var locations = new[]
        {
            new GeoCoordinate(0.0, 0.0),
            new GeoCoordinate(1.0, 0.0),
            new GeoCoordinate(0.0, 1.0)
        };

        try
        {
            // Act
            var result = locations.PairwiseDistancesGpu();

            // Assert - Should have 3x3 = 9 distances
            Assert.Equal(3, result.GetLength(0));
            Assert.Equal(3, result.GetLength(1));

            // Diagonal should be zero (distance to self)
            Assert.Equal(0.0f, result[0, 0], precision: 4); // [0,0] to [0,0]
            Assert.Equal(0.0f, result[1, 1], precision: 4); // [1,0] to [1,0]
            Assert.Equal(0.0f, result[2, 2], precision: 4); // [0,1] to [0,1]

            // Non-diagonal should be positive
            Assert.True(result[0, 1] > 0); // [0,0] to [1,0]
            Assert.True(result[0, 2] > 0); // [0,0] to [0,1]
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void HaversineDistance_Equator_CalculatesCorrectly()
    {
        // Arrange - Two points on the equator, 1 degree apart
        var coord1 = new GeoCoordinate(0.0, 0.0);
        var coord2 = new GeoCoordinate(0.0, 1.0);

        try
        {
            // Act
            var result = coord1.HaversineDistance(coord2);

            // Assert - 1 degree at equator ≈ 111 km
            Assert.InRange(result, 110f, 112f);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void WithinRadiusGpu_EmptySequence_ReturnsEmpty()
    {
        // Arrange
        var center = new GeoCoordinate(40.7128, -74.0060);
        var locations = Array.Empty<GeoCoordinate>();

        try
        {
            // Act
            var result = locations.WithinRadiusGpu(c => c, center, 100.0f).ToArray();

            // Assert
            Assert.Empty(result);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void NearestGpu_SinglePoint_ReturnsThatPoint()
    {
        // Arrange
        var target = new GeoCoordinate(40.7128, -74.0060);
        var candidates = new[] { new GeoCoordinate(51.5074, -0.1278) };

        try
        {
            // Act
            var result = candidates.NearestGpu(c => c, target, 1).First();

            // Assert
            Assert.Equal(51.5074, result.Latitude, precision: 4);
            Assert.Equal(-0.1278, result.Longitude, precision: 4);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void NearestGpu_EmptySequence_ThrowsException()
    {
        // Arrange
        var target = new GeoCoordinate(40.7128, -74.0060);
        var candidates = Array.Empty<GeoCoordinate>();

        try
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => candidates.NearestGpu(c => c, target, 1).First());
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void HaversineDistance_AntipodePoints_CalculatesMaxDistance()
    {
        // Arrange - Opposite sides of Earth
        var north = new GeoCoordinate(90.0, 0.0);  // North pole
        var south = new GeoCoordinate(-90.0, 0.0); // South pole

        try
        {
            // Act
            var result = north.HaversineDistance(south);

            // Assert - Should be approximately half Earth's circumference (~20,000 km)
            Assert.InRange(result, 19000f, 21000f);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }
}
