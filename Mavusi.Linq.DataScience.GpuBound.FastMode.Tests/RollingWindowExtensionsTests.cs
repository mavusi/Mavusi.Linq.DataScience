using Mavusi.Linq.DataScience.GpuBound.FastMode;

namespace Mavusi.Linq.DataScience.GpuBound.FastMode.Tests;

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
            Assert.Equal(2.0f, result[0], precision: 4); // (1+2+3)/3
            Assert.Equal(3.0f, result[1], precision: 4); // (2+3+4)/3
            Assert.Equal(4.0f, result[2], precision: 4); // (3+4+5)/3
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            // GPU not available, skip test
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
            Assert.Equal(15.0f, result[0], precision: 4);
            Assert.Equal(25.0f, result[1], precision: 4);
            Assert.Equal(35.0f, result[2], precision: 4);
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
            Assert.Equal(6.0f, result[0], precision: 4); // 1+2+3
            Assert.Equal(9.0f, result[1], precision: 4); // 2+3+4
            Assert.Equal(12.0f, result[2], precision: 4); // 3+4+5
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
        var data = new[] { 5.0, 2.0, 8.0, 1.0, 6.0 };
        var windowSize = 3;

        try
        {
            // Act
            var result = data.RollingMinGpu(windowSize).ToArray();

            // Assert
            Assert.Equal(3, result.Length);
            Assert.Equal(2.0f, result[0], precision: 4);
            Assert.Equal(1.0f, result[1], precision: 4);
            Assert.Equal(1.0f, result[2], precision: 4);
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
        var data = new[] { 5.0, 2.0, 8.0, 1.0, 6.0 };
        var windowSize = 3;

        try
        {
            // Act
            var result = data.RollingMaxGpu(windowSize).ToArray();

            // Assert
            Assert.Equal(3, result.Length);
            Assert.Equal(8.0f, result[0], precision: 4);
            Assert.Equal(8.0f, result[1], precision: 4);
            Assert.Equal(8.0f, result[2], precision: 4);
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
        var data = new[] { 2.0f, 4.0f, 4.0f, 4.0f, 5.0f, 5.0f, 7.0f, 9.0f };
        var windowSize = 4;

        try
        {
            // Act
            var result = data.RollingStandardDeviationGpu(windowSize).ToArray();

            // Assert
            Assert.Equal(5, result.Length);
            Assert.True(result.All(x => x > 0));
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void RollingAverageGpu_EmptyInput_ReturnsEmpty()
    {
        // Arrange
        var data = Array.Empty<double>();

        try
        {
            // Act
            var result = data.RollingAverageGpu(3).ToArray();

            // Assert
            Assert.Empty(result);
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

        try
        {
            // Act
            var result = data.RollingAverageGpu(5).ToArray();

            // Assert
            Assert.Empty(result);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }
}
