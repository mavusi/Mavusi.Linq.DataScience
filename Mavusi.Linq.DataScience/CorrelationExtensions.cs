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
        var stats = ComputePairStats(sourceX, sourceY);

        if (stats.SumSquaresX <= 0 || stats.SumSquaresY <= 0) return 0;

        return stats.SumCross / Math.Sqrt(stats.SumSquaresX * stats.SumSquaresY);
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

        var stats = ComputePairStats(source, selectorX, selectorY);

        if (stats.SumSquaresX <= 0 || stats.SumSquaresY <= 0) return 0;

        return stats.SumCross / Math.Sqrt(stats.SumSquaresX * stats.SumSquaresY);
    }

    /// <summary>
    /// Calculates the covariance between two sequences.
    /// </summary>
    public static double Covariance(this IEnumerable<double> sourceX, IEnumerable<double> sourceY)
    {
        if (sourceX == null) throw new ArgumentNullException(nameof(sourceX));
        if (sourceY == null) throw new ArgumentNullException(nameof(sourceY));

        var stats = ComputePairStats(sourceX, sourceY);
        return stats.SumCross / stats.Count;
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

        var stats = ComputePairStats(source, selectorX, selectorY);
        return stats.SumCross / stats.Count;
    }

    private static PairStats ComputePairStats(IEnumerable<double> sourceX, IEnumerable<double> sourceY)
    {
        using var enumX = sourceX.GetEnumerator();
        using var enumY = sourceY.GetEnumerator();

        if (!enumX.MoveNext()) throw new InvalidOperationException("Source X contains no elements");
        if (!enumY.MoveNext()) throw new InvalidOperationException("Source Y contains no elements");

        var stats = new PairStats();
        stats.Add(enumX.Current, enumY.Current);

        while (true)
        {
            var hasX = enumX.MoveNext();
            var hasY = enumY.MoveNext();

            if (hasX != hasY) throw new ArgumentException("Sequences must have the same length");
            if (!hasX) break;

            stats.Add(enumX.Current, enumY.Current);
        }

        return stats;
    }

    private static PairStats ComputePairStats<T>(
        IEnumerable<T> source,
        Func<T, double> selectorX,
        Func<T, double> selectorY)
    {
        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext()) throw new InvalidOperationException("Source contains no elements");

        var stats = new PairStats();

        do
        {
            var item = enumerator.Current;
            stats.Add(selectorX(item), selectorY(item));
        }
        while (enumerator.MoveNext());

        return stats;
    }

    private struct PairStats
    {
        public int Count { get; private set; }
        public double MeanX { get; private set; }
        public double MeanY { get; private set; }
        public double SumSquaresX { get; private set; }
        public double SumSquaresY { get; private set; }
        public double SumCross { get; private set; }

        public void Add(double x, double y)
        {
            Count++;

            var dx = x - MeanX;
            MeanX += dx / Count;

            var dy = y - MeanY;
            MeanY += dy / Count;

            SumSquaresX += dx * (x - MeanX);
            SumSquaresY += dy * (y - MeanY);
            SumCross += dx * (y - MeanY);
        }
    }
}
