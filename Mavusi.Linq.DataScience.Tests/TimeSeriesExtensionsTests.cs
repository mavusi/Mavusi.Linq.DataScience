using Mavusi.Linq.DataScience.Models;

namespace Mavusi.Linq.DataScience.Tests;

public class TimeSeriesExtensionsTests
{
    [Fact]
    public void ToTimeSeries_ConvertsDataCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new { Date = new DateTime(2024, 1, 1), Price = 100.0 },
            new { Date = new DateTime(2024, 1, 2), Price = 105.0 },
            new { Date = new DateTime(2024, 1, 3), Price = 103.0 }
        };

        // Act
        var timeSeries = data.ToTimeSeries(d => d.Date, d => d.Price).ToList();

        // Assert
        Assert.Equal(3, timeSeries.Count);
        Assert.Equal(new DateTime(2024, 1, 1), timeSeries[0].Timestamp);
        Assert.Equal(100.0, timeSeries[0].Value);
    }

    [Fact]
    public void GroupByInterval_GroupsByHour_Correctly()
    {
        // Arrange
        var timeSeries = new[]
        {
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 1, 10, 15, 0), 100.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 1, 10, 45, 0), 105.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 1, 11, 20, 0), 110.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 1, 11, 50, 0), 108.0)
        };

        // Act
        var grouped = timeSeries.GroupByInterval(TimeSpan.FromHours(1)).ToList();

        // Assert
        Assert.Equal(2, grouped.Count); // Two hourly buckets
        Assert.Equal(2, grouped[0].Count()); // 10:15 and 10:45
        Assert.Equal(2, grouped[1].Count()); // 11:20 and 11:50
    }

    [Fact]
    public void Resample_AggregatesCorrectly()
    {
        // Arrange
        var timeSeries = new[]
        {
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 1, 10, 0, 0), 100.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 1, 10, 30, 0), 110.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 1, 11, 0, 0), 120.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 1, 11, 30, 0), 130.0)
        };

        // Act - resample to hourly averages
        var resampled = timeSeries.Resample(TimeSpan.FromHours(1), values => values.Average()).ToList();

        // Assert
        Assert.Equal(2, resampled.Count);
        Assert.Equal(105.0, resampled[0].Value); // (100+110)/2
        Assert.Equal(125.0, resampled[1].Value); // (120+130)/2
    }

    [Fact]
    public void Difference_CalculatesFirstOrderDifference()
    {
        // Arrange
        var timeSeries = new[]
        {
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 1), 100.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 2), 105.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 3), 103.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 4), 108.0)
        };

        // Act
        var differences = timeSeries.Difference().ToList();

        // Assert
        Assert.Equal(3, differences.Count); // One less than original
        Assert.Equal(5.0, differences[0].Value);   // 105-100
        Assert.Equal(-2.0, differences[1].Value);  // 103-105
        Assert.Equal(5.0, differences[2].Value);   // 108-103
    }

    [Fact]
    public void PercentageChange_CalculatesCorrectly()
    {
        // Arrange
        var timeSeries = new[]
        {
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 1), 100.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 2), 110.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 3), 99.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 4), 105.0)
        };

        // Act
        var percentChanges = timeSeries.PercentageChange().ToList();

        // Assert
        Assert.Equal(3, percentChanges.Count);
        Assert.Equal(0.10, percentChanges[0].Value, precision: 5); // 10% increase
        Assert.Equal(-0.1, percentChanges[1].Value, precision: 2);  // ~10% decrease
    }

    [Fact]
    public void PercentageChange_WithZeroValue_ReturnsNaN()
    {
        // Arrange
        var timeSeries = new[]
        {
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 1), 0.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 2), 100.0)
        };

        // Act
        var percentChanges = timeSeries.PercentageChange().ToList();

        // Assert
        Assert.Equal(1, percentChanges.Count);
        Assert.True(double.IsNaN(percentChanges[0].Value));
    }

    [Fact]
    public void MovingAverage_CalculatesSMA()
    {
        // Arrange
        var timeSeries = new[]
        {
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 1), 10.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 2), 20.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 3), 30.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 4), 40.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 5), 50.0)
        };

        // Act
        var sma = timeSeries.MovingAverage(3).ToList();

        // Assert
        Assert.Equal(3, sma.Count);
        Assert.Equal(20.0, sma[0].Value); // (10+20+30)/3
        Assert.Equal(30.0, sma[1].Value); // (20+30+40)/3
        Assert.Equal(40.0, sma[2].Value); // (30+40+50)/3
    }

    [Fact]
    public void ExponentialMovingAverage_CalculatesEMA()
    {
        // Arrange
        var timeSeries = new[]
        {
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 1), 100.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 2), 110.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 3), 120.0)
        };
        var alpha = 0.5;

        // Act
        var ema = timeSeries.ExponentialMovingAverage(alpha).ToList();

        // Assert
        Assert.Equal(3, ema.Count);
        Assert.Equal(100.0, ema[0].Value); // First value is seed
        Assert.Equal(105.0, ema[1].Value); // 0.5*110 + 0.5*100
        Assert.Equal(112.5, ema[2].Value); // 0.5*120 + 0.5*105
    }

    [Fact]
    public void FillGaps_FillsMissingTimePoints()
    {
        // Arrange
        var timeSeries = new[]
        {
            new TimeSeriesPoint<int>(new DateTime(2024, 1, 1), 100),
            new TimeSeriesPoint<int>(new DateTime(2024, 1, 4), 400) // Missing Jan 2 and 3
        };

        // Act
        var filled = timeSeries.FillGaps(TimeSpan.FromDays(1), fillValue: 0).ToList();

        // Assert
        Assert.Equal(4, filled.Count);
        Assert.Equal(100, filled[0].Value);
        Assert.Equal(0, filled[1].Value);   // Jan 2 filled
        Assert.Equal(0, filled[2].Value);   // Jan 3 filled
        Assert.Equal(400, filled[3].Value);
    }

    [Fact]
    public void FillGaps_NoGaps_ReturnsOriginal()
    {
        // Arrange
        var timeSeries = new[]
        {
            new TimeSeriesPoint<int>(new DateTime(2024, 1, 1), 100),
            new TimeSeriesPoint<int>(new DateTime(2024, 1, 2), 200),
            new TimeSeriesPoint<int>(new DateTime(2024, 1, 3), 300)
        };

        // Act
        var filled = timeSeries.FillGaps(TimeSpan.FromDays(1), fillValue: 0).ToList();

        // Assert
        Assert.Equal(3, filled.Count);
        Assert.Equal(100, filled[0].Value);
        Assert.Equal(200, filled[1].Value);
        Assert.Equal(300, filled[2].Value);
    }

    [Fact]
    public void Resample_DailyToWeekly_WorksCorrectly()
    {
        // Arrange - 7 days of data
        var startDate = new DateTime(2024, 1, 1);
        var dailyData = Enumerable.Range(0, 7)
            .Select(i => new TimeSeriesPoint<double>(startDate.AddDays(i), (i + 1) * 10.0))
            .ToArray();

        // Act - resample to weekly sum
        var weekly = dailyData.Resample(TimeSpan.FromDays(7), values => values.Sum()).ToList();

        // Assert
        Assert.Equal(1, weekly.Count);
        Assert.Equal(280.0, weekly[0].Value); // 10+20+30+40+50+60+70
    }

    [Fact]
    public void RealWorldExample_StockPriceAnalysis()
    {
        // Arrange - simulate stock prices over 10 days
        var stockPrices = new[]
        {
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 1), 100.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 2), 102.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 3), 105.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 4), 103.0),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 5), 107.0)
        };

        // Act - calculate daily returns
        var dailyReturns = stockPrices.PercentageChange().ToList();
        var movingAvg = stockPrices.MovingAverage(3).ToList();

        // Assert
        Assert.Equal(4, dailyReturns.Count);
        Assert.True(dailyReturns[0].Value > 0); // Positive return on day 2
        Assert.Equal(3, movingAvg.Count);
    }

    [Fact]
    public void ExponentialMovingAverage_InvalidAlpha_ThrowsException()
    {
        // Arrange
        var timeSeries = new[]
        {
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 1), 100.0)
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => timeSeries.ExponentialMovingAverage(0).ToList());
        Assert.Throws<ArgumentException>(() => timeSeries.ExponentialMovingAverage(1.5).ToList());
    }
}
