# GPU-Accelerated Correlation Extensions

This folder contains GPU-accelerated versions of correlation and covariance calculations using ILGPU.

## Features

- **GPU-Accelerated Calculations**: Leverages ILGPU to perform parallel computations on CUDA, OpenCL, or CPU accelerators
- **Automatic Device Selection**: Automatically selects the best available GPU (CUDA → CPU → OpenCL)
- **Lazy Initialization**: GPU context is initialized only when first used
- **Clear Method Naming**: All methods end with `Gpu` suffix (e.g., `CorrelationGpu`, `CovarianceGpu`) to distinguish them from CPU-bound versions

## Usage

```csharp
using Mavusi.Linq.DataScience.GpuBound;

// Calculate correlation using GPU acceleration
var x = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
var y = new[] { 2.0, 4.0, 6.0, 8.0, 10.0 };

double correlation = x.CorrelationGpu(y); // Returns 1.0

// Calculate covariance using GPU acceleration
double covariance = x.CovarianceGpu(y);

// Use with selectors
var data = new[]
{
    new { Height = 160.0, Weight = 60.0 },
    new { Height = 170.0, Weight = 70.0 },
    new { Height = 180.0, Weight = 80.0 }
};

double heightWeightCorrelation = data.CorrelationGpu(d => d.Height, d => d.Weight);
```

## Performance Considerations

- **Best for Large Datasets**: GPU acceleration provides significant performance benefits for large datasets (typically > 1000 elements)
- **Small Datasets**: For small datasets, the CPU-bound version in the parent namespace may be faster due to GPU initialization overhead
- **Resource Management**: The GPU context is created once and reused. Call `CorrelationExtensions.DisposeGpuContext()` when done to free resources

## Implementation Details

The GPU-accelerated implementation uses ILGPU kernels to perform:

1. **Mean Calculation**: Parallel sum reduction on GPU
2. **Correlation Components**: Parallel calculation of covariance and variance components
3. **Covariance**: Parallel calculation of covariance sum

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
