# NuGet Packages v3.0.0 Release

## Summary

Successfully created NuGet packages for both projects with version **3.0.0**.

## Package Details

### 📦 Mavusi.Linq.DataScience v3.0.0
- **Package File**: `Mavusi.Linq.DataScience.3.0.0.nupkg` (75.4 KB)
- **Symbols File**: `Mavusi.Linq.DataScience.3.0.0.snupkg` (40.0 KB)
- **Target Frameworks**: .NET 8.0, 9.0, 10.0
- **Dependencies**: None (zero external dependencies)
- **Description**: CPU-optimized core library with statistical, correlation, distribution, time-series, linear algebra, and geospatial operations

### 📦 Mavusi.Linq.DataScience.GpuBound v3.0.0
- **Package File**: `Mavusi.Linq.DataScience.GpuBound.3.0.0.nupkg` (74.4 KB)
- **Symbols File**: `Mavusi.Linq.DataScience.GpuBound.3.0.0.snupkg` (43.0 KB)
- **Target Frameworks**: .NET 8.0, 9.0, 10.0
- **Dependencies**: 
  - ILGPU 1.5.3
  - ILGPU.Algorithms 1.5.3
  - Mavusi.Linq.DataScience.Models
- **Description**: GPU-accelerated extensions providing up to 100x performance on large datasets

## Package Location

All packages are located in: `.\nupkgs\`

## Key Changes in v3.0.0

### Architecture Separation
- **Split into two packages** for better dependency management
- Users can install core library without ILGPU dependencies
- GPU acceleration is now optional via separate package

### Core Package Updates
- Removed ILGPU dependencies
- Updated package description to reflect CPU-optimized nature
- Multi-targets .NET 8, 9, and 10
- Clean separation of concerns

### GPU Package Features
- Dedicated package for GPU acceleration
- All GPU methods suffixed with `Gpu`
- Automatic hardware detection (CUDA, OpenCL, CPU fallback)
- Comprehensive NuGet metadata for discoverability
- Includes dedicated README.md

## Documentation

Both packages include:
- ✅ Comprehensive README.md files
- ✅ XML documentation comments
- ✅ Usage examples
- ✅ Performance benchmarks
- ✅ Hardware requirements
- ✅ Architecture diagrams

### README Files Created/Updated

1. **Main README.md** - Updated to reflect package separation with:
   - Clear installation instructions for both packages
   - Separate sections for CPU and GPU methods
   - Architecture comparison table
   - Performance guidelines
   - Comprehensive examples for all features

2. **Mavusi.Linq.DataScience.GpuBound/README.md** - New comprehensive guide with:
   - Package overview and benefits
   - Installation instructions
   - Complete API reference
   - Usage examples for all GPU methods
   - Performance benchmarks
   - Hardware requirements
   - Comparison with CPU package

## Publishing to NuGet.org

To publish these packages to NuGet.org:

```powershell
# Set your NuGet API key (one time)
dotnet nuget setapikey YOUR_API_KEY --source https://api.nuget.org/v3/index.json

# Push the core package
dotnet nuget push ".\nupkgs\Mavusi.Linq.DataScience.3.0.0.nupkg" --source https://api.nuget.org/v3/index.json

# Push the GPU package
dotnet nuget push ".\nupkgs\Mavusi.Linq.DataScience.GpuBound.3.0.0.nupkg" --source https://api.nuget.org/v3/index.json
```

The symbol packages (.snupkg) will be automatically pushed with the main packages.

## Installation Commands

### For Users

```bash
# Install core package (required)
dotnet add package Mavusi.Linq.DataScience --version 3.0.0

# Install GPU acceleration (optional)
dotnet add package Mavusi.Linq.DataScience.GpuBound --version 3.0.0
```

## Package Metadata Highlights

### Tags for Discoverability
Both packages include comprehensive tags:
- Core: `linq`, `data-science`, `statistics`, `correlation`, `time-series`, `linear-algebra`, `geospatial`
- GPU: `gpu`, `cuda`, `opencl`, `ilgpu`, `parallel`, `high-performance`, `acceleration`

### License
- MIT License (permissive open source)

### Source Control
- Published repository URL
- Source Link enabled for debugging
- Symbol packages for enhanced debugging experience

## Testing Status

- ✅ 285+ comprehensive tests
- ✅ Both CPU and GPU implementations verified
- ✅ Edge case handling validated
- ✅ Hardware compatibility tested

## Next Steps

1. **Review** the packages in the `nupkgs` folder
2. **Test** installation in a separate project
3. **Publish** to NuGet.org when ready
4. **Update** GitHub release with packages
5. **Announce** the new separated architecture

## Breaking Changes from v2.0.0

⚠️ **Important**: This is a breaking change for users who relied on GPU methods in the core package.

**Migration Guide**:
```bash
# If you were using GPU methods, add the new package
dotnet add package Mavusi.Linq.DataScience.GpuBound --version 3.0.0
```

Then add the using directive:
```csharp
using Mavusi.Linq.DataScience.GpuBound; // For GPU methods
```

All CPU methods remain unchanged in the core package.

## Support

- 📚 [Documentation](https://github.com/mavusi/Mavusi.Linq.DataScience)
- 🐛 [Report Issues](https://github.com/mavusi/Mavusi.Linq.DataScience/issues)
- 💬 [Discussions](https://github.com/mavusi/Mavusi.Linq.DataScience/discussions)

---

**Release Date**: May 5, 2026  
**Version**: 3.0.0  
**Status**: Ready for publishing
