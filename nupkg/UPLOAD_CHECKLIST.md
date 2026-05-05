# NuGet Package Upload Checklist - v1.7.0

## ✅ Pre-Upload Verification

- [x] Version updated to 1.7.0 in .csproj file
- [x] README.md updated with new geospatial features
- [x] Package release notes updated in .csproj
- [x] All tests passing (69/69 geospatial tests)
- [x] Build successful with no errors
- [x] Package created successfully
- [x] Symbol package (.snupkg) created
- [x] All target frameworks included (.NET 8, 9, 10)
- [x] XML documentation generated and included

## 📦 Package Files

### Main Package
- **File**: `Mavusi.Linq.DataScience.1.7.0.nupkg`
- **Location**: `C:\dev\personal\Mavusi.Linq.DataScience\nupkg\`
- **Size**: ~78 KB
- **Contents**:
  - ✅ lib/net8.0/Mavusi.Linq.DataScience.dll
  - ✅ lib/net8.0/Mavusi.Linq.DataScience.xml
  - ✅ lib/net9.0/Mavusi.Linq.DataScience.dll
  - ✅ lib/net9.0/Mavusi.Linq.DataScience.xml
  - ✅ lib/net10.0/Mavusi.Linq.DataScience.dll
  - ✅ lib/net10.0/Mavusi.Linq.DataScience.xml
  - ✅ README.md
  - ✅ .nuspec file

### Symbol Package
- **File**: `Mavusi.Linq.DataScience.1.7.0.snupkg`
- **Location**: `C:\dev\personal\Mavusi.Linq.DataScience\nupkg\`
- **Size**: ~41 KB

## 🚀 Upload Instructions

### Option 1: Via NuGet.org Web Interface

1. Go to https://www.nuget.org/packages/upload
2. Sign in with your NuGet account
3. Click "Browse" and select: `Mavusi.Linq.DataScience.1.7.0.nupkg`
4. Review the package details
5. Click "Submit"
6. After main package is uploaded, upload the symbol package: `Mavusi.Linq.DataScience.1.7.0.snupkg`

### Option 2: Via Command Line

```powershell
# Set your API key (do this once)
dotnet nuget push "C:\dev\personal\Mavusi.Linq.DataScience\nupkg\Mavusi.Linq.DataScience.1.7.0.nupkg" --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json

# Upload symbol package
dotnet nuget push "C:\dev\personal\Mavusi.Linq.DataScience\nupkg\Mavusi.Linq.DataScience.1.7.0.snupkg" --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

### Option 3: Via PowerShell Script

```powershell
# Navigate to package directory
cd "C:\dev\personal\Mavusi.Linq.DataScience\nupkg"

# Push main package
nuget push Mavusi.Linq.DataScience.1.7.0.nupkg -Source https://api.nuget.org/v3/index.json -ApiKey YOUR_API_KEY

# Push symbol package
nuget push Mavusi.Linq.DataScience.1.7.0.snupkg -Source https://api.nuget.org/v3/index.json -ApiKey YOUR_API_KEY
```

## 📝 Post-Upload Tasks

- [ ] Verify package appears on NuGet.org
- [ ] Check that all target frameworks are listed
- [ ] Verify README.md displays correctly
- [ ] Test installation: `dotnet add package Mavusi.Linq.DataScience --version 1.7.0`
- [ ] Update GitHub repository with release tag
- [ ] Create GitHub release with release notes
- [ ] Announce the release (social media, blog, etc.)

## 🔍 Package Validation

After upload, verify:
- Package ID: `Mavusi.Linq.DataScience`
- Version: `1.7.0`
- Target Frameworks: .NET 8.0, .NET 9.0, .NET 10.0
- License: MIT
- Dependencies: Microsoft.SourceLink.GitHub (dev only)
- Tags include: geospatial, gis, haversine, location, gps, mapping

## 📋 Version History

- v1.0.0 - Initial release
- v1.5.0 - Distribution extensions
- **v1.7.0 - Geospatial extensions (CURRENT)**

## 🐛 Known Issues

- None currently identified

## 💡 Notes

- Symbol package enables debugging through source code
- SourceLink integration provides GitHub source browsing
- All public APIs have XML documentation
- Package supports nullable reference types
- Built with C# 14.0 (latest features)

## ⚠️ Important Reminders

1. **API Key Security**: Never commit your NuGet API key to source control
2. **Package Immutability**: Once uploaded, you cannot modify a version
3. **Testing**: Consider uploading to a test feed first
4. **Git Tags**: Tag your repository with v1.7.0 after successful upload

## 🔗 Useful Links

- NuGet Package Manager: https://www.nuget.org/packages/Mavusi.Linq.DataScience
- Upload Page: https://www.nuget.org/packages/upload
- API Keys: https://www.nuget.org/account/apikeys
- GitHub Repo: https://github.com/mavusi/Mavusi.Linq.DataScience

---

**Ready to upload!** 🎉

All checks complete. The package is ready for upload to NuGet.org.
