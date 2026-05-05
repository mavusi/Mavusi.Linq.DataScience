using ILGPU;
using ILGPU.Runtime;

namespace Mavusi.Linq.DataScience.GpuBound;

/// <summary>
/// GPU-accelerated statistical calculations using ILGPU.
/// </summary>
public static class StatisticalExtensions
{
    private static (Context Context, Accelerator Accelerator) GpuContext => 
        GpuContextBase.GpuContext;

    /// <summary>
    /// Calculates the population standard deviation of a sequence of values using GPU acceleration.
    /// </summary>
    public static float StandardDeviationGpu(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var values = source.ToArray();
        if (values.Length == 0) throw new InvalidOperationException("Sequence contains no elements");

        var (context, accelerator) = GpuContext;
        var variance = CalculateVarianceGpu(accelerator, values, false);
        return (float)Math.Sqrt(variance);
    }

    /// <summary>
    /// Calculates the sample standard deviation of a sequence of values using GPU acceleration.
    /// </summary>
    public static float StandardDeviationSampleGpu(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var values = source.ToArray();
        if (values.Length <= 1) throw new InvalidOperationException("Sequence must contain at least two elements");

        var (context, accelerator) = GpuContext;
        var variance = CalculateVarianceGpu(accelerator, values, true);
        return (float)Math.Sqrt(variance);
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
    public static float StandardDeviationSampleGpu<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).StandardDeviationSampleGpu();
    }

    /// <summary>
    /// Calculates the variance of a sequence of values using GPU acceleration.
    /// </summary>
    public static float VarianceGpu(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var values = source.ToArray();
        if (values.Length == 0) throw new InvalidOperationException("Sequence contains no elements");

        var (context, accelerator) = GpuContext;
        return (float)CalculateVarianceGpu(accelerator, values, false);
    }

    /// <summary>
    /// Calculates the sample variance of a sequence of values using GPU acceleration.
    /// </summary>
    public static float VarianceSampleGpu(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var values = source.ToArray();
        if (values.Length <= 1) throw new InvalidOperationException("Sequence must contain at least two elements");

        var (context, accelerator) = GpuContext;
        return (float)CalculateVarianceGpu(accelerator, values, true);
    }

    private static double CalculateVarianceGpu(Accelerator accelerator, double[] data, bool isSample)
    {
        // Convert to float for GPU processing
        var floatData = data.Select(d => (float)d).ToArray();

        // Calculate mean first
        using var deviceData = accelerator.Allocate1D(floatData);
        using var deviceSum = accelerator.Allocate1D<float>(1);

        deviceSum.MemSetToZero();

        var sumKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<float>, ArrayView<float>>(SumKernel);

        sumKernel(floatData.Length, deviceData.View, deviceSum.View);
        accelerator.Synchronize();

        var mean = deviceSum.GetAsArray1D()[0] / floatData.Length;

        // Calculate sum of squared differences
        using var deviceSumSquares = accelerator.Allocate1D<float>(1);
        deviceSumSquares.MemSetToZero();

        var varianceKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<float>, float, ArrayView<float>>(VarianceKernel);

        varianceKernel(data.Length, deviceData.View, mean, deviceSumSquares.View);
        accelerator.Synchronize();

        var sumOfSquares = deviceSumSquares.GetAsArray1D()[0];
        var divisor = isSample ? data.Length - 1 : data.Length;

        return sumOfSquares / divisor;
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

    // GPU Kernel: Calculate sum of squared differences from mean
    private static void VarianceKernel(
        Index1D index,
        ArrayView<float> input,
        float mean,
        ArrayView<float> output)
    {
        var sumSquares = 0.0f;
        for (int i = index; i < input.Length; i += Grid.DimX * Group.DimX)
        {
            var diff = input[i] - mean;
            sumSquares += diff * diff;
        }
        Atomic.Add(ref output[0], sumSquares);
    }
}
