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
            yield return new Window<T>(i, values.GetRange(i, windowSize));
        }
    }

    /// <summary>
    /// Calculates a rolling average (moving average) with a specified window size.
    /// </summary>
    public static IEnumerable<double> RollingAverage(this IEnumerable<double> source, int windowSize)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (windowSize <= 0) throw new ArgumentException("Window size must be greater than zero", nameof(windowSize));

        return RollingAverageIterator(source, windowSize);
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

        return RollingSumIterator(source, windowSize);
    }

    /// <summary>
    /// Calculates a rolling standard deviation with a specified window size.
    /// </summary>
    public static IEnumerable<double> RollingStandardDeviation(this IEnumerable<double> source, int windowSize)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (windowSize <= 0) throw new ArgumentException("Window size must be greater than zero", nameof(windowSize));

        return RollingStandardDeviationIterator(source, windowSize);
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

    private static IEnumerable<double> RollingSumIterator(IEnumerable<double> source, int windowSize)
    {
        var buffer = new Queue<double>(windowSize);
        var sum = new CompensatedSum();

        foreach (var value in source)
        {
            buffer.Enqueue(value);
            sum.Add(value);

            if (buffer.Count == windowSize)
            {
                yield return sum.Value;

                var removed = buffer.Dequeue();
                sum.Add(-removed);
            }
        }
    }

    private static IEnumerable<double> RollingAverageIterator(IEnumerable<double> source, int windowSize)
    {
        foreach (var windowSum in RollingSumIterator(source, windowSize))
        {
            yield return windowSum / windowSize;
        }
    }

    private static IEnumerable<double> RollingStandardDeviationIterator(IEnumerable<double> source, int windowSize)
    {
        var buffer = new Queue<double>(windowSize);
        var sum = new CompensatedSum();
        var sumSquares = new CompensatedSum();

        foreach (var value in source)
        {
            buffer.Enqueue(value);
            sum.Add(value);
            sumSquares.Add(value * value);

            if (buffer.Count == windowSize)
            {
                var mean = sum.Value / windowSize;
                var variance = (sumSquares.Value / windowSize) - (mean * mean);

                // Guard against tiny negative values from floating-point roundoff.
                if (variance < 0 && variance > -1e-12)
                {
                    variance = 0;
                }

                yield return Math.Sqrt(variance);

                var removed = buffer.Dequeue();
                sum.Add(-removed);
                sumSquares.Add(-(removed * removed));
            }
        }
    }

    private struct CompensatedSum
    {
        private double _sum;
        private double _compensation;

        public double Value => _sum;

        public void Add(double value)
        {
            var corrected = value - _compensation;
            var next = _sum + corrected;
            _compensation = (next - _sum) - corrected;
            _sum = next;
        }
    }
}
