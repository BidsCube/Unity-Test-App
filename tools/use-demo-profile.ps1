param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("direct", "applovin", "levelplay")]
    [string]$Profile
)

$RootDir = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$PackagesDir = Join-Path $RootDir "Packages"

$SourceManifest = Join-Path $PackagesDir "manifest.$Profile.json"
$TargetManifest = Join-Path $PackagesDir "manifest.json"
$LockFile = Join-Path $PackagesDir "packages-lock.json"

Copy-Item $SourceManifest $TargetManifest -Force

if (Test-Path $LockFile) {
    Remove-Item $LockFile -Force
}

Write-Host "Selected BidsCube demo profile: $Profile"
Write-Host ""
Write-Host "Next steps:"
Write-Host "1. Open the project in Unity."
Write-Host "2. Wait until Package Manager resolves dependencies."
Write-Host "3. For AppLovin/LevelPlay, run External Dependency Manager Android Resolver if needed."
Write-Host "4. Open the sample scene and test the selected integration."
