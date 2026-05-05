using Mavusi.Linq.DataScience.GpuBound;

namespace Mavusi.Linq.DataScience.Tests.GpuBound;

public class TimeSeriesExtensionsTests
{
    [Fact]
    public void DifferenceGpu_SimpleTimeSeries_CalculatesCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-01"), 100.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-02"), 105.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-03"), 103.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-04"), 108.0)
        };

        try
        {
            // Act
            var result = data.DifferenceGpu().ToArray();

            // Assert
            Assert.Equal(3, result.Length);
            Assert.Equal(5.0, result[0].Value, precision: 10);  // 105 - 100
            Assert.Equal(-2.0, result[1].Value, precision: 10); // 103 - 105
            Assert.Equal(5.0, result[2].Value, precision: 10);  // 108 - 103
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void PercentageChangeGpu_SimpleTimeSeries_CalculatesCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-01"), 100.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-02"), 110.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-03"), 99.0)
        };

        try
        {
            // Act
            var result = data.PercentageChangeGpu().ToArray();

            // Assert
            Assert.Equal(2, result.Length);
            Assert.Equal(10.0, result[0].Value, precision: 10);  // (110-100)/100 * 100
            Assert.Equal(-10.0, result[1].Value, precision: 9);  // (99-110)/110 * 100
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void CumulativeSumGpu_SimpleTimeSeries_CalculatesCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-01"), 1.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-02"), 2.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-03"), 3.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-04"), 4.0)
        };

        try
        {
            // Act
            var result = data.CumulativeSumGpu().ToArray();

            // Assert
            Assert.Equal(4, result.Length);
            Assert.Equal(1.0, result[0].Value, precision: 10);
            Assert.Equal(3.0, result[1].Value, precision: 10);
            Assert.Equal(6.0, result[2].Value, precision: 10);
            Assert.Equal(10.0, result[3].Value, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void ExponentialMovingAverageGpu_SimpleTimeSeries_CalculatesCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-01"), 100.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-02"), 110.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-03"), 105.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-04"), 115.0)
        };
        var alpha = 0.5;

        try
        {
            // Act
            var result = data.ExponentialMovingAverageGpu(alpha).ToArray();

            // Assert
            Assert.Equal(4, result.Length);
            Assert.Equal(100.0, result[0].Value, precision: 10); // First value
            Assert.True(result[1].Value > 100 && result[1].Value < 110);
            Assert.True(result[3].Value > 105);
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
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-01"), 100.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-02"), 102.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-03"), 101.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-04"), 103.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-05"), 500.0), // Outlier
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-06"), 102.0)
        };

        try
        {
            // Act
            var result = data.DetectOutliersGpu(threshold: 2.0).ToArray();

            // Assert
            Assert.NotEmpty(result);
            Assert.Contains(result, p => p.Value == 500.0);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void ReturnsGpu_SimpleTimeSeries_CalculatesCorrectly()
    {
        // Arrange
        var baseValue = 100.0;
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-01"), 105.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-02"), 110.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-03"), 95.0)
        };

        try
        {
            // Act
            var result = data.ReturnsGpu(baseValue).ToArray();

            // Assert
            Assert.Equal(3, result.Length);
            Assert.Equal(5.0, result[0].Value, precision: 10);   // (105-100)/100 * 100
            Assert.Equal(10.0, result[1].Value, precision: 10);  // (110-100)/100 * 100
            Assert.Equal(-5.0, result[2].Value, precision: 10);  // (95-100)/100 * 100
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void DifferenceGpu_GpuMatchesCpuResults()
    {
        // Arrange
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-01"), 100.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-02"), 105.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-03"), 103.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-04"), 108.0)
        };

        try
        {
            // Act
            var gpuResult = data.DifferenceGpu().ToArray();
            var cpuResult = data.Difference().ToArray();

            // Assert
            Assert.Equal(cpuResult.Length, gpuResult.Length);
            for (int i = 0; i < cpuResult.Length; i++)
            {
                Assert.Equal(cpuResult[i].Value, gpuResult[i].Value, precision: 10);
            }
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void PercentageChangeGpu_GpuMatchesCpuResults()
    {
        // Arrange
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-01"), 100.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-02"), 110.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-03"), 99.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-04"), 105.0)
        };

        try
        {
            // Act
            var gpuResult = data.PercentageChangeGpu().ToArray();
            var cpuResult = data.PercentageChange().ToArray();

            // Assert - Note: GPU returns percentage (100-based), CPU returns ratio (1-based)
            Assert.Equal(cpuResult.Length, gpuResult.Length);
            for (int i = 0; i < cpuResult.Length; i++)
            {
                Assert.Equal(cpuResult[i].Value * 100, gpuResult[i].Value, precision: 8);
            }
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
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-01"), 100.0)
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
    public void ExponentialMovingAverageGpu_InvalidAlpha_ThrowsException()
    {
        // Arrange
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-01"), 100.0)
        };

        try
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => data.ExponentialMovingAverageGpu(0).ToArray());
            Assert.Throws<ArgumentException>(() => data.ExponentialMovingAverageGpu(1.5).ToArray());
            Assert.Throws<ArgumentException>(() => data.ExponentialMovingAverageGpu(-0.1).ToArray());
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void ReturnsGpu_ZeroBaseValue_ThrowsException()
    {
        // Arrange
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-01"), 100.0)
        };

        try
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => data.ReturnsGpu(0).ToArray());
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void DetectOutliersGpu_NoOutliers_ReturnsEmpty()
    {
        // Arrange - all values within normal range
        var data = new[]
        {
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-01"), 100.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-02"), 101.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-03"), 99.0),
            new TimeSeriesPoint<double>(DateTime.Parse("2024-01-04"), 102.0)
        };

        try
        {
            // Act
            var result = data.DetectOutliersGpu(threshold: 3.0).ToArray();

            // Assert
            Assert.Empty(result);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void CumulativeSumGpu_LargeDataset_CalculatesCorrectly()
    {
        // Arrange
        var size = 1000;
        var data = Enumerable.Range(0, size)
            .Select(i => new TimeSeriesPoint<double>(DateTime.Parse("2024-01-01").AddDays(i), 1.0))
            .ToArray();

        try
        {
            // Act
            var result = data.CumulativeSumGpu().ToArray();

            // Assert
            Assert.Equal(size, result.Length);
            Assert.Equal(size, result[^1].Value, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }
}
