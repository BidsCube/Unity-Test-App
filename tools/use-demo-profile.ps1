param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("direct", "applovin", "applovin-lite", "applovin-video", "levelplay", "levelplay-lite", "levelplay-video")]
    [string]$Profile
)

$RootDir = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$PackagesDir = Join-Path $RootDir "Packages"
$Map = @{
    "direct"          = "manifest.direct.json"
    "applovin"        = "manifest.applovin.json"
    "applovin-lite"   = "manifest.applovin.json"
    "applovin-video"  = "manifest.applovin.json"
    "levelplay"       = "manifest.levelplay.json"
    "levelplay-lite"  = "manifest.levelplay.json"
    "levelplay-video" = "manifest.levelplay.json"
}

$SourceManifest = Join-Path $PackagesDir $Map[$Profile]
$TargetManifest = Join-Path $PackagesDir "manifest.json"
$LockFile = Join-Path $PackagesDir "packages-lock.json"

Copy-Item $SourceManifest $TargetManifest -Force

if (Test-Path $LockFile) {
    Remove-Item $LockFile -Force
}

$asset = Join-Path $RootDir "Assets\BidscubeAndroidExportSettings.asset"
$meta = Join-Path $RootDir "Assets\BidscubeAndroidExportSettings.asset.meta"
$tpl = Join-Path $RootDir "tools\templates\BidscubeAndroidExportSettings.Lite.asset"
$tplMeta = Join-Path $RootDir "tools\templates\BidscubeAndroidExportSettings.Lite.asset.meta"

function Ensure-LiteAsset {
    if (-not (Test-Path $asset)) {
        New-Item -ItemType Directory -Force -Path (Split-Path $asset) | Out-Null
        Copy-Item $tpl $asset -Force
        Copy-Item $tplMeta $meta -Force
    }
}

function Set-ExportMode([int]$featureSet, [int]$enableDesugaring) {
    $t = Get-Content $asset -Raw
    $t = [regex]::Replace($t, "(?m)^  featureSet:.*$", "  featureSet: $featureSet")
    $t = [regex]::Replace($t, "(?m)^  enableDesugaring:.*$", "  enableDesugaring: $enableDesugaring")
    Set-Content -Path $asset -Value $t -Encoding UTF8
}

if ($Profile -eq "direct") {
    Remove-Item $asset, $meta -ErrorAction SilentlyContinue
}
elseif ($Profile -in @("applovin", "applovin-lite", "levelplay", "levelplay-lite")) {
    Ensure-LiteAsset
    Set-ExportMode 0 0
}
elseif ($Profile -in @("applovin-video", "levelplay-video")) {
    Ensure-LiteAsset
    Set-ExportMode 1 1
}

Write-Host "Selected BidsCube demo profile: $Profile"
Write-Host ""
Write-Host "Next steps:"
Write-Host "1. Open the project in Unity."
Write-Host "2. Wait until Package Manager resolves dependencies."
Write-Host "3. For AppLovin/LevelPlay, run External Dependency Manager Android Resolver if needed."
Write-Host "4. Open the sample scene and test the selected integration."
