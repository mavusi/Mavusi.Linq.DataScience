using ILGPU;
using ILGPU.Runtime;

namespace Mavusi.Linq.DataScience.GpuBound;

/// <summary>
/// GPU-accelerated correlation and covariance calculations using ILGPU.
/// </summary>
public static class CorrelationExtensions
{
    private static (Context Context, Accelerator Accelerator) GpuContext => 
        GpuContextBase.GpuContext;

    /// <summary>
    /// Calculates the Pearson correlation coefficient between two sequences using GPU acceleration.
    /// Returns a value between -1 and 1, where 1 is perfect positive correlation,
    /// -1 is perfect negative correlation, and 0 is no correlation.
    /// </summary>
    public static float CorrelationGpu(this IEnumerable<double> sourceX, IEnumerable<double> sourceY)
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

        return (float)(covariance / (stdX * stdY));
    }

    /// <summary>
    /// Calculates the Pearson correlation coefficient between two sequences using selectors and GPU acceleration.
    /// </summary>
    public static float CorrelationGpu<T>(this IEnumerable<T> source,
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
    public static float CovarianceGpu(this IEnumerable<double> sourceX, IEnumerable<double> sourceY)
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

        return (float)(covariance / x.Length);
    }

    /// <summary>
    /// Calculates the covariance between two sequences using selectors and GPU acceleration.
    /// </summary>
    public static float CovarianceGpu<T>(this IEnumerable<T> source,
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
        var floatData = data.Select(d => (float)d).ToArray();
        using var deviceData = accelerator.Allocate1D(floatData);
        using var deviceResult = accelerator.Allocate1D<float>(1);

        // Initialize result to zero
        deviceResult.MemSetToZero();

        var sumKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<float>, ArrayView<float>>(SumKernel);

        sumKernel(floatData.Length, deviceData.View, deviceResult.View);
        accelerator.Synchronize();

        var sum = deviceResult.GetAsArray1D()[0];
        return sum / floatData.Length;
    }

    private static (double covariance, double stdX, double stdY) CalculateCorrelationComponentsGpu(
        Accelerator accelerator, double[] x, double[] y, double meanX, double meanY)
    {
        var floatX = x.Select(d => (float)d).ToArray();
        var floatY = y.Select(d => (float)d).ToArray();

        using var deviceX = accelerator.Allocate1D(floatX);
        using var deviceY = accelerator.Allocate1D(floatY);
        using var deviceResults = accelerator.Allocate1D<float>(3);

        // Initialize results to zero
        deviceResults.MemSetToZero();

        var correlationKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<float>, ArrayView<float>, float, float, ArrayView<float>>(
            CorrelationComponentsKernel);

        correlationKernel(floatX.Length, deviceX.View, deviceY.View, (float)meanX, (float)meanY, deviceResults.View);
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
        var floatX = x.Select(d => (float)d).ToArray();
        var floatY = y.Select(d => (float)d).ToArray();

        using var deviceX = accelerator.Allocate1D(floatX);
        using var deviceY = accelerator.Allocate1D(floatY);
        using var deviceResult = accelerator.Allocate1D<float>(1);

        // Initialize result to zero
        deviceResult.MemSetToZero();

        var covarianceKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<float>, ArrayView<float>, float, float, ArrayView<float>>(
            CovarianceKernel);

        covarianceKernel(floatX.Length, deviceX.View, deviceY.View, (float)meanX, (float)meanY, deviceResult.View);
        accelerator.Synchronize();

        return deviceResult.GetAsArray1D()[0];
    }

    // GPU Kernel: Sum all elements
    private static void SumKernel(Index1D index, ArrayView<float> input, ArrayView<float> output)
    {
        var sum = 0.0f;
        for (int i = index; i < input.Length; i += Grid.DimX * Group.DimX)
        {
            sum += input[i];
        }
        Atomic.Add(ref output[0], sum);
    }

    // GPU Kernel: Calculate covariance and variance components
    private static void CorrelationComponentsKernel(
        Index1D index,
        ArrayView<float> x,
        ArrayView<float> y,
        float meanX,
        float meanY,
        ArrayView<float> results)
    {
        var covariance = 0.0f;
        var varianceX = 0.0f;
        var varianceY = 0.0f;

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
        ArrayView<float> x,
        ArrayView<float> y,
        float meanX,
        float meanY,
        ArrayView<float> result)
    {
        var covariance = 0.0f;

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
        GpuContextBase.DisposeGpuContext();
    }
}
