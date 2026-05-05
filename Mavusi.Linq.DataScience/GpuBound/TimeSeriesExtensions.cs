using ILGPU;
using ILGPU.Runtime;

namespace Mavusi.Linq.DataScience.GpuBound;

/// <summary>
/// GPU-accelerated time series operations using ILGPU.
/// </summary>
public static class TimeSeriesExtensions
{
    private static (Context Context, Accelerator Accelerator) GpuContext => 
        CorrelationExtensions.GpuContext;

    /// <summary>
    /// Calculates the difference between consecutive values (first-order differencing) using GPU acceleration.
    /// </summary>
    public static IEnumerable<TimeSeriesPoint<double>> DifferenceGpu(
        this IEnumerable<TimeSeriesPoint<double>> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var points = source.ToArray();
        if (points.Length <= 1) yield break;

        var (context, accelerator) = GpuContext;

        var values = points.Select(p => p.Value).ToArray();
        var results = new double[values.Length - 1];

        using var deviceValues = accelerator.Allocate1D(values);
        using var deviceResults = accelerator.Allocate1D<double>(results.Length);

        var differenceKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<double>, ArrayView<double>>(DifferenceKernel);

        differenceKernel(results.Length, deviceValues.View, deviceResults.View);
        accelerator.Synchronize();

        deviceResults.CopyToCPU(results);

        for (int i = 0; i < results.Length; i++)
        {
            yield return new TimeSeriesPoint<double>(points[i + 1].Timestamp, results[i]);
        }
    }

    /// <summary>
    /// Calculates the percentage change between consecutive values using GPU acceleration.
    /// </summary>
    public static IEnumerable<TimeSeriesPoint<double>> PercentageChangeGpu(
        this IEnumerable<TimeSeriesPoint<double>> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var points = source.ToArray();
        if (points.Length <= 1) yield break;

        var (context, accelerator) = GpuContext;

        var values = points.Select(p => p.Value).ToArray();
        var results = new double[values.Length - 1];

        using var deviceValues = accelerator.Allocate1D(values);
        using var deviceResults = accelerator.Allocate1D<double>(results.Length);

        var percentChangeKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<double>, ArrayView<double>>(PercentageChangeKernel);

        percentChangeKernel(results.Length, deviceValues.View, deviceResults.View);
        accelerator.Synchronize();

        deviceResults.CopyToCPU(results);

        for (int i = 0; i < results.Length; i++)
        {
            yield return new TimeSeriesPoint<double>(points[i + 1].Timestamp, results[i]);
        }
    }

    /// <summary>
    /// Calculates the cumulative sum of time series values using GPU acceleration.
    /// </summary>
    public static IEnumerable<TimeSeriesPoint<double>> CumulativeSumGpu(
        this IEnumerable<TimeSeriesPoint<double>> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var points = source.ToArray();
        if (points.Length == 0) yield break;

        var (context, accelerator) = GpuContext;

        var values = points.Select(p => p.Value).ToArray();
        var results = new double[values.Length];

        using var deviceValues = accelerator.Allocate1D(values);
        using var deviceResults = accelerator.Allocate1D<double>(results.Length);

        var cumulativeSumKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<double>, ArrayView<double>>(CumulativeSumKernel);

        cumulativeSumKernel(results.Length, deviceValues.View, deviceResults.View);
        accelerator.Synchronize();

        deviceResults.CopyToCPU(results);

        for (int i = 0; i < results.Length; i++)
        {
            yield return new TimeSeriesPoint<double>(points[i].Timestamp, results[i]);
        }
    }

    /// <summary>
    /// Calculates exponentially weighted moving average using GPU acceleration.
    /// </summary>
    public static IEnumerable<TimeSeriesPoint<double>> ExponentialMovingAverageGpu(
        this IEnumerable<TimeSeriesPoint<double>> source,
        double alpha)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (alpha <= 0 || alpha > 1) throw new ArgumentException("Alpha must be between 0 and 1", nameof(alpha));

        var points = source.ToArray();
        if (points.Length == 0) yield break;

        var (context, accelerator) = GpuContext;

        var values = points.Select(p => p.Value).ToArray();
        var results = new double[values.Length];

        using var deviceValues = accelerator.Allocate1D(values);
        using var deviceResults = accelerator.Allocate1D<double>(results.Length);

        var emaKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<double>, double, ArrayView<double>>(ExponentialMovingAverageKernel);

        emaKernel(results.Length, deviceValues.View, alpha, deviceResults.View);
        accelerator.Synchronize();

        deviceResults.CopyToCPU(results);

        for (int i = 0; i < results.Length; i++)
        {
            yield return new TimeSeriesPoint<double>(points[i].Timestamp, results[i]);
        }
    }

    /// <summary>
    /// Detects outliers using z-score method with GPU acceleration.
    /// </summary>
    public static IEnumerable<TimeSeriesPoint<double>> DetectOutliersGpu(
        this IEnumerable<TimeSeriesPoint<double>> source,
        double threshold = 3.0)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (threshold <= 0) throw new ArgumentException("Threshold must be positive", nameof(threshold));

        var points = source.ToArray();
        if (points.Length == 0) yield break;

        var values = points.Select(p => p.Value).ToArray();

        // Calculate mean and standard deviation using GPU
        var mean = values.AsEnumerable().ToArray().Average();
        var stdDev = values.StandardDeviationGpu();

        if (stdDev == 0) yield break;

        foreach (var point in points)
        {
            var zScore = Math.Abs((point.Value - mean) / stdDev);
            if (zScore > threshold)
            {
                yield return point;
            }
        }
    }

    /// <summary>
    /// Calculates returns (percentage change from a base value) using GPU acceleration.
    /// </summary>
    public static IEnumerable<TimeSeriesPoint<double>> ReturnsGpu(
        this IEnumerable<TimeSeriesPoint<double>> source,
        double baseValue)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (baseValue == 0) throw new ArgumentException("Base value cannot be zero", nameof(baseValue));

        var points = source.ToArray();
        if (points.Length == 0) yield break;

        var (context, accelerator) = GpuContext;

        var values = points.Select(p => p.Value).ToArray();
        var results = new double[values.Length];

        using var deviceValues = accelerator.Allocate1D(values);
        using var deviceResults = accelerator.Allocate1D<double>(results.Length);

        var returnsKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<double>, double, ArrayView<double>>(ReturnsKernel);

        returnsKernel(results.Length, deviceValues.View, baseValue, deviceResults.View);
        accelerator.Synchronize();

        deviceResults.CopyToCPU(results);

        for (int i = 0; i < results.Length; i++)
        {
            yield return new TimeSeriesPoint<double>(points[i].Timestamp, results[i]);
        }
    }

    // GPU Kernels

    private static void DifferenceKernel(
        Index1D index,
        ArrayView<double> input,
        ArrayView<double> output)
    {
        if (index < output.Length)
        {
            output[index] = input[index + 1] - input[index];
        }
    }

    private static void PercentageChangeKernel(
        Index1D index,
        ArrayView<double> input,
        ArrayView<double> output)
    {
        if (index < output.Length)
        {
            var previous = input[index];
            if (previous != 0)
            {
                output[index] = ((input[index + 1] - previous) / previous) * 100.0;
            }
            else
            {
                output[index] = 0.0;
            }
        }
    }

    private static void CumulativeSumKernel(
        Index1D index,
        ArrayView<double> input,
        ArrayView<double> output)
    {
        if (index < output.Length)
        {
            double sum = 0;
            for (int i = 0; i <= index; i++)
            {
                sum += input[i];
            }
            output[index] = sum;
        }
    }

    private static void ExponentialMovingAverageKernel(
        Index1D index,
        ArrayView<double> input,
        double alpha,
        ArrayView<double> output)
    {
        if (index < output.Length)
        {
            if (index == 0)
            {
                output[0] = input[0];
            }
            else
            {
                // Note: This is a simplified version that recalculates from start
                // For large datasets, consider a two-pass approach
                double ema = input[0];
                for (int i = 1; i <= index; i++)
                {
                    ema = alpha * input[i] + (1 - alpha) * ema;
                }
                output[index] = ema;
            }
        }
    }

    private static void ReturnsKernel(
        Index1D index,
        ArrayView<double> input,
        double baseValue,
        ArrayView<double> output)
    {
        if (index < output.Length)
        {
            output[index] = ((input[index] - baseValue) / baseValue) * 100.0;
        }
    }
}
