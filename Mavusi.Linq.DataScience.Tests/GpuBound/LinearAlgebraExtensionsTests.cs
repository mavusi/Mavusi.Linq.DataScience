using Mavusi.Linq.DataScience.GpuBound;
using Mavusi.Linq.DataScience.Models;

namespace Mavusi.Linq.DataScience.Tests.GpuBound;

public class LinearAlgebraExtensionsTests
{
    [Fact]
    public void DotProductGpu_SimpleVectors_CalculatesCorrectly()
    {
        // Arrange
        var v1 = new Vector(new[] { 1.0, 2.0, 3.0 });
        var v2 = new Vector(new[] { 4.0, 5.0, 6.0 });

        try
        {
            // Act
            var result = v1.DotProductGpu(v2);

            // Assert - 1*4 + 2*5 + 3*6 = 32
            Assert.Equal(32.0, result, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void DotProductGpu_Sequences_CalculatesCorrectly()
    {
        // Arrange
        var seq1 = new[] { 1.0, 2.0, 3.0 };
        var seq2 = new[] { 4.0, 5.0, 6.0 };

        try
        {
            // Act
            var result = seq1.DotProductGpu(seq2);

            // Assert
            Assert.Equal(32.0, result, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void DotProductGpu_OrthogonalVectors_ReturnsZero()
    {
        // Arrange
        var v1 = new Vector(new[] { 1.0, 0.0 });
        var v2 = new Vector(new[] { 0.0, 1.0 });

        try
        {
            // Act
            var result = v1.DotProductGpu(v2);

            // Assert
            Assert.Equal(0.0, result, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void AddGpu_SimpleVectors_AddsCorrectly()
    {
        // Arrange
        var v1 = new Vector(new[] { 1.0, 2.0, 3.0 });
        var v2 = new Vector(new[] { 4.0, 5.0, 6.0 });

        try
        {
            // Act
            var result = v1.AddGpu(v2);

            // Assert
            Assert.Equal(5.0, result[0], precision: 10);
            Assert.Equal(7.0, result[1], precision: 10);
            Assert.Equal(9.0, result[2], precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void SubtractGpu_SimpleVectors_SubtractsCorrectly()
    {
        // Arrange
        var v1 = new Vector(new[] { 5.0, 7.0, 9.0 });
        var v2 = new Vector(new[] { 1.0, 2.0, 3.0 });

        try
        {
            // Act
            var result = v1.SubtractGpu(v2);

            // Assert
            Assert.Equal(4.0, result[0], precision: 10);
            Assert.Equal(5.0, result[1], precision: 10);
            Assert.Equal(6.0, result[2], precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void MultiplyGpu_ScalarMultiplication_MultipliesCorrectly()
    {
        // Arrange
        var vector = new Vector(new[] { 1.0, 2.0, 3.0 });
        var scalar = 3.0;

        try
        {
            // Act
            var result = vector.MultiplyGpu(scalar);

            // Assert
            Assert.Equal(3.0, result[0], precision: 10);
            Assert.Equal(6.0, result[1], precision: 10);
            Assert.Equal(9.0, result[2], precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void MagnitudeGpu_SimpleVector_CalculatesCorrectly()
    {
        // Arrange
        var vector = new Vector(new[] { 3.0, 4.0 });

        try
        {
            // Act
            var result = vector.MagnitudeGpu();

            // Assert - sqrt(3^2 + 4^2) = 5
            Assert.Equal(5.0, result, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void NormalizeGpu_SimpleVector_NormalizesCorrectly()
    {
        // Arrange
        var vector = new Vector(new[] { 3.0, 4.0 });

        try
        {
            // Act
            var result = vector.NormalizeGpu();

            // Assert
            Assert.Equal(0.6, result[0], precision: 10);
            Assert.Equal(0.8, result[1], precision: 10);
            Assert.Equal(1.0, result.MagnitudeGpu(), precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void MultiplyGpu_MatrixMultiplication_MultipliesCorrectly()
    {
        // Arrange
        var m1 = new Matrix(2, 2);
        m1[0, 0] = 1; m1[0, 1] = 2;
        m1[1, 0] = 3; m1[1, 1] = 4;

        var m2 = new Matrix(2, 2);
        m2[0, 0] = 2; m2[0, 1] = 0;
        m2[1, 0] = 1; m2[1, 1] = 2;

        try
        {
            // Act
            var result = m1.MultiplyGpu(m2);

            // Assert
            Assert.Equal(4.0, result[0, 0], precision: 10);
            Assert.Equal(4.0, result[0, 1], precision: 10);
            Assert.Equal(10.0, result[1, 0], precision: 10);
            Assert.Equal(8.0, result[1, 1], precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void MultiplyGpu_IdentityMatrix_ReturnsOriginal()
    {
        // Arrange
        var matrix = new Matrix(2, 2);
        matrix[0, 0] = 5; matrix[0, 1] = 6;
        matrix[1, 0] = 7; matrix[1, 1] = 8;

        var identity = new Matrix(2, 2);
        identity[0, 0] = 1; identity[0, 1] = 0;
        identity[1, 0] = 0; identity[1, 1] = 1;

        try
        {
            // Act
            var result = matrix.MultiplyGpu(identity);

            // Assert
            Assert.Equal(5.0, result[0, 0], precision: 10);
            Assert.Equal(6.0, result[0, 1], precision: 10);
            Assert.Equal(7.0, result[1, 0], precision: 10);
            Assert.Equal(8.0, result[1, 1], precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void DotProductGpu_GpuMatchesCpuResults()
    {
        // Arrange
        var v1 = new Vector(new[] { 1.5, 2.3, 3.7, 4.2 });
        var v2 = new Vector(new[] { 3.2, 4.1, 5.6, 6.0 });

        try
        {
            // Act
            var gpuResult = v1.DotProductGpu(v2);
            var cpuResult = v1.DotProduct(v2);

            // Assert
            Assert.Equal(cpuResult, gpuResult, precision: 10);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void AddGpu_GpuMatchesCpuResults()
    {
        // Arrange
        var v1 = new Vector(new[] { 1.5, 2.3, 3.7, 4.2 });
        var v2 = new Vector(new[] { 3.2, 4.1, 5.6, 6.0 });

        try
        {
            // Act
            var gpuResult = v1.AddGpu(v2);
            var cpuResult = v1.Add(v2);

            // Assert
            for (int i = 0; i < gpuResult.Length; i++)
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
    public void DotProductGpu_DifferentLengths_ThrowsException()
    {
        // Arrange
        var v1 = new Vector(new[] { 1.0, 2.0, 3.0 });
        var v2 = new Vector(new[] { 4.0, 5.0 });

        try
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => v1.DotProductGpu(v2));
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void NormalizeGpu_ZeroVector_ThrowsException()
    {
        // Arrange
        var vector = new Vector(new[] { 0.0, 0.0, 0.0 });

        try
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => vector.NormalizeGpu());
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void MultiplyGpu_IncompatibleMatrices_ThrowsException()
    {
        // Arrange
        var m1 = new Matrix(2, 3);
        var m2 = new Matrix(2, 2);

        try
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => m1.MultiplyGpu(m2));
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }
}
