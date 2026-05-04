# Mavusi.Linq.DataScience - NuGet Package Configuration

## Project Configuration

### Multi-Target Framework Support
The project now targets:
- ✅ .NET 8.0
- ✅ .NET 9.0
- ✅ .NET 10.0

### NuGet Package Metadata

| Property | Value |
|----------|-------|
| **Package ID** | Mavusi.Linq.DataScience |
| **Version** | 1.0.0 |
| **Authors** | Mavusi |
| **License** | MIT |
| **Repository** | https://github.com/mavusi/Mavusi.Linq.DataScience |
| **Package Tags** | linq, data-science, statistics, correlation, time-series, rolling-windows, linear-algebra, machine-learning, analytics, math |

### Features Included
- ✅ XML Documentation (GenerateDocumentationFile)
- ✅ Source Link Integration (Microsoft.SourceLink.GitHub)
- ✅ Symbol Package (.snupkg format)
- ✅ Embedded Source Files
- ✅ README.md included in package
- ✅ Repository URL publishing

## PowerShell One-Liner to Package

```powershell
dotnet pack .\Mavusi.Linq.DataScience\Mavusi.Linq.DataScience.csproj -c Release -o .\nupkg
```

### What this command does:
1. **dotnet pack** - Creates a NuGet package
2. **-c Release** - Uses Release configuration (optimized build)
3. **-o .\nupkg** - Outputs packages to the `nupkg` folder

## Package Output

The command creates:
1. **Mavusi.Linq.DataScience.1.0.0.nupkg** (~49 KB)
   - Contains compiled assemblies for all target frameworks
   - Includes XML documentation
   - Includes README.md

2. **Mavusi.Linq.DataScience.1.0.0.snupkg** (~34 KB)
   - Contains debugging symbols
   - Enables source debugging for consumers

## Publishing to NuGet.org

To publish the package to NuGet.org:

```powershell
dotnet nuget push .\nupkg\Mavusi.Linq.DataScience.1.0.0.nupkg -k YOUR_API_KEY -s https://api.nuget.org/v3/index.json
```

Replace `YOUR_API_KEY` with your actual NuGet API key from https://www.nuget.org/account/apikeys

## Versioning

To create a new version, update the `<Version>` property in the `.csproj` file:

```xml
<Version>1.0.1</Version>
```

Or specify it on the command line:

```powershell
dotnet pack .\Mavusi.Linq.DataScience\Mavusi.Linq.DataScience.csproj -c Release -o .\nupkg -p:Version=1.0.1
```

## Local Testing

To test the package locally before publishing:

```powershell
# Add a local NuGet source
dotnet nuget add source C:\dev\personal\Mavusi.Linq.DataScience\nupkg --name LocalMavusiSource

# Install in another project
dotnet add package Mavusi.Linq.DataScience --version 1.0.0 --source LocalMavusiSource
```

## Package Contents

The package includes:
- ✅ Compiled DLLs for net8.0, net9.0, net10.0
- ✅ XML documentation files
- ✅ README.md
- ✅ License information (MIT)
- ✅ Repository metadata

## Build Status

All frameworks build successfully with only documentation warnings (missing XML comments on some public members).

---

**Note**: The test project also supports multi-targeting but is not packaged (marked with `<IsPackable>false</IsPackable>`).
