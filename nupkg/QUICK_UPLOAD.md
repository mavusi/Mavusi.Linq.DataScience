# Quick Upload Commands - v1.7.0

## 📦 Package Location
```
C:\dev\personal\Mavusi.Linq.DataScience\nupkg\
```

## 🚀 Upload Commands

### Using dotnet CLI (Recommended)

```powershell
# Navigate to package directory
cd C:\dev\personal\Mavusi.Linq.DataScience\nupkg

# Upload main package
dotnet nuget push Mavusi.Linq.DataScience.1.7.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json

# Upload symbol package (for debugging support)
dotnet nuget push Mavusi.Linq.DataScience.1.7.0.snupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

### Using nuget.exe

```powershell
# Upload main package
nuget push Mavusi.Linq.DataScience.1.7.0.nupkg -Source https://api.nuget.org/v3/index.json -ApiKey YOUR_API_KEY

# Upload symbol package
nuget push Mavusi.Linq.DataScience.1.7.0.snupkg -Source https://api.nuget.org/v3/index.json -ApiKey YOUR_API_KEY
```

## 🔑 API Key Management

### Get your API key
1. Go to https://www.nuget.org/account/apikeys
2. Create new API key or use existing one
3. Copy the key

### Store API key securely (one-time setup)
```powershell
# Store in environment variable (current session)
$env:NUGET_API_KEY = "YOUR_API_KEY"

# Then use in commands
dotnet nuget push Mavusi.Linq.DataScience.1.7.0.nupkg --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json
```

## 🧪 Test Installation

After uploading, test the package:

```powershell
# Create a test project
dotnet new console -n TestApp
cd TestApp

# Add the package (may take a few minutes to propagate)
dotnet add package Mavusi.Linq.DataScience --version 1.7.0

# Run the project
dotnet run
```

## ✅ Verification

### Check package status
```powershell
# View in browser
Start-Process "https://www.nuget.org/packages/Mavusi.Linq.DataScience/1.7.0"
```

### Check package installation
```powershell
# Search for the package
dotnet nuget search Mavusi.Linq.DataScience

# List installed packages
dotnet list package
```

## 📊 Package Info

- **Version**: 1.7.0
- **Main Package Size**: ~78 KB
- **Symbol Package Size**: ~41 KB
- **Target Frameworks**: net8.0, net9.0, net10.0
- **License**: MIT

## 🎯 Quick Copy-Paste

Replace YOUR_API_KEY with your actual key:

```powershell
cd C:\dev\personal\Mavusi.Linq.DataScience\nupkg
dotnet nuget push Mavusi.Linq.DataScience.1.7.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push Mavusi.Linq.DataScience.1.7.0.snupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

---

**Note**: Package validation may take a few minutes. Check status at https://www.nuget.org/packages/Mavusi.Linq.DataScience
