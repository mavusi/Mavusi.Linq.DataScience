# Publishing to NuGet

This guide explains how to publish the Mavusi.Linq.DataScience package to NuGet.org.

## Prerequisites

1. **NuGet Account**: Create an account at [nuget.org](https://www.nuget.org/)
2. **API Key**: Generate an API key from your NuGet account
   - Go to [API Keys](https://www.nuget.org/account/apikeys)
   - Click "Create"
   - Set appropriate scopes (Push and Unlist for your package)
   - Copy the API key (you'll only see it once!)

## Building the Package

The package has already been built and is located in the `nupkg` folder:
- **Main Package**: `Mavusi.Linq.DataScience.2.0.0.nupkg` (127 KB)
- **Symbol Package**: `Mavusi.Linq.DataScience.2.0.0.snupkg` (57 KB)

To rebuild the package:

```powershell
dotnet pack Mavusi.Linq.DataScience\Mavusi.Linq.DataScience.csproj -c Release -o .\nupkg
```

## Testing the Package Locally

Before publishing, test the package locally:

```powershell
# Create a test project
mkdir TestProject
cd TestProject
dotnet new console

# Add the local package
dotnet add package Mavusi.Linq.DataScience --source C:\dev\personal\Mavusi.Linq.DataScience\nupkg --version 2.0.0

# Test GPU features
# Edit Program.cs and add:
# using Mavusi.Linq.DataScience.GpuBound;
# var data = Enumerable.Range(0, 10000).Select(i => (double)i).ToArray();
# var stdDev = data.StandardDeviationGpu();
# Console.WriteLine($"Standard Deviation: {stdDev}");

dotnet run
```

## Publishing to NuGet

### Option 1: Using dotnet CLI (Recommended)

```powershell
# Set your API key (do this once)
dotnet nuget push nupkg\Mavusi.Linq.DataScience.2.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json

# The symbol package (.snupkg) is automatically pushed with the main package
```

### Option 2: Using NuGet Web Interface

1. Go to [nuget.org/packages/manage/upload](https://www.nuget.org/packages/manage/upload)
2. Select your `.nupkg` file
3. Click "Upload"
4. The symbol package will be uploaded automatically if present

## Post-Publishing

After publishing:

1. **Verify the Package**: Visit https://www.nuget.org/packages/Mavusi.Linq.DataScience/
2. **Wait for Indexing**: It may take a few minutes for the package to be searchable
3. **Test Installation**: Try installing the package in a fresh project

```powershell
dotnet add package Mavusi.Linq.DataScience --version 2.0.0
```

## Version Management

Current version: **2.0.0** (GPU Acceleration Release)

To update the version for future releases:
1. Edit `Mavusi.Linq.DataScience\Mavusi.Linq.DataScience.csproj`
2. Update the `<Version>` and `<PackageReleaseNotes>` elements
3. Rebuild and republish

### Semantic Versioning

Follow [Semantic Versioning](https://semver.org/):
- **Major** (X.0.0): Breaking changes (e.g., 1.7.0 → 2.0.0 for GPU features)
- **Minor** (2.X.0): New features, backward compatible
- **Patch** (2.0.X): Bug fixes, backward compatible

## Package Contents

The package includes:

- ✅ All CPU-based statistical extensions
- ✅ All GPU-accelerated extensions (ILGPU)
- ✅ XML documentation for IntelliSense
- ✅ Source Link for debugging
- ✅ Symbol files (.pdb) for debugging
- ✅ Multi-targeted: .NET 8.0, 9.0, 10.0
- ✅ README.md embedded in package
- ✅ MIT License

## Package Dependencies

The package depends on:
- **ILGPU** (v1.5.3) - GPU computing framework
- **ILGPU.Algorithms** (v1.5.3) - GPU algorithms

These are automatically installed when users add the package.

## Troubleshooting

### Error: "The specified API key is invalid"
- Verify your API key is correct
- Check that the API key has the correct scopes (Push and Unlist)
- Ensure the API key hasn't expired

### Error: "Package already exists"
- You cannot republish the same version
- Increment the version number and rebuild

### Warning: "Missing XML documentation"
- These warnings are normal and don't affect the package
- The library includes XML docs for all public APIs

## Support

For issues or questions:
- **GitHub Issues**: https://github.com/mavusi/Mavusi.Linq.DataScience/issues
- **NuGet Package**: https://www.nuget.org/packages/Mavusi.Linq.DataScience

## Release Checklist

Before publishing a new version:

- [ ] All tests pass (`dotnet test`)
- [ ] Build succeeds (`dotnet build`)
- [ ] Version number updated in `.csproj`
- [ ] Release notes updated in `.csproj`
- [ ] README.md reflects new features
- [ ] Local package tested
- [ ] Git changes committed
- [ ] Git tag created (e.g., `v2.0.0`)
- [ ] Package built (`dotnet pack`)
- [ ] Package published to NuGet
- [ ] GitHub release created with notes

## Quick Publish Script

Save this as `publish.ps1` for quick publishing:

```powershell
param(
    [Parameter(Mandatory=$true)]
    [string]$ApiKey,

    [Parameter(Mandatory=$false)]
    [string]$Version = "2.0.0"
)

# Build and test
Write-Host "Building and testing..." -ForegroundColor Cyan
dotnet test
if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed!" -ForegroundColor Red
    exit 1
}

# Pack
Write-Host "Creating package..." -ForegroundColor Cyan
dotnet pack Mavusi.Linq.DataScience\Mavusi.Linq.DataScience.csproj -c Release -o .\nupkg

# Publish
Write-Host "Publishing to NuGet..." -ForegroundColor Cyan
dotnet nuget push "nupkg\Mavusi.Linq.DataScience.$Version.nupkg" --api-key $ApiKey --source https://api.nuget.org/v3/index.json

Write-Host "Done!" -ForegroundColor Green
Write-Host "Package URL: https://www.nuget.org/packages/Mavusi.Linq.DataScience/$Version" -ForegroundColor Cyan
```

Usage:
```powershell
.\publish.ps1 -ApiKey "YOUR_API_KEY"
```
