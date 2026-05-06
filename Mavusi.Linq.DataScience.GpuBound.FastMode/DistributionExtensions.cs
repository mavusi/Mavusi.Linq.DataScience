using ILGPU;
using ILGPU.Runtime;

namespace Mavusi.Linq.DataScience.GpuBound;

/// <summary>
/// GPU-accelerated statistical distribution operations using ILGPU.
/// </summary>
public static class DistributionExtensions
{
    private static (Context Context, Accelerator Accelerator) GpuContext => 
        GpuContextBase.GpuContext;

    // Cached kernel delegates for performance
    private static Action<Index1D, ArrayView<float>, ArrayView<float>>? _sumKernel;
    private static Action<Index1D, ArrayView<float>, float, ArrayView<float>>? _skewnessKernel;
    private static Action<Index1D, ArrayView<float>, float, ArrayView<float>>? _kurtosisKernel;

    /// <summary>
    /// Calculates the median (50th percentile) of a sequence of values.
    /// Note: Sorting is performed on CPU; no GPU acceleration for this operation.
    /// </summary>
    public static float MedianGpu(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var sorted = source.OrderBy(x => x).ToArray();
        if (sorted.Length == 0) throw new InvalidOperationException("Sequence contains no elements");

        int count = sorted.Length;
        if (count % 2 == 0)
        {
            return (float)((sorted[count / 2 - 1] + sorted[count / 2]) * 0.5);
        }
        else
        {
            return (float)sorted[count / 2];
        }
    }

    /// <summary>
    /// Calculates the median of a sequence using a selector function.
    /// Note: Sorting is performed on CPU; no GPU acceleration for this operation.
    /// </summary>
    public static float MedianGpu<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        var values = new List<double>();
        foreach (var item in source)
        {
            values.Add(selector(item));
        }
        return values.MedianGpu();
    }

    /// <summary>
    /// Calculates the specified quartile of a sequence.
    /// Note: Sorting is performed on CPU; no GPU acceleration for this operation.
    /// </summary>
    public static float QuartileGpu(this IEnumerable<double> source, int quartile)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (quartile < 1 || quartile > 3)
            throw new ArgumentException("Quartile must be 1, 2, or 3", nameof(quartile));

        return source.PercentileGpu(quartile * 25.0);
    }

    /// <summary>
    /// Calculates the specified quartile of a sequence using a selector function.
    /// Note: Sorting is performed on CPU; no GPU acceleration for this operation.
    /// </summary>
    public static float QuartileGpu<T>(this IEnumerable<T> source, int quartile, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        var values = new List<double>();
        foreach (var item in source)
        {
            values.Add(selector(item));
        }
        return values.QuartileGpu(quartile);
    }

    /// <summary>
    /// Calculates the specified percentile of a sequence.
    /// Note: Sorting is performed on CPU; no GPU acceleration for this operation.
    /// </summary>
    public static float PercentileGpu(this IEnumerable<double> source, double percentile)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (percentile < 0 || percentile > 100)
            throw new ArgumentException("Percentile must be between 0 and 100", nameof(percentile));

        var sorted = source.OrderBy(x => x).ToArray();
        if (sorted.Length == 0) throw new InvalidOperationException("Sequence contains no elements");

        if (percentile == 0) return (float)sorted[0];
        if (percentile == 100) return (float)sorted[^1];

        float index = (float)(percentile * 0.01) * (sorted.Length - 1);
        int lowerIndex = (int)MathF.Floor(index);
        int upperIndex = (int)MathF.Ceiling(index);

        if (lowerIndex == upperIndex)
        {
            return (float)sorted[lowerIndex];
        }

        float fraction = index - lowerIndex;
        return (float)sorted[lowerIndex] * (1f - fraction) + (float)sorted[upperIndex] * fraction;
    }

    /// <summary>
    /// Calculates the specified percentile of a sequence using a selector function.
    /// Note: Sorting is performed on CPU; no GPU acceleration for this operation.
    /// </summary>
    public static float PercentileGpu<T>(this IEnumerable<T> source, double percentile, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        var values = new List<double>();
        foreach (var item in source)
        {
            values.Add(selector(item));
        }
        return values.PercentileGpu(percentile);
    }

    /// <summary>
    /// Calculates the interquartile range (IQR = Q3 - Q1).
    /// Optimized to sort only once. Note: Sorting is performed on CPU; no GPU acceleration for this operation.
    /// </summary>
    public static float InterquartileRangeGpu(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var sorted = source.OrderBy(x => x).ToArray();
        if (sorted.Length == 0) throw new InvalidOperationException("Sequence contains no elements");

        float q1Index = 0.25f * (sorted.Length - 1);
        int q1Lower = (int)MathF.Floor(q1Index);
        int q1Upper = (int)MathF.Ceiling(q1Index);
        float q1Fraction = q1Index - q1Lower;
        float q1 = q1Lower == q1Upper 
            ? (float)sorted[q1Lower] 
            : (float)sorted[q1Lower] * (1f - q1Fraction) + (float)sorted[q1Upper] * q1Fraction;

        float q3Index = 0.75f * (sorted.Length - 1);
        int q3Lower = (int)MathF.Floor(q3Index);
        int q3Upper = (int)MathF.Ceiling(q3Index);
        float q3Fraction = q3Index - q3Lower;
        float q3 = q3Lower == q3Upper 
            ? (float)sorted[q3Lower] 
            : (float)sorted[q3Lower] * (1f - q3Fraction) + (float)sorted[q3Upper] * q3Fraction;

        return q3 - q1;
    }

    /// <summary>
    /// Calculates the interquartile range (IQR = Q3 - Q1) using a selector function.
    /// Optimized to sort only once. Note: Sorting is performed on CPU; no GPU acceleration for this operation.
    /// </summary>
    public static float InterquartileRangeGpu<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        var values = new List<double>();
        foreach (var item in source)
        {
            values.Add(selector(item));
        }
        return values.InterquartileRangeGpu();
    }

    /// <summary>
    /// Calculates skewness (measure of asymmetry) using GPU acceleration.
    /// </summary>
    public static float SkewnessGpu(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        float[] floatValues;
        if (source is double[] doubleArray)
        {
            floatValues = new float[doubleArray.Length];
            for (int i = 0; i < doubleArray.Length; i++)
            {
                floatValues[i] = (float)doubleArray[i];
            }
        }
        else
        {
            var list = new List<float>();
            foreach (var value in source)
            {
                list.Add((float)value);
            }
            floatValues = list.ToArray();
        }

        if (floatValues.Length == 0) throw new InvalidOperationException("Sequence contains no elements");

        var (context, accelerator) = GpuContext;

        using var deviceData = accelerator.Allocate1D(floatValues);
        using var deviceResult = accelerator.Allocate1D<float>(1);

        if (_sumKernel == null)
        {
            _sumKernel = accelerator.LoadAutoGroupedStreamKernel<
                Index1D, ArrayView<float>, ArrayView<float>>(SumKernel);
        }

        deviceResult.MemSetToZero();
        _sumKernel(floatValues.Length, deviceData.View, deviceResult.View);
        accelerator.Synchronize();

        float mean = deviceResult.GetAsArray1D()[0] / floatValues.Length;

        using var deviceMoments = accelerator.Allocate1D<float>(2);
        deviceMoments.MemSetToZero();

        if (_skewnessKernel == null)
        {
            _skewnessKernel = accelerator.LoadAutoGroupedStreamKernel<
                Index1D, ArrayView<float>, float, ArrayView<float>>(SkewnessKernel);
        }

        _skewnessKernel(floatValues.Length, deviceData.View, mean, deviceMoments.View);
        accelerator.Synchronize();

        var moments = deviceMoments.GetAsArray1D();
        float m2 = moments[0] / floatValues.Length;
        float m3 = moments[1] / floatValues.Length;

        float stdDev = MathF.Sqrt(m2);
        if (stdDev == 0) return 0;

        return m3 / (stdDev * stdDev * stdDev);
    }

    /// <summary>
    /// Calculates skewness using a selector function and GPU acceleration.
    /// </summary>
    public static float SkewnessGpu<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        var values = new List<double>();
        foreach (var item in source)
        {
            values.Add(selector(item));
        }
        return values.SkewnessGpu();
    }

    /// <summary>
    /// Calculates kurtosis (measure of tailedness) using GPU acceleration.
    /// </summary>
    public static float KurtosisGpu(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        float[] floatValues;
        if (source is double[] doubleArray)
        {
            floatValues = new float[doubleArray.Length];
            for (int i = 0; i < doubleArray.Length; i++)
            {
                floatValues[i] = (float)doubleArray[i];
            }
        }
        else
        {
            var list = new List<float>();
            foreach (var value in source)
            {
                list.Add((float)value);
            }
            floatValues = list.ToArray();
        }

        if (floatValues.Length == 0) throw new InvalidOperationException("Sequence contains no elements");

        var (context, accelerator) = GpuContext;

        using var deviceData = accelerator.Allocate1D(floatValues);
        using var deviceResult = accelerator.Allocate1D<float>(1);

        if (_sumKernel == null)
        {
            _sumKernel = accelerator.LoadAutoGroupedStreamKernel<
                Index1D, ArrayView<float>, ArrayView<float>>(SumKernel);
        }

        deviceResult.MemSetToZero();
        _sumKernel(floatValues.Length, deviceData.View, deviceResult.View);
        accelerator.Synchronize();

        float mean = deviceResult.GetAsArray1D()[0] / floatValues.Length;

        using var deviceMoments = accelerator.Allocate1D<float>(2);
        deviceMoments.MemSetToZero();

        if (_kurtosisKernel == null)
        {
            _kurtosisKernel = accelerator.LoadAutoGroupedStreamKernel<
                Index1D, ArrayView<float>, float, ArrayView<float>>(KurtosisKernel);
        }

        _kurtosisKernel(floatValues.Length, deviceData.View, mean, deviceMoments.View);
        accelerator.Synchronize();

        var moments = deviceMoments.GetAsArray1D();
        float m2 = moments[0] / floatValues.Length;
        float m4 = moments[1] / floatValues.Length;

        if (m2 == 0) return 0;

        return (m4 / (m2 * m2)) - 3f;
    }

    /// <summary>
    /// Calculates kurtosis using a selector function and GPU acceleration.
    /// </summary>
    public static float KurtosisGpu<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        var values = new List<double>();
        foreach (var item in source)
        {
            values.Add(selector(item));
        }
        return values.KurtosisGpu();
    }

    // GPU Kernels

    private static void SumKernel(Index1D index, ArrayView<float> input, ArrayView<float> output)
    {
        var sum = 0.0f;
        for (int i = index; i < input.Length; i += Grid.DimX * Group.DimX)
        {
            sum += input[i];
        }
        Atomic.Add(ref output[0], sum);
    }

    private static void SkewnessKernel(
        Index1D index,
        ArrayView<float> input,
        float mean,
        ArrayView<float> output)
    {
        var m2 = 0.0f;
        var m3 = 0.0f;

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
        ArrayView<float> input,
        float mean,
        ArrayView<float> output)
    {
        var m2 = 0.0f;
        var m4 = 0.0f;

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
