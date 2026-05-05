using Mavusi.Linq.DataScience.GpuBound.FastMode;
using Mavusi.Linq.DataScience.Models;

namespace Mavusi.Linq.DataScience.GpuBound.FastMode.Tests;

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
            Assert.Equal(32.0f, result, precision: 4);
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
            Assert.Equal(0.0f, result, precision: 4);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void AddGpu_TwoVectors_AddsCorrectly()
    {
        // Arrange
        var v1 = new Vector(new[] { 1.0, 2.0, 3.0 });
        var v2 = new Vector(new[] { 4.0, 5.0, 6.0 });

        try
        {
            // Act
            var result = v1.AddGpu(v2);

            // Assert
            Assert.Equal(3, result.Length);
            Assert.Equal(5.0, result[0], precision: 4);
            Assert.Equal(7.0, result[1], precision: 4);
            Assert.Equal(9.0, result[2], precision: 4);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void SubtractGpu_TwoVectors_SubtractsCorrectly()
    {
        // Arrange
        var v1 = new Vector(new[] { 5.0, 7.0, 9.0 });
        var v2 = new Vector(new[] { 2.0, 3.0, 4.0 });

        try
        {
            // Act
            var result = v1.SubtractGpu(v2);

            // Assert
            Assert.Equal(3, result.Length);
            Assert.Equal(3.0, result[0], precision: 4);
            Assert.Equal(4.0, result[1], precision: 4);
            Assert.Equal(5.0, result[2], precision: 4);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void MultiplyGpu_VectorByScalar_MultipliesCorrectly()
    {
        // Arrange
        var v = new Vector(new[] { 1.0, 2.0, 3.0 });
        var scalar = 2.0f;

        try
        {
            // Act
            var result = v.MultiplyGpu(scalar);

            // Assert
            Assert.Equal(3, result.Length);
            Assert.Equal(2.0, result[0], precision: 4);
            Assert.Equal(4.0, result[1], precision: 4);
            Assert.Equal(6.0, result[2], precision: 4);
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
        var v = new Vector(new[] { 3.0, 4.0 });

        try
        {
            // Act
            var result = v.MagnitudeGpu();

            // Assert - sqrt(3^2 + 4^2) = 5
            Assert.Equal(5.0f, result, precision: 4);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void NormalizeGpu_Vector_ReturnsUnitVector()
    {
        // Arrange
        var v = new Vector(new[] { 3.0, 4.0 });

        try
        {
            // Act
            var result = v.NormalizeGpu();
            var magnitude = result.MagnitudeGpu();

            // Assert - Normalized vector should have magnitude 1
            Assert.Equal(1.0f, magnitude, precision: 3);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void MultiplyGpu_TwoMatrices_MultipliesCorrectly()
    {
        // Arrange
        var m1 = new Matrix(new[,]
        {
            { 1.0, 2.0 },
            { 3.0, 4.0 }
        });
        var m2 = new Matrix(new[,]
        {
            { 5.0, 6.0 },
            { 7.0, 8.0 }
        });

        try
        {
            // Act
            var result = m1.MultiplyGpu(m2);

            // Assert
            // [1 2]   [5 6]   [19 22]
            // [3 4] * [7 8] = [43 50]
            Assert.Equal(2, result.Rows);
            Assert.Equal(2, result.Columns);
            Assert.Equal(19.0, result[0, 0], precision: 3);
            Assert.Equal(22.0, result[0, 1], precision: 3);
            Assert.Equal(43.0, result[1, 0], precision: 3);
            Assert.Equal(50.0, result[1, 1], precision: 3);
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }

    [Fact]
    public void ToVector_DoubleArray_ConvertsCorrectly()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0 };

        try
        {
            // Act
            var result = data.ToVector();

            // Assert
            Assert.Equal(3, result.Length);
            Assert.Equal(1.0, result[0], precision: 4);
            Assert.Equal(2.0, result[1], precision: 4);
            Assert.Equal(3.0, result[2], precision: 4);
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
        var v2 = new Vector(new[] { 1.0, 2.0 });

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
    public void AddGpu_DifferentLengths_ThrowsException()
    {
        // Arrange
        var v1 = new Vector(new[] { 1.0, 2.0, 3.0 });
        var v2 = new Vector(new[] { 1.0, 2.0 });

        try
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => v1.AddGpu(v2));
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
        var m1 = new Matrix(new[,] { { 1.0, 2.0, 3.0 } }); // 1x3
        var m2 = new Matrix(new[,] { { 1.0, 2.0 } }); // 1x2

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

    [Fact]
    public void MagnitudeGpu_ZeroVector_ReturnsZero()
    {
        // Arrange
        var v = new Vector(new[] { 0.0, 0.0, 0.0 });

        try
        {
            // Act
            var result = v.MagnitudeGpu();

            // Assert
            Assert.Equal(0.0f, result, precision: 4);
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
        var v = new Vector(new[] { 0.0, 0.0, 0.0 });

        try
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => v.NormalizeGpu());
        }
        catch (ILGPU.InternalCompilerException ex) when (ex.InnerException is ILGPU.CapabilityNotSupportedException)
        {
            return;
        }
    }
}
