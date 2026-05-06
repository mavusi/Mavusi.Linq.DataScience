using ILGPU;
using ILGPU.Runtime;
using Mavusi.Linq.DataScience.Models;

namespace Mavusi.Linq.DataScience.GpuBound;

/// <summary>
/// GPU-accelerated time series operations using ILGPU.
/// </summary>
public static class TimeSeriesExtensions
{
    private static (Context Context, Accelerator Accelerator) GpuContext => 
        GpuContextBase.FastGpuContext;

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

        var values = points.Select(p => (float)p.Value).ToArray();
        var results = new float[values.Length - 1];

        using var deviceValues = accelerator.Allocate1D(values);
        using var deviceResults = accelerator.Allocate1D<float>(results.Length);

        var differenceKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<float>, ArrayView<float>>(DifferenceKernel);

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

        var values = points.Select(p => (float)p.Value).ToArray();
        var results = new float[values.Length - 1];

        using var deviceValues = accelerator.Allocate1D(values);
        using var deviceResults = accelerator.Allocate1D<float>(results.Length);

        var percentChangeKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<float>, ArrayView<float>>(PercentageChangeKernel);

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
    /// Note: Due to the sequential nature of cumulative sum, this uses CPU for calculation.
    /// </summary>
    public static IEnumerable<TimeSeriesPoint<double>> CumulativeSumGpu(
        this IEnumerable<TimeSeriesPoint<double>> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var points = source.ToArray();
        if (points.Length == 0) yield break;

        // Cumulative sum is inherently sequential, CPU is more efficient for this operation
        float sum = 0;
        for (int i = 0; i < points.Length; i++)
        {
            sum += (float)points[i].Value;
            yield return new TimeSeriesPoint<double>(points[i].Timestamp, sum);
        }
    }

    /// <summary>
    /// Calculates exponentially weighted moving average using GPU acceleration.
    /// Note: Due to the sequential dependency, this uses CPU for calculation.
    /// </summary>
    public static IEnumerable<TimeSeriesPoint<double>> ExponentialMovingAverageGpu(
        this IEnumerable<TimeSeriesPoint<double>> source,
        float alpha)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (alpha <= 0 || alpha > 1) throw new ArgumentException("Alpha must be between 0 and 1", nameof(alpha));

        var points = source.ToArray();
        if (points.Length == 0) yield break;

        // EMA is inherently sequential with each value depending on the previous
        float ema = (float)points[0].Value;
        yield return new TimeSeriesPoint<double>(points[0].Timestamp, ema);

        for (int i = 1; i < points.Length; i++)
        {
            ema = alpha * (float)points[i].Value + (1 - alpha) * ema;
            yield return new TimeSeriesPoint<double>(points[i].Timestamp, ema);
        }
    }

    /// <summary>
    /// Detects outliers using z-score method with GPU acceleration.
    /// </summary>
    public static IEnumerable<TimeSeriesPoint<double>> DetectOutliersGpu(
        this IEnumerable<TimeSeriesPoint<double>> source,
        float threshold = 3.0f)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (threshold <= 0) throw new ArgumentException("Threshold must be positive", nameof(threshold));

        var points = source.ToArray();
        if (points.Length == 0) yield break;

        var (context, accelerator) = GpuContext;

        // Use float precision throughout for optimal 32-bit performance
        var values = points.Select(p => (float)p.Value).ToArray();

        // Calculate mean and variance in a single pass for consistency
        var (mean, variance) = StatisticalExtensions.CalculateMeanAndVarianceGpuOptimized(accelerator, values, false);
        var stdDev = MathF.Sqrt(variance);

        if (stdDev == 0) yield break;

        foreach (var point in points)
        {
            var zScore = MathF.Abs(((float)point.Value - mean) / stdDev);
            if (zScore >= threshold)
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
        float baseValue)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (baseValue == 0) throw new ArgumentException("Base value cannot be zero", nameof(baseValue));

        var points = source.ToArray();
        if (points.Length == 0) yield break;

        var (context, accelerator) = GpuContext;

        var values = points.Select(p => (float)p.Value).ToArray();
        var results = new float[values.Length];

        using var deviceValues = accelerator.Allocate1D(values);
        using var deviceResults = accelerator.Allocate1D<float>(results.Length);

        var returnsKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<float>, float, ArrayView<float>>(ReturnsKernel);

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
        ArrayView<float> input,
        ArrayView<float> output)
    {
        if (index < output.Length)
        {
            output[index] = input[index + 1] - input[index];
        }
    }

    private static void PercentageChangeKernel(
        Index1D index,
        ArrayView<float> input,
        ArrayView<float> output)
    {
        if (index < output.Length)
        {
            var previous = input[index];
            if (previous != 0)
            {
                output[index] = ((input[index + 1] - previous) / previous) * 100.0f;
            }
            else
            {
                output[index] = 0.0f;
            }
        }
    }

    private static void ReturnsKernel(
        Index1D index,
        ArrayView<float> input,
        float baseValue,
        ArrayView<float> output)
    {
        if (index < output.Length)
        {
            output[index] = ((input[index] - baseValue) / baseValue) * 100.0f;
        }
    }
}
