# Mavusi.Linq.DataScience

A comprehensive .NET library that extends LINQ to Objects with powerful statistical and data science features. The library is split into two packages for optimal dependency management:

- **Mavusi.Linq.DataScience** - CPU-optimized core library (this package)
- **Mavusi.Linq.DataScience.GpuBound** - GPU-accelerated extensions with ILGPU (optional)

## 📦 Installation

### Core Package (CPU-Optimized)

```bash
dotnet add package Mavusi.Linq.DataScience
```

This package includes all CPU-optimized statistical, correlation, distribution, time-series, linear algebra, and geospatial operations with zero external dependencies.

### GPU-Accelerated Package (Optional)

```bash
dotnet add package Mavusi.Linq.DataScience.GpuBound
```

Add this package for **up to 100x performance** on large datasets (10,000+ elements) using GPU acceleration via ILGPU. All GPU methods are suffixed with `Gpu` and can be used as drop-in replacements.

## ✨ Features Overview

### 📊 Statistical Extensions
- **Standard Deviation**: Calculate population and sample standard deviation
- **Variance**: Calculate population and sample variance
- Support for both direct values and selector functions

### 📈 Distribution Extensions
- **Median**: Calculate the median (50th percentile) of a dataset
- **Mode**: Find the most frequently occurring value
- **Quartiles**: Calculate Q1, Q2 (median), and Q3
- **Percentiles**: Calculate any percentile (0-100) with linear interpolation
- **Interquartile Range (IQR)**: Measure statistical dispersion
- **Skewness**: Measure distribution asymmetry (left/right skewed)
- **Kurtosis**: Measure distribution tail heaviness
- **Range**: Calculate the difference between max and min
- **Mean Absolute Deviation (MAD)**: Alternative measure of variability
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

### 🌍 Geospatial Extensions
- **Haversine Distance**: Calculate great-circle distances between coordinates (km/miles)
- **Radius Filtering**: Find all items within a specified radius
- **Nearest Neighbor**: Find the closest item(s) to a target location
- **Geographic Center**: Calculate the centroid of multiple coordinates
- **Bounding Boxes**: Create and query geographical bounds
- **Route Calculations**: Calculate total distance for routes
- **Proximity Clustering**: Group nearby points together
- **Pairwise Distances**: Calculate all distances between coordinate pairs

## 📖 Usage Examples

### CPU-Optimized Methods (Core Package)

All examples below use the **Mavusi.Linq.DataScience** package with CPU-optimized implementations.

#### Statistical Operations

```csharp
using Mavusi.Linq.DataScience;

var data = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };

// Standard deviation
var stdDev = data.StandardDeviation();
var sampleStdDev = data.StandardDeviationSample();

// Variance
var variance = data.Variance();
var sampleVariance = data.VarianceSample();

// With selectors
var people = new[]
{
    new { Name = "Alice", Age = 25.0 },
    new { Name = "Bob", Age = 30.0 }
};
var ageStdDev = people.StandardDeviation(p => p.Age);
```

#### Correlation Analysis

```csharp
using Mavusi.Linq.DataScience;

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

#### Distribution Analysis

```csharp
using Mavusi.Linq.DataScience;

var testScores = new[] { 65.0, 70.0, 75.0, 80.0, 85.0, 90.0, 95.0, 72.0, 88.0, 78.0 };

// Central tendency
var mean = testScores.Average();
var median = testScores.Median();
var mode = new[] { 1, 2, 2, 3, 3, 3, 4 }.Mode(); // Returns 3

// Percentiles and quartiles
var p90 = testScores.Percentile(90);  // 90th percentile
var q1 = testScores.Quartile(1);      // 25th percentile (Q1)
var q2 = testScores.Quartile(2);      // 50th percentile (median)
var q3 = testScores.Quartile(3);      // 75th percentile (Q3)
var iqr = testScores.InterquartileRange(); // Q3 - Q1

// Distribution shape
var skewness = testScores.Skewness();   // Measure of asymmetry
var kurtosis = testScores.Kurtosis();   // Measure of tail heaviness

// Dispersion measures
var range = testScores.Range();         // Max - Min
var mad = testScores.MeanAbsoluteDeviation(); // Average absolute deviation

// Using selectors
var employees = new[]
{
    new { Name = "Alice", Salary = 50000.0 },
    new { Name = "Bob", Salary = 60000.0 },
    new { Name = "Charlie", Salary = 55000.0 }
};

var medianSalary = employees.Median(e => e.Salary);
var salaryIQR = employees.InterquartileRange(e => e.Salary);
var salaryRange = employees.Range(e => e.Salary);
```

#### Rolling Windows

```csharp
using Mavusi.Linq.DataScience;

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

#### Time-Series Analysis

```csharp
using Mavusi.Linq.DataScience;
using Mavusi.Linq.DataScience.Models;

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

#### Linear Algebra

```csharp
using Mavusi.Linq.DataScience;
using Mavusi.Linq.DataScience.Models;

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

#### Geospatial Analysis

```csharp
using Mavusi.Linq.DataScience;
using Mavusi.Linq.DataScience.Models;

// Define coordinates
var newYork = new GeoCoordinate(40.7128, -74.0060);
var losAngeles = new GeoCoordinate(34.0522, -118.2437);

// Calculate distance
var distanceKm = newYork.HaversineDistance(losAngeles);     // ~3944 km
var distanceMiles = newYork.HaversineDistanceMiles(losAngeles); // ~2451 miles

// Find locations within radius
var restaurants = new[]
{
    new { Name = "Pizza Place", Location = new GeoCoordinate(40.748817, -73.985428) },
    new { Name = "Burger Joint", Location = new GeoCoordinate(40.750580, -73.993584) },
    new { Name = "Sushi Bar", Location = new GeoCoordinate(40.752726, -73.977229) }
};

var myLocation = new GeoCoordinate(40.750, -73.985);
var nearbyRestaurants = restaurants
    .WithinRadius(r => r.Location, myLocation, radiusKm: 1.0)
    .ToList();

// Find nearest locations
var nearest = restaurants.Nearest(r => r.Location, myLocation);
var nearest3 = restaurants.NearestN(r => r.Location, myLocation, 3).ToList();

// Calculate geographic center
var cities = new[]
{
    new GeoCoordinate(40.7128, -74.0060),  // New York
    new GeoCoordinate(34.0522, -118.2437), // Los Angeles
    new GeoCoordinate(41.8781, -87.6298)   // Chicago
};
var center = cities.GeographicCenter();

// Get bounding box
var bounds = cities.BoundingBox();
bool isInside = bounds.Contains(new GeoCoordinate(40.0, -75.0));

// Calculate route distance
var roadTrip = new[]
{
    new GeoCoordinate(34.0522, -118.2437), // Los Angeles
    new GeoCoordinate(36.7783, -119.4179), // Fresno
    new GeoCoordinate(37.7749, -122.4194), // San Francisco
    new GeoCoordinate(38.5816, -121.4944)  // Sacramento
};
var totalDistance = roadTrip.TotalDistance(); // Total km

// Group by proximity
var stores = new[]
{
    new { Id = 1, Location = new GeoCoordinate(40.0, -74.0) },
    new { Id = 2, Location = new GeoCoordinate(40.01, -74.0) },
    new { Id = 3, Location = new GeoCoordinate(41.0, -73.0) }
};
var clusters = stores.GroupByProximity(s => s.Location, thresholdKm: 10).ToList();
```

### GPU-Accelerated Methods (Optional GpuBound Package)

For massive performance gains on large datasets, install the **Mavusi.Linq.DataScience.GpuBound** package and use GPU-accelerated versions:

```csharp
using Mavusi.Linq.DataScience.GpuBound;

// Large dataset (1M+ elements)
var largeDataset = Enumerable.Range(0, 1000000).Select(i => (double)i).ToArray();
var y = Enumerable.Range(0, 1000000).Select(i => (double)i * 2 + 5).ToArray();

// GPU-accelerated correlation (up to 100x faster!)
var correlation = largeDataset.CorrelationGpu(y);

// GPU-accelerated standard deviation
var stdDev = largeDataset.StandardDeviationGpu();

// GPU-accelerated rolling average
var rollingAvg = largeDataset.RollingAverageGpu(windowSize: 100);

// GPU-accelerated linear algebra
var v1 = new[] { 1.0, 2.0, 3.0 }.ToVector();
var v2 = new[] { 4.0, 5.0, 6.0 }.ToVector();
var dotProduct = v1.DotProductGpu(v2);

// GPU-accelerated geospatial calculations
var locations = GetThousandsOfLocations();
var center = new GeoCoordinate(40.0, -75.0);
var distances = locations.CalculateDistancesGpu(l => l.Coordinate, center);
```

**Performance Tips:**
- GPU acceleration provides the most benefit with datasets of 10,000+ elements
- For smaller datasets (<1,000 elements), CPU methods are faster due to GPU initialization overhead
- GPU methods automatically detect and use available hardware (CUDA, OpenCL, or CPU fallback)
- All GPU methods are fully tested and produce identical results to CPU versions

For full GPU documentation and examples, see the [GPU package README](./Mavusi.Linq.DataScience.GpuBound/README.md).

## 🏗️ Architecture

The library is split into two packages:

| Package | Purpose | Dependencies | Best For |
|---------|---------|--------------|----------|
| **Mavusi.Linq.DataScience** | CPU-optimized core library | None | Small-medium datasets, general use |
| **Mavusi.Linq.DataScience.GpuBound** | GPU-accelerated extensions | ILGPU | Large datasets (10K+), batch processing |

This separation allows you to:
- ✅ Use the core library without ILGPU dependencies
- ✅ Add GPU acceleration only when needed
- ✅ Keep your application lightweight
- ✅ Deploy to environments without GPU support

## 📋 Requirements

### Core Package
- .NET 8.0, 9.0, or 10.0
- No external dependencies

### GPU Package (Optional)
- .NET 8.0, 9.0, or 10.0
- ILGPU 1.5.3+
- GPU hardware recommended (CUDA/OpenCL) but not required (CPU fallback available)

## 📊 Performance Comparison

| Dataset Size | CPU Time | GPU Time | Speedup |
|-------------|----------|----------|---------|
| 1,000       | ~1ms     | ~2ms     | 0.5x (CPU better) |
| 10,000      | ~10ms    | ~3ms     | 3.3x |
| 100,000     | ~100ms   | ~5ms     | 20x |
| 1,000,000   | ~1,000ms | ~15ms    | 66x |

*Benchmarks performed on NVIDIA RTX 3080. Use CPU methods for small datasets, GPU methods for large datasets.*

## 🧪 Testing

Both packages are extensively tested with 285+ comprehensive tests ensuring:
- ✅ Correct statistical calculations
- ✅ Edge case handling
- ✅ Identical results between CPU and GPU versions
- ✅ Hardware compatibility

## 📚 Additional Resources

- [GPU Package Documentation](./Mavusi.Linq.DataScience.GpuBound/README.md)
- [GitHub Repository](https://github.com/mavusi/Mavusi.Linq.DataScience)
- [NuGet Package - Core](https://www.nuget.org/packages/Mavusi.Linq.DataScience)
- [NuGet Package - GPU](https://www.nuget.org/packages/Mavusi.Linq.DataScience.GpuBound)

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

MIT License - see [LICENSE](LICENSE) for details.

## 🙏 Acknowledgments

GPU acceleration powered by [ILGPU](https://github.com/m4rs-mt/ILGPU).
