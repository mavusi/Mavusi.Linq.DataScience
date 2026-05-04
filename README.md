# Mavusi.Linq.DataScience

A comprehensive .NET library that extends LINQ to Objects with powerful statistical and data science features.

## Features

### 📊 Statistical Extensions
- **Standard Deviation**: Calculate population and sample standard deviation
- **Variance**: Calculate population and sample variance
- Support for both direct values and selector functions

### 🔗 Correlation Extensions
- **Pearson Correlation**: Measure linear correlation between two datasets
- **Covariance**: Calculate covariance between sequences
- Support for paired data analysis

### 🪟 Rolling Window Extensions
- **Rolling Windows**: Create sliding windows over sequences
- **Moving Averages**: Calculate rolling averages
- **Rolling Statistics**: Apply any aggregation function over rolling windows
- **Customizable step size**: Control window overlap

### ⏱️ Time-Series Extensions
- **Time-Series Points**: Structured data with timestamps
- **Resampling**: Aggregate data to different time intervals
- **Differencing**: Calculate first-order differences
- **Percentage Change**: Track relative changes over time
- **Moving Averages**: Simple and exponential moving averages
- **Gap Filling**: Fill missing time points with default values

### 🧮 Linear Algebra Extensions
- **Vector Operations**: Create and manipulate vectors
- **Matrix Operations**: Matrix multiplication, transpose, trace
- **Dot Product**: Vector dot product calculations
- **Vector Math**: Addition, subtraction, scalar multiplication
- **Normalization**: Normalize vectors to unit length
- **Identity Matrices**: Create identity matrices

## Installation

Add this library to your project:

```bash
dotnet add reference Mavusi.Linq.DataScience
```

## Usage Examples

### Statistical Operations

```csharp
using Mavusi.Linq.DataScience;

var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };

// Standard deviation
var stdDev = data.StandardDeviation();
var sampleStdDev = data.StandardDeviationSample();

// Variance
var variance = data.Variance();
var sampleVariance = data.VarianceSample();
```

### Correlation Analysis

```csharp
var x = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
var y = new[] { 2.0, 4.0, 6.0, 8.0, 10.0 };

// Pearson correlation
var correlation = x.Correlation(y); // Returns 1.0 (perfect positive correlation)

// Covariance
var covariance = x.Covariance(y);

// Using selectors
var people = new[]
{
    new { Height = 170, Weight = 70 },
    new { Height = 180, Weight = 80 }
};
var heightWeightCorr = people.Correlation(p => p.Height, p => p.Weight);
```

### Rolling Windows

```csharp
var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 };

// Create rolling windows
var windows = data.RollingWindow(3);
// Results: [1,2,3], [2,3,4], [3,4,5], [4,5,6]

// Moving average
var movingAvg = data.RollingAverage(3);

// Rolling sum
var rollingSum = data.RollingSum(3);

// Custom aggregation
var rollingMax = data.RollingAggregate(3, w => w.Max());

// Windows with custom step
var steppedWindows = data.RollingWindow(3, step: 2);
// Results: [1,2,3], [3,4,5]
```

### Time-Series Analysis

```csharp
// Create time-series data
var timeSeries = new[]
{
    new TimeSeriesPoint<double>(DateTime.Today, 100.0),
    new TimeSeriesPoint<double>(DateTime.Today.AddDays(1), 105.0),
    new TimeSeriesPoint<double>(DateTime.Today.AddDays(2), 103.0)
};

// Calculate percentage change
var changes = timeSeries.PercentageChange();

// Resample to different interval
var hourlyData = timeSeries.Resample(TimeSpan.FromHours(1), values => values.Average());

// Moving average
var ma = timeSeries.MovingAverage(3);

// Exponential moving average
var ema = timeSeries.ExponentialMovingAverage(alpha: 0.3);

// Fill gaps in data
var filled = timeSeries.FillGaps(TimeSpan.FromDays(1), fillValue: 0.0);

// Convert from existing data
var stocks = new[]
{
    new { Date = DateTime.Today, Price = 100.0 },
    new { Date = DateTime.Today.AddDays(1), Price = 105.0 }
};
var stockTimeSeries = stocks.ToTimeSeries(s => s.Date, s => s.Price);
```

### Linear Algebra

```csharp
// Vector operations
var v1 = new[] { 1.0, 2.0, 3.0 }.ToVector();
var v2 = new[] { 4.0, 5.0, 6.0 }.ToVector();

var dotProduct = v1.DotProduct(v2);
var sum = v1.Add(v2);
var diff = v1.Subtract(v2);
var scaled = v1.Multiply(2.0);
var magnitude = v1.Magnitude();
var normalized = v1.Normalize();

// Matrix operations
var matrix1 = new[]
{
    new[] { 1.0, 2.0 },
    new[] { 3.0, 4.0 }
}.ToMatrix();

var matrix2 = new[]
{
    new[] { 5.0, 6.0 },
    new[] { 7.0, 8.0 }
}.ToMatrix();

var product = matrix1.Multiply(matrix2);
var transposed = matrix1.Transpose();
var trace = matrix1.Trace();

// Matrix-vector multiplication
var vector = new[] { 1.0, 2.0 }.ToVector();
var result = matrix1.Multiply(vector);

// Identity matrix
var identity = LinearAlgebraExtensions.CreateIdentityMatrix(3);
```

## Requirements

- .NET 8.0 or higher
- C# 10.0 or higher (for record types and init properties)

## License

This is a personal project. Please adjust the license according to your needs.

## Contributing

Contributions are welcome! Feel free to submit issues or pull requests.
