namespace Mavusi.Linq.DataScience;

public static class StatisticalExtensions
{
    /// <summary>
    /// Calculates the population standard deviation of a sequence of values.
    /// </summary>
    public static double StandardDeviation(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var values = source.ToList();
        if (values.Count == 0) throw new InvalidOperationException("Sequence contains no elements");

        var mean = values.Average();
        var sumOfSquares = values.Sum(x => Math.Pow(x - mean, 2));
        return Math.Sqrt(sumOfSquares / values.Count);
    }

    /// <summary>
    /// Calculates the sample standard deviation of a sequence of values.
    /// </summary>
    public static double StandardDeviationSample(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var values = source.ToList();
        if (values.Count <= 1) throw new InvalidOperationException("Sequence must contain at least two elements");

        var mean = values.Average();
        var sumOfSquares = values.Sum(x => Math.Pow(x - mean, 2));
        return Math.Sqrt(sumOfSquares / (values.Count - 1));
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

        var values = source.ToList();
        if (values.Count == 0) throw new InvalidOperationException("Sequence contains no elements");

        var mean = values.Average();
        return values.Sum(x => Math.Pow(x - mean, 2)) / values.Count;
    }

    /// <summary>
    /// Calculates the sample variance of a sequence of values.
    /// </summary>
    public static double VarianceSample(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var values = source.ToList();
        if (values.Count <= 1) throw new InvalidOperationException("Sequence must contain at least two elements");

        var mean = values.Average();
        return values.Sum(x => Math.Pow(x - mean, 2)) / (values.Count - 1);
    }
}
