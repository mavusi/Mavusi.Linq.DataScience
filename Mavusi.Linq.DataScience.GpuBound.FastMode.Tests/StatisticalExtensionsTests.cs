using Mavusi.Linq.DataScience.GpuBound.FastMode;

namespace Mavusi.Linq.DataScience.GpuBound.FastMode.Tests;

public class StatisticalExtensionsTests
{
    [Fact]
    public void StandardDeviationGpu_PopulationStdDev_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 2.0, 4.0, 4.0, 4.0, 5.0, 5.0, 7.0, 9.0 };

        try
        {
            // Act
            var result = data.StandardDeviationGpu();

            // Assert - Expected population stddev ≈ 2.0
            Assert.InRange(result, 1.9f, 2.1f);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void StandardDeviationSampleGpu_SampleStdDev_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 2.0, 4.0, 4.0, 4.0, 5.0, 5.0, 7.0, 9.0 };

        try
        {
            // Act
            var result = data.StandardDeviationSampleGpu();

            // Assert - Sample stddev should be slightly larger than population
            Assert.InRange(result, 2.0f, 2.3f);
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
            new { Value = 2.0 },
            new { Value = 4.0 },
            new { Value = 4.0 },
            new { Value = 4.0 }
        };

        try
        {
            // Act
            var result = data.StandardDeviationGpu(x => x.Value);

            // Assert
            Assert.True(result > 0);
            Assert.InRange(result, 0.8f, 1.0f);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void VarianceGpu_PopulationVariance_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 2.0, 4.0, 4.0, 4.0, 5.0, 5.0, 7.0, 9.0 };

        try
        {
            // Act
            var result = data.VarianceGpu();

            // Assert - Variance should be stddev squared ≈ 4.0
            Assert.InRange(result, 3.5f, 4.5f);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void VarianceSampleGpu_SampleVariance_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 2.0, 4.0, 4.0, 4.0, 5.0, 5.0, 7.0, 9.0 };

        try
        {
            // Act
            var result = data.VarianceSampleGpu();

            // Assert - Sample variance should be larger than population variance
            Assert.InRange(result, 4.0f, 5.5f);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void StandardDeviationGpu_SingleValue_ReturnsZero()
    {
        // Arrange
        var data = new[] { 5.0 };

        try
        {
            // Act
            var result = data.StandardDeviationGpu();

            // Assert
            Assert.Equal(0f, result, precision: 4);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void StandardDeviationGpu_IdenticalValues_ReturnsZero()
    {
        // Arrange
        var data = new[] { 5.0, 5.0, 5.0, 5.0 };

        try
        {
            // Act
            var result = data.StandardDeviationGpu();

            // Assert
            Assert.Equal(0f, result, precision: 4);
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
    public void VarianceGpu_WithSelector_CalculatesCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new { Score = 85.0 },
            new { Score = 90.0 },
            new { Score = 78.0 },
            new { Score = 92.0 }
        };

        try
        {
            // Act
            var values = data.Select(x => x.Score);
            var result = values.VarianceGpu();

            // Assert
            Assert.True(result > 0);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }
}
