namespace Mavusi.Linq.DataScience.Tests;

public class LinearAlgebraExtensionsTests
{
    [Fact]
    public void ToVector_CreatesVectorFromSequence()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };

        // Act
        var vector = data.ToVector();

        // Assert
        Assert.Equal(5, vector.Length);
        Assert.Equal(1.0, vector[0]);
        Assert.Equal(5.0, vector[4]);
    }

    [Fact]
    public void DotProduct_CalculatesCorrectly()
    {
        // Arrange
        var v1 = new Vector(1.0, 2.0, 3.0);
        var v2 = new Vector(4.0, 5.0, 6.0);

        // Act
        var result = v1.DotProduct(v2);

        // Assert
        // 1*4 + 2*5 + 3*6 = 4 + 10 + 18 = 32
        Assert.Equal(32.0, result);
    }

    [Fact]
    public void DotProduct_WithSequences_WorksCorrectly()
    {
        // Arrange
        var seq1 = new[] { 1.0, 2.0, 3.0 };
        var seq2 = new[] { 4.0, 5.0, 6.0 };

        // Act
        var result = seq1.DotProduct(seq2);

        // Assert
        Assert.Equal(32.0, result);
    }

    [Fact]
    public void VectorAdd_AddsVectorsElementWise()
    {
        // Arrange
        var v1 = new Vector(1.0, 2.0, 3.0);
        var v2 = new Vector(4.0, 5.0, 6.0);

        // Act
        var result = v1.Add(v2);

        // Assert
        Assert.Equal(5.0, result[0]);
        Assert.Equal(7.0, result[1]);
        Assert.Equal(9.0, result[2]);
    }

    [Fact]
    public void VectorSubtract_SubtractsVectorsElementWise()
    {
        // Arrange
        var v1 = new Vector(10.0, 20.0, 30.0);
        var v2 = new Vector(1.0, 2.0, 3.0);

        // Act
        var result = v1.Subtract(v2);

        // Assert
        Assert.Equal(9.0, result[0]);
        Assert.Equal(18.0, result[1]);
        Assert.Equal(27.0, result[2]);
    }

    [Fact]
    public void VectorMultiply_ScalesVector()
    {
        // Arrange
        var vector = new Vector(1.0, 2.0, 3.0);
        var scalar = 3.0;

        // Act
        var result = vector.Multiply(scalar);

        // Assert
        Assert.Equal(3.0, result[0]);
        Assert.Equal(6.0, result[1]);
        Assert.Equal(9.0, result[2]);
    }

    [Fact]
    public void VectorMagnitude_CalculatesEuclideanNorm()
    {
        // Arrange
        var vector = new Vector(3.0, 4.0); // 3-4-5 triangle

        // Act
        var magnitude = vector.Magnitude();

        // Assert
        Assert.Equal(5.0, magnitude);
    }

    [Fact]
    public void VectorNormalize_CreatesUnitVector()
    {
        // Arrange
        var vector = new Vector(3.0, 4.0);

        // Act
        var normalized = vector.Normalize();

        // Assert
        Assert.Equal(1.0, normalized.Magnitude(), precision: 10);
        Assert.Equal(0.6, normalized[0], precision: 10); // 3/5
        Assert.Equal(0.8, normalized[1], precision: 10); // 4/5
    }

    [Fact]
    public void ToMatrix_CreatesMatrixFromSequences()
    {
        // Arrange
        var data = new[]
        {
            new[] { 1.0, 2.0, 3.0 },
            new[] { 4.0, 5.0, 6.0 }
        };

        // Act
        var matrix = data.ToMatrix();

        // Assert
        Assert.Equal(2, matrix.Rows);
        Assert.Equal(3, matrix.Columns);
        Assert.Equal(1.0, matrix[0, 0]);
        Assert.Equal(6.0, matrix[1, 2]);
    }

    [Fact]
    public void MatrixMultiply_MultipliesMatricesCorrectly()
    {
        // Arrange
        var m1 = new Matrix(new[,] { { 1.0, 2.0 }, { 3.0, 4.0 } });
        var m2 = new Matrix(new[,] { { 5.0, 6.0 }, { 7.0, 8.0 } });

        // Act
        var result = m1.Multiply(m2);

        // Assert
        // [1*5+2*7, 1*6+2*8] = [19, 22]
        // [3*5+4*7, 3*6+4*8] = [43, 50]
        Assert.Equal(19.0, result[0, 0]);
        Assert.Equal(22.0, result[0, 1]);
        Assert.Equal(43.0, result[1, 0]);
        Assert.Equal(50.0, result[1, 1]);
    }

    [Fact]
    public void MatrixVectorMultiply_WorksCorrectly()
    {
        // Arrange
        var matrix = new Matrix(new[,] { { 1.0, 2.0 }, { 3.0, 4.0 } });
        var vector = new Vector(5.0, 6.0);

        // Act
        var result = matrix.Multiply(vector);

        // Assert
        // [1*5+2*6] = [17]
        // [3*5+4*6] = [39]
        Assert.Equal(17.0, result[0]);
        Assert.Equal(39.0, result[1]);
    }

    [Fact]
    public void MatrixTranspose_SwapsRowsAndColumns()
    {
        // Arrange
        var matrix = new Matrix(new[,] 
        { 
            { 1.0, 2.0, 3.0 }, 
            { 4.0, 5.0, 6.0 } 
        });

        // Act
        var transposed = matrix.Transpose();

        // Assert
        Assert.Equal(3, transposed.Rows);
        Assert.Equal(2, transposed.Columns);
        Assert.Equal(1.0, transposed[0, 0]);
        Assert.Equal(4.0, transposed[0, 1]);
        Assert.Equal(6.0, transposed[2, 1]);
    }

    [Fact]
    public void CreateIdentityMatrix_CreatesCorrectMatrix()
    {
        // Arrange & Act
        var identity = LinearAlgebraExtensions.CreateIdentityMatrix(3);

        // Assert
        Assert.Equal(3, identity.Rows);
        Assert.Equal(3, identity.Columns);
        Assert.Equal(1.0, identity[0, 0]);
        Assert.Equal(1.0, identity[1, 1]);
        Assert.Equal(1.0, identity[2, 2]);
        Assert.Equal(0.0, identity[0, 1]);
        Assert.Equal(0.0, identity[1, 0]);
    }

    [Fact]
    public void MatrixTrace_CalculatesSumOfDiagonal()
    {
        // Arrange
        var matrix = new Matrix(new[,] 
        { 
            { 1.0, 2.0, 3.0 }, 
            { 4.0, 5.0, 6.0 },
            { 7.0, 8.0, 9.0 }
        });

        // Act
        var trace = matrix.Trace();

        // Assert
        // 1 + 5 + 9 = 15
        Assert.Equal(15.0, trace);
    }

    [Fact]
    public void MatrixGetRow_ReturnsCorrectRow()
    {
        // Arrange
        var matrix = new Matrix(new[,] 
        { 
            { 1.0, 2.0, 3.0 }, 
            { 4.0, 5.0, 6.0 } 
        });

        // Act
        var row1 = matrix.GetRow(1);

        // Assert
        Assert.Equal(3, row1.Length);
        Assert.Equal(4.0, row1[0]);
        Assert.Equal(5.0, row1[1]);
        Assert.Equal(6.0, row1[2]);
    }

    [Fact]
    public void MatrixGetColumn_ReturnsCorrectColumn()
    {
        // Arrange
        var matrix = new Matrix(new[,] 
        { 
            { 1.0, 2.0, 3.0 }, 
            { 4.0, 5.0, 6.0 } 
        });

        // Act
        var col1 = matrix.GetColumn(1);

        // Assert
        Assert.Equal(2, col1.Length);
        Assert.Equal(2.0, col1[0]);
        Assert.Equal(5.0, col1[1]);
    }

    [Fact]
    public void VectorOperations_DifferentLengths_ThrowsException()
    {
        // Arrange
        var v1 = new Vector(1.0, 2.0, 3.0);
        var v2 = new Vector(1.0, 2.0);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => v1.DotProduct(v2));
        Assert.Throws<ArgumentException>(() => v1.Add(v2));
        Assert.Throws<ArgumentException>(() => v1.Subtract(v2));
    }

    [Fact]
    public void MatrixMultiply_IncompatibleDimensions_ThrowsException()
    {
        // Arrange
        var m1 = new Matrix(2, 3); // 2x3
        var m2 = new Matrix(2, 2); // 2x2

        // Act & Assert
        Assert.Throws<ArgumentException>(() => m1.Multiply(m2));
    }

    [Fact]
    public void RealWorldExample_LinearRegression()
    {
        // Arrange - simple 2D data points for line fitting
        // y = 2x + 1
        var xValues = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var yValues = new[] { 3.0, 5.0, 7.0, 9.0, 11.0 };

        // Act - calculate correlation using vectors
        var xVector = xValues.ToVector();
        var yVector = yValues.ToVector();
        var xMean = xValues.Average();
        var yMean = yValues.Average();

        // Manual correlation calculation
        var numerator = xValues.Zip(yValues, (x, y) => (x - xMean) * (y - yMean)).Sum();
        var denomX = Math.Sqrt(xValues.Sum(x => Math.Pow(x - xMean, 2)));
        var denomY = Math.Sqrt(yValues.Sum(y => Math.Pow(y - yMean, 2)));
        var correlation = numerator / (denomX * denomY);

        // Assert - perfect correlation for linear relationship
        Assert.Equal(1.0, correlation, precision: 10);
    }

    [Fact]
    public void RealWorldExample_GeometricTransformation()
    {
        // Arrange - 2D rotation matrix (90 degrees)
        var rotationMatrix = new Matrix(new[,] 
        { 
            { 0.0, -1.0 }, 
            { 1.0, 0.0 } 
        });
        var point = new Vector(1.0, 0.0);

        // Act - rotate point
        var rotatedPoint = rotationMatrix.Multiply(point);

        // Assert - (1,0) rotated 90° should be (0,1)
        Assert.Equal(0.0, rotatedPoint[0], precision: 10);
        Assert.Equal(1.0, rotatedPoint[1], precision: 10);
    }

    [Fact]
    public void IdentityMatrix_MultiplicationProperty()
    {
        // Arrange
        var matrix = new Matrix(new[,] 
        { 
            { 1.0, 2.0 }, 
            { 3.0, 4.0 } 
        });
        var identity = LinearAlgebraExtensions.CreateIdentityMatrix(2);

        // Act
        var result = matrix.Multiply(identity);

        // Assert - Matrix * Identity = Matrix
        Assert.Equal(1.0, result[0, 0]);
        Assert.Equal(2.0, result[0, 1]);
        Assert.Equal(3.0, result[1, 0]);
        Assert.Equal(4.0, result[1, 1]);
    }
}
