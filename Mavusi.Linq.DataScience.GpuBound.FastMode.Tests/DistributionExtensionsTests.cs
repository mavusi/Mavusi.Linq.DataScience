using Mavusi.Linq.DataScience.GpuBound.FastMode;

namespace Mavusi.Linq.DataScience.GpuBound.FastMode.Tests;

public class DistributionExtensionsTests
{
    [Fact]
    public void MedianGpu_OddCount_ReturnsMiddleValue()
    {
        // Arrange
        var data = new[] { 1.0, 3.0, 2.0, 5.0, 4.0 };

        try
        {
            // Act
            var result = data.MedianGpu();

            // Assert
            Assert.Equal(3.0f, result, precision: 4);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void MedianGpu_EvenCount_ReturnsAverage()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0 };

        try
        {
            // Act
            var result = data.MedianGpu();

            // Assert
            Assert.Equal(2.5f, result, precision: 4);
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
            new { Value = 10.0 },
            new { Value = 30.0 },
            new { Value = 20.0 }
        };

        try
        {
            // Act
            var result = data.MedianGpu(x => x.Value);

            // Assert
            Assert.Equal(20.0f, result, precision: 4);
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
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 };

        try
        {
            // Act
            var result = data.QuartileGpu(1);

            // Assert
            Assert.InRange(result, 2.5f, 3.5f);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void QuartileGpu_SecondQuartile_EqualsMedian()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };

        try
        {
            // Act
            var q2 = data.QuartileGpu(2);
            var median = data.MedianGpu();

            // Assert
            Assert.Equal(median, q2, precision: 4);
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
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 };

        try
        {
            // Act
            var result = data.QuartileGpu(3);

            // Assert
            Assert.InRange(result, 7.5f, 8.5f);
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
            var percentile50 = data.PercentileGpu(50);
            var median = data.MedianGpu();

            // Assert
            Assert.Equal(median, percentile50, precision: 4);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void PercentileGpu_90thPercentile_CalculatesCorrectly()
    {
        // Arrange
        var data = Enumerable.Range(1, 100).Select(x => (double)x).ToArray();

        try
        {
            // Act
            var result = data.PercentileGpu(90);

            // Assert
            Assert.InRange(result, 89f, 91f);
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

            // Assert - IQR should be Q3 - Q1
            Assert.True(result > 0);
            Assert.InRange(result, 4f, 6f);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void SkewnessGpu_SymmetricData_ReturnsNearZero()
    {
        // Arrange - Symmetric distribution around mean
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };

        try
        {
            // Act
            var result = data.SkewnessGpu();

            // Assert - Should be close to 0 for symmetric data
            Assert.InRange(result, -0.5f, 0.5f);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void SkewnessGpu_RightSkewed_ReturnsPositive()
    {
        // Arrange - Right-skewed data
        var data = new[] { 1.0, 2.0, 2.0, 3.0, 10.0 };

        try
        {
            // Act
            var result = data.SkewnessGpu();

            // Assert - Should be positive for right-skewed data
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
        // Arrange - Approximately normal distribution
        var data = new[] { 1.0, 2.0, 3.0, 3.0, 3.0, 4.0, 5.0 };

        try
        {
            // Act
            var result = data.KurtosisGpu();

            // Assert - Excess kurtosis should be near 0 for normal distribution
            Assert.InRange(result, -3f, 3f);
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
    public void QuartileGpu_InvalidQuartile_ThrowsException()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0 };

        try
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => data.QuartileGpu(0));
            Assert.Throws<ArgumentException>(() => data.QuartileGpu(4));
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }
}
