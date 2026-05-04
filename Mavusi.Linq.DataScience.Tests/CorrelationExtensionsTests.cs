namespace Mavusi.Linq.DataScience.Tests;

public class CorrelationExtensionsTests
{
    [Fact]
    public void Correlation_PerfectPositiveCorrelation_ReturnsOne()
    {
        // Arrange
        var x = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var y = new[] { 2.0, 4.0, 6.0, 8.0, 10.0 };

        // Act
        var result = x.Correlation(y);

        // Assert
        Assert.Equal(1.0, result, precision: 10);
    }

    [Fact]
    public void Correlation_PerfectNegativeCorrelation_ReturnsMinusOne()
    {
        // Arrange
        var x = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var y = new[] { 10.0, 8.0, 6.0, 4.0, 2.0 };

        // Act
        var result = x.Correlation(y);

        // Assert
        Assert.Equal(-1.0, result, precision: 10);
    }

    [Fact]
    public void Correlation_NoCorrelation_ReturnsZero()
    {
        // Arrange - perfectly uncorrelated data
        var x = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var y = new[] { 3.0, 3.0, 3.0, 3.0, 3.0 }; // constant

        // Act
        var result = x.Correlation(y);

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Correlation_WithSelectors_ReturnsCorrectValue()
    {
        // Arrange
        var data = new[]
        {
            new { Height = 160.0, Weight = 60.0 },
            new { Height = 170.0, Weight = 70.0 },
            new { Height = 180.0, Weight = 80.0 },
            new { Height = 190.0, Weight = 90.0 }
        };

        // Act
        var result = data.Correlation(d => d.Height, d => d.Weight);

        // Assert - perfect positive correlation
        Assert.Equal(1.0, result, precision: 10);
    }

    [Fact]
    public void Correlation_DifferentLengthSequences_ThrowsException()
    {
        // Arrange
        var x = new[] { 1.0, 2.0, 3.0 };
        var y = new[] { 1.0, 2.0 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => x.Correlation(y));
    }

    [Fact]
    public void Covariance_WithLinearRelationship_ReturnsPositiveValue()
    {
        // Arrange
        var x = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var y = new[] { 2.0, 4.0, 6.0, 8.0, 10.0 };

        // Act
        var result = x.Covariance(y);

        // Assert
        Assert.True(result > 0);
    }

    [Fact]
    public void Covariance_WithInverseRelationship_ReturnsNegativeValue()
    {
        // Arrange
        var x = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var y = new[] { 10.0, 8.0, 6.0, 4.0, 2.0 };

        // Act
        var result = x.Covariance(y);

        // Assert
        Assert.True(result < 0);
    }

    [Fact]
    public void Covariance_WithSelectors_CalculatesCorrectly()
    {
        // Arrange
        var students = new[]
        {
            new { StudyHours = 1.0, Score = 50.0 },
            new { StudyHours = 2.0, Score = 60.0 },
            new { StudyHours = 3.0, Score = 70.0 },
            new { StudyHours = 4.0, Score = 80.0 }
        };

        // Act
        var result = students.Covariance(s => s.StudyHours, s => s.Score);

        // Assert - positive covariance expected
        Assert.True(result > 0);
    }

    [Fact]
    public void Correlation_RealWorldExample_StockPrices()
    {
        // Arrange - simulating two correlated stock prices
        var stockA = new[] { 100.0, 102.0, 101.0, 105.0, 107.0 };
        var stockB = new[] { 50.0, 51.0, 50.5, 52.5, 53.5 };

        // Act
        var correlation = stockA.Correlation(stockB);

        // Assert - should be highly correlated
        Assert.True(correlation > 0.9);
    }
}
