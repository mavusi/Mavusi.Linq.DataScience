# Mavusi.Linq.DataScience - Feature Roadmap

## Current Implementation Status ✅

### Phase 1: Foundation (Complete)
- ✅ **Statistical Extensions**: Mean, Variance, Standard Deviation (Population & Sample)
- ✅ **Correlation Extensions**: Pearson Correlation, Covariance
- ✅ **Rolling Window Extensions**: Sliding windows, Moving averages, Rolling aggregations
- ✅ **Time Series Extensions**: Resampling, Differencing, Percentage change, EMA, Gap filling
- ✅ **Linear Algebra Extensions**: Vector/Matrix operations, Dot product, Transpose, Normalization

---

## Future Feature Roadmap 🚀

### Phase 2: Advanced Statistics (High Priority)
**Target: Q1 2025**

#### 2.1 Distribution Extensions
```csharp
public static class DistributionExtensions
{
    // Descriptive statistics
    double Median<T>(this IEnumerable<T> source);
    double Mode<T>(this IEnumerable<T> source);
    double Quartile<T>(this IEnumerable<T> source, int quartile); // Q1, Q2, Q3
    double InterquartileRange<T>(this IEnumerable<T> source);
    double Percentile<T>(this IEnumerable<T> source, double percentile);

    // Distribution shape
    double Skewness(this IEnumerable<double> source);
    double Kurtosis(this IEnumerable<double> source);

    // Range statistics
    double Range<T>(this IEnumerable<T> source);
    double MeanAbsoluteDeviation(this IEnumerable<double> source);
}
```

#### 2.2 Hypothesis Testing Extensions
```csharp
public static class HypothesisTestingExtensions
{
    // T-tests
    TTestResult TTest(this IEnumerable<double> sample1, IEnumerable<double> sample2);
    TTestResult OneSampleTTest(this IEnumerable<double> sample, double populationMean);

    // Chi-square test
    ChiSquareResult ChiSquareTest(this IEnumerable<int> observed, IEnumerable<int> expected);

    // Z-test
    ZTestResult ZTest(this IEnumerable<double> sample, double populationMean, double populationStdDev);

    // ANOVA
    AnovaResult OneWayAnova(this IEnumerable<IEnumerable<double>> groups);
}
```

#### 2.3 Outlier Detection Extensions
```csharp
public static class OutlierDetectionExtensions
{
    // IQR method
    IEnumerable<T> RemoveOutliersIQR<T>(this IEnumerable<T> source, Func<T, double> selector, double factor = 1.5);
    IEnumerable<OutlierInfo<T>> DetectOutliersIQR<T>(this IEnumerable<T> source, Func<T, double> selector, double factor = 1.5);

    // Z-score method
    IEnumerable<T> RemoveOutliersZScore<T>(this IEnumerable<T> source, Func<T, double> selector, double threshold = 3.0);
    IEnumerable<OutlierInfo<T>> DetectOutliersZScore<T>(this IEnumerable<T> source, Func<T, double> selector, double threshold = 3.0);

    // Modified Z-score (using median)
    IEnumerable<OutlierInfo<T>> DetectOutliersModifiedZScore<T>(this IEnumerable<T> source, Func<T, double> selector, double threshold = 3.5);
}
```

---

### Phase 3: Machine Learning Foundations (Medium Priority)
**Target: Q2 2025**

#### 3.1 Data Preprocessing Extensions
```csharp
public static class PreprocessingExtensions
{
    // Normalization
    IEnumerable<double> MinMaxNormalization(this IEnumerable<double> source, double min = 0, double max = 1);
    IEnumerable<double> ZScoreNormalization(this IEnumerable<double> source);
    IEnumerable<double> RobustScaling(this IEnumerable<double> source); // Using median and IQR

    // Binning
    IEnumerable<int> Bin<T>(this IEnumerable<T> source, int numberOfBins, Func<T, double> selector);
    IEnumerable<string> BinWithLabels<T>(this IEnumerable<T> source, double[] edges, string[] labels, Func<T, double> selector);

    // Encoding
    IEnumerable<int> LabelEncode<T>(this IEnumerable<T> source);
    Dictionary<T, double[]> OneHotEncode<T>(this IEnumerable<T> source);

    // Missing value handling
    IEnumerable<T> FillMissing<T>(this IEnumerable<T?> source, T fillValue) where T : struct;
    IEnumerable<double> InterpolateLinear(this IEnumerable<double?> source);
}
```

#### 3.2 Distance Metrics Extensions
```csharp
public static class DistanceMetricsExtensions
{
    // Distance calculations
    double EuclideanDistance(this IEnumerable<double> point1, IEnumerable<double> point2);
    double ManhattanDistance(this IEnumerable<double> point1, IEnumerable<double> point2);
    double CosineDistance(this IEnumerable<double> point1, IEnumerable<double> point2);
    double HammingDistance<T>(this IEnumerable<T> seq1, IEnumerable<T> seq2);
    double JaccardDistance<T>(this IEnumerable<T> set1, IEnumerable<T> set2);

    // Similarity
    double CosineSimilarity(this IEnumerable<double> vector1, IEnumerable<double> vector2);
    double JaccardSimilarity<T>(this IEnumerable<T> set1, IEnumerable<T> set2);
}
```

#### 3.3 Simple Clustering Extensions
```csharp
public static class ClusteringExtensions
{
    // K-Means clustering
    IEnumerable<Cluster<T>> KMeans<T>(this IEnumerable<T> source, 
        int k, 
        Func<T, double[]> featureSelector, 
        int maxIterations = 100);

    // Hierarchical clustering
    DendrogramNode<T> HierarchicalCluster<T>(this IEnumerable<T> source, 
        Func<T, T, double> distanceFunc);
}
```

---

### Phase 4: Advanced Time Series (Medium Priority)
**Target: Q2 2025**

#### 4.1 Advanced Time Series Extensions
```csharp
public static class AdvancedTimeSeriesExtensions
{
    // Seasonal decomposition
    SeasonalDecomposition Decompose(this IEnumerable<TimeSeriesPoint<double>> source, int period);

    // Autocorrelation
    IEnumerable<double> Autocorrelation(this IEnumerable<double> source, int maxLag);
    IEnumerable<double> PartialAutocorrelation(this IEnumerable<double> source, int maxLag);

    // Smoothing techniques
    IEnumerable<TimeSeriesPoint<double>> ExponentialSmoothing(this IEnumerable<TimeSeriesPoint<double>> source, double alpha);
    IEnumerable<TimeSeriesPoint<double>> DoubleExponentialSmoothing(this IEnumerable<TimeSeriesPoint<double>> source, double alpha, double beta);
    IEnumerable<TimeSeriesPoint<double>> TripleExponentialSmoothing(this IEnumerable<TimeSeriesPoint<double>> source, double alpha, double beta, double gamma, int seasonalPeriod);

    // Trend detection
    TrendInfo DetectTrend(this IEnumerable<TimeSeriesPoint<double>> source);
    IEnumerable<TimeSeriesPoint<double>> Detrend(this IEnumerable<TimeSeriesPoint<double>> source);

    // Stationarity tests
    bool IsStationary(this IEnumerable<double> source); // Augmented Dickey-Fuller test
}
```

#### 4.2 Time Series Forecasting Extensions
```csharp
public static class ForecastingExtensions
{
    // Simple forecasting
    IEnumerable<TimeSeriesPoint<double>> NaiveForecast(this IEnumerable<TimeSeriesPoint<double>> source, int periods);
    IEnumerable<TimeSeriesPoint<double>> SeasonalNaiveForecast(this IEnumerable<TimeSeriesPoint<double>> source, int periods, int seasonalPeriod);
    IEnumerable<TimeSeriesPoint<double>> DriftForecast(this IEnumerable<TimeSeriesPoint<double>> source, int periods);

    // Linear regression forecast
    IEnumerable<TimeSeriesPoint<double>> LinearTrendForecast(this IEnumerable<TimeSeriesPoint<double>> source, int periods);
}
```

---

### Phase 5: Regression & Classification Basics (Lower Priority)
**Target: Q3 2025**

#### 5.1 Regression Extensions
```csharp
public static class RegressionExtensions
{
    // Simple linear regression
    LinearRegressionResult SimpleLinearRegression(this IEnumerable<(double x, double y)> source);

    // Multiple linear regression
    MultipleRegressionResult MultipleLinearRegression<T>(this IEnumerable<T> source, 
        Func<T, double[]> featuresSelector, 
        Func<T, double> targetSelector);

    // Polynomial regression
    PolynomialRegressionResult PolynomialRegression(this IEnumerable<(double x, double y)> source, int degree);

    // Evaluation metrics
    double MeanSquaredError(this IEnumerable<double> actual, IEnumerable<double> predicted);
    double MeanAbsoluteError(this IEnumerable<double> actual, IEnumerable<double> predicted);
    double RSquared(this IEnumerable<double> actual, IEnumerable<double> predicted);
    double RootMeanSquaredError(this IEnumerable<double> actual, IEnumerable<double> predicted);
}
```

#### 5.2 Classification Metrics Extensions
```csharp
public static class ClassificationMetricsExtensions
{
    // Confusion matrix
    ConfusionMatrix<T> CreateConfusionMatrix<T>(this IEnumerable<T> actual, IEnumerable<T> predicted);

    // Binary classification metrics
    double Accuracy<T>(this IEnumerable<T> actual, IEnumerable<T> predicted);
    double Precision<T>(this IEnumerable<T> actual, IEnumerable<T> predicted, T positiveClass);
    double Recall<T>(this IEnumerable<T> actual, IEnumerable<T> predicted, T positiveClass);
    double F1Score<T>(this IEnumerable<T> actual, IEnumerable<T> predicted, T positiveClass);

    // Multi-class metrics
    double MacroAveragedF1Score<T>(this IEnumerable<T> actual, IEnumerable<T> predicted);
    double WeightedF1Score<T>(this IEnumerable<T> actual, IEnumerable<T> predicted);
}
```

---

### Phase 6: Data Transformation & Feature Engineering (Lower Priority)
**Target: Q4 2025**

#### 6.1 Feature Engineering Extensions
```csharp
public static class FeatureEngineeringExtensions
{
    // Polynomial features
    IEnumerable<double[]> CreatePolynomialFeatures(this IEnumerable<double[]> source, int degree);

    // Interaction features
    IEnumerable<double[]> CreateInteractionFeatures(this IEnumerable<double[]> source);

    // Lag features (for time series)
    IEnumerable<T> CreateLagFeatures<T>(this IEnumerable<T> source, int lag);
    IEnumerable<double[]> CreateMultipleLagFeatures<T>(this IEnumerable<T> source, int[] lags, Func<T, double> selector);

    // Window features
    IEnumerable<WindowFeatures> CreateWindowFeatures<T>(this IEnumerable<T> source, 
        int windowSize, 
        Func<T, double> selector);
}
```

#### 6.2 Data Sampling Extensions
```csharp
public static class SamplingExtensions
{
    // Random sampling
    IEnumerable<T> RandomSample<T>(this IEnumerable<T> source, int count, int? seed = null);
    IEnumerable<T> RandomSamplePercentage<T>(this IEnumerable<T> source, double percentage, int? seed = null);

    // Stratified sampling
    IEnumerable<T> StratifiedSample<T, TKey>(this IEnumerable<T> source, 
        Func<T, TKey> stratifySelector, 
        int countPerStratum, 
        int? seed = null);

    // Train-test split
    (IEnumerable<T> Train, IEnumerable<T> Test) TrainTestSplit<T>(this IEnumerable<T> source, 
        double testSize = 0.2, 
        int? seed = null);

    // Cross-validation folds
    IEnumerable<(IEnumerable<T> Train, IEnumerable<T> Test)> CreateKFolds<T>(this IEnumerable<T> source, 
        int k = 5, 
        int? seed = null);

    // Bootstrap sampling
    IEnumerable<IEnumerable<T>> Bootstrap<T>(this IEnumerable<T> source, 
        int numberOfSamples, 
        int? seed = null);
}
```

---

### Phase 7: Advanced Correlation & Dependencies (Lower Priority)
**Target: Q4 2025**

#### 7.1 Advanced Correlation Extensions
```csharp
public static class AdvancedCorrelationExtensions
{
    // Non-parametric correlations
    double SpearmanCorrelation(this IEnumerable<double> x, IEnumerable<double> y);
    double KendallTauCorrelation(this IEnumerable<double> x, IEnumerable<double> y);

    // Correlation matrix
    Matrix CorrelationMatrix<T>(this IEnumerable<T> source, params Func<T, double>[] selectors);

    // Partial correlation
    double PartialCorrelation(this IEnumerable<double> x, 
        IEnumerable<double> y, 
        IEnumerable<IEnumerable<double>> controlVariables);

    // Cross-correlation
    IEnumerable<double> CrossCorrelation(this IEnumerable<double> signal1, 
        IEnumerable<double> signal2, 
        int maxLag);
}
```

---

### Phase 8: Signal Processing (Future/Optional)
**Target: 2026**

#### 8.1 Signal Processing Extensions
```csharp
public static class SignalProcessingExtensions
{
    // Filtering
    IEnumerable<double> LowPassFilter(this IEnumerable<double> signal, double cutoffFrequency, double samplingRate);
    IEnumerable<double> HighPassFilter(this IEnumerable<double> signal, double cutoffFrequency, double samplingRate);
    IEnumerable<double> BandPassFilter(this IEnumerable<double> signal, double lowFreq, double highFreq, double samplingRate);

    // Convolution
    IEnumerable<double> Convolve(this IEnumerable<double> signal, IEnumerable<double> kernel);

    // Peak detection
    IEnumerable<PeakInfo> DetectPeaks(this IEnumerable<double> signal, double threshold, int minDistance);

    // FFT (Fast Fourier Transform) - may require external library
    ComplexNumber[] FFT(this IEnumerable<double> signal);
}
```

---

### Phase 9: Performance Optimization (Ongoing)
**Target: Ongoing across all phases**

#### 9.1 Performance Enhancements
- ✨ Add `Span<T>` and `Memory<T>` support for zero-allocation operations
- ✨ Implement parallel processing for large datasets using `PLINQ`
- ✨ Add lazy evaluation where appropriate
- ✨ Implement buffering strategies for streaming data
- ✨ Add benchmarking suite using BenchmarkDotNet

#### 9.2 API Improvements
- ✨ Add async/await support for operations on large datasets
- ✨ Provide both materialized (ToList) and streaming (IEnumerable) versions
- ✨ Add comprehensive XML documentation
- ✨ Create fluent API builders for complex operations

---

### Phase 10: Visualization Helpers (Future/Optional)
**Target: 2026**

#### 10.1 Data Preparation for Visualization
```csharp
public static class VisualizationExtensions
{
    // Histogram data
    HistogramData CreateHistogram<T>(this IEnumerable<T> source, int bins, Func<T, double> selector);

    // Box plot data
    BoxPlotData CreateBoxPlot<T>(this IEnumerable<T> source, Func<T, double> selector);

    // Scatter plot data
    IEnumerable<(double x, double y)> ToScatterData<T>(this IEnumerable<T> source, 
        Func<T, double> xSelector, 
        Func<T, double> ySelector);

    // Heatmap data
    double[,] ToHeatmap<T>(this IEnumerable<T> source, 
        Func<T, int> rowSelector, 
        Func<T, int> colSelector, 
        Func<IEnumerable<T>, double> aggregator);
}
```

---

## Priority Matrix

### High Priority (Next 3-6 months)
1. **Distribution Extensions** - Essential statistical functions (Median, Quartiles, Percentiles)
2. **Outlier Detection** - Critical for data cleaning
3. **Data Preprocessing** - Normalization and scaling are fundamental
4. **Distance Metrics** - Foundation for many algorithms

### Medium Priority (6-12 months)
1. **Hypothesis Testing** - Statistical validation
2. **Advanced Time Series** - Extend existing time series capabilities
3. **Clustering** - Basic unsupervised learning
4. **Feature Engineering** - Data transformation utilities

### Lower Priority (12+ months)
1. **Regression Extensions** - Predictive modeling
2. **Classification Metrics** - Model evaluation
3. **Advanced Correlation** - Specialized statistical measures
4. **Signal Processing** - Specialized domain

---

## Implementation Guidelines

### Code Quality Standards
- ✅ Maintain consistent API design with existing extensions
- ✅ Include comprehensive unit tests for all new methods (95%+ coverage)
- ✅ Add XML documentation with examples
- ✅ Follow SOLID principles and keep methods focused
- ✅ Support both direct values and selectors
- ✅ Validate inputs and provide clear error messages

### Testing Strategy
- Unit tests for each method
- Integration tests for combined operations
- Performance benchmarks for large datasets
- Edge case handling (empty sequences, null values, etc.)

### Documentation
- Update README.md with new features
- Add code examples for common scenarios
- Create tutorial documentation
- Add benchmark results

---

## Success Metrics

### Library Adoption
- NuGet download count
- GitHub stars and forks
- Community contributions
- Usage in production projects

### Code Quality
- Test coverage > 95%
- No critical code smells
- Performance benchmarks meet targets
- Zero security vulnerabilities

### Community Engagement
- Active issue resolution
- Regular releases (every 2-3 months)
- Community feature requests incorporated
- Documentation completeness

---

## Notes & Considerations

### Dependencies
- Keep external dependencies minimal
- Consider optional dependencies for advanced features (e.g., MathNet.Numerics for FFT)
- Target latest LTS .NET versions

### Breaking Changes
- Maintain semantic versioning
- Provide migration guides for major versions
- Deprecate features before removal

### Performance
- Profile before optimizing
- Consider memory allocation patterns
- Benchmark against existing libraries (e.g., MathNet.Numerics, Accord.NET)

---

## Conclusion

This roadmap provides a comprehensive plan for expanding Mavusi.Linq.DataScience into a complete data science toolkit while maintaining the library's LINQ-focused philosophy. The phased approach allows for incremental development, testing, and community feedback.

**Next Immediate Steps:**
1. Implement Distribution Extensions (Median, Quartiles, Percentiles)
2. Add Outlier Detection (IQR and Z-Score methods)
3. Create comprehensive examples and tutorials
4. Set up performance benchmarking infrastructure

---

*Last Updated: January 2025*
*Version: 2.0*
