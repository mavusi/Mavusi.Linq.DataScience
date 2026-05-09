namespace Mavusi.Linq.DataScience;

/// <summary>
/// Provides extension methods for statistical distribution analysis.
/// </summary>
public static class DistributionExtensions
{
    /// <summary>
    /// Calculates the median (50th percentile) of a sequence of values.
    /// </summary>
    /// <param name="source">A sequence of double values.</param>
    /// <returns>The median value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when source contains no elements.</exception>
    public static double Median(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var sorted = MaterializeAndSort(source);

        int count = sorted.Count;
        if (count % 2 == 0)
        {
            // Even number of elements - average of middle two
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }
        else
        {
            // Odd number of elements - middle element
            return sorted[count / 2];
        }
    }

    /// <summary>
    /// Calculates the median of a sequence using a selector function.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">A sequence of values.</param>
    /// <param name="selector">A function to extract a double value from each element.</param>
    /// <returns>The median value.</returns>
    public static double Median<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).Median();
    }

    /// <summary>
    /// Calculates the mode (most frequently occurring value) of a sequence.
    /// If multiple modes exist, returns the first one encountered.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">A sequence of values.</param>
    /// <returns>The mode value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when source contains no elements.</exception>
    public static T Mode<T>(this IEnumerable<T> source) where T : notnull
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var counts = new Dictionary<T, int>();
        var hasItems = false;
        var maxCount = 0;
        T mode = default!;

        foreach (var item in source)
        {
            hasItems = true;

            var count = 1;
            if (counts.TryGetValue(item, out var existing))
            {
                count = existing + 1;
            }

            counts[item] = count;

            // Strictly greater preserves the first encountered item in ties.
            if (count > maxCount)
            {
                maxCount = count;
                mode = item;
            }
        }

        if (!hasItems) throw new InvalidOperationException("Sequence contains no elements");

        return mode;
    }

    /// <summary>
    /// Calculates the specified quartile of a sequence.
    /// </summary>
    /// <param name="source">A sequence of double values.</param>
    /// <param name="quartile">The quartile to calculate (1 for Q1, 2 for Q2/median, 3 for Q3).</param>
    /// <returns>The quartile value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <exception cref="ArgumentException">Thrown when quartile is not 1, 2, or 3.</exception>
    /// <exception cref="InvalidOperationException">Thrown when source contains no elements.</exception>
    public static double Quartile(this IEnumerable<double> source, int quartile)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (quartile < 1 || quartile > 3)
            throw new ArgumentException("Quartile must be 1, 2, or 3", nameof(quartile));

        return source.Percentile(quartile * 25.0);
    }

    /// <summary>
    /// Calculates the specified quartile of a sequence using a selector function.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">A sequence of values.</param>
    /// <param name="quartile">The quartile to calculate (1 for Q1, 2 for Q2/median, 3 for Q3).</param>
    /// <param name="selector">A function to extract a double value from each element.</param>
    /// <returns>The quartile value.</returns>
    public static double Quartile<T>(this IEnumerable<T> source, int quartile, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).Quartile(quartile);
    }

    /// <summary>
    /// Calculates the interquartile range (IQR = Q3 - Q1) of a sequence.
    /// </summary>
    /// <param name="source">A sequence of double values.</param>
    /// <returns>The interquartile range.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when source contains no elements.</exception>
    public static double InterquartileRange(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var sorted = MaterializeAndSort(source);
        return PercentileFromSorted(sorted, 75.0) - PercentileFromSorted(sorted, 25.0);
    }

    /// <summary>
    /// Calculates the interquartile range of a sequence using a selector function.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">A sequence of values.</param>
    /// <param name="selector">A function to extract a double value from each element.</param>
    /// <returns>The interquartile range.</returns>
    public static double InterquartileRange<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).InterquartileRange();
    }

    /// <summary>
    /// Calculates the specified percentile of a sequence.
    /// Uses the linear interpolation method (R-7/Excel method).
    /// </summary>
    /// <param name="source">A sequence of double values.</param>
    /// <param name="percentile">The percentile to calculate (0-100).</param>
    /// <returns>The percentile value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <exception cref="ArgumentException">Thrown when percentile is not between 0 and 100.</exception>
    /// <exception cref="InvalidOperationException">Thrown when source contains no elements.</exception>
    public static double Percentile(this IEnumerable<double> source, double percentile)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (percentile < 0 || percentile > 100)
            throw new ArgumentException("Percentile must be between 0 and 100", nameof(percentile));

        var sorted = MaterializeAndSort(source);
        return PercentileFromSorted(sorted, percentile);
    }

    /// <summary>
    /// Calculates the specified percentile of a sequence using a selector function.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">A sequence of values.</param>
    /// <param name="percentile">The percentile to calculate (0-100).</param>
    /// <param name="selector">A function to extract a double value from each element.</param>
    /// <returns>The percentile value.</returns>
    public static double Percentile<T>(this IEnumerable<T> source, double percentile, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).Percentile(percentile);
    }

    /// <summary>
    /// Calculates the skewness of a distribution.
    /// Positive skewness indicates a right-skewed distribution, negative indicates left-skewed.
    /// </summary>
    /// <param name="source">A sequence of double values.</param>
    /// <returns>The skewness value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when source contains insufficient elements.</exception>
    public static double Skewness(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var moments = ComputeMoments(source);
        if (moments.Count < 3) throw new InvalidOperationException("Sequence must contain at least three elements");
        if (moments.M2 == 0) return 0;

        double n = moments.Count;
        double m2 = moments.M2 / n;
        double m3 = moments.M3 / n;

        // Sample skewness with bias correction.
        double skewness = m3 / (m2 * Math.Sqrt(m2));
        return skewness * Math.Sqrt(n * (n - 1)) / (n - 2);
    }

    /// <summary>
    /// Calculates the skewness of a distribution using a selector function.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">A sequence of values.</param>
    /// <param name="selector">A function to extract a double value from each element.</param>
    /// <returns>The skewness value.</returns>
    public static double Skewness<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).Skewness();
    }

    /// <summary>
    /// Calculates the excess kurtosis of a distribution.
    /// Positive kurtosis indicates heavy tails, negative indicates light tails.
    /// Excess kurtosis of 0 indicates a normal distribution.
    /// </summary>
    /// <param name="source">A sequence of double values.</param>
    /// <returns>The excess kurtosis value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when source contains insufficient elements.</exception>
    public static double Kurtosis(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var moments = ComputeMoments(source);
        if (moments.Count < 4) throw new InvalidOperationException("Sequence must contain at least four elements");
        if (moments.M2 == 0) return 0;

        double n = moments.Count;
        double m2 = moments.M2 / n;
        double m4 = moments.M4 / n;

        // Sample excess kurtosis with bias correction.
        double kurtosis = m4 / (m2 * m2) - 3.0;
        double adjustment = (n - 1) / ((n - 2) * (n - 3));
        return adjustment * ((n + 1) * kurtosis + 6);
    }

    /// <summary>
    /// Calculates the excess kurtosis of a distribution using a selector function.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">A sequence of values.</param>
    /// <param name="selector">A function to extract a double value from each element.</param>
    /// <returns>The excess kurtosis value.</returns>
    public static double Kurtosis<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).Kurtosis();
    }

    /// <summary>
    /// Calculates the range (maximum - minimum) of a sequence.
    /// </summary>
    /// <param name="source">A sequence of double values.</param>
    /// <returns>The range value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when source contains no elements.</exception>
    public static double Range(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext()) throw new InvalidOperationException("Sequence contains no elements");

        var min = enumerator.Current;
        var max = enumerator.Current;

        while (enumerator.MoveNext())
        {
            var value = enumerator.Current;
            if (value < min) min = value;
            if (value > max) max = value;
        }

        return max - min;
    }

    /// <summary>
    /// Calculates the range of a sequence using a selector function.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">A sequence of values.</param>
    /// <param name="selector">A function to extract a double value from each element.</param>
    /// <returns>The range value.</returns>
    public static double Range<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).Range();
    }

    /// <summary>
    /// Calculates the mean absolute deviation (MAD) from the mean.
    /// </summary>
    /// <param name="source">A sequence of double values.</param>
    /// <returns>The mean absolute deviation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when source contains no elements.</exception>
    public static double MeanAbsoluteDeviation(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var values = source.ToList();
        if (values.Count == 0) throw new InvalidOperationException("Sequence contains no elements");

        double sum = 0;
        for (int i = 0; i < values.Count; i++)
        {
            sum += values[i];
        }

        double mean = sum / values.Count;
        double absoluteDeviationSum = 0;

        for (int i = 0; i < values.Count; i++)
        {
            absoluteDeviationSum += Math.Abs(values[i] - mean);
        }

        return absoluteDeviationSum / values.Count;
    }

    private static List<double> MaterializeAndSort(IEnumerable<double> source)
    {
        var sorted = source.ToList();
        if (sorted.Count == 0) throw new InvalidOperationException("Sequence contains no elements");

        sorted.Sort();
        return sorted;
    }

    private static double PercentileFromSorted(IReadOnlyList<double> sorted, double percentile)
    {
        if (percentile == 0) return sorted[0];
        if (percentile == 100) return sorted[sorted.Count - 1];

        // Linear interpolation between closest ranks (R-7 / Excel).
        double rank = (percentile / 100.0) * (sorted.Count - 1);
        int lowerIndex = (int)Math.Floor(rank);
        int upperIndex = (int)Math.Ceiling(rank);

        if (lowerIndex == upperIndex)
        {
            return sorted[lowerIndex];
        }

        double fraction = rank - lowerIndex;
        return sorted[lowerIndex] + fraction * (sorted[upperIndex] - sorted[lowerIndex]);
    }

    private static MomentStats ComputeMoments(IEnumerable<double> source)
    {
        var stats = new MomentStats();

        foreach (var value in source)
        {
            stats.Add(value);
        }

        return stats;
    }

    private struct MomentStats
    {
        public int Count { get; private set; }
        public double Mean { get; private set; }
        public double M2 { get; private set; }
        public double M3 { get; private set; }
        public double M4 { get; private set; }

        public void Add(double value)
        {
            int n1 = Count;
            Count++;

            double delta = value - Mean;
            double deltaN = delta / Count;
            double deltaN2 = deltaN * deltaN;
            double term1 = delta * deltaN * n1;

            Mean += deltaN;
            M4 += term1 * deltaN2 * (Count * Count - 3 * Count + 3)
                + 6 * deltaN2 * M2
                - 4 * deltaN * M3;
            M3 += term1 * deltaN * (Count - 2) - 3 * deltaN * M2;
            M2 += term1;
        }
    }

    /// <summary>
    /// Calculates the mean absolute deviation using a selector function.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="source">A sequence of values.</param>
    /// <param name="selector">A function to extract a double value from each element.</param>
    /// <returns>The mean absolute deviation.</returns>
    public static double MeanAbsoluteDeviation<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).MeanAbsoluteDeviation();
    }
}
