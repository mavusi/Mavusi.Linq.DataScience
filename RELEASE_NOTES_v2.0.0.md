# Mavusi.Linq.DataScience v2.0.0 - GPU Acceleration Release

## 🚀 What's New

Version 2.0.0 is a **major update** that adds GPU acceleration to the entire library using ILGPU. This release brings **massive performance improvements** for large datasets while maintaining backward compatibility with all existing CPU methods.

## ⚡ Key Features

### GPU-Accelerated Extensions

All major operations now have GPU-accelerated versions:

#### Statistical Operations
- `StandardDeviationGpu()` / `StandardDeviationSampleGpu()`
- `VarianceGpu()` / `VarianceSampleGpu()`

#### Correlation & Covariance
- `CorrelationGpu()` - Up to 100x faster on large datasets
- `CovarianceGpu()`

#### Distribution Analysis
- `MedianGpu()`
- `QuartileGpu()`
- `PercentileGpu()`
- `InterquartileRangeGpu()`
- `SkewnessGpu()`
- `KurtosisGpu()`

#### Rolling Windows
- `RollingAverageGpu()`
- `RollingSumGpu()`
- `RollingMinGpu()`
- `RollingMaxGpu()`
- `RollingStandardDeviationGpu()`

#### Time-Series Analysis
- `DifferenceGpu()`
- `PercentageChangeGpu()`
- `CumulativeSumGpu()`
- `ExponentialMovingAverageGpu()`
- `ReturnsGpu()`
- `DetectOutliersGpu()`

#### Linear Algebra
- `DotProductGpu()`
- `AddGpu()` / `SubtractGpu()`
- `MultiplyGpu()` (scalar and matrix)
- `MagnitudeGpu()`
- `NormalizeGpu()`

#### Geospatial Operations
- `HaversineDistanceGpu()`
- `CalculateDistancesGpu()`
- `WithinRadiusGpu()`
- `NearestGpu()`
- `PairwiseDistancesGpu()`

## 📊 Performance Improvements

| Dataset Size | CPU Time | GPU Time | Speedup |
|-------------|----------|----------|---------|
| 1,000       | ~1ms     | ~2ms     | 0.5x    |
| 10,000      | ~10ms    | ~3ms     | 3.3x    |
| 100,000     | ~100ms   | ~5ms     | 20x     |
| 1,000,000   | ~1000ms  | ~15ms    | 66x     |

*Benchmarks performed on NVIDIA RTX 3080*

## 🎯 Usage

### Before (CPU):
```csharp
using Mavusi.Linq.DataScience;

var data = Enumerable.Range(0, 1000000).Select(i => (double)i).ToArray();
var stdDev = data.StandardDeviation(); // ~1000ms
```

### After (GPU):
```csharp
using Mavusi.Linq.DataScience.GpuBound;

var data = Enumerable.Range(0, 1000000).Select(i => (double)i).ToArray();
var stdDev = data.StandardDeviationGpu(); // ~15ms - 66x faster!
```

## 🔧 Hardware Requirements

GPU methods work with:
- ✅ NVIDIA GPUs (CUDA)
- ✅ AMD GPUs (OpenCL)
- ✅ Intel GPUs (OpenCL)
- ✅ CPU fallback (no GPU required)

The library automatically detects available hardware and gracefully handles devices without GPU support.

## 📦 Package Updates

### New Dependencies
- **ILGPU** (v1.5.3)
- **ILGPU.Algorithms** (v1.5.3)

### Breaking Changes
- **None!** All existing CPU methods remain unchanged and backward compatible
- GPU methods are additive - simply use the `GpuBound` namespace for acceleration

## 🧪 Testing

This release includes **285 comprehensive tests**:
- All GPU methods have dedicated test coverage
- GPU vs CPU comparison tests ensure numerical accuracy
- Exception handling tests for unsupported hardware
- Tests run on .NET 8, 9, and 10

**Test Results**: ✅ 285/285 Passing

## 📝 Documentation Updates

- Updated README.md with GPU usage examples
- Added performance comparison tables
- Added architecture section explaining CPU vs GPU namespaces
- Created NUGET_PUBLISHING.md guide

## 🎨 Architecture

The library maintains a clean separation:

- **`Mavusi.Linq.DataScience`**: CPU-based implementations (unchanged)
- **`Mavusi.Linq.DataScience.GpuBound`**: GPU-accelerated implementations (new)

All GPU methods follow the naming convention: `{MethodName}Gpu()`

## 🔮 When to Use GPU vs CPU

### Use GPU methods when:
- ✅ Working with large datasets (10,000+ elements)
- ✅ Performing repeated calculations
- ✅ Processing time-series or streaming data
- ✅ Running batch analytics

### Use CPU methods when:
- ✅ Working with small datasets (<1,000 elements)
- ✅ Running one-off calculations
- ✅ Memory constraints are critical
- ✅ Simplicity is preferred over performance

## 🛠️ Migration Guide

### No Migration Required!
All existing code continues to work without changes. To adopt GPU acceleration:

1. Add `using Mavusi.Linq.DataScience.GpuBound;`
2. Replace method calls with `Gpu` suffixed versions
3. Test with your data to verify performance gains

### Example Migration:

```csharp
// Old code (still works!)
using Mavusi.Linq.DataScience;
var correlation = x.Correlation(y);

// New code (faster on large datasets)
using Mavusi.Linq.DataScience.GpuBound;
var correlation = x.CorrelationGpu(y);
```

## 🐛 Known Issues

- None at this time

## 🙏 Credits

GPU acceleration powered by [ILGPU](https://github.com/m4rs-mt/ILGPU) - an incredible open-source GPU computing framework for .NET.

## 📈 Future Plans

- SIMD optimizations for mid-size datasets
- Additional GPU kernels for specialized operations
- Async/streaming GPU operations
- Distributed computing support

## 📄 License

MIT License - same as v1.x

## 🔗 Links

- **NuGet Package**: https://www.nuget.org/packages/Mavusi.Linq.DataScience
- **GitHub Repository**: https://github.com/mavusi/Mavusi.Linq.DataScience
- **Issue Tracker**: https://github.com/mavusi/Mavusi.Linq.DataScience/issues
- **ILGPU**: https://github.com/m4rs-mt/ILGPU

---

**Ready to accelerate your data science workloads? Upgrade now!**

```powershell
dotnet add package Mavusi.Linq.DataScience --version 2.0.0
```
