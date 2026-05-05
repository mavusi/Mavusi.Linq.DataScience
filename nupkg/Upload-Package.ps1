# Upload script for Mavusi.Linq.DataScience v1.7.0
# This script uploads the package to NuGet.org

param(
    [Parameter(Mandatory=$true)]
    [string]$ApiKey
)

# Colors for output
$Green = "Green"
$Yellow = "Yellow"
$Red = "Red"
$Cyan = "Cyan"

# Package details
$PackageDir = "C:\dev\personal\Mavusi.Linq.DataScience\nupkg"
$MainPackage = "Mavusi.Linq.DataScience.1.7.0.nupkg"
$SymbolPackage = "Mavusi.Linq.DataScience.1.7.0.snupkg"
$NuGetSource = "https://api.nuget.org/v3/index.json"

Write-Host "`n========================================" -ForegroundColor $Cyan
Write-Host "   NuGet Package Upload Script" -ForegroundColor $Green
Write-Host "   Mavusi.Linq.DataScience v1.7.0" -ForegroundColor $Green
Write-Host "========================================`n" -ForegroundColor $Cyan

# Change to package directory
Set-Location $PackageDir
Write-Host "📁 Working directory: $PackageDir`n" -ForegroundColor $Yellow

# Verify packages exist
if (-not (Test-Path $MainPackage)) {
    Write-Host "❌ Error: Main package not found: $MainPackage" -ForegroundColor $Red
    exit 1
}

if (-not (Test-Path $SymbolPackage)) {
    Write-Host "⚠️  Warning: Symbol package not found: $SymbolPackage" -ForegroundColor $Yellow
} else {
    Write-Host "✅ Found main package: $MainPackage" -ForegroundColor $Green
    Write-Host "✅ Found symbol package: $SymbolPackage`n" -ForegroundColor $Green
}

# Confirm upload
Write-Host "You are about to upload:" -ForegroundColor $Yellow
Write-Host "  • $MainPackage (78 KB)" -ForegroundColor $Yellow
Write-Host "  • $SymbolPackage (41 KB)" -ForegroundColor $Yellow
Write-Host "  to NuGet.org`n" -ForegroundColor $Yellow

$Confirm = Read-Host "Continue with upload? (Y/N)"
if ($Confirm -ne "Y" -and $Confirm -ne "y") {
    Write-Host "`n❌ Upload cancelled by user" -ForegroundColor $Red
    exit 0
}

Write-Host ""

# Upload main package
Write-Host "📦 Uploading main package..." -ForegroundColor $Cyan
try {
    dotnet nuget push $MainPackage --api-key $ApiKey --source $NuGetSource
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Main package uploaded successfully!`n" -ForegroundColor $Green
    } else {
        Write-Host "❌ Failed to upload main package (Exit code: $LASTEXITCODE)" -ForegroundColor $Red
        exit 1
    }
} catch {
    Write-Host "❌ Error uploading main package: $_" -ForegroundColor $Red
    exit 1
}

# Upload symbol package
if (Test-Path $SymbolPackage) {
    Write-Host "📦 Uploading symbol package..." -ForegroundColor $Cyan
    try {
        dotnet nuget push $SymbolPackage --api-key $ApiKey --source $NuGetSource
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Symbol package uploaded successfully!`n" -ForegroundColor $Green
        } else {
            Write-Host "⚠️  Warning: Symbol package upload failed (Exit code: $LASTEXITCODE)" -ForegroundColor $Yellow
        }
    } catch {
        Write-Host "⚠️  Warning: Error uploading symbol package: $_" -ForegroundColor $Yellow
    }
}

# Success message
Write-Host "========================================" -ForegroundColor $Cyan
Write-Host "   ✅ Upload Complete!" -ForegroundColor $Green
Write-Host "========================================`n" -ForegroundColor $Cyan

Write-Host "Next steps:" -ForegroundColor $Yellow
Write-Host "  1. Verify package at: https://www.nuget.org/packages/Mavusi.Linq.DataScience/1.7.0" -ForegroundColor $Yellow
Write-Host "  2. Wait 5-10 minutes for package to propagate" -ForegroundColor $Yellow
Write-Host "  3. Test installation: dotnet add package Mavusi.Linq.DataScience --version 1.7.0" -ForegroundColor $Yellow
Write-Host "  4. Tag your Git repository: git tag v1.7.0 && git push origin v1.7.0" -ForegroundColor $Yellow
Write-Host "  5. Create GitHub release with RELEASE_NOTES_v1.7.0.md`n" -ForegroundColor $Yellow

Write-Host "🎉 Congratulations! Your package is now live on NuGet.org!`n" -ForegroundColor $Green
