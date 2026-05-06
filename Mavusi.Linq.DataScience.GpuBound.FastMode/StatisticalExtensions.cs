using ILGPU;
using ILGPU.Runtime;
using System.Runtime.CompilerServices;

namespace Mavusi.Linq.DataScience.GpuBound;

/// <summary>
/// GPU-accelerated statistical calculations using ILGPU.
/// </summary>
public static class StatisticalExtensions
{
    private static (Context Context, Accelerator Accelerator) GpuContext => 
        GpuContextBase.FastGpuContext;

    private static Action<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>>? _cachedVarianceKernel;
    private static readonly object _kernelLock = new object();

    /// <summary>
    /// Calculates the population standard deviation of a sequence of values using GPU acceleration.
    /// </summary>
    public static float StandardDeviationGpu(this IEnumerable<float> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var values = source as float[] ?? source.ToArray();
        if (values.Length == 0) throw new InvalidOperationException("Sequence contains no elements");

        var (context, accelerator) = GpuContext;
        var variance = CalculateVarianceGpuOptimized(accelerator, values, false);
        return MathF.Sqrt(variance);
    }

    /// <summary>
    /// Calculates the population standard deviation of a sequence of values using GPU acceleration.
    /// </summary>
    public static float StandardDeviationGpu(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var values = source as double[] ?? source.ToArray();
        if (values.Length == 0) throw new InvalidOperationException("Sequence contains no elements");

        var floatValues = ConvertToFloat(values);
        var (context, accelerator) = GpuContext;
        var variance = CalculateVarianceGpuOptimized(accelerator, floatValues, false);
        return MathF.Sqrt(variance);
    }

    /// <summary>
    /// Calculates the sample standard deviation of a sequence of values using GPU acceleration.
    /// </summary>
    public static float StandardDeviationSampleGpu(this IEnumerable<float> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var values = source as float[] ?? source.ToArray();
        if (values.Length <= 1) throw new InvalidOperationException("Sequence must contain at least two elements");

        var (context, accelerator) = GpuContext;
        var variance = CalculateVarianceGpuOptimized(accelerator, values, true);
        return MathF.Sqrt(variance);
    }

    /// <summary>
    /// Calculates the sample standard deviation of a sequence of values using GPU acceleration.
    /// </summary>
    public static float StandardDeviationSampleGpu(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var values = source as double[] ?? source.ToArray();
        if (values.Length <= 1) throw new InvalidOperationException("Sequence must contain at least two elements");

        var floatValues = ConvertToFloat(values);
        var (context, accelerator) = GpuContext;
        var variance = CalculateVarianceGpuOptimized(accelerator, floatValues, true);
        return MathF.Sqrt(variance);
    }

    /// <summary>
    /// Calculates the population standard deviation of a sequence of values using a selector and GPU acceleration.
    /// </summary>
    public static float StandardDeviationGpu<T>(this IEnumerable<T> source, Func<T, float> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).StandardDeviationGpu();
    }

    /// <summary>
    /// Calculates the population standard deviation of a sequence of values using a selector and GPU acceleration.
    /// </summary>
    public static float StandardDeviationGpu<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).StandardDeviationGpu();
    }

    /// <summary>
    /// Calculates the sample standard deviation of a sequence of values using a selector and GPU acceleration.
    /// </summary>
    public static float StandardDeviationSampleGpu<T>(this IEnumerable<T> source, Func<T, float> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).StandardDeviationSampleGpu();
    }

    /// <summary>
    /// Calculates the sample standard deviation of a sequence of values using a selector and GPU acceleration.
    /// </summary>
    public static float StandardDeviationSampleGpu<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).StandardDeviationSampleGpu();
    }

    /// <summary>
    /// Calculates the variance of a sequence of values using GPU acceleration.
    /// </summary>
    public static float VarianceGpu(this IEnumerable<float> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var values = source as float[] ?? source.ToArray();
        if (values.Length == 0) throw new InvalidOperationException("Sequence contains no elements");

        var (context, accelerator) = GpuContext;
        return CalculateVarianceGpuOptimized(accelerator, values, false);
    }

    /// <summary>
    /// Calculates the variance of a sequence of values using GPU acceleration.
    /// </summary>
    public static float VarianceGpu(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var values = source as double[] ?? source.ToArray();
        if (values.Length == 0) throw new InvalidOperationException("Sequence contains no elements");

        var floatValues = ConvertToFloat(values);
        var (context, accelerator) = GpuContext;
        return CalculateVarianceGpuOptimized(accelerator, floatValues, false);
    }

    /// <summary>
    /// Calculates the sample variance of a sequence of values using GPU acceleration.
    /// </summary>
    public static float VarianceSampleGpu(this IEnumerable<float> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var values = source as float[] ?? source.ToArray();
        if (values.Length <= 1) throw new InvalidOperationException("Sequence must contain at least two elements");

        var (context, accelerator) = GpuContext;
        return CalculateVarianceGpuOptimized(accelerator, values, true);
    }

    /// <summary>
    /// Calculates the sample variance of a sequence of values using GPU acceleration.
    /// </summary>
    public static float VarianceSampleGpu(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var values = source as double[] ?? source.ToArray();
        if (values.Length <= 1) throw new InvalidOperationException("Sequence must contain at least two elements");

        var floatValues = ConvertToFloat(values);
        var (context, accelerator) = GpuContext;
        return CalculateVarianceGpuOptimized(accelerator, floatValues, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float[] ConvertToFloat(double[] data)
    {
        var result = new float[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (float)data[i];
        }
        return result;
    }

    private static float CalculateVarianceGpuOptimized(Accelerator accelerator, float[] data, bool isSample)
    {
        var (mean, variance) = CalculateMeanAndVarianceGpuOptimized(accelerator, data, isSample);
        return variance;
    }

    internal static (float Mean, float Variance) CalculateMeanAndVarianceGpuOptimized(Accelerator accelerator, float[] data, bool isSample)
    {
        using var deviceData = accelerator.Allocate1D(data);
        using var deviceResults = accelerator.Allocate1D<float>(2);

        deviceResults.MemSetToZero();

        var kernel = GetOrCreateVarianceKernel(accelerator);
        kernel(data.Length, deviceData.View, deviceResults.View, deviceResults.View.SubView(1, 1));
        accelerator.Synchronize();

        var results = deviceResults.GetAsArray1D();
        var sum = results[0];
        var sumOfSquares = results[1];

        var n = data.Length;
        var mean = sum / n;
        var variance = (sumOfSquares / n) - (mean * mean);

        if (isSample && n > 1)
        {
            variance *= (float)n / (n - 1);
        }

        return (mean, variance);
    }

    private static Action<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>> GetOrCreateVarianceKernel(Accelerator accelerator)
    {
        if (_cachedVarianceKernel == null)
        {
            lock (_kernelLock)
            {
                if (_cachedVarianceKernel == null)
                {
                    _cachedVarianceKernel = accelerator.LoadAutoGroupedStreamKernel<
                        Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>>(SinglePassVarianceKernel);
                }
            }
        }
        return _cachedVarianceKernel;
    }

    private static void SinglePassVarianceKernel(
        Index1D index,
        ArrayView<float> input,
        ArrayView<float> sumOutput,
        ArrayView<float> sumSquaresOutput)
    {
        var sum = 0.0f;
        var sumSquares = 0.0f;

        for (int i = index; i < input.Length; i += Grid.DimX * Group.DimX)
        {
            var value = input[i];
            sum += value;
            sumSquares += value * value;
        }

        Atomic.Add(ref sumOutput[0], sum);
        Atomic.Add(ref sumSquaresOutput[0], sumSquares);
    }
}
