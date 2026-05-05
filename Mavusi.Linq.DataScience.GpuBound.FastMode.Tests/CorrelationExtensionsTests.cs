using Mavusi.Linq.DataScience.GpuBound.FastMode;

namespace Mavusi.Linq.DataScience.GpuBound.FastMode.Tests;

public class CorrelationExtensionsTests
{
    [Fact]
    public void CorrelationGpu_PerfectPositiveCorrelation_ReturnsOne()
    {
        // Arrange
        var x = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var y = new[] { 2.0, 4.0, 6.0, 8.0, 10.0 };

        try
        {
            // Act
            var result = x.CorrelationGpu(y);

            // Assert - Perfect positive correlation
            Assert.InRange(result, 0.99f, 1.01f);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void CorrelationGpu_PerfectNegativeCorrelation_ReturnsNegativeOne()
    {
        // Arrange
        var x = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var y = new[] { 10.0, 8.0, 6.0, 4.0, 2.0 };

        try
        {
            // Act
            var result = x.CorrelationGpu(y);

            // Assert - Perfect negative correlation
            Assert.InRange(result, -1.01f, -0.99f);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void CorrelationGpu_NoCorrelation_ReturnsNearZero()
    {
        // Arrange
        var x = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var y = new[] { 3.0, 3.0, 3.0, 3.0, 3.0 }; // Constant values

        try
        {
            // Act
            var result = x.CorrelationGpu(y);

            // Assert - No correlation when one series is constant
            Assert.True(float.IsNaN(result) || Math.Abs(result) < 0.1f);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void CorrelationGpu_WithSelectors_CalculatesCorrectly()
    {
        // Arrange
        var data1 = new[]
        {
            new { X = 1.0 },
            new { X = 2.0 },
            new { X = 3.0 }
        };
        var data2 = new[]
        {
            new { Y = 2.0 },
            new { Y = 4.0 },
            new { Y = 6.0 }
        };

        try
        {
            // Act
            var values1 = data1.Select(a => a.X);
            var values2 = data2.Select(b => b.Y);
            var result = values1.CorrelationGpu(values2);

            // Assert
            Assert.InRange(result, 0.99f, 1.01f);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void CovarianceGpu_PositiveCovariance_ReturnsPositive()
    {
        // Arrange
        var x = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var y = new[] { 2.0, 4.0, 6.0, 8.0, 10.0 };

        try
        {
            // Act
            var result = x.CovarianceGpu(y);

            // Assert
            Assert.True(result > 0);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void CovarianceGpu_NegativeCovariance_ReturnsNegative()
    {
        // Arrange
        var x = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var y = new[] { 10.0, 8.0, 6.0, 4.0, 2.0 };

        try
        {
            // Act
            var result = x.CovarianceGpu(y);

            // Assert
            Assert.True(result < 0);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void CovarianceGpu_WithSelectors_CalculatesCorrectly()
    {
        // Arrange
        var data1 = new[]
        {
            new { Value = 1.0 },
            new { Value = 2.0 },
            new { Value = 3.0 }
        };
        var data2 = new[]
        {
            new { Value = 3.0 },
            new { Value = 2.0 },
            new { Value = 1.0 }
        };

        try
        {
            // Act
            var values1 = data1.Select(a => a.Value);
            var values2 = data2.Select(b => b.Value);
            var result = values1.CovarianceGpu(values2);

            // Assert - Should be negative covariance
            Assert.True(result < 0);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void CorrelationGpu_UnequalLengths_ThrowsException()
    {
        // Arrange
        var x = new[] { 1.0, 2.0, 3.0 };
        var y = new[] { 1.0, 2.0 };

        try
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => x.CorrelationGpu(y));
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void CovarianceGpu_EmptySequences_ThrowsException()
    {
        // Arrange
        var x = Array.Empty<double>();
        var y = Array.Empty<double>();

        try
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => x.CovarianceGpu(y));
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void CorrelationGpu_RealWorldData_CalculatesCorrectly()
    {
        // Arrange - Temperature and ice cream sales (should be positively correlated)
        var temperature = new[] { 20.0, 22.0, 25.0, 28.0, 30.0, 32.0 };
        var sales = new[] { 50.0, 55.0, 65.0, 75.0, 80.0, 90.0 };

        try
        {
            // Act
            var result = temperature.CorrelationGpu(sales);

            // Assert - Should show strong positive correlation
            Assert.InRange(result, 0.8f, 1.0f);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }
}
