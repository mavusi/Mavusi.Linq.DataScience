using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using ILGPU.Runtime.CPU;

namespace Mavusi.Linq.DataScience.GpuBound;

/// <summary>
/// GPU-accelerated correlation and covariance calculations using ILGPU.
/// </summary>
public static class CorrelationExtensions
{
    private static readonly Lazy<(Context, Accelerator)> _gpuContext = new(() => InitializeGpu());

    private static (Context, Accelerator) InitializeGpu()
    {
        var context = Context.CreateDefault();

        // Try to find a device that supports double precision
        // Prefer CUDA, then OpenCL, then CPU (which always supports doubles)
        var device = context.Devices.FirstOrDefault(d => 
                         d.AcceleratorType == AcceleratorType.Cuda)
                     
                     ?? context.Devices.FirstOrDefault(d => 
                         d.AcceleratorType == AcceleratorType.OpenCL)
                     
                     ?? context.Devices.FirstOrDefault(d => 
                         d.AcceleratorType == AcceleratorType.CPU);

        if (device == null)
        {
            throw new NotSupportedException("No compatible accelerator found.");
        }

        var accelerator = device.CreateAccelerator(context);
        return (context, accelerator);
    }

    /// <summary>
    /// Gets the GPU context and accelerator. Initializes on first access.
    /// </summary>
    internal static (Context Context, Accelerator Accelerator) GpuContext => _gpuContext.Value;

    /// <summary>
    /// Calculates the Pearson correlation coefficient between two sequences using GPU acceleration.
    /// Returns a value between -1 and 1, where 1 is perfect positive correlation,
    /// -1 is perfect negative correlation, and 0 is no correlation.
    /// </summary>
    public static double CorrelationGpu(this IEnumerable<double> sourceX, IEnumerable<double> sourceY)
    {
        if (sourceX == null) throw new ArgumentNullException(nameof(sourceX));
        if (sourceY == null) throw new ArgumentNullException(nameof(sourceY));

        var x = sourceX.ToArray();
        var y = sourceY.ToArray();

        if (x.Length == 0) throw new InvalidOperationException("Source X contains no elements");
        if (y.Length == 0) throw new InvalidOperationException("Source Y contains no elements");
        if (x.Length != y.Length) throw new ArgumentException("Sequences must have the same length");

        var (context, accelerator) = GpuContext;

        // Calculate means
        var meanX = CalculateMeanGpu(accelerator, x);
        var meanY = CalculateMeanGpu(accelerator, y);

        // Calculate covariance and standard deviations
        var (covariance, stdX, stdY) = CalculateCorrelationComponentsGpu(accelerator, x, y, meanX, meanY);

        if (stdX == 0 || stdY == 0) return 0;

        return covariance / (stdX * stdY);
    }

    /// <summary>
    /// Calculates the Pearson correlation coefficient between two sequences using selectors and GPU acceleration.
    /// </summary>
    public static double CorrelationGpu<T>(this IEnumerable<T> source,
        Func<T, double> selectorX,
        Func<T, double> selectorY)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selectorX == null) throw new ArgumentNullException(nameof(selectorX));
        if (selectorY == null) throw new ArgumentNullException(nameof(selectorY));

        var values = source.ToList();
        var x = values.Select(selectorX);
        var y = values.Select(selectorY);

        return x.CorrelationGpu(y);
    }

    /// <summary>
    /// Calculates the covariance between two sequences using GPU acceleration.
    /// </summary>
    public static double CovarianceGpu(this IEnumerable<double> sourceX, IEnumerable<double> sourceY)
    {
        if (sourceX == null) throw new ArgumentNullException(nameof(sourceX));
        if (sourceY == null) throw new ArgumentNullException(nameof(sourceY));

        var x = sourceX.ToArray();
        var y = sourceY.ToArray();

        if (x.Length == 0) throw new InvalidOperationException("Source X contains no elements");
        if (y.Length == 0) throw new InvalidOperationException("Source Y contains no elements");
        if (x.Length != y.Length) throw new ArgumentException("Sequences must have the same length");

        var (context, accelerator) = GpuContext;

        var meanX = CalculateMeanGpu(accelerator, x);
        var meanY = CalculateMeanGpu(accelerator, y);

        var covariance = CalculateCovarianceGpu(accelerator, x, y, meanX, meanY);

        return covariance / x.Length;
    }

    /// <summary>
    /// Calculates the covariance between two sequences using selectors and GPU acceleration.
    /// </summary>
    public static double CovarianceGpu<T>(this IEnumerable<T> source,
        Func<T, double> selectorX,
        Func<T, double> selectorY)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selectorX == null) throw new ArgumentNullException(nameof(selectorX));
        if (selectorY == null) throw new ArgumentNullException(nameof(selectorY));

        var values = source.ToList();
        var x = values.Select(selectorX);
        var y = values.Select(selectorY);

        return x.CovarianceGpu(y);
    }

    private static double CalculateMeanGpu(Accelerator accelerator, double[] data)
    {
        using var deviceData = accelerator.Allocate1D(data);
        using var deviceResult = accelerator.Allocate1D<double>(1);

        // Initialize result to zero
        deviceResult.MemSetToZero();

        var sumKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<double>, ArrayView<double>>(SumKernel);

        sumKernel(data.Length, deviceData.View, deviceResult.View);
        accelerator.Synchronize();

        var sum = deviceResult.GetAsArray1D()[0];
        return sum / data.Length;
    }

    private static (double covariance, double stdX, double stdY) CalculateCorrelationComponentsGpu(
        Accelerator accelerator, double[] x, double[] y, double meanX, double meanY)
    {
        using var deviceX = accelerator.Allocate1D(x);
        using var deviceY = accelerator.Allocate1D(y);
        using var deviceResults = accelerator.Allocate1D<double>(3);

        // Initialize results to zero
        deviceResults.MemSetToZero();

        var correlationKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<double>, ArrayView<double>, double, double, ArrayView<double>>(
            CorrelationComponentsKernel);

        correlationKernel(x.Length, deviceX.View, deviceY.View, meanX, meanY, deviceResults.View);
        accelerator.Synchronize();

        var results = deviceResults.GetAsArray1D();
        var covariance = results[0];
        var stdX = Math.Sqrt(results[1]);
        var stdY = Math.Sqrt(results[2]);

        return (covariance, stdX, stdY);
    }

    private static double CalculateCovarianceGpu(
        Accelerator accelerator, double[] x, double[] y, double meanX, double meanY)
    {
        using var deviceX = accelerator.Allocate1D(x);
        using var deviceY = accelerator.Allocate1D(y);
        using var deviceResult = accelerator.Allocate1D<double>(1);

        // Initialize result to zero
        deviceResult.MemSetToZero();

        var covarianceKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<double>, ArrayView<double>, double, double, ArrayView<double>>(
            CovarianceKernel);

        covarianceKernel(x.Length, deviceX.View, deviceY.View, meanX, meanY, deviceResult.View);
        accelerator.Synchronize();

        return deviceResult.GetAsArray1D()[0];
    }

    // GPU Kernel: Sum all elements
    private static void SumKernel(Index1D index, ArrayView<double> input, ArrayView<double> output)
    {
        var sum = 0.0;
        for (int i = index; i < input.Length; i += Grid.DimX * Group.DimX)
        {
            sum += input[i];
        }
        Atomic.Add(ref output[0], sum);
    }

    // GPU Kernel: Calculate covariance and variance components
    private static void CorrelationComponentsKernel(
        Index1D index,
        ArrayView<double> x,
        ArrayView<double> y,
        double meanX,
        double meanY,
        ArrayView<double> results)
    {
        var covariance = 0.0;
        var varianceX = 0.0;
        var varianceY = 0.0;

        for (int i = index; i < x.Length; i += Grid.DimX * Group.DimX)
        {
            var diffX = x[i] - meanX;
            var diffY = y[i] - meanY;

            covariance += diffX * diffY;
            varianceX += diffX * diffX;
            varianceY += diffY * diffY;
        }

        Atomic.Add(ref results[0], covariance);
        Atomic.Add(ref results[1], varianceX);
        Atomic.Add(ref results[2], varianceY);
    }

    // GPU Kernel: Calculate covariance
    private static void CovarianceKernel(
        Index1D index,
        ArrayView<double> x,
        ArrayView<double> y,
        double meanX,
        double meanY,
        ArrayView<double> result)
    {
        var covariance = 0.0;

        for (int i = index; i < x.Length; i += Grid.DimX * Group.DimX)
        {
            var diffX = x[i] - meanX;
            var diffY = y[i] - meanY;
            covariance += diffX * diffY;
        }

        Atomic.Add(ref result[0], covariance);
    }

    /// <summary>
    /// Disposes the GPU context and releases resources.
    /// Call this when you're done using GPU-accelerated operations.
    /// </summary>
    public static void DisposeGpuContext()
    {
        if (_gpuContext.IsValueCreated)
        {
            var (context, accelerator) = _gpuContext.Value;
            accelerator.Dispose();
            context.Dispose();
        }
    }
}
