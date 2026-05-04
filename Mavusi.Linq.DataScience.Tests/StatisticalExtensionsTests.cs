namespace Mavusi.Linq.DataScience.Tests;

public class StatisticalExtensionsTests
{
    [Fact]
    public void StandardDeviation_WithSimpleData_ReturnsCorrectValue()
    {
        // Arrange
        var data = new[] { 2.0, 4.0, 4.0, 4.0, 5.0, 5.0, 7.0, 9.0 };

        // Act
        var result = data.StandardDeviation();

        // Assert
        Assert.Equal(2.0, result, precision: 10);
    }

    [Fact]
    public void StandardDeviationSample_WithSimpleData_ReturnsCorrectValue()
    {
        // Arrange
        var data = new[] { 2.0, 4.0, 4.0, 4.0, 5.0, 5.0, 7.0, 9.0 };

        // Act
        var result = data.StandardDeviationSample();

        // Assert - Sample SD should be slightly larger than population SD
        Assert.True(result > 2.0);
        Assert.Equal(2.138, result, precision: 2);
    }

    [Fact]
    public void StandardDeviation_WithSelector_ReturnsCorrectValue()
    {
        // Arrange
        var people = new[]
        {
            new { Name = "Alice", Age = 25.0 },
            new { Name = "Bob", Age = 30.0 },
            new { Name = "Charlie", Age = 35.0 }
        };

        // Act
        var result = people.StandardDeviation(p => p.Age);

        // Assert
        Assert.True(result > 0);
    }

    [Fact]
    public void StandardDeviation_WithEmptySequence_ThrowsException()
    {
        // Arrange
        var data = Array.Empty<double>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => data.StandardDeviation());
    }

    [Fact]
    public void Variance_WithKnownData_ReturnsCorrectValue()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };

        // Act
        var result = data.Variance();

        // Assert
        Assert.Equal(2.0, result, precision: 10);
    }

    [Fact]
    public void VarianceSample_WithKnownData_ReturnsCorrectValue()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };

        // Act
        var result = data.VarianceSample();

        // Assert
        Assert.Equal(2.5, result, precision: 10);
    }

    [Fact]
    public void StandardDeviation_WithConstantValues_ReturnsZero()
    {
        // Arrange
        var data = new[] { 5.0, 5.0, 5.0, 5.0, 5.0 };

        // Act
        var result = data.StandardDeviation();

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Variance_SquareOfStandardDeviation_IsConsistent()
    {
        // Arrange
        var data = new[] { 1.5, 2.3, 3.7, 4.2, 5.8, 6.1 };

        // Act
        var variance = data.Variance();
        var stdDev = data.StandardDeviation();

        // Assert
        Assert.Equal(variance, Math.Pow(stdDev, 2), precision: 10);
    }
}
