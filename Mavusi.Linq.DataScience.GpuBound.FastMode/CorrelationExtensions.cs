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
    public static float CorrelationGpu(this IEnumerable<float> sourceX, IEnumerable<float> sourceY)
    {
        if (sourceX == null) throw new ArgumentNullException(nameof(sourceX));
        if (sourceY == null) throw new ArgumentNullException(nameof(sourceY));

        var x = sourceX.ToArray();
        var y = sourceY.ToArray();

        if (x.Length == 0) throw new InvalidOperationException("Source X contains no elements");
        if (y.Length == 0) throw new InvalidOperationException("Source Y contains no elements");
        if (x.Length != y.Length) throw new ArgumentException("Sequences must have the same length");

        var (context, accelerator) = GpuContext;

        // Calculate mean and correlation components in a single pass
        var (meanX, meanY, covariance, stdX, stdY) = CalculateCorrelationGpu(accelerator, x, y);

        if (stdX == 0 || stdY == 0) return 0;

        return covariance / (stdX * stdY);
    }

    /// <summary>
    /// Calculates the Pearson correlation coefficient between two sequences using selectors and GPU acceleration.
    /// </summary>
    public static float CorrelationGpu<T>(this IEnumerable<T> source,
        Func<T, float> selectorX,
        Func<T, float> selectorY)
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
    public static float CovarianceGpu(this IEnumerable<float> sourceX, IEnumerable<float> sourceY)
    {
        if (sourceX == null) throw new ArgumentNullException(nameof(sourceX));
        if (sourceY == null) throw new ArgumentNullException(nameof(sourceY));

        var x = sourceX.ToArray();
        var y = sourceY.ToArray();

        if (x.Length == 0) throw new InvalidOperationException("Source X contains no elements");
        if (y.Length == 0) throw new InvalidOperationException("Source Y contains no elements");
        if (x.Length != y.Length) throw new ArgumentException("Sequences must have the same length");

        var (context, accelerator) = GpuContext;

        var covariance = CalculateCovarianceGpu(accelerator, x, y);

        return covariance / x.Length;
    }

    /// <summary>
    /// Calculates the covariance between two sequences using selectors and GPU acceleration.
    /// </summary>
    public static float CovarianceGpu<T>(this IEnumerable<T> source,
        Func<T, float> selectorX,
        Func<T, float> selectorY)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selectorX == null) throw new ArgumentNullException(nameof(selectorX));
        if (selectorY == null) throw new ArgumentNullException(nameof(selectorY));

        var values = source.ToList();
        var x = values.Select(selectorX);
        var y = values.Select(selectorY);

        return x.CovarianceGpu(y);
    }

    private static (float meanX, float meanY, float covariance, float stdX, float stdY) CalculateCorrelationGpu(
        Accelerator accelerator, float[] x, float[] y)
    {
        using var deviceX = accelerator.Allocate1D(x);
        using var deviceY = accelerator.Allocate1D(y);
        using var deviceResults = accelerator.Allocate1D<float>(5);

        deviceResults.MemSetToZero();

        var kernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<float>, ArrayView<float>, int, ArrayView<float>>(
            CorrelationAllInOneKernel);

        kernel(x.Length, deviceX.View, deviceY.View, x.Length, deviceResults.View);
        accelerator.Synchronize();

        var results = deviceResults.GetAsArray1D();
        var n = x.Length;
        var sumX = results[0];
        var sumY = results[1];
        var sumXY = results[2];
        var sumX2 = results[3];
        var sumY2 = results[4];

        var meanX = sumX / n;
        var meanY = sumY / n;

        // Computational formula: cov(X,Y) = E[XY] - E[X]E[Y]
        var covariance = (sumXY / n) - (meanX * meanY);

        // Computational formula: var(X) = E[X²] - E[X]²
        var varianceX = (sumX2 / n) - (meanX * meanX);
        var varianceY = (sumY2 / n) - (meanY * meanY);

        var stdX = MathF.Sqrt(varianceX);
        var stdY = MathF.Sqrt(varianceY);

        return (meanX, meanY, covariance, stdX, stdY);
    }

    private static float CalculateCovarianceGpu(Accelerator accelerator, float[] x, float[] y)
    {
        using var deviceX = accelerator.Allocate1D(x);
        using var deviceY = accelerator.Allocate1D(y);
        using var deviceResults = accelerator.Allocate1D<float>(3);

        deviceResults.MemSetToZero();

        var kernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<float>, ArrayView<float>, int, ArrayView<float>>(
            CovarianceAllInOneKernel);

        kernel(x.Length, deviceX.View, deviceY.View, x.Length, deviceResults.View);
        accelerator.Synchronize();

        var results = deviceResults.GetAsArray1D();
        var n = x.Length;
        var sumX = results[0];
        var sumY = results[1];
        var sumXY = results[2];

        var meanX = sumX / n;
        var meanY = sumY / n;

        // Computational formula: cov(X,Y) = E[XY] - E[X]E[Y]
        var covariance = (sumXY / n) - (meanX * meanY);

        return covariance * n;
    }

    // GPU Kernel: Calculate everything in one pass (mean + correlation components)
    private static void CorrelationAllInOneKernel(
        Index1D index,
        ArrayView<float> x,
        ArrayView<float> y,
        int n,
        ArrayView<float> results)
    {
        var sumX = 0.0f;
        var sumY = 0.0f;
        var sumXY = 0.0f;
        var sumX2 = 0.0f;
        var sumY2 = 0.0f;

        for (int i = index; i < n; i += Grid.DimX * Group.DimX)
        {
            var xVal = x[i];
            var yVal = y[i];

            sumX += xVal;
            sumY += yVal;
            sumXY += xVal * yVal;
            sumX2 += xVal * xVal;
            sumY2 += yVal * yVal;
        }

        Atomic.Add(ref results[0], sumX);
        Atomic.Add(ref results[1], sumY);
        Atomic.Add(ref results[2], sumXY);
        Atomic.Add(ref results[3], sumX2);
        Atomic.Add(ref results[4], sumY2);
    }

    // GPU Kernel: Calculate covariance components in one pass
    private static void CovarianceAllInOneKernel(
        Index1D index,
        ArrayView<float> x,
        ArrayView<float> y,
        int n,
        ArrayView<float> results)
    {
        var sumX = 0.0f;
        var sumY = 0.0f;
        var sumXY = 0.0f;

        for (int i = index; i < n; i += Grid.DimX * Group.DimX)
        {
            var xVal = x[i];
            var yVal = y[i];

            sumX += xVal;
            sumY += yVal;
            sumXY += xVal * yVal;
        }

        Atomic.Add(ref results[0], sumX);
        Atomic.Add(ref results[1], sumY);
        Atomic.Add(ref results[2], sumXY);
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
