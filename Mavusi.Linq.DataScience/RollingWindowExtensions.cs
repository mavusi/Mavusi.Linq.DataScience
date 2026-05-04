namespace Mavusi.Linq.DataScience;

/// <summary>
/// Represents a window of values from a rolling window operation.
/// </summary>
/// <typeparam name="T">The type of elements in the window.</typeparam>
public class Window<T>
{
    public int Index { get; init; }
    public IReadOnlyList<T> Values { get; init; }

    public Window(int index, IReadOnlyList<T> values)
    {
        Index = index;
        Values = values;
    }
}

public static class RollingWindowExtensions
{
    /// <summary>
    /// Creates rolling windows of a specified size from a sequence.
    /// Each window slides by one element.
    /// </summary>
    public static IEnumerable<Window<T>> RollingWindow<T>(this IEnumerable<T> source, int windowSize)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (windowSize <= 0) throw new ArgumentException("Window size must be greater than zero", nameof(windowSize));

        var buffer = new Queue<T>(windowSize);
        var index = 0;
        var windowIndex = 0;

        foreach (var item in source)
        {
            buffer.Enqueue(item);

            if (buffer.Count == windowSize)
            {
                yield return new Window<T>(windowIndex, buffer.ToList());
                buffer.Dequeue();
                windowIndex++;
            }

            index++;
        }
    }

    /// <summary>
    /// Creates rolling windows of a specified size with a custom step.
    /// </summary>
    public static IEnumerable<Window<T>> RollingWindow<T>(this IEnumerable<T> source, int windowSize, int step)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (windowSize <= 0) throw new ArgumentException("Window size must be greater than zero", nameof(windowSize));
        if (step <= 0) throw new ArgumentException("Step must be greater than zero", nameof(step));

        var values = source.ToList();

        for (int i = 0; i <= values.Count - windowSize; i += step)
        {
            yield return new Window<T>(i, values.Skip(i).Take(windowSize).ToList());
        }
    }

    /// <summary>
    /// Calculates a rolling average (moving average) with a specified window size.
    /// </summary>
    public static IEnumerable<double> RollingAverage(this IEnumerable<double> source, int windowSize)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (windowSize <= 0) throw new ArgumentException("Window size must be greater than zero", nameof(windowSize));

        return source.RollingWindow(windowSize).Select(w => w.Values.Average());
    }

    /// <summary>
    /// Calculates a rolling average (moving average) with a specified window size using a selector.
    /// </summary>
    public static IEnumerable<double> RollingAverage<T>(this IEnumerable<T> source, int windowSize, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (windowSize <= 0) throw new ArgumentException("Window size must be greater than zero", nameof(windowSize));

        return source.Select(selector).RollingAverage(windowSize);
    }

    /// <summary>
    /// Calculates a rolling sum with a specified window size.
    /// </summary>
    public static IEnumerable<double> RollingSum(this IEnumerable<double> source, int windowSize)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (windowSize <= 0) throw new ArgumentException("Window size must be greater than zero", nameof(windowSize));

        return source.RollingWindow(windowSize).Select(w => w.Values.Sum());
    }

    /// <summary>
    /// Calculates a rolling standard deviation with a specified window size.
    /// </summary>
    public static IEnumerable<double> RollingStandardDeviation(this IEnumerable<double> source, int windowSize)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (windowSize <= 0) throw new ArgumentException("Window size must be greater than zero", nameof(windowSize));

        return source.RollingWindow(windowSize).Select(w => w.Values.StandardDeviation());
    }

    /// <summary>
    /// Applies a custom aggregation function to each rolling window.
    /// </summary>
    public static IEnumerable<TResult> RollingAggregate<T, TResult>(
        this IEnumerable<T> source, 
        int windowSize, 
        Func<IEnumerable<T>, TResult> aggregator)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (aggregator == null) throw new ArgumentNullException(nameof(aggregator));
        if (windowSize <= 0) throw new ArgumentException("Window size must be greater than zero", nameof(windowSize));

        return source.RollingWindow(windowSize).Select(w => aggregator(w.Values));
    }
}
