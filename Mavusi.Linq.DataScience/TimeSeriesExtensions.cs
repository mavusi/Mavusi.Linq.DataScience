namespace Mavusi.Linq.DataScience;

/// <summary>
/// Represents a time-series data point with a timestamp and value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class TimeSeriesPoint<T>
{
    public DateTime Timestamp { get; init; }
    public T Value { get; init; }

    public TimeSeriesPoint(DateTime timestamp, T value)
    {
        Timestamp = timestamp;
        Value = value;
    }
}

public static class TimeSeriesExtensions
{
    /// <summary>
    /// Groups time-series data by time intervals (e.g., hourly, daily).
    /// </summary>
    public static IEnumerable<IGrouping<DateTime, TimeSeriesPoint<T>>> GroupByInterval<T>(
        this IEnumerable<TimeSeriesPoint<T>> source,
        TimeSpan interval)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (interval <= TimeSpan.Zero) throw new ArgumentException("Interval must be greater than zero", nameof(interval));

        return source.GroupBy(point =>
        {
            var ticks = point.Timestamp.Ticks / interval.Ticks;
            return new DateTime(ticks * interval.Ticks, point.Timestamp.Kind);
        });
    }

    /// <summary>
    /// Resamples time-series data to a specific interval with an aggregation function.
    /// </summary>
    public static IEnumerable<TimeSeriesPoint<TResult>> Resample<T, TResult>(
        this IEnumerable<TimeSeriesPoint<T>> source,
        TimeSpan interval,
        Func<IEnumerable<T>, TResult> aggregator)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (aggregator == null) throw new ArgumentNullException(nameof(aggregator));
        if (interval <= TimeSpan.Zero) throw new ArgumentException("Interval must be greater than zero", nameof(interval));

        return source.GroupByInterval(interval)
            .Select(g => new TimeSeriesPoint<TResult>(
                g.Key,
                aggregator(g.Select(p => p.Value))));
    }

    /// <summary>
    /// Calculates the difference between consecutive values (first-order differencing).
    /// </summary>
    public static IEnumerable<TimeSeriesPoint<double>> Difference(
        this IEnumerable<TimeSeriesPoint<double>> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var previous = default(TimeSeriesPoint<double>);
        var isFirst = true;

        foreach (var point in source)
        {
            if (isFirst)
            {
                isFirst = false;
                previous = point;
                continue;
            }

            yield return new TimeSeriesPoint<double>(
                point.Timestamp,
                point.Value - previous.Value);

            previous = point;
        }
    }

    /// <summary>
    /// Calculates the percentage change between consecutive values.
    /// </summary>
    public static IEnumerable<TimeSeriesPoint<double>> PercentageChange(
        this IEnumerable<TimeSeriesPoint<double>> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var previous = default(TimeSeriesPoint<double>);
        var isFirst = true;

        foreach (var point in source)
        {
            if (isFirst)
            {
                isFirst = false;
                previous = point;
                continue;
            }

            if (previous.Value == 0)
            {
                yield return new TimeSeriesPoint<double>(point.Timestamp, double.NaN);
            }
            else
            {
                yield return new TimeSeriesPoint<double>(
                    point.Timestamp,
                    (point.Value - previous.Value) / previous.Value);
            }

            previous = point;
        }
    }

    /// <summary>
    /// Calculates a simple moving average for time-series data.
    /// </summary>
    public static IEnumerable<TimeSeriesPoint<double>> MovingAverage(
        this IEnumerable<TimeSeriesPoint<double>> source,
        int windowSize)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (windowSize <= 0) throw new ArgumentException("Window size must be greater than zero", nameof(windowSize));

        var buffer = new Queue<TimeSeriesPoint<double>>(windowSize);

        foreach (var point in source)
        {
            buffer.Enqueue(point);

            if (buffer.Count == windowSize)
            {
                var average = buffer.Average(p => p.Value);
                yield return new TimeSeriesPoint<double>(point.Timestamp, average);
                buffer.Dequeue();
            }
        }
    }

    /// <summary>
    /// Calculates an exponential moving average (EMA) for time-series data.
    /// </summary>
    public static IEnumerable<TimeSeriesPoint<double>> ExponentialMovingAverage(
        this IEnumerable<TimeSeriesPoint<double>> source,
        double alpha)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (alpha <= 0 || alpha > 1) throw new ArgumentException("Alpha must be between 0 and 1", nameof(alpha));

        double? ema = null;

        foreach (var point in source)
        {
            if (!ema.HasValue)
            {
                ema = point.Value;
            }
            else
            {
                ema = alpha * point.Value + (1 - alpha) * ema.Value;
            }

            yield return new TimeSeriesPoint<double>(point.Timestamp, ema.Value);
        }
    }

    /// <summary>
    /// Fills missing time points in a time series with a specified value.
    /// </summary>
    public static IEnumerable<TimeSeriesPoint<T>> FillGaps<T>(
        this IEnumerable<TimeSeriesPoint<T>> source,
        TimeSpan interval,
        T fillValue)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (interval <= TimeSpan.Zero) throw new ArgumentException("Interval must be greater than zero", nameof(interval));

        var orderedSource = source.OrderBy(p => p.Timestamp).ToList();
        if (orderedSource.Count == 0) yield break;

        var current = orderedSource[0];
        yield return current;

        for (int i = 1; i < orderedSource.Count; i++)
        {
            var next = orderedSource[i];
            var expectedTime = current.Timestamp.Add(interval);

            while (expectedTime < next.Timestamp)
            {
                yield return new TimeSeriesPoint<T>(expectedTime, fillValue);
                expectedTime = expectedTime.Add(interval);
            }

            yield return next;
            current = next;
        }
    }

    /// <summary>
    /// Creates time-series points from a sequence with a timestamp selector.
    /// </summary>
    public static IEnumerable<TimeSeriesPoint<TValue>> ToTimeSeries<T, TValue>(
        this IEnumerable<T> source,
        Func<T, DateTime> timestampSelector,
        Func<T, TValue> valueSelector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (timestampSelector == null) throw new ArgumentNullException(nameof(timestampSelector));
        if (valueSelector == null) throw new ArgumentNullException(nameof(valueSelector));

        return source.Select(item => new TimeSeriesPoint<TValue>(
            timestampSelector(item),
            valueSelector(item)));
    }
}
