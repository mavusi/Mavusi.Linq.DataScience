namespace Mavusi.Linq.DataScience.Tests;

public class DistributionExtensionsTests
{
    [Fact]
    public void Median_WithOddCount_ReturnsMiddleValue()
    {
        // Arrange
        var data = new[] { 1.0, 3.0, 2.0, 5.0, 4.0 }; // Sorted: 1, 2, 3, 4, 5

        // Act
        var median = data.Median();

        // Assert
        Assert.Equal(3.0, median);
    }

    [Fact]
    public void Median_WithEvenCount_ReturnsAverageOfMiddleTwo()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0 };

        // Act
        var median = data.Median();

        // Assert
        Assert.Equal(2.5, median); // (2 + 3) / 2
    }

    [Fact]
    public void Median_WithSingleValue_ReturnsThatValue()
    {
        // Arrange
        var data = new[] { 42.0 };

        // Act
        var median = data.Median();

        // Assert
        Assert.Equal(42.0, median);
    }

    [Fact]
    public void Median_WithSelector_WorksCorrectly()
    {
        // Arrange
        var people = new[]
        {
            new { Name = "Alice", Age = 25 },
            new { Name = "Bob", Age = 30 },
            new { Name = "Charlie", Age = 35 }
        };

        // Act
        var medianAge = people.Median(p => p.Age);

        // Assert
        Assert.Equal(30.0, medianAge);
    }

    [Fact]
    public void Median_WithEmptySequence_ThrowsException()
    {
        // Arrange
        var data = Array.Empty<double>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => data.Median());
    }

    [Fact]
    public void Mode_WithClearMode_ReturnsCorrectValue()
    {
        // Arrange
        var data = new[] { 1, 2, 2, 3, 3, 3, 4 };

        // Act
        var mode = data.Mode();

        // Assert
        Assert.Equal(3, mode);
    }

    [Fact]
    public void Mode_WithStrings_ReturnsCorrectValue()
    {
        // Arrange
        var data = new[] { "apple", "banana", "apple", "cherry", "apple" };

        // Act
        var mode = data.Mode();

        // Assert
        Assert.Equal("apple", mode);
    }

    [Fact]
    public void Mode_WithAllSameFrequency_ReturnsFirstValue()
    {
        // Arrange
        var data = new[] { 1, 2, 3, 4, 5 };

        // Act
        var mode = data.Mode();

        // Assert
        Assert.Equal(1, mode); // First in sequence
    }

    [Fact]
    public void Quartile_Q1_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 };

        // Act
        var q1 = data.Quartile(1);

        // Assert
        Assert.Equal(3.25, q1, precision: 2);
    }

    [Fact]
    public void Quartile_Q2_EqualsMedian()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };

        // Act
        var q2 = data.Quartile(2);
        var median = data.Median();

        // Assert
        Assert.Equal(median, q2);
        Assert.Equal(3.0, q2);
    }

    [Fact]
    public void Quartile_Q3_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 };

        // Act
        var q3 = data.Quartile(3);

        // Assert
        Assert.Equal(7.75, q3, precision: 2);
    }

    [Fact]
    public void Quartile_InvalidQuartile_ThrowsException()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => data.Quartile(0));
        Assert.Throws<ArgumentException>(() => data.Quartile(4));
    }

    [Fact]
    public void Quartile_WithSelector_WorksCorrectly()
    {
        // Arrange
        var scores = new[]
        {
            new { Student = "A", Score = 65.0 },
            new { Student = "B", Score = 70.0 },
            new { Student = "C", Score = 75.0 },
            new { Student = "D", Score = 80.0 },
            new { Student = "E", Score = 85.0 }
        };

        // Act
        var q1 = scores.Quartile(1, s => s.Score);

        // Assert
        Assert.True(q1 >= 65.0 && q1 <= 75.0);
    }

    [Fact]
    public void InterquartileRange_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 };

        // Act
        var iqr = data.InterquartileRange();

        // Assert - IQR = Q3 - Q1 = 7.75 - 3.25 = 4.5
        Assert.Equal(4.5, iqr, precision: 2);
    }

    [Fact]
    public void InterquartileRange_WithSelector_WorksCorrectly()
    {
        // Arrange
        var temperatures = new[]
        {
            new { City = "A", Temp = 10.0 },
            new { City = "B", Temp = 20.0 },
            new { City = "C", Temp = 30.0 },
            new { City = "D", Temp = 40.0 }
        };

        // Act
        var iqr = temperatures.InterquartileRange(t => t.Temp);

        // Assert
        Assert.True(iqr > 0);
    }

    [Fact]
    public void Percentile_0_ReturnsMinimum()
    {
        // Arrange
        var data = new[] { 10.0, 20.0, 30.0, 40.0, 50.0 };

        // Act
        var p0 = data.Percentile(0);

        // Assert
        Assert.Equal(10.0, p0);
    }

    [Fact]
    public void Percentile_100_ReturnsMaximum()
    {
        // Arrange
        var data = new[] { 10.0, 20.0, 30.0, 40.0, 50.0 };

        // Act
        var p100 = data.Percentile(100);

        // Assert
        Assert.Equal(50.0, p100);
    }

    [Fact]
    public void Percentile_50_EqualsMedian()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };

        // Act
        var p50 = data.Percentile(50);
        var median = data.Median();

        // Assert
        Assert.Equal(median, p50);
    }

    [Fact]
    public void Percentile_90_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 };

        // Act
        var p90 = data.Percentile(90);

        // Assert - 90th percentile should be 9.1
        Assert.Equal(9.1, p90, precision: 1);
    }

    [Fact]
    public void Percentile_InvalidPercentile_ThrowsException()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => data.Percentile(-1));
        Assert.Throws<ArgumentException>(() => data.Percentile(101));
    }

    [Fact]
    public void Percentile_WithSelector_WorksCorrectly()
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

        // Act
        var p75 = data.Percentile(75, d => d.Value);

        // Assert
        Assert.Equal(40.0, p75, precision: 1);
    }

    [Fact]
    public void Skewness_SymmetricDistribution_ReturnsNearZero()
    {
        // Arrange - symmetric distribution
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };

        // Act
        var skewness = data.Skewness();

        // Assert - should be near zero for symmetric distribution
        Assert.True(Math.Abs(skewness) < 0.1);
    }

    [Fact]
    public void Skewness_RightSkewed_ReturnsPositive()
    {
        // Arrange - right-skewed distribution (long right tail)
        var data = new[] { 1.0, 1.0, 1.0, 2.0, 2.0, 3.0, 10.0 };

        // Act
        var skewness = data.Skewness();

        // Assert
        Assert.True(skewness > 0);
    }

    [Fact]
    public void Skewness_LeftSkewed_ReturnsNegative()
    {
        // Arrange - left-skewed distribution (long left tail)
        var data = new[] { 1.0, 8.0, 9.0, 9.0, 10.0, 10.0, 10.0 };

        // Act
        var skewness = data.Skewness();

        // Assert
        Assert.True(skewness < 0);
    }

    [Fact]
    public void Skewness_InsufficientData_ThrowsException()
    {
        // Arrange
        var data = new[] { 1.0, 2.0 };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => data.Skewness());
    }

    [Fact]
    public void Skewness_WithSelector_WorksCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new { Value = 1.0 },
            new { Value = 2.0 },
            new { Value = 3.0 },
            new { Value = 4.0 },
            new { Value = 5.0 }
        };

        // Act
        var skewness = data.Skewness(d => d.Value);

        // Assert
        Assert.True(Math.Abs(skewness) < 0.1);
    }

    [Fact]
    public void Kurtosis_NormalDistribution_ReturnsNearZero()
    {
        // Arrange - approximately normal distribution
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0 };

        // Act
        var kurtosis = data.Kurtosis();

        // Assert - excess kurtosis near 0 for normal distribution
        Assert.True(Math.Abs(kurtosis) < 2.0);
    }

    [Fact]
    public void Kurtosis_HeavyTails_ReturnsPositive()
    {
        // Arrange - distribution with heavy tails (outliers)
        var data = new[] { 1.0, 5.0, 5.0, 5.0, 5.0, 5.0, 5.0, 100.0 };

        // Act
        var kurtosis = data.Kurtosis();

        // Assert
        Assert.True(kurtosis > 0);
    }

    [Fact]
    public void Kurtosis_InsufficientData_ThrowsException()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0 };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => data.Kurtosis());
    }

    [Fact]
    public void Kurtosis_WithSelector_WorksCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new { Value = 1.0 },
            new { Value = 2.0 },
            new { Value = 3.0 },
            new { Value = 4.0 },
            new { Value = 5.0 },
            new { Value = 6.0 },
            new { Value = 7.0 }
        };

        // Act
        var kurtosis = data.Kurtosis(d => d.Value);

        // Assert
        Assert.True(Math.Abs(kurtosis) < 2.0);
    }

    [Fact]
    public void Range_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 10.0, 25.0, 15.0, 30.0, 5.0 };

        // Act
        var range = data.Range();

        // Assert
        Assert.Equal(25.0, range); // 30 - 5
    }

    [Fact]
    public void Range_WithSingleValue_ReturnsZero()
    {
        // Arrange
        var data = new[] { 42.0 };

        // Act
        var range = data.Range();

        // Assert
        Assert.Equal(0.0, range);
    }

    [Fact]
    public void Range_WithSelector_WorksCorrectly()
    {
        // Arrange
        var products = new[]
        {
            new { Name = "A", Price = 10.0 },
            new { Name = "B", Price = 50.0 },
            new { Name = "C", Price = 30.0 }
        };

        // Act
        var priceRange = products.Range(p => p.Price);

        // Assert
        Assert.Equal(40.0, priceRange); // 50 - 10
    }

    [Fact]
    public void MeanAbsoluteDeviation_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 2.0, 4.0, 6.0, 8.0, 10.0 }; // Mean = 6

        // Act
        var mad = data.MeanAbsoluteDeviation();

        // Assert - MAD = (|2-6| + |4-6| + |6-6| + |8-6| + |10-6|) / 5 = (4+2+0+2+4)/5 = 2.4
        Assert.Equal(2.4, mad, precision: 5);
    }

    [Fact]
    public void MeanAbsoluteDeviation_WithNoDeviation_ReturnsZero()
    {
        // Arrange
        var data = new[] { 5.0, 5.0, 5.0, 5.0 };

        // Act
        var mad = data.MeanAbsoluteDeviation();

        // Assert
        Assert.Equal(0.0, mad);
    }

    [Fact]
    public void MeanAbsoluteDeviation_WithSelector_WorksCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new { Value = 2.0 },
            new { Value = 4.0 },
            new { Value = 6.0 },
            new { Value = 8.0 },
            new { Value = 10.0 }
        };

        // Act
        var mad = data.MeanAbsoluteDeviation(d => d.Value);

        // Assert
        Assert.Equal(2.4, mad, precision: 5);
    }

    [Fact]
    public void Distribution_RealWorldExample_TestScores()
    {
        // Arrange - Student test scores
        var scores = new[] { 65.0, 70.0, 75.0, 80.0, 85.0, 90.0, 95.0, 72.0, 88.0, 78.0 };

        // Act
        var mean = scores.Average();
        var median = scores.Median();
        var q1 = scores.Quartile(1);
        var q3 = scores.Quartile(3);
        var iqr = scores.InterquartileRange();
        var range = scores.Range();
        var stdDev = scores.StandardDeviation();

        // Assert - verify all calculations produce reasonable results
        Assert.True(median >= q1 && median <= q3);
        Assert.True(iqr > 0);
        Assert.Equal(30.0, range);
        Assert.True(stdDev > 0);
    }

    [Fact]
    public void Distribution_RealWorldExample_SalaryData()
    {
        // Arrange - Employee salaries
        var employees = new[]
        {
            new { Name = "Alice", Salary = 50000.0 },
            new { Name = "Bob", Salary = 60000.0 },
            new { Name = "Charlie", Salary = 55000.0 },
            new { Name = "David", Salary = 70000.0 },
            new { Name = "Eve", Salary = 65000.0 },
            new { Name = "Frank", Salary = 80000.0 },
            new { Name = "Grace", Salary = 150000.0 } // Outlier
        };

        // Act
        var medianSalary = employees.Median(e => e.Salary);
        var meanSalary = employees.Average(e => e.Salary);
        var q1 = employees.Quartile(1, e => e.Salary);
        var q3 = employees.Quartile(3, e => e.Salary);
        var iqr = employees.InterquartileRange(e => e.Salary);

        // Assert - median should be less affected by outlier than mean
        Assert.True(medianSalary < meanSalary); // Due to high outlier
        Assert.Equal(65000.0, medianSalary);
        Assert.True(iqr > 0);
    }

    [Fact]
    public void Distribution_FiveNumberSummary_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 };

        // Act - Five-number summary: min, Q1, median, Q3, max
        var min = data.Min();
        var q1 = data.Quartile(1);
        var median = data.Median();
        var q3 = data.Quartile(3);
        var max = data.Max();

        // Assert
        Assert.Equal(1.0, min);
        Assert.True(q1 > min && q1 < median);
        Assert.Equal(5.5, median);
        Assert.True(q3 > median && q3 < max);
        Assert.Equal(10.0, max);
    }
}
