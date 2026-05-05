namespace Mavusi.Linq.DataScience.Tests;

public class RollingWindowExtensionsTests
{
    [Fact]
    public void RollingWindow_WithBasicData_CreatesCorrectWindows()
    {
        // Arrange
        var data = new[] { 1, 2, 3, 4, 5, 6 };
        var windowSize = 3;

        // Act
        var windows = data.RollingWindow(windowSize).ToList();

        // Assert
        Assert.Equal(4, windows.Count); // 6 - 3 + 1 = 4 windows
        Assert.Equal(new[] { 1, 2, 3 }, windows[0].Values);
        Assert.Equal(new[] { 2, 3, 4 }, windows[1].Values);
        Assert.Equal(new[] { 3, 4, 5 }, windows[2].Values);
        Assert.Equal(new[] { 4, 5, 6 }, windows[3].Values);
    }

    [Fact]
    public void RollingWindow_WithStep_SkipsCorrectly()
    {
        // Arrange
        var data = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var windowSize = 3;
        var step = 2;

        // Act
        var windows = data.RollingWindow(windowSize, step).ToList();

        // Assert
        Assert.Equal(3, windows.Count); // positions 0, 2, 4
        Assert.Equal(new[] { 1, 2, 3 }, windows[0].Values);
        Assert.Equal(new[] { 3, 4, 5 }, windows[1].Values);
        Assert.Equal(new[] { 5, 6, 7 }, windows[2].Values);
    }

    [Fact]
    public void RollingWindow_TracksIndexCorrectly()
    {
        // Arrange
        var data = new[] { 10, 20, 30, 40, 50 };

        // Act
        var windows = data.RollingWindow(2).ToList();

        // Assert
        Assert.Equal(0, windows[0].Index);
        Assert.Equal(1, windows[1].Index);
        Assert.Equal(2, windows[2].Index);
        Assert.Equal(3, windows[3].Index);
    }

    [Fact]
    public void RollingAverage_CalculatesMovingAverage()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };

        // Act
        var movingAvg = data.RollingAverage(3).ToList();

        // Assert
        Assert.Equal(3, movingAvg.Count);
        Assert.Equal(2.0, movingAvg[0]); // (1+2+3)/3
        Assert.Equal(3.0, movingAvg[1]); // (2+3+4)/3
        Assert.Equal(4.0, movingAvg[2]); // (3+4+5)/3
    }

    [Fact]
    public void RollingAverage_WithSelector_WorksCorrectly()
    {
        // Arrange
        var sales = new[]
        {
            new { Day = 1, Amount = 100.0 },
            new { Day = 2, Amount = 200.0 },
            new { Day = 3, Amount = 300.0 },
            new { Day = 4, Amount = 200.0 }
        };

        // With Mavusi.Linq.DataScience.Extensions.RollingWindowExtensions.RollingAverage method
        var movingAvg = sales.RollingAverage(2, s => s.Amount).ToList();

        //without
        var manualMovingAvg = sales
            .Select((s, i) => new
            {
                s.Day,
                s.Amount,
                RollingAvg = i >= 1 ? (sales[i - 1].Amount + s.Amount) / 2 : (double?)null
            })
            .Where(x => x.RollingAvg.HasValue)
            .Select(x => x.RollingAvg.Value)
            .ToList();

        // Assert
        Assert.Equal(3, movingAvg.Count);
        Assert.Equal(150.0, movingAvg[0]); // (100+200)/2
        Assert.Equal(250.0, movingAvg[1]); // (200+300)/2
        Assert.Equal(250.0, movingAvg[2]); // (300+200)/2
    }

    [Fact]
    public void RollingSum_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };

        // Act
        var rollingSum = data.RollingSum(3).ToList();

        // Assert
        Assert.Equal(3, rollingSum.Count);
        Assert.Equal(6.0, rollingSum[0]);  // 1+2+3
        Assert.Equal(9.0, rollingSum[1]);  // 2+3+4
        Assert.Equal(12.0, rollingSum[2]); // 3+4+5
    }

    [Fact]
    public void RollingStandardDeviation_CalculatesCorrectly()
    {
        // Arrange
        var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 };

        // Act
        var rollingStd = data.RollingStandardDeviation(3).ToList();

        // Assert
        Assert.Equal(4, rollingStd.Count);
        foreach (var std in rollingStd)
        {
            Assert.True(std > 0);
        }
    }

    [Fact]
    public void RollingAggregate_CustomAggregation_WorksCorrectly()
    {
        // Arrange
        var data = new[] { 5, 2, 8, 1, 9, 3 };

        // Act - find max in each window
        var rollingMax = data.RollingAggregate(3, w => w.Max()).ToList();

        // Assert
        Assert.Equal(4, rollingMax.Count);
        Assert.Equal(8, rollingMax[0]); // max(5,2,8)
        Assert.Equal(8, rollingMax[1]); // max(2,8,1)
        Assert.Equal(9, rollingMax[2]); // max(8,1,9)
        Assert.Equal(9, rollingMax[3]); // max(1,9,3)
    }

    [Fact]
    public void RollingWindow_InvalidWindowSize_ThrowsException()
    {
        // Arrange
        var data = new[] { 1, 2, 3 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => data.RollingWindow(0).ToList());
        Assert.Throws<ArgumentException>(() => data.RollingWindow(-1).ToList());
    }

    [Fact]
    public void RollingWindow_RealWorldExample_StockPrices()
    {
        // Arrange - simulating daily stock prices
        var stockPrices = new[] { 100.0, 102.0, 98.0, 101.0, 105.0, 103.0, 107.0 };

        // Act - 5-day moving average
        var sma5 = stockPrices.RollingAverage(5).ToList();

        // Assert
        Assert.Equal(3, sma5.Count);
        Assert.True(sma5.All(price => price > 0));

        // First 5-day average: (100+102+98+101+105)/5 = 101.2
        Assert.Equal(101.2, sma5[0], precision: 1);
    }

    [Fact]
    public void RollingAggregate_RangeCalculation_WorksCorrectly()
    {
        // Arrange
        var data = new[] { 10.0, 15.0, 12.0, 20.0, 18.0, 25.0 };

        // Act - calculate range (max - min) in each window
        var rollingRange = data.RollingAggregate(3, w => w.Max() - w.Min()).ToList();

        // Assert
        Assert.Equal(4, rollingRange.Count);
        Assert.Equal(5.0, rollingRange[0]);  // 15-10
        Assert.Equal(8.0, rollingRange[1]);  // 20-12
        Assert.Equal(8.0, rollingRange[2]);  // 20-12
        Assert.Equal(7.0, rollingRange[3]);  // 25-18
    }
}
