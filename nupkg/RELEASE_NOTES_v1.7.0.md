# Mavusi.Linq.DataScience v1.7.0 Release Notes

## 🎉 What's New in v1.7.0

### 🌍 Geospatial Extensions (NEW!)

We're excited to introduce comprehensive geospatial analysis capabilities to Mavusi.Linq.DataScience!

#### New Features:

1. **Distance Calculations**
   - `HaversineDistance()` - Calculate great-circle distances in kilometers
   - `HaversineDistanceMiles()` - Calculate distances in miles
   - Accurate Earth-curvature calculations using the Haversine formula

2. **Spatial Filtering**
   - `WithinRadius()` - Find all items within a specified radius from a center point
   - `WithinBounds()` - Filter items within a geographical bounding box

3. **Proximity Search**
   - `Nearest()` - Find the closest item to a target location
   - `NearestN()` - Find the N nearest items to a target location

4. **Geographic Analysis**
   - `GeographicCenter()` - Calculate the centroid of multiple coordinates
   - `BoundingBox()` - Create geographical bounds (min/max lat/lon)
   - `GroupByProximity()` - Cluster nearby points based on distance threshold

5. **Route Calculations**
   - `ConsecutiveDistances()` - Calculate distances between consecutive waypoints
   - `PairwiseDistances()` - Calculate all pairwise distances between coordinates
   - `TotalDistance()` - Calculate total route distance

6. **Data Types**
   - `GeoCoordinate` - Immutable record for latitude/longitude
   - `GeoBounds` - Bounding box with Contains() method
   - `GeoDistance` - Distance result with both km and miles

#### Use Cases:

- 📍 Location-based queries (find nearby stores, restaurants, etc.)
- 🚚 Delivery radius calculations
- 🗺️ Trip planning and route optimization
- 📊 Spatial data analysis
- 🎯 Proximity-based recommendations
- 📦 Warehouse location optimization
- 🏪 Store location analysis

#### Example Usage:

```csharp
using Mavusi.Linq.DataScience;

// Define locations
var myLocation = new GeoCoordinate(40.7128, -74.0060); // New York
var stores = new[]
{
    new { Name = "Store A", Location = new GeoCoordinate(40.748, -73.985) },
    new { Name = "Store B", Location = new GeoCoordinate(40.712, -74.006) },
    new { Name = "Store C", Location = new GeoCoordinate(41.878, -87.629) }
};

// Find stores within 5km
var nearbyStores = stores
    .WithinRadius(s => s.Location, myLocation, radiusKm: 5.0)
    .ToList();

// Find 2 nearest stores
var nearest = stores
    .NearestN(s => s.Location, myLocation, 2)
    .ToList();

// Calculate route distance
var route = new[]
{
    new GeoCoordinate(40.7128, -74.0060),
    new GeoCoordinate(34.0522, -118.2437),
    new GeoCoordinate(41.8781, -87.6298)
};
var totalKm = route.TotalDistance();
```

## 📦 Package Information

- **Package**: Mavusi.Linq.DataScience.1.7.0.nupkg
- **Symbols**: Mavusi.Linq.DataScience.1.7.0.snupkg
- **Targets**: .NET 8.0, .NET 9.0, .NET 10.0
- **Total Tests**: 23 new geospatial tests (all passing)
- **Package Size**: ~78 KB

## 🧪 Testing

All 23 new geospatial extension tests are passing:
- Distance calculation tests (NYC to LA, London to Paris, etc.)
- Radius filtering tests
- Nearest neighbor search tests
- Geographic center calculations
- Bounding box operations
- Route distance calculations
- Proximity clustering
- Real-world scenarios (restaurants, delivery, trip planning)
- Edge cases and error handling

## 📚 Documentation

The README.md has been updated with:
- Complete geospatial feature list
- Comprehensive usage examples
- Real-world use case examples
- All public APIs are documented with XML comments

## 🚀 How to Install

```bash
dotnet add package Mavusi.Linq.DataScience --version 1.7.0
```

Or via NuGet Package Manager:
```
Install-Package Mavusi.Linq.DataScience -Version 1.7.0
```

## 📝 Previous Features (Still Available)

- ✅ Statistical Extensions (Standard Deviation, Variance)
- ✅ Distribution Extensions (Median, Mode, Quartiles, Percentiles, Skewness, Kurtosis)
- ✅ Correlation Extensions (Pearson, Covariance)
- ✅ Rolling Window Extensions (Moving Averages, Custom Aggregations)
- ✅ Time-Series Extensions (Resampling, Differencing, EMA)
- ✅ Linear Algebra Extensions (Vectors, Matrices, Operations)

## 🔗 Links

- **GitHub**: https://github.com/mavusi/Mavusi.Linq.DataScience
- **NuGet**: https://www.nuget.org/packages/Mavusi.Linq.DataScience
- **License**: MIT

## 🙏 Acknowledgments

Thank you for using Mavusi.Linq.DataScience! We hope these new geospatial features help you build amazing location-aware applications.

---

**Build Date**: May 5, 2026  
**Package Location**: `C:\dev\personal\Mavusi.Linq.DataScience\nupkg\`
