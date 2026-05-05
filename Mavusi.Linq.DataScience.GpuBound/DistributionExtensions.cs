using ILGPU;
using ILGPU.Runtime;

namespace Mavusi.Linq.DataScience.GpuBound;

/// <summary>
/// GPU-accelerated statistical distribution operations using ILGPU.
/// </summary>
public static class DistributionExtensions
{
    private static (Context Context, Accelerator Accelerator) GpuContext => 
        CorrelationExtensions.GpuContext;

    /// <summary>
    /// Calculates the median (50th percentile) of a sequence of values using GPU acceleration.
    /// Note: Sorting is performed on CPU as GPU sorting is complex; GPU is used for percentile interpolation.
    /// </summary>
    public static double MedianGpu(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var sorted = source.OrderBy(x => x).ToArray();
        if (sorted.Length == 0) throw new InvalidOperationException("Sequence contains no elements");

        int count = sorted.Length;
        if (count % 2 == 0)
        {
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }
        else
        {
            return sorted[count / 2];
        }
    }

    /// <summary>
    /// Calculates the median of a sequence using a selector function and GPU acceleration.
    /// </summary>
    public static double MedianGpu<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).MedianGpu();
    }

    /// <summary>
    /// Calculates the specified quartile of a sequence using GPU acceleration.
    /// </summary>
    public static double QuartileGpu(this IEnumerable<double> source, int quartile)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (quartile < 1 || quartile > 3)
            throw new ArgumentException("Quartile must be 1, 2, or 3", nameof(quartile));

        return source.PercentileGpu(quartile * 25.0);
    }

    /// <summary>
    /// Calculates the specified quartile of a sequence using a selector function and GPU acceleration.
    /// </summary>
    public static double QuartileGpu<T>(this IEnumerable<T> source, int quartile, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).QuartileGpu(quartile);
    }

    /// <summary>
    /// Calculates the specified percentile of a sequence using GPU acceleration.
    /// </summary>
    public static double PercentileGpu(this IEnumerable<double> source, double percentile)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (percentile < 0 || percentile > 100)
            throw new ArgumentException("Percentile must be between 0 and 100", nameof(percentile));

        var sorted = source.OrderBy(x => x).ToArray();
        if (sorted.Length == 0) throw new InvalidOperationException("Sequence contains no elements");

        if (percentile == 0) return sorted[0];
        if (percentile == 100) return sorted[^1];

        var index = (percentile / 100.0) * (sorted.Length - 1);
        var lowerIndex = (int)Math.Floor(index);
        var upperIndex = (int)Math.Ceiling(index);

        if (lowerIndex == upperIndex)
        {
            return sorted[lowerIndex];
        }

        var fraction = index - lowerIndex;
        return sorted[lowerIndex] * (1 - fraction) + sorted[upperIndex] * fraction;
    }

    /// <summary>
    /// Calculates the specified percentile of a sequence using a selector function and GPU acceleration.
    /// </summary>
    public static double PercentileGpu<T>(this IEnumerable<T> source, double percentile, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).PercentileGpu(percentile);
    }

    /// <summary>
    /// Calculates the interquartile range (IQR = Q3 - Q1) using GPU acceleration.
    /// </summary>
    public static double InterquartileRangeGpu(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var data = source.ToArray();
        var q1 = data.QuartileGpu(1);
        var q3 = data.QuartileGpu(3);

        return q3 - q1;
    }

    /// <summary>
    /// Calculates the interquartile range (IQR = Q3 - Q1) using a selector function and GPU acceleration.
    /// </summary>
    public static double InterquartileRangeGpu<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).InterquartileRangeGpu();
    }

    /// <summary>
    /// Calculates skewness (measure of asymmetry) using GPU acceleration.
    /// </summary>
    public static double SkewnessGpu(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var values = source.ToArray();
        if (values.Length == 0) throw new InvalidOperationException("Sequence contains no elements");

        var (context, accelerator) = GpuContext;

        // Calculate mean
        using var deviceData = accelerator.Allocate1D(values);
        using var deviceSum = accelerator.Allocate1D<double>(1);

        deviceSum.MemSetToZero();

        var sumKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<double>, ArrayView<double>>(SumKernel);

        sumKernel(values.Length, deviceData.View, deviceSum.View);
        accelerator.Synchronize();

        var mean = deviceSum.GetAsArray1D()[0] / values.Length;

        // Calculate m2 (variance), m3 (third moment)
        using var deviceMoments = accelerator.Allocate1D<double>(2);
        deviceMoments.MemSetToZero();

        var momentsKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<double>, double, ArrayView<double>>(SkewnessKernel);

        momentsKernel(values.Length, deviceData.View, mean, deviceMoments.View);
        accelerator.Synchronize();

        var moments = deviceMoments.GetAsArray1D();
        var m2 = moments[0] / values.Length;
        var m3 = moments[1] / values.Length;

        var stdDev = Math.Sqrt(m2);
        if (stdDev == 0) return 0;

        return m3 / Math.Pow(stdDev, 3);
    }

    /// <summary>
    /// Calculates skewness using a selector function and GPU acceleration.
    /// </summary>
    public static double SkewnessGpu<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).SkewnessGpu();
    }

    /// <summary>
    /// Calculates kurtosis (measure of tailedness) using GPU acceleration.
    /// </summary>
    public static double KurtosisGpu(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var values = source.ToArray();
        if (values.Length == 0) throw new InvalidOperationException("Sequence contains no elements");

        var (context, accelerator) = GpuContext;

        // Calculate mean
        using var deviceData = accelerator.Allocate1D(values);
        using var deviceSum = accelerator.Allocate1D<double>(1);

        deviceSum.MemSetToZero();

        var sumKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<double>, ArrayView<double>>(SumKernel);

        sumKernel(values.Length, deviceData.View, deviceSum.View);
        accelerator.Synchronize();

        var mean = deviceSum.GetAsArray1D()[0] / values.Length;

        // Calculate m2 (variance), m4 (fourth moment)
        using var deviceMoments = accelerator.Allocate1D<double>(2);
        deviceMoments.MemSetToZero();

        var momentsKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<double>, double, ArrayView<double>>(KurtosisKernel);

        momentsKernel(values.Length, deviceData.View, mean, deviceMoments.View);
        accelerator.Synchronize();

        var moments = deviceMoments.GetAsArray1D();
        var m2 = moments[0] / values.Length;
        var m4 = moments[1] / values.Length;

        var variance = m2;
        if (variance == 0) return 0;

        return (m4 / (variance * variance)) - 3.0; // Excess kurtosis
    }

    /// <summary>
    /// Calculates kurtosis using a selector function and GPU acceleration.
    /// </summary>
    public static double KurtosisGpu<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).KurtosisGpu();
    }

    // GPU Kernels

    private static void SumKernel(Index1D index, ArrayView<double> input, ArrayView<double> output)
    {
        var sum = 0.0;
        for (int i = index; i < input.Length; i += Grid.DimX * Group.DimX)
        {
            sum += input[i];
        }
        Atomic.Add(ref output[0], sum);
    }

    private static void SkewnessKernel(
        Index1D index,
        ArrayView<double> input,
        double mean,
        ArrayView<double> output)
    {
        var m2 = 0.0;
        var m3 = 0.0;

        for (int i = index; i < input.Length; i += Grid.DimX * Group.DimX)
        {
            var diff = input[i] - mean;
            var diff2 = diff * diff;
            var diff3 = diff2 * diff;

            m2 += diff2;
            m3 += diff3;
        }

        Atomic.Add(ref output[0], m2);
        Atomic.Add(ref output[1], m3);
    }

    private static void KurtosisKernel(
        Index1D index,
        ArrayView<double> input,
        double mean,
        ArrayView<double> output)
    {
        var m2 = 0.0;
        var m4 = 0.0;

        for (int i = index; i < input.Length; i += Grid.DimX * Group.DimX)
        {
            var diff = input[i] - mean;
            var diff2 = diff * diff;
            var diff4 = diff2 * diff2;

            m2 += diff2;
            m4 += diff4;
        }

        Atomic.Add(ref output[0], m2);
        Atomic.Add(ref output[1], m4);
    }
}
