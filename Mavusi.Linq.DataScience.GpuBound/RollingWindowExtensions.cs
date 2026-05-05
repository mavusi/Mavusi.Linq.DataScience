using ILGPU;
using ILGPU.Runtime;

namespace Mavusi.Linq.DataScience.GpuBound;

/// <summary>
/// GPU-accelerated rolling window operations using ILGPU.
/// </summary>
public static class RollingWindowExtensions
{
    private static (Context Context, Accelerator Accelerator) GpuContext => 
        GpuContextBase.GpuContext;

    /// <summary>
    /// Calculates a rolling average (moving average) with a specified window size using GPU acceleration.
    /// </summary>
    public static IEnumerable<double> RollingAverageGpu(this IEnumerable<double> source, int windowSize)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (windowSize <= 0) throw new ArgumentException("Window size must be greater than zero", nameof(windowSize));

        var values = source.ToArray();
        if (values.Length < windowSize) yield break;

        var (context, accelerator) = GpuContext;
        var resultCount = values.Length - windowSize + 1;
        var results = new double[resultCount];

        using var deviceInput = accelerator.Allocate1D(values);
        using var deviceOutput = accelerator.Allocate1D<double>(resultCount);

        var rollingAvgKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<double>, int, ArrayView<double>>(RollingAverageKernel);

        rollingAvgKernel(resultCount, deviceInput.View, windowSize, deviceOutput.View);
        accelerator.Synchronize();

        deviceOutput.CopyToCPU(results);

        foreach (var result in results)
        {
            yield return result;
        }
    }

    /// <summary>
    /// Calculates a rolling average (moving average) with a specified window size using a selector and GPU acceleration.
    /// </summary>
    public static IEnumerable<double> RollingAverageGpu<T>(this IEnumerable<T> source, int windowSize, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (windowSize <= 0) throw new ArgumentException("Window size must be greater than zero", nameof(windowSize));

        return source.Select(selector).RollingAverageGpu(windowSize);
    }

    /// <summary>
    /// Calculates a rolling sum with a specified window size using GPU acceleration.
    /// </summary>
    public static IEnumerable<double> RollingSumGpu(this IEnumerable<double> source, int windowSize)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (windowSize <= 0) throw new ArgumentException("Window size must be greater than zero", nameof(windowSize));

        var values = source.ToArray();
        if (values.Length < windowSize) yield break;

        var (context, accelerator) = GpuContext;
        var resultCount = values.Length - windowSize + 1;
        var results = new double[resultCount];

        using var deviceInput = accelerator.Allocate1D(values);
        using var deviceOutput = accelerator.Allocate1D<double>(resultCount);

        var rollingSumKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<double>, int, ArrayView<double>>(RollingSumKernel);

        rollingSumKernel(resultCount, deviceInput.View, windowSize, deviceOutput.View);
        accelerator.Synchronize();

        deviceOutput.CopyToCPU(results);

        foreach (var result in results)
        {
            yield return result;
        }
    }

    /// <summary>
    /// Calculates a rolling sum with a specified window size using a selector and GPU acceleration.
    /// </summary>
    public static IEnumerable<double> RollingSumGpu<T>(this IEnumerable<T> source, int windowSize, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (windowSize <= 0) throw new ArgumentException("Window size must be greater than zero", nameof(windowSize));

        return source.Select(selector).RollingSumGpu(windowSize);
    }

    /// <summary>
    /// Calculates a rolling minimum with a specified window size using GPU acceleration.
    /// </summary>
    public static IEnumerable<double> RollingMinGpu(this IEnumerable<double> source, int windowSize)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (windowSize <= 0) throw new ArgumentException("Window size must be greater than zero", nameof(windowSize));

        var values = source.ToArray();
        if (values.Length < windowSize) yield break;

        var (context, accelerator) = GpuContext;
        var resultCount = values.Length - windowSize + 1;
        var results = new double[resultCount];

        using var deviceInput = accelerator.Allocate1D(values);
        using var deviceOutput = accelerator.Allocate1D<double>(resultCount);

        var rollingMinKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<double>, int, ArrayView<double>>(RollingMinKernel);

        rollingMinKernel(resultCount, deviceInput.View, windowSize, deviceOutput.View);
        accelerator.Synchronize();

        deviceOutput.CopyToCPU(results);

        foreach (var result in results)
        {
            yield return result;
        }
    }

    /// <summary>
    /// Calculates a rolling maximum with a specified window size using GPU acceleration.
    /// </summary>
    public static IEnumerable<double> RollingMaxGpu(this IEnumerable<double> source, int windowSize)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (windowSize <= 0) throw new ArgumentException("Window size must be greater than zero", nameof(windowSize));

        var values = source.ToArray();
        if (values.Length < windowSize) yield break;

        var (context, accelerator) = GpuContext;
        var resultCount = values.Length - windowSize + 1;
        var results = new double[resultCount];

        using var deviceInput = accelerator.Allocate1D(values);
        using var deviceOutput = accelerator.Allocate1D<double>(resultCount);

        var rollingMaxKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<double>, int, ArrayView<double>>(RollingMaxKernel);

        rollingMaxKernel(resultCount, deviceInput.View, windowSize, deviceOutput.View);
        accelerator.Synchronize();

        deviceOutput.CopyToCPU(results);

        foreach (var result in results)
        {
            yield return result;
        }
    }

    /// <summary>
    /// Calculates a rolling standard deviation with a specified window size using GPU acceleration.
    /// </summary>
    public static IEnumerable<double> RollingStandardDeviationGpu(this IEnumerable<double> source, int windowSize)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (windowSize <= 0) throw new ArgumentException("Window size must be greater than zero", nameof(windowSize));

        var values = source.ToArray();
        if (values.Length < windowSize) yield break;

        var (context, accelerator) = GpuContext;
        var resultCount = values.Length - windowSize + 1;
        var results = new double[resultCount];

        using var deviceInput = accelerator.Allocate1D(values);
        using var deviceOutput = accelerator.Allocate1D<double>(resultCount);

        var rollingStdDevKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<double>, int, ArrayView<double>>(RollingStandardDeviationKernel);

        rollingStdDevKernel(resultCount, deviceInput.View, windowSize, deviceOutput.View);
        accelerator.Synchronize();

        deviceOutput.CopyToCPU(results);

        foreach (var result in results)
        {
            yield return result;
        }
    }

    // GPU Kernels

    private static void RollingAverageKernel(
        Index1D index,
        ArrayView<double> input,
        int windowSize,
        ArrayView<double> output)
    {
        if (index < output.Length)
        {
            double sum = 0;
            for (int i = 0; i < windowSize; i++)
            {
                sum += input[index + i];
            }
            output[index] = sum / windowSize;
        }
    }

    private static void RollingSumKernel(
        Index1D index,
        ArrayView<double> input,
        int windowSize,
        ArrayView<double> output)
    {
        if (index < output.Length)
        {
            double sum = 0;
            for (int i = 0; i < windowSize; i++)
            {
                sum += input[index + i];
            }
            output[index] = sum;
        }
    }

    private static void RollingMinKernel(
        Index1D index,
        ArrayView<double> input,
        int windowSize,
        ArrayView<double> output)
    {
        if (index < output.Length)
        {
            double min = input[index];
            for (int i = 1; i < windowSize; i++)
            {
                var value = input[index + i];
                if (value < min) min = value;
            }
            output[index] = min;
        }
    }

    private static void RollingMaxKernel(
        Index1D index,
        ArrayView<double> input,
        int windowSize,
        ArrayView<double> output)
    {
        if (index < output.Length)
        {
            double max = input[index];
            for (int i = 1; i < windowSize; i++)
            {
                var value = input[index + i];
                if (value > max) max = value;
            }
            output[index] = max;
        }
    }

    private static void RollingStandardDeviationKernel(
        Index1D index,
        ArrayView<double> input,
        int windowSize,
        ArrayView<double> output)
    {
        if (index < output.Length)
        {
            // Calculate mean
            double sum = 0;
            for (int i = 0; i < windowSize; i++)
            {
                sum += input[index + i];
            }
            double mean = sum / windowSize;

            // Calculate variance
            double sumSquares = 0;
            for (int i = 0; i < windowSize; i++)
            {
                var diff = input[index + i] - mean;
                sumSquares += diff * diff;
            }

            output[index] = Math.Sqrt(sumSquares / windowSize);
        }
    }
}
