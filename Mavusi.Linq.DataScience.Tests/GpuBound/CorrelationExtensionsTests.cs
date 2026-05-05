using Mavusi.Linq.DataScience.GpuBound;
using GpuCorrelation = Mavusi.Linq.DataScience.GpuBound.CorrelationExtensions;

namespace Mavusi.Linq.DataScience.Tests.GpuBound;

public class CorrelationExtensionsTests
{
    [Fact]
    public void Correlation_PerfectPositiveCorrelation_ReturnsOne()
    {
        // Arrange
        var x = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var y = new[] { 2.0, 4.0, 6.0, 8.0, 10.0 };

        // Act
        var result = x.CorrelationGpu(y);

        // Assert
        Assert.Equal(1.0, result, precision: 10);
    }

    [Fact]
    public void Correlation_PerfectNegativeCorrelation_ReturnsMinusOne()
    {
        // Arrange
        var x = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var y = new[] { 10.0, 8.0, 6.0, 4.0, 2.0 };

        // Act
        var result = x.CorrelationGpu(y);

        // Assert
        Assert.Equal(-1.0, result, precision: 10);
    }

    [Fact]
    public void Correlation_NoCorrelation_ReturnsZero()
    {
        // Arrange - perfectly uncorrelated data
        var x = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var y = new[] { 3.0, 3.0, 3.0, 3.0, 3.0 }; // constant

        // Act
        var result = x.CorrelationGpu(y);

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Correlation_WithSelectors_ReturnsCorrectValue()
    {
        // Arrange
        var data = new[]
        {
            new { Height = 160.0, Weight = 60.0 },
            new { Height = 170.0, Weight = 70.0 },
            new { Height = 180.0, Weight = 80.0 },
            new { Height = 190.0, Weight = 90.0 }
        };

        // Act
        var result = data.CorrelationGpu(d => d.Height, d => d.Weight);

        // Assert - perfect positive correlation
        Assert.Equal(1.0, result, precision: 10);
    }

    [Fact]
    public void Correlation_DifferentLengthSequences_ThrowsException()
    {
        // Arrange
        var x = new[] { 1.0, 2.0, 3.0 };
        var y = new[] { 1.0, 2.0 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => x.CorrelationGpu(y));
    }

    [Fact]
    public void Covariance_WithLinearRelationship_ReturnsPositiveValue()
    {
        // Arrange
        var x = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var y = new[] { 2.0, 4.0, 6.0, 8.0, 10.0 };

        // Act
        var result = x.CovarianceGpu(y);

        // Assert
        Assert.True(result > 0);
    }

    [Fact]
    public void Covariance_WithInverseRelationship_ReturnsNegativeValue()
    {
        // Arrange
        var x = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var y = new[] { 10.0, 8.0, 6.0, 4.0, 2.0 };

        // Act
        var result = x.CovarianceGpu(y);

        // Assert
        Assert.True(result < 0);
    }

    [Fact]
    public void Covariance_WithSelectors_CalculatesCorrectly()
    {
        // Arrange
        var students = new[]
        {
            new { StudyHours = 1.0, Score = 50.0 },
            new { StudyHours = 2.0, Score = 60.0 },
            new { StudyHours = 3.0, Score = 70.0 },
            new { StudyHours = 4.0, Score = 80.0 }
        };

        // Act
        var result = students.CovarianceGpu(s => s.StudyHours, s => s.Score);

        // Assert - positive covariance expected
        Assert.True(result > 0);
    }

    [Fact]
    public void Correlation_RealWorldExample_StockPrices()
    {
        // Arrange - simulating two correlated stock prices
        var stockA = new[] { 100.0, 102.0, 101.0, 105.0, 107.0 };
        var stockB = new[] { 50.0, 51.0, 50.5, 52.5, 53.5 };

        // Act
        var correlation = stockA.CorrelationGpu(stockB);

        // Assert - should be highly correlated
        Assert.True(correlation > 0.9);
    }

    [Fact]
    public void Correlation_LargeDataset_CalculatesCorrectly()
    {
        // Arrange - large dataset to benefit from GPU parallelization
        var size = 10000;
        var x = Enumerable.Range(0, size).Select(i => (float)i).ToArray();
        var y = Enumerable.Range(0, size).Select(i => (float)i * 2 + 5).ToArray();

        // Act
        var result = x.CorrelationGpu(y);

        // Assert - perfect positive correlation
        Assert.Equal(1.0, result, precision: 10);
    }

    [Fact]
    public void Correlation_GpuMatchesCpuResults()
    {
        // Arrange
        var x = new[] { 1.5, 2.3, 3.7, 4.2, 5.8, 6.1, 7.9, 8.4, 9.2, 10.5 };
        var y = new[] { 3.2, 4.1, 5.6, 6.0, 7.3, 8.5, 9.1, 10.2, 11.8, 12.4 };

        try
        {
            // Act - GPU version from this namespace
            var gpuResult = x.CorrelationGpu(y);
            // CPU version from base namespace
            var cpuResult = DataScience.CorrelationExtensions.Correlation(x, y);

            // Assert - GPU and CPU should produce the same result
            Assert.Equal(cpuResult, gpuResult, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            // Skip test if device doesn't support double precision - test passes
            return;
        }
    }

    [Fact]
    public void Covariance_GpuMatchesCpuResults()
    {
        // Arrange
        var x = new[] { 1.5, 2.3, 3.7, 4.2, 5.8, 6.1, 7.9, 8.4, 9.2, 10.5 };
        var y = new[] { 3.2, 4.1, 5.6, 6.0, 7.3, 8.5, 9.1, 10.2, 11.8, 12.4 };

        try
        {
            // Act - GPU version from this namespace
            var gpuResult = x.CovarianceGpu(y);
            // CPU version from base namespace
            var cpuResult = DataScience.CorrelationExtensions.Covariance(x, y);

            // Assert - GPU and CPU should produce the same result
            Assert.Equal(cpuResult, gpuResult, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            // Skip test if device doesn't support double precision - test passes
            return;
        }
    }
}
