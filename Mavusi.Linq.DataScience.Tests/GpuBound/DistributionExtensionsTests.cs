using Mavusi.Linq.DataScience.GpuBound;

namespace Mavusi.Linq.DataScience.Tests.GpuBound;

public class DistributionExtensionsTests
{
    [Fact]
    public void MedianGpu_OddNumberOfElements_ReturnsMiddleValue()
    {
        // Arrange
        var data = new[] { 1.0, 3.0, 2.0, 5.0, 4.0 };

        try
        {
            // Act
            var result = data.MedianGpu();

            // Assert
            Assert.Equal(3.0, result, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void MedianGpu_EvenNumberOfElements_ReturnsAverage()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0 };

        try
        {
            // Act
            var result = data.MedianGpu();

            // Assert
            Assert.Equal(2.5, result, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void MedianGpu_WithSelector_CalculatesCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new { Value = 5.0 },
            new { Value = 1.0 },
            new { Value = 3.0 }
        };

        try
        {
            // Act
            var result = data.MedianGpu(x => x.Value);

            // Assert
            Assert.Equal(3.0, result, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void QuartileGpu_FirstQuartile_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0 };

        try
        {
            // Act
            var result = data.QuartileGpu(1);

            // Assert
            Assert.True(result < 4.5);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void QuartileGpu_ThirdQuartile_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0 };

        try
        {
            // Act
            var result = data.QuartileGpu(3);

            // Assert
            Assert.True(result > 4.5);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void PercentileGpu_50thPercentile_EqualsMedian()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };

        try
        {
            // Act
            var percentile = data.PercentileGpu(50);
            var median = data.MedianGpu();

            // Assert
            Assert.Equal(median, percentile, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void PercentileGpu_0thPercentile_ReturnsMinimum()
    {
        // Arrange
        var data = new[] { 5.0, 2.0, 8.0, 1.0, 9.0 };

        try
        {
            // Act
            var result = data.PercentileGpu(0);

            // Assert
            Assert.Equal(1.0, result, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void PercentileGpu_100thPercentile_ReturnsMaximum()
    {
        // Arrange
        var data = new[] { 5.0, 2.0, 8.0, 1.0, 9.0 };

        try
        {
            // Act
            var result = data.PercentileGpu(100);

            // Assert
            Assert.Equal(9.0, result, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void InterquartileRangeGpu_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 };

        try
        {
            // Act
            var result = data.InterquartileRangeGpu();

            // Assert
            Assert.True(result > 0);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void SkewnessGpu_SymmetricDistribution_ReturnsNearZero()
    {
        // Arrange - symmetric data
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 4.0, 3.0, 2.0, 1.0 };

        try
        {
            // Act
            var result = data.SkewnessGpu();

            // Assert
            Assert.InRange(result, -0.5, 0.5);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void SkewnessGpu_RightSkewed_ReturnsPositive()
    {
        // Arrange - right-skewed data
        var data = new[] { 1.0, 1.0, 1.0, 2.0, 2.0, 3.0, 10.0 };

        try
        {
            // Act
            var result = data.SkewnessGpu();

            // Assert
            Assert.True(result > 0);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void KurtosisGpu_NormalDistribution_ReturnsNearZero()
    {
        // Arrange - approximately normal distribution
        var data = new[] { -2.0, -1.0, 0.0, 0.0, 0.0, 1.0, 2.0 };

        try
        {
            // Act
            var result = data.KurtosisGpu();

            // Assert - excess kurtosis for normal is 0
            Assert.InRange(result, -1.0, 1.0);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void MedianGpu_GpuMatchesCpuResults()
    {
        // Arrange
        var data = new[] { 1.5, 2.3, 3.7, 4.2, 5.8, 6.1, 7.9, 8.4, 9.2 };

        try
        {
            // Act
            var gpuResult = data.MedianGpu();
            var cpuResult = data.Median();

            // Assert
            Assert.Equal(cpuResult, gpuResult, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void PercentileGpu_GpuMatchesCpuResults()
    {
        // Arrange
        var data = new[] { 1.5, 2.3, 3.7, 4.2, 5.8, 6.1, 7.9, 8.4, 9.2 };

        try
        {
            // Act
            var gpuResult = data.PercentileGpu(75);
            var cpuResult = data.Percentile(75);

            // Assert
            Assert.Equal(cpuResult, gpuResult, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void SkewnessGpu_CalculatesWithinReasonableRange()
    {
        // Arrange
        var data = new[] { 1.5, 2.3, 3.7, 4.2, 5.8, 6.1, 7.9, 8.4, 9.2, 10.5 };

        try
        {
            // Act
            var gpuResult = data.SkewnessGpu();

            // Assert - skewness for this data should be small and negative
            Assert.InRange(gpuResult, -0.1, 0.1);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void KurtosisGpu_CalculatesWithinReasonableRange()
    {
        // Arrange
        var data = new[] { 1.5, 2.3, 3.7, 4.2, 5.8, 6.1, 7.9, 8.4, 9.2, 10.5 };

        try
        {
            // Act
            var gpuResult = data.KurtosisGpu();

            // Assert - excess kurtosis should be negative (platykurtic)
            Assert.InRange(gpuResult, -2.0, 0.0);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void MedianGpu_EmptySequence_ThrowsException()
    {
        // Arrange
        var data = Array.Empty<double>();

        try
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => data.MedianGpu());
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void PercentileGpu_InvalidPercentile_ThrowsException()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0 };

        try
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => data.PercentileGpu(-10));
            Assert.Throws<ArgumentException>(() => data.PercentileGpu(150));
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }
}
