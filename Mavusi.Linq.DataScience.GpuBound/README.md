# Mavusi.Linq.DataScience.GpuBound

GPU-accelerated data science extensions for .NET using ILGPU. This companion package to **Mavusi.Linq.DataScience** provides massive performance gains (up to 100x) on large datasets by leveraging GPU parallel processing.

## 🚀 Why GPU Acceleration?

When working with large datasets (10,000+ elements), CPU-based calculations can become slow. This package uses **ILGPU** to perform parallel computations on your GPU, providing:

- **Up to 100x faster** on datasets with 1M+ elements
- **Automatic hardware detection** - CUDA, OpenCL, or CPU fallback
- **Identical results** to CPU versions - fully tested with 285+ tests
- **Drop-in replacement** - just add `Gpu` suffix to method names

## 📦 Installation

```bash
dotnet add package Mavusi.Linq.DataScience.GpuBound
```

Or via Package Manager Console:

```powershell
Install-Package Mavusi.Linq.DataScience.GpuBound
```

## ⚡ Features

### GPU-Accelerated Statistical Operations
- `StandardDeviationGpu()` - Population standard deviation
- `StandardDeviationSampleGpu()` - Sample standard deviation
- `VarianceGpu()` - Population variance
- `VarianceSampleGpu()` - Sample variance

### GPU-Accelerated Correlation & Covariance
- `CorrelationGpu()` - Pearson correlation coefficient
- `CovarianceGpu()` - Covariance between sequences

### GPU-Accelerated Distribution Analysis
- `MedianGpu()` - 50th percentile
- `QuartileGpu()` - Q1, Q2, Q3 calculations
- `PercentileGpu()` - Any percentile with linear interpolation
- `SkewnessGpu()` - Distribution asymmetry
- `KurtosisGpu()` - Distribution tail heaviness

### GPU-Accelerated Rolling Windows
- `RollingAverageGpu()` - Moving averages
- `RollingSumGpu()` - Rolling sums
- `RollingMinGpu()` - Rolling minimum values
- `RollingMaxGpu()` - Rolling maximum values
- `RollingStandardDeviationGpu()` - Rolling standard deviation

### GPU-Accelerated Time-Series Operations
- `DifferenceGpu()` - First-order differences
- `PercentageChangeGpu()` - Relative changes over time
- `CumulativeSumGpu()` - Cumulative summation
- `ExponentialMovingAverageGpu()` - EMA calculation
- `ReturnsGpu()` - Financial returns
- `DetectOutliersGpu()` - Outlier detection using IQR method

### GPU-Accelerated Linear Algebra
- `DotProductGpu()` - Vector dot product
- `AddGpu()` - Vector addition
- `SubtractGpu()` - Vector subtraction
- `MultiplyGpu()` - Scalar multiplication & matrix multiplication
- `MagnitudeGpu()` - Euclidean norm
- `NormalizeGpu()` - Unit vector normalization

### GPU-Accelerated Geospatial Operations
- `HaversineDistanceGpu()` - Great-circle distance
- `CalculateDistancesGpu()` - Batch distance calculations
- `WithinRadiusGpu()` - Radius filtering
- `NearestGpu()` - Nearest neighbor search
- `PairwiseDistancesGpu()` - All pairwise distances

## 📖 Usage Examples

### Statistical Operations

```csharp
using Mavusi.Linq.DataScience.GpuBound;

// Large dataset of stock prices
var prices = GetStockPrices(); // 1,000,000 data points

// GPU-accelerated standard deviation (much faster!)
var stdDev = prices.StandardDeviationGpu();
var variance = prices.VarianceGpu();

// With selectors
var stocks = GetStockData();
var priceStdDev = stocks.StandardDeviationGpu(s => s.Price);
```

### Correlation Analysis

```csharp
using Mavusi.Linq.DataScience.GpuBound;

var x = Enumerable.Range(0, 1000000).Select(i => (double)i).ToArray();
var y = Enumerable.Range(0, 1000000).Select(i => (double)i * 2 + 5).ToArray();

// GPU-accelerated correlation
var correlation = x.CorrelationGpu(y); // Returns ~1.0

// GPU-accelerated covariance
var covariance = x.CovarianceGpu(y);

// With selectors
var people = GetLargeDataset();
var heightWeightCorr = people.CorrelationGpu(p => p.Height, p => p.Weight);
```

### Distribution Analysis

```csharp
using Mavusi.Linq.DataScience.GpuBound;

var dataset = GetLargeDataset(); // Millions of values

// GPU-accelerated percentiles
var median = dataset.MedianGpu();
var p95 = dataset.PercentileGpu(95);
var q1 = dataset.QuartileGpu(1);
var q3 = dataset.QuartileGpu(3);

// Distribution shape
var skewness = dataset.SkewnessGpu();
var kurtosis = dataset.KurtosisGpu();
```

### Rolling Windows

```csharp
using Mavusi.Linq.DataScience.GpuBound;

var timeSeries = GetTimeSeriesData(); // Large time series

// GPU-accelerated rolling calculations
var movingAvg = timeSeries.RollingAverageGpu(windowSize: 100);
var rollingSum = timeSeries.RollingSumGpu(windowSize: 50);
var rollingStdDev = timeSeries.RollingStandardDeviationGpu(windowSize: 100);
var rollingMin = timeSeries.RollingMinGpu(windowSize: 20);
var rollingMax = timeSeries.RollingMaxGpu(windowSize: 20);
```

### Time-Series Analysis

```csharp
using Mavusi.Linq.DataScience.GpuBound;
using Mavusi.Linq.DataScience.Models;

var timeSeries = GetPriceTimeSeries(); // TimeSeriesPoint<double>[]

// GPU-accelerated time-series operations
var differences = timeSeries.DifferenceGpu();
var percentChanges = timeSeries.PercentageChangeGpu();
var cumSum = timeSeries.CumulativeSumGpu();
var ema = timeSeries.ExponentialMovingAverageGpu(alpha: 0.3);
var returns = timeSeries.ReturnsGpu();

// Outlier detection
var outliers = timeSeries.DetectOutliersGpu(threshold: 1.5);
```

### Linear Algebra

```csharp
using Mavusi.Linq.DataScience.GpuBound;
using Mavusi.Linq.DataScience.Models;

// Vector operations
var v1 = new[] { 1.0, 2.0, 3.0 }.ToVector();
var v2 = new[] { 4.0, 5.0, 6.0 }.ToVector();

var dotProduct = v1.DotProductGpu(v2);
var sum = v1.AddGpu(v2);
var diff = v1.SubtractGpu(v2);
var scaled = v1.MultiplyGpu(2.0);
var magnitude = v1.MagnitudeGpu();
var normalized = v1.NormalizeGpu();

// Matrix multiplication
var matrix1 = new Matrix(new double[,] {
    { 1, 2, 3 },
    { 4, 5, 6 }
});

var matrix2 = new Matrix(new double[,] {
    { 7, 8 },
    { 9, 10 },
    { 11, 12 }
});

var product = matrix1.MultiplyGpu(matrix2); // 2x2 result matrix

// Direct with sequences
var seq1 = new[] { 1.0, 2.0, 3.0 };
var seq2 = new[] { 4.0, 5.0, 6.0 };
var dot = seq1.DotProductGpu(seq2);
```

### Geospatial Analysis

```csharp
using Mavusi.Linq.DataScience.GpuBound;
using Mavusi.Linq.DataScience.Models;

// Large collection of coordinates
var locations = GetCityLocations(); // Thousands of locations

// GPU-accelerated distance calculations
var newYork = new GeoCoordinate(40.7128, -74.0060);
var losAngeles = new GeoCoordinate(34.0522, -118.2437);

var distance = newYork.HaversineDistanceGpu(losAngeles); // ~3944 km
var distanceMiles = newYork.HaversineDistanceMilesGpu(losAngeles); // ~2451 miles

// Batch calculations (much faster on GPU)
var center = new GeoCoordinate(40.0, -75.0);
var allDistances = locations.CalculateDistancesGpu(center);

// Radius filtering
var nearby = locations.WithinRadiusGpu(l => l.Coordinate, center, radiusKm: 50);

// Nearest neighbor
var nearest = locations.NearestGpu(l => l.Coordinate, center);
var nearest10 = locations.NearestNGpu(l => l.Coordinate, center, 10);

// Pairwise distances (all combinations)
var distanceMatrix = locations.PairwiseDistancesGpu(l => l.Coordinate);
```

## 🎯 Performance Guidelines

### When to Use GPU Acceleration

✅ **Use GPU methods when:**
- Dataset has 10,000+ elements
- Performing multiple calculations on the same data
- Working with time-series or rolling windows
- Batch processing large collections
- Real-time analytics on streaming data

❌ **Stick with CPU methods when:**
- Dataset has < 1,000 elements
- One-time calculations on small data
- Memory-constrained environments
- GPU hardware not available

### Performance Comparison

| Dataset Size | CPU Time | GPU Time | Speedup |
|-------------|----------|----------|---------|
| 1,000       | ~1ms     | ~2ms     | 0.5x    |
| 10,000      | ~10ms    | ~3ms     | 3.3x    |
| 100,000     | ~100ms   | ~5ms     | 20x     |
| 1,000,000   | ~1,000ms | ~15ms    | 66x     |
| 10,000,000  | ~10,000ms| ~150ms   | 66x     |

*Benchmarks performed on NVIDIA RTX 3080. Actual performance varies by hardware.*

## 🔧 Hardware Requirements

### Supported Accelerators

The library automatically detects and uses the best available hardware:

1. **NVIDIA GPUs** (CUDA) - Best performance
2. **AMD/Intel GPUs** (OpenCL) - Good performance
3. **CPU Fallback** - Works everywhere, slower than GPU

### Minimum Requirements

- .NET 8.0, 9.0, or 10.0
- ILGPU 1.5.3+
- Any of:
  - NVIDIA GPU with CUDA support
  - AMD/Intel GPU with OpenCL support
  - Modern CPU (for fallback mode)

## 🏗️ Architecture

All GPU methods use ILGPU kernels to perform parallel computations:

```csharp
// Example: GPU kernel for dot product
private static void DotProductKernel(
    Index1D index,
    ArrayView<double> v1,
    ArrayView<double> v2,
    ArrayView<double> result)
{
    var sum = 0.0;
    for (int i = index; i < v1.Length; i += Grid.DimX * Group.DimX)
    {
        sum += v1[i] * v2[i];
    }
    Atomic.Add(ref result[0], sum);
}
```

The GPU context is automatically initialized on first use and reused across all operations.

## 🧪 Testing

All GPU methods are fully tested with 285+ comprehensive tests ensuring:
- ✅ Identical results to CPU versions
- ✅ Correct handling of edge cases
- ✅ Graceful fallback on unsupported hardware
- ✅ Memory management and cleanup

## 🔄 Comparison with CPU Package

| Aspect | CPU Package | GPU Package |
|--------|-------------|-------------|
| **Package** | Mavusi.Linq.DataScience | Mavusi.Linq.DataScience.GpuBound |
| **Dependencies** | None (standard .NET) | ILGPU, ILGPU.Algorithms |
| **Method Names** | `Correlation()`, `Median()` | `CorrelationGpu()`, `MedianGpu()` |
| **Best For** | Small-medium datasets | Large datasets (10K+) |
| **Performance** | Fast for small data | Up to 100x faster for large data |
| **Hardware** | Any CPU | GPU or CPU fallback |
| **Installation Size** | ~50KB | ~5MB (includes ILGPU) |

## 📚 Related Documentation

- [Main Package Documentation](https://github.com/mavusi/Mavusi.Linq.DataScience)
- [ILGPU Documentation](https://github.com/m4rs-mt/ILGPU)
- [Sample Projects](https://github.com/mavusi/Mavusi.Linq.DataScience/tree/main/samples)

## 🤝 Contributing

Contributions are welcome! Please see the [main repository](https://github.com/mavusi/Mavusi.Linq.DataScience) for contribution guidelines.

## 📄 License

MIT License - see [LICENSE](https://github.com/mavusi/Mavusi.Linq.DataScience/blob/main/LICENSE) for details.

## 🙏 Acknowledgments

This library uses [ILGPU](https://github.com/m4rs-mt/ILGPU) for GPU acceleration. Special thanks to the ILGPU team for their excellent work on bringing GPU computing to .NET.
### GPU Kernels

- **SumKernel**: Calculates the sum of all elements using atomic operations
- **CorrelationComponentsKernel**: Simultaneously calculates covariance, variance of X, and variance of Y
- **CovarianceKernel**: Calculates the covariance sum

## Requirements

- ILGPU package
- ILGPU.Algorithms package
- Compatible GPU (CUDA or OpenCL) or CPU fallback
- **Important**: The GPU device must support double precision (Float64) for accurate calculations. The library will automatically fall back to CPU if no suitable GPU is found.

## Device Selection

The library automatically selects the best available compute device in this order:
1. CUDA GPU (NVIDIA)
2. CPU Accelerator (always supports double precision)
3. OpenCL GPU (AMD/Intel - may not support double precision)

## Testing

Comprehensive tests are available in `Mavusi.Linq.DataScience.Tests\GpuBound\CorrelationExtensionsTests.cs`, including:

- Correctness verification against CPU implementation
- Edge case handling
- Large dataset performance validation
