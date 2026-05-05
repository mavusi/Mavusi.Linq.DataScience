# Commit Message

## feat: Add comprehensive Distribution Extensions (Phase 2.1)

### New Features
- Add DistributionExtensions class with 9 new statistical methods
- Median: Calculate 50th percentile for odd/even sequences
- Mode: Find most frequently occurring value (generic)
- Quartile: Calculate Q1, Q2, Q3 for five-number summary
- Percentile: Calculate any percentile (0-100) with linear interpolation
- InterquartileRange: Q3-Q1 for measuring dispersion
- Skewness: Measure distribution asymmetry with bias correction
- Kurtosis: Measure tail heaviness (excess kurtosis)
- Range: Calculate max-min difference
- MeanAbsoluteDeviation: Robust alternative to standard deviation

### Testing
- Add 39 comprehensive unit tests in DistributionExtensionsTests
- Include edge cases, selector tests, and real-world scenarios
- 100% test coverage with all 327 tests passing
- Tests pass on .NET 8, 9, and 10

### Documentation
- Add XML documentation for all public methods
- Update README.md with Distribution Extensions section
- Add detailed usage examples with real-world scenarios
- Create PHASE2_IMPLEMENTATION_SUMMARY.md with full details

### Technical Details
- Uses linear interpolation for percentiles (R-7/Excel method)
- Implements bias-corrected skewness and kurtosis formulas
- Consistent API design with selector function overloads
- Proper input validation with descriptive error messages

### Breaking Changes
None - This is a purely additive change

### Files Added
- Mavusi.Linq.DataScience/DistributionExtensions.cs
- Mavusi.Linq.DataScience.Tests/DistributionExtensionsTests.cs
- PHASE2_IMPLEMENTATION_SUMMARY.md
- FEATURE_ROADMAP.md

### Files Modified
- README.md

### Related Issues
Implements Phase 2.1 from FEATURE_ROADMAP.md

---

## Quick Start

```csharp
using Mavusi.Linq.DataScience;

var data = new[] { 65.0, 70.0, 75.0, 80.0, 85.0, 90.0, 95.0 };

// Central tendency
var median = data.Median();

// Quartiles for box plots
var q1 = data.Quartile(1);
var q3 = data.Quartile(3);
var iqr = data.InterquartileRange();

// Distribution shape
var skewness = data.Skewness();
var kurtosis = data.Kurtosis();

// With selectors
var employees = new[] { new { Salary = 50000.0 }, /* ... */ };
var medianSalary = employees.Median(e => e.Salary);
```
