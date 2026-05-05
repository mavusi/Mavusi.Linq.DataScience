using Mavusi.Linq.DataScience.GpuBound;

namespace Mavusi.Linq.DataScience.Tests.GpuBound;

public class StatisticalExtensionsTests
{
    [Fact]
    public void StandardDeviationGpu_SimpleDataset_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 2.0, 4.0, 4.0, 4.0, 5.0, 5.0, 7.0, 9.0 };

        try
        {
            // Act
            var result = data.StandardDeviationGpu();

            // Assert
            Assert.True(result > 0);
            Assert.InRange(result, 1.5, 2.5);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void StandardDeviationSampleGpu_SimpleDataset_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 2.0, 4.0, 4.0, 4.0, 5.0, 5.0, 7.0, 9.0 };

        try
        {
            // Act
            var result = data.StandardDeviationSampleGpu();

            // Assert
            Assert.True(result > 0);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void StandardDeviationGpu_WithSelector_CalculatesCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new { Value = 10.0 },
            new { Value = 20.0 },
            new { Value = 30.0 },
            new { Value = 40.0 },
            new { Value = 50.0 }
        };

        try
        {
            // Act
            var result = data.StandardDeviationGpu(x => x.Value);

            // Assert
            Assert.True(result > 0);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void VarianceGpu_SimpleDataset_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };

        try
        {
            // Act
            var result = data.VarianceGpu();

            // Assert
            Assert.Equal(2.0, result, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void VarianceSampleGpu_SimpleDataset_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };

        try
        {
            // Act
            var result = data.VarianceSampleGpu();

            // Assert
            Assert.Equal(2.5, result, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void StandardDeviationGpu_GpuMatchesCpuResults()
    {
        // Arrange
        var data = new[] { 1.5, 2.3, 3.7, 4.2, 5.8, 6.1, 7.9, 8.4, 9.2, 10.5 };

        try
        {
            // Act
            var gpuResult = data.StandardDeviationGpu();
            var cpuResult = data.StandardDeviation();

            // Assert
            Assert.Equal(cpuResult, gpuResult, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void StandardDeviationSampleGpu_GpuMatchesCpuResults()
    {
        // Arrange
        var data = new[] { 1.5, 2.3, 3.7, 4.2, 5.8, 6.1, 7.9, 8.4, 9.2, 10.5 };

        try
        {
            // Act
            var gpuResult = data.StandardDeviationSampleGpu();
            var cpuResult = data.StandardDeviationSample();

            // Assert
            Assert.Equal(cpuResult, gpuResult, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void VarianceGpu_GpuMatchesCpuResults()
    {
        // Arrange
        var data = new[] { 1.5, 2.3, 3.7, 4.2, 5.8, 6.1, 7.9, 8.4, 9.2, 10.5 };

        try
        {
            // Act
            var gpuResult = data.VarianceGpu();
            var cpuResult = data.Variance();

            // Assert
            Assert.Equal(cpuResult, gpuResult, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void VarianceSampleGpu_GpuMatchesCpuResults()
    {
        // Arrange
        var data = new[] { 1.5, 2.3, 3.7, 4.2, 5.8, 6.1, 7.9, 8.4, 9.2, 10.5 };

        try
        {
            // Act
            var gpuResult = data.VarianceSampleGpu();
            var cpuResult = data.VarianceSample();

            // Assert
            Assert.Equal(cpuResult, gpuResult, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void StandardDeviationGpu_LargeDataset_CalculatesCorrectly()
    {
        // Arrange
        var size = 10000;
        var data = Enumerable.Range(0, size).Select(i => (double)i).ToArray();

        try
        {
            // Act
            var result = data.StandardDeviationGpu();

            // Assert
            Assert.True(result > 0);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void StandardDeviationGpu_EmptySequence_ThrowsException()
    {
        // Arrange
        var data = Array.Empty<double>();

        try
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => data.StandardDeviationGpu());
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void VarianceGpu_ConstantValues_ReturnsZero()
    {
        // Arrange
        var data = new[] { 5.0, 5.0, 5.0, 5.0, 5.0 };

        try
        {
            // Act
            var result = data.VarianceGpu();

            // Assert
            Assert.Equal(0.0, result, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }
}
