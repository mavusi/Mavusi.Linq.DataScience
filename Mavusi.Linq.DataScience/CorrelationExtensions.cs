namespace Mavusi.Linq.DataScience;

public static class CorrelationExtensions
{
    /// <summary>
    /// Calculates the Pearson correlation coefficient between two sequences.
    /// Returns a value between -1 and 1, where 1 is perfect positive correlation,
    /// -1 is perfect negative correlation, and 0 is no correlation.
    /// </summary>
    public static double Correlation(this IEnumerable<double> sourceX, IEnumerable<double> sourceY)
    {
        if (sourceX == null) throw new ArgumentNullException(nameof(sourceX));
        if (sourceY == null) throw new ArgumentNullException(nameof(sourceY));

        var x = sourceX.ToList();
        var y = sourceY.ToList();

        if (x.Count == 0) throw new InvalidOperationException("Source X contains no elements");
        if (y.Count == 0) throw new InvalidOperationException("Source Y contains no elements");
        if (x.Count != y.Count) throw new ArgumentException("Sequences must have the same length");

        var meanX = x.Average();
        var meanY = y.Average();

        var covariance = x.Zip(y, (xi, yi) => (xi - meanX) * (yi - meanY)).Sum();
        var stdX = Math.Sqrt(x.Sum(xi => Math.Pow(xi - meanX, 2)));
        var stdY = Math.Sqrt(y.Sum(yi => Math.Pow(yi - meanY, 2)));

        if (stdX == 0 || stdY == 0) return 0;

        return covariance / (stdX * stdY);
    }

    /// <summary>
    /// Calculates the Pearson correlation coefficient between two sequences using selectors.
    /// </summary>
    public static double Correlation<T>(this IEnumerable<T> source, 
        Func<T, double> selectorX, 
        Func<T, double> selectorY)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selectorX == null) throw new ArgumentNullException(nameof(selectorX));
        if (selectorY == null) throw new ArgumentNullException(nameof(selectorY));

        var values = source.ToList();
        var x = values.Select(selectorX);
        var y = values.Select(selectorY);

        return x.Correlation(y);
    }

    /// <summary>
    /// Calculates the covariance between two sequences.
    /// </summary>
    public static double Covariance(this IEnumerable<double> sourceX, IEnumerable<double> sourceY)
    {
        if (sourceX == null) throw new ArgumentNullException(nameof(sourceX));
        if (sourceY == null) throw new ArgumentNullException(nameof(sourceY));

        var x = sourceX.ToList();
        var y = sourceY.ToList();

        if (x.Count == 0) throw new InvalidOperationException("Source X contains no elements");
        if (y.Count == 0) throw new InvalidOperationException("Source Y contains no elements");
        if (x.Count != y.Count) throw new ArgumentException("Sequences must have the same length");

        var meanX = x.Average();
        var meanY = y.Average();

        return x.Zip(y, (xi, yi) => (xi - meanX) * (yi - meanY)).Sum() / x.Count;
    }

    /// <summary>
    /// Calculates the covariance between two sequences using selectors.
    /// </summary>
    public static double Covariance<T>(this IEnumerable<T> source, 
        Func<T, double> selectorX, 
        Func<T, double> selectorY)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selectorX == null) throw new ArgumentNullException(nameof(selectorX));
        if (selectorY == null) throw new ArgumentNullException(nameof(selectorY));

        var values = source.ToList();
        var x = values.Select(selectorX);
        var y = values.Select(selectorY);

        return x.Covariance(y);
    }
}
