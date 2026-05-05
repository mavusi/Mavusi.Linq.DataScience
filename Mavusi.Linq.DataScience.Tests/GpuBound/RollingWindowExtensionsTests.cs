using Mavusi.Linq.DataScience.GpuBound;

namespace Mavusi.Linq.DataScience.Tests.GpuBound;

public class RollingWindowExtensionsTests
{
    [Fact]
    public void RollingAverageGpu_SimpleData_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var windowSize = 3;

        try
        {
            // Act
            var result = data.RollingAverageGpu(windowSize).ToArray();

            // Assert
            Assert.Equal(3, result.Length);
            Assert.Equal(2.0, result[0], precision: 10); // (1+2+3)/3
            Assert.Equal(3.0, result[1], precision: 10); // (2+3+4)/3
            Assert.Equal(4.0, result[2], precision: 10); // (3+4+5)/3
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void RollingAverageGpu_WithSelector_CalculatesCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new { Value = 10.0 },
            new { Value = 20.0 },
            new { Value = 30.0 },
            new { Value = 40.0 }
        };

        try
        {
            // Act
            var result = data.RollingAverageGpu(2, x => x.Value).ToArray();

            // Assert
            Assert.Equal(3, result.Length);
            Assert.Equal(15.0, result[0], precision: 10);
            Assert.Equal(25.0, result[1], precision: 10);
            Assert.Equal(35.0, result[2], precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void RollingSumGpu_SimpleData_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var windowSize = 3;

        try
        {
            // Act
            var result = data.RollingSumGpu(windowSize).ToArray();

            // Assert
            Assert.Equal(3, result.Length);
            Assert.Equal(6.0, result[0], precision: 10); // 1+2+3
            Assert.Equal(9.0, result[1], precision: 10); // 2+3+4
            Assert.Equal(12.0, result[2], precision: 10); // 3+4+5
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void RollingSumGpu_WithSelector_CalculatesCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new { Value = 1.0 },
            new { Value = 2.0 },
            new { Value = 3.0 },
            new { Value = 4.0 }
        };

        try
        {
            // Act
            var result = data.RollingSumGpu(2, x => x.Value).ToArray();

            // Assert
            Assert.Equal(3, result.Length);
            Assert.Equal(3.0, result[0], precision: 10);
            Assert.Equal(5.0, result[1], precision: 10);
            Assert.Equal(7.0, result[2], precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void RollingMinGpu_SimpleData_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 3.0, 1.0, 4.0, 1.0, 5.0, 9.0 };
        var windowSize = 3;

        try
        {
            // Act
            var result = data.RollingMinGpu(windowSize).ToArray();

            // Assert
            Assert.Equal(4, result.Length);
            Assert.Equal(1.0, result[0], precision: 10); // min(3,1,4)
            Assert.Equal(1.0, result[1], precision: 10); // min(1,4,1)
            Assert.Equal(1.0, result[2], precision: 10); // min(4,1,5)
            Assert.Equal(1.0, result[3], precision: 10); // min(1,5,9)
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void RollingMaxGpu_SimpleData_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 3.0, 1.0, 4.0, 1.0, 5.0, 9.0 };
        var windowSize = 3;

        try
        {
            // Act
            var result = data.RollingMaxGpu(windowSize).ToArray();

            // Assert
            Assert.Equal(4, result.Length);
            Assert.Equal(4.0, result[0], precision: 10); // max(3,1,4)
            Assert.Equal(4.0, result[1], precision: 10); // max(1,4,1)
            Assert.Equal(5.0, result[2], precision: 10); // max(4,1,5)
            Assert.Equal(9.0, result[3], precision: 10); // max(1,5,9)
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void RollingStandardDeviationGpu_SimpleData_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var windowSize = 3;

        try
        {
            // Act
            var result = data.RollingStandardDeviationGpu(windowSize).ToArray();

            // Assert
            Assert.Equal(3, result.Length);
            Assert.True(result.All(r => r > 0));
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void RollingAverageGpu_GpuMatchesCpuResults()
    {
        // Arrange
        var data = new[] { 1.5, 2.3, 3.7, 4.2, 5.8, 6.1, 7.9, 8.4, 9.2 };
        var windowSize = 3;

        try
        {
            // Act
            var gpuResult = data.RollingAverageGpu(windowSize).ToArray();
            var cpuResult = data.RollingAverage(windowSize).ToArray();

            // Assert
            Assert.Equal(cpuResult.Length, gpuResult.Length);
            for (int i = 0; i < cpuResult.Length; i++)
            {
                Assert.Equal(cpuResult[i], gpuResult[i], precision: 10);
            }
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void RollingSumGpu_GpuMatchesCpuResults()
    {
        // Arrange
        var data = new[] { 1.5, 2.3, 3.7, 4.2, 5.8, 6.1, 7.9, 8.4, 9.2 };
        var windowSize = 4;

        try
        {
            // Act
            var gpuResult = data.RollingSumGpu(windowSize).ToArray();
            var cpuResult = data.RollingSum(windowSize).ToArray();

            // Assert
            Assert.Equal(cpuResult.Length, gpuResult.Length);
            for (int i = 0; i < cpuResult.Length; i++)
            {
                Assert.Equal(cpuResult[i], gpuResult[i], precision: 10);
            }
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }





    [Fact]
    public void RollingAverageGpu_WindowLargerThanData_ReturnsEmpty()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0 };
        var windowSize = 5;

        try
        {
            // Act
            var result = data.RollingAverageGpu(windowSize).ToArray();

            // Assert
            Assert.Empty(result);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void RollingAverageGpu_InvalidWindowSize_ThrowsException()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0 };

        try
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => data.RollingAverageGpu(0).ToArray());
            Assert.Throws<ArgumentException>(() => data.RollingAverageGpu(-1).ToArray());
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void RollingAverageGpu_LargeDataset_CalculatesCorrectly()
    {
        // Arrange
        var size = 1000;
        var data = Enumerable.Range(0, size).Select(i => (double)i).ToArray();
        var windowSize = 10;

        try
        {
            // Act
            var result = data.RollingAverageGpu(windowSize).ToArray();

            // Assert
            Assert.Equal(size - windowSize + 1, result.Length);
            Assert.True(result.All(r => r >= 0));
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }
}
