using Mavusi.Linq.DataScience.GpuBound.FastMode;
using Mavusi.Linq.DataScience.Models;

namespace Mavusi.Linq.DataScience.GpuBound.FastMode.Tests;

public class TimeSeriesExtensionsTests
{
    [Fact]
    public void DifferenceGpu_SimpleData_CalculatesCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Now, 10.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(1), 15.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(2), 12.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(3), 18.0)
        };

        try
        {
            // Act
            var result = data.DifferenceGpu().ToArray();

            // Assert
            Assert.Equal(3, result.Length);
            Assert.Equal(5.0f, (float)result[0].Value, precision: 4); // 15 - 10
            Assert.Equal(-3.0f, (float)result[1].Value, precision: 4); // 12 - 15
            Assert.Equal(6.0f, (float)result[2].Value, precision: 4); // 18 - 12
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void PercentageChangeGpu_SimpleData_CalculatesCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Now, 100.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(1), 110.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(2), 99.0)
        };

        try
        {
            // Act
            var result = data.PercentageChangeGpu().ToArray();

            // Assert
            Assert.Equal(2, result.Length);
            Assert.Equal(10.0f, (float)result[0].Value, precision: 3); // 10% increase
            Assert.True(result[1].Value < 0); // Decrease from 110 to 99
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void CumulativeSumGpu_SimpleData_CalculatesCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Now, 1.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(1), 2.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(2), 3.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(3), 4.0)
        };

        try
        {
            // Act
            var result = data.CumulativeSumGpu().ToArray();

            // Assert
            Assert.Equal(4, result.Length);
            Assert.Equal(1.0f, (float)result[0].Value, precision: 4);
            Assert.Equal(3.0f, (float)result[1].Value, precision: 4);
            Assert.Equal(6.0f, (float)result[2].Value, precision: 4);
            Assert.Equal(10.0f, (float)result[3].Value, precision: 4);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void ExponentialMovingAverageGpu_SimpleData_CalculatesCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Now, 10.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(1), 20.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(2), 30.0)
        };
        var alpha = 0.5f;

        try
        {
            // Act
            var result = data.ExponentialMovingAverageGpu(alpha).ToArray();

            // Assert
            Assert.Equal(3, result.Length);
            Assert.Equal(10.0f, (float)result[0].Value, precision: 4); // First value
            Assert.True(result[1].Value > 10.0 && result[1].Value < 20.0); // Weighted average
            Assert.True(result[2].Value > result[1].Value); // Increasing trend
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void DetectOutliersGpu_WithOutliers_DetectsCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Now, 10.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(1), 12.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(2), 11.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(3), 100.0), // Outlier
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(4), 10.5)
        };

        try
        {
            // Act
            var result = data.DetectOutliersGpu(2.0f).ToArray();

            // Assert
            Assert.NotEmpty(result);
            Assert.Contains(result, p => p.Value == 100.0);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void DetectOutliersGpu_NoOutliers_ReturnsEmpty()
    {
        // Arrange
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Now, 10.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(1), 11.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(2), 12.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(3), 11.0)
        };

        try
        {
            // Act
            var result = data.DetectOutliersGpu(3.0f).ToArray();

            // Assert
            Assert.Empty(result);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void ReturnsGpu_SimpleData_CalculatesCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Now, 100.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(1), 110.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(2), 121.0)
        };

        try
        {
            // Act
            var result = data.ReturnsGpu(100.0f).ToArray();

            // Assert
            Assert.Equal(3, result.Length);
            Assert.Equal(0.0f, (float)result[0].Value, precision: 4); // Base is 0%
            Assert.Equal(10.0f, (float)result[1].Value, precision: 3); // 10% return
            Assert.Equal(21.0f, (float)result[2].Value, precision: 3); // 21% return
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void DifferenceGpu_EmptySequence_ReturnsEmpty()
    {
        // Arrange
        var data = Array.Empty<TimeSeriesPoint<double>>();

        try
        {
            // Act
            var result = data.DifferenceGpu().ToArray();

            // Assert
            Assert.Empty(result);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void DifferenceGpu_SinglePoint_ReturnsEmpty()
    {
        // Arrange
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Now, 10.0)
        };

        try
        {
            // Act
            var result = data.DifferenceGpu().ToArray();

            // Assert
            Assert.Empty(result);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void PercentageChangeGpu_WithZeroValue_HandlesCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Now, 0.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(1), 10.0)
        };

        try
        {
            // Act
            var result = data.PercentageChangeGpu().ToArray();

            // Assert
            Assert.Single(result);
            Assert.Equal(0.0f, (float)result[0].Value, precision: 4); // Should handle division by zero
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void ExponentialMovingAverageGpu_InvalidAlpha_ThrowsException()
    {
        // Arrange
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Now, 10.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(1), 20.0)
        };

        try
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => data.ExponentialMovingAverageGpu(-0.5f).ToArray());
            Assert.Throws<ArgumentException>(() => data.ExponentialMovingAverageGpu(1.5f).ToArray());
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void CumulativeSumGpu_NegativeValues_CalculatesCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Now, 10.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(1), -5.0),
            new TimeSeriesPoint<double>(DateTime.Now.AddDays(2), 3.0)
        };

        try
        {
            // Act
            var result = data.CumulativeSumGpu().ToArray();

            // Assert
            Assert.Equal(3, result.Length);
            Assert.Equal(10.0f, (float)result[0].Value, precision: 4);
            Assert.Equal(5.0f, (float)result[1].Value, precision: 4);
            Assert.Equal(8.0f, (float)result[2].Value, precision: 4);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }
}
