using Mavusi.Linq.DataScience.Models;

namespace Mavusi.Linq.DataScience.Tests;

/// <summary>
/// Integration tests that demonstrate using multiple extension methods together
/// for real-world data science scenarios.
/// </summary>
public class IntegrationTests
{
    [Fact]
    public void StockAnalysis_CombinesMultipleFeatures()
    {
        // Arrange - simulate 10 days of stock data
        var stockData = new[]
        {
            new { Date = new DateTime(2024, 1, 1), Price = 100.0, Volume = 1000000 },
            new { Date = new DateTime(2024, 1, 2), Price = 102.0, Volume = 1100000 },
            new { Date = new DateTime(2024, 1, 3), Price = 105.0, Volume = 1200000 },
            new { Date = new DateTime(2024, 1, 4), Price = 103.0, Volume = 1050000 },
            new { Date = new DateTime(2024, 1, 5), Price = 107.0, Volume = 1300000 },
            new { Date = new DateTime(2024, 1, 8), Price = 110.0, Volume = 1400000 },
            new { Date = new DateTime(2024, 1, 9), Price = 108.0, Volume = 1250000 },
            new { Date = new DateTime(2024, 1, 10), Price = 112.0, Volume = 1500000 },
            new { Date = new DateTime(2024, 1, 11), Price = 115.0, Volume = 1600000 },
            new { Date = new DateTime(2024, 1, 12), Price = 113.0, Volume = 1450000 }
        };

        // Act 1: Calculate price statistics
        var priceStdDev = stockData.Select(s => s.Price).StandardDeviation();
        var priceVolCorrelation = stockData.Correlation(s => s.Price, s => (double)s.Volume);

        // Act 2: Calculate moving averages
        var prices = stockData.Select(s => s.Price);
        var sma3 = prices.RollingAverage(3).ToList();

        // Act 3: Convert to time series and calculate returns
        var timeSeries = stockData.ToTimeSeries(s => s.Date, s => s.Price);
        var dailyReturns = timeSeries.PercentageChange().ToList();

        // Assert
        Assert.True(priceStdDev > 0);
        Assert.True(priceVolCorrelation > 0); // Price and volume should be positively correlated
        Assert.Equal(8, sma3.Count); // 10 prices - 3 window + 1
        Assert.Equal(9, dailyReturns.Count);
        Assert.True(dailyReturns.Any(r => r.Value > 0)); // At least one positive return
    }

    [Fact]
    public void SensorDataAnalysis_RollingStatistics()
    {
        // Arrange - simulate temperature sensor readings
        var sensorReadings = new[]
        {
            new { Time = DateTime.Today.AddHours(0), Temp = 20.0, Humidity = 65.0 },
            new { Time = DateTime.Today.AddHours(1), Temp = 21.0, Humidity = 64.0 },
            new { Time = DateTime.Today.AddHours(2), Temp = 22.0, Humidity = 63.0 },
            new { Time = DateTime.Today.AddHours(3), Temp = 23.0, Humidity = 62.0 },
            new { Time = DateTime.Today.AddHours(4), Temp = 24.0, Humidity = 61.0 },
            new { Time = DateTime.Today.AddHours(5), Temp = 25.0, Humidity = 60.0 },
            new { Time = DateTime.Today.AddHours(6), Temp = 26.0, Humidity = 59.0 }
        };

        // Act: Calculate rolling statistics with 3-hour windows
        var temps = sensorReadings.Select(s => s.Temp);
        var rollingAvg = temps.RollingAverage(3).ToList();
        var rollingStdDev = temps.RollingStandardDeviation(3).ToList();
        var tempHumidityCorr = sensorReadings.Correlation(s => s.Temp, s => s.Humidity);

        // Assert
        Assert.Equal(5, rollingAvg.Count);
        Assert.Equal(5, rollingStdDev.Count);
        Assert.Equal(21.0, rollingAvg[0]); // (20+21+22)/3
        Assert.True(tempHumidityCorr < 0); // Negative correlation (as temp rises, humidity falls)
    }

    [Fact]
    public void PortfolioAnalysis_MatrixOperations()
    {
        // Arrange - portfolio with 3 assets and their returns
        var asset1Returns = new[] { 0.05, 0.03, 0.07, 0.02, 0.06 };
        var asset2Returns = new[] { 0.04, 0.05, 0.03, 0.06, 0.04 };
        var asset3Returns = new[] { 0.06, 0.04, 0.08, 0.03, 0.07 };

        // Act 1: Calculate correlations
        var corr12 = asset1Returns.Correlation(asset2Returns);
        var corr13 = asset1Returns.Correlation(asset3Returns);
        var corr23 = asset2Returns.Correlation(asset3Returns);

        // Act 2: Portfolio weights as vector
        var weights = new Vector(0.4, 0.3, 0.3);
        var weightsMagnitude = weights.Magnitude();

        // Act 3: Calculate individual asset statistics
        var asset1StdDev = asset1Returns.StandardDeviation();
        var asset2StdDev = asset2Returns.StandardDeviation();
        var asset3StdDev = asset3Returns.StandardDeviation();

        // Assert
        Assert.True(corr12 >= -1 && corr12 <= 1);
        Assert.True(corr13 >= -1 && corr13 <= 1);
        Assert.True(corr23 >= -1 && corr23 <= 1);
        Assert.True(weightsMagnitude > 0);
        Assert.True(asset1StdDev > 0);
        Assert.True(asset2StdDev > 0);
        Assert.True(asset3StdDev > 0);
    }

    [Fact]
    public void TimeSeriesForecasting_ExponentialMovingAverage()
    {
        // Arrange - sales data over time
        var salesData = new[]
        {
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 1), 1000),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 2), 1100),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 3), 1050),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 4), 1200),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 5), 1150),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 6), 1300),
            new TimeSeriesPoint<double>(new DateTime(2024, 1, 7), 1250)
        };

        // Act: Calculate different EMAs for trend detection
        var emaFast = salesData.ExponentialMovingAverage(0.5).ToList(); // Fast EMA
        var emaSlow = salesData.ExponentialMovingAverage(0.2).ToList(); // Slow EMA
        var dailyChange = salesData.Difference().ToList();

        // Assert
        Assert.Equal(7, emaFast.Count);
        Assert.Equal(7, emaSlow.Count);
        Assert.Equal(6, dailyChange.Count);

        // Fast EMA should be closer to recent values
        Assert.True(Math.Abs(emaFast[6].Value - 1250) < Math.Abs(emaSlow[6].Value - 1250));
    }

    [Fact]
    public void LinearRegression_VectorOperations()
    {
        // Arrange - simple linear relationship: y = 2x + 3
        var xValues = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var yValues = new[] { 5.0, 7.0, 9.0, 11.0, 13.0 };

        // Act: Use vectors for calculations
        var xVector = xValues.ToVector();
        var yVector = yValues.ToVector();

        // Calculate means
        var xMean = xValues.Average();
        var yMean = yValues.Average();

        // Calculate slope using vector operations
        var numerator = xValues.Zip(yValues, (x, y) => (x - xMean) * (y - yMean)).Sum();
        var denominator = xValues.Sum(x => Math.Pow(x - xMean, 2));
        var slope = numerator / denominator;
        var intercept = yMean - slope * xMean;

        // Calculate R-squared using correlation
        var correlation = xValues.Correlation(yValues);
        var rSquared = Math.Pow(correlation, 2);

        // Assert
        Assert.Equal(2.0, slope, precision: 10);
        Assert.Equal(3.0, intercept, precision: 10);
        Assert.Equal(1.0, rSquared, precision: 10); // Perfect fit
    }

    [Fact]
    public void AnomalyDetection_RollingStdDev()
    {
        // Arrange - data with an outlier
        var data = new[] { 10.0, 12.0, 11.0, 13.0, 50.0, 12.0, 11.0, 13.0, 12.0, 11.0 };
        var windowSize = 3;

        // Act: Calculate rolling statistics to detect anomaly
        var rollingAvg = data.RollingAverage(windowSize).ToList();
        var rollingStdDev = data.RollingStandardDeviation(windowSize).ToList();

        // Calculate z-score like metric for each window
        var anomalyScores = rollingAvg
            .Zip(rollingStdDev, (avg, std) => std / avg)
            .ToList();

        // Assert
        Assert.Equal(8, anomalyScores.Count);

        // The window containing the outlier should have high variability
        var maxAnomalyScore = anomalyScores.Max();
        var minAnomalyScore = anomalyScores.Min();

        // There should be significant difference between max and min scores
        Assert.True(maxAnomalyScore > 0.5); // High relative standard deviation
        Assert.True(maxAnomalyScore > minAnomalyScore * 2); // Max is at least 2x the min
    }

    [Fact]
    public void MultiVariateAnalysis_CovarianceMatrix()
    {
        // Arrange - three variables
        var var1 = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var var2 = new[] { 2.0, 4.0, 6.0, 8.0, 10.0 };
        var var3 = new[] { 5.0, 4.0, 3.0, 2.0, 1.0 };

        // Act: Calculate all pairwise covariances
        var cov12 = var1.Covariance(var2);
        var cov13 = var1.Covariance(var3);
        var cov23 = var2.Covariance(var3);

        // Create covariance matrix
        var covMatrix = new[]
        {
            new[] { var1.Variance(), cov12, cov13 },
            new[] { cov12, var2.Variance(), cov23 },
            new[] { cov13, cov23, var3.Variance() }
        }.ToMatrix();

        // Assert
        Assert.Equal(3, covMatrix.Rows);
        Assert.Equal(3, covMatrix.Columns);
        Assert.True(cov12 > 0); // Positive relationship
        Assert.True(cov13 < 0); // Negative relationship

        // Covariance matrix should be symmetric
        Assert.Equal(covMatrix[0, 1], covMatrix[1, 0]);
        Assert.Equal(covMatrix[0, 2], covMatrix[2, 0]);
        Assert.Equal(covMatrix[1, 2], covMatrix[2, 1]);
    }

    [Fact]
    public void DataResampling_TimeSeriesAggregation()
    {
        // Arrange - hourly data for 3 days
        var startTime = new DateTime(2024, 1, 1, 0, 0, 0);
        var hourlyData = Enumerable.Range(0, 72) // 3 days * 24 hours
            .Select(h => new TimeSeriesPoint<double>(
                startTime.AddHours(h),
                100 + (h % 24) * 2 + new Random(h).NextDouble() * 5))
            .ToArray();

        // Act: Resample to daily aggregates
        var dailyMax = hourlyData.Resample(TimeSpan.FromDays(1), values => values.Max()).ToList();
        var dailyMin = hourlyData.Resample(TimeSpan.FromDays(1), values => values.Min()).ToList();
        var dailyAvg = hourlyData.Resample(TimeSpan.FromDays(1), values => values.Average()).ToList();

        // Assert
        Assert.Equal(3, dailyMax.Count);
        Assert.Equal(3, dailyMin.Count);
        Assert.Equal(3, dailyAvg.Count);

        // Max should be greater than or equal to average, which should be greater than or equal to min
        for (int i = 0; i < 3; i++)
        {
            Assert.True(dailyMax[i].Value >= dailyAvg[i].Value);
            Assert.True(dailyAvg[i].Value >= dailyMin[i].Value);
        }
    }
}
