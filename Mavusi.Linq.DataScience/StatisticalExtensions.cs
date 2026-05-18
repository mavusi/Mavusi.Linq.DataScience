namespace Mavusi.Linq.DataScience;

public static class StatisticalExtensions
{
    /// <summary>
    /// Calculates the population standard deviation of a sequence of values.
    /// </summary>
    public static double StandardDeviation(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var stats = ComputeRunningStats(source);
        if (stats.Count == 0) throw new InvalidOperationException("Sequence contains no elements");

        return Math.Sqrt(stats.M2 / stats.Count);
    }

    /// <summary>
    /// Calculates the sample standard deviation of a sequence of values.
    /// </summary>
    public static double StandardDeviationSample(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var stats = ComputeRunningStats(source);
        if (stats.Count <= 1) throw new InvalidOperationException("Sequence must contain at least two elements");

        return Math.Sqrt(stats.M2 / (stats.Count - 1));
    }

    /// <summary>
    /// Calculates the population standard deviation of a sequence of values using a selector.
    /// </summary>
    public static double StandardDeviation<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).StandardDeviation();
    }

    /// <summary>
    /// Calculates the sample standard deviation of a sequence of values using a selector.
    /// </summary>
    public static double StandardDeviationSample<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).StandardDeviationSample();
    }

    /// <summary>
    /// Calculates the variance of a sequence of values.
    /// </summary>
    public static double Variance(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var stats = ComputeRunningStats(source);
        if (stats.Count == 0) throw new InvalidOperationException("Sequence contains no elements");

        return stats.M2 / stats.Count;
    }

    /// <summary>
    /// Calculates the sample variance of a sequence of values.
    /// </summary>
    public static double VarianceSample(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var stats = ComputeRunningStats(source);
        if (stats.Count <= 1) throw new InvalidOperationException("Sequence must contain at least two elements");

        return stats.M2 / (stats.Count - 1);
    }

    private static RunningStats ComputeRunningStats(IEnumerable<double> source)
    {
        var stats = new RunningStats();

        foreach (var value in source)
        {
            stats.Add(value);
        }

        return stats;
    }

    private struct RunningStats
    {
        public int Count { get; private set; }
        public double Mean { get; private set; }
        public double M2 { get; private set; }

        public void Add(double value)
        {
            Count++;

            var delta = value - Mean;
            Mean += delta / Count;
            var delta2 = value - Mean;
            M2 += delta * delta2;
        }
    }
}
