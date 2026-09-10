param(
    [string]$ValheimPath = "D:\SteamLibrary\steamapps\common\Valheim",
    [string]$DeployProfile = "$env:USERPROFILE\AppData\Roaming\com.kesomannen.gale\valheim\profiles\New Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$solution = Join-Path $root "PortalStation.sln"
$modConstantsPath = Join-Path $root "src\PortalStation\ModConstants.cs"
$modConstants = Get-Content $modConstantsPath -Raw
$modVersion = if ($modConstants -match 'ModVersion = "([^"]+)"') { $Matches[1] } else { "unknown" }
$buildLabel = if ($modConstants -match 'BuildLabel = "([^"]+)"') { $Matches[1] } else { "unknown" }

Write-Host "Building Portal Station $modVersion ($buildLabel) against Valheim 1.0 / Unity 6..."
Write-Host "  ValheimManaged: $ValheimPath\valheim_Data\Managed"
Write-Host "  BepInExCore:    Gale cache denikson-BepInExPack_Valheim 5.4.2350"

dotnet build $solution -c Release --no-incremental
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$artifactDir = Join-Path $root "artifacts"
$pluginsRoot = Join-Path $DeployProfile "BepInEx\plugins"
$pluginDir = Join-Path $pluginsRoot "Hardwire99-PortalStation"
New-Item -ItemType Directory -Force -Path $pluginDir | Out-Null

$sourceDll = Join-Path $artifactDir "PortalStation.dll"
if (-not (Test-Path $sourceDll)) {
    throw "Missing build output: $sourceDll"
}

$destDll = Join-Path $pluginDir "PortalStation.dll"
Copy-Item $sourceDll $destDll -Force
Write-Host "Deployed to $destDll"

$iconSource = Join-Path $root "art\Station_Icon.png"
$iconDest = Join-Path $pluginDir "Station_Icon.png"
if (Test-Path $iconSource) {
    Copy-Item $iconSource $iconDest -Force
    Write-Host "Deployed hammer icon to $iconDest"
} else {
    Write-Warning "Missing hammer icon source: $iconSource"
}

$buildStamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$buildInfoPath = Join-Path $pluginDir "BUILD.txt"
@(
    "Portal Station $modVersion ($buildLabel)"
    "Built: $buildStamp"
    "Target: Valheim 1.0 / Unity 6 (BepInExPack 5.4.2350)"
    "Verify in BepInEx log - should show `"$modVersion ($buildLabel) loaded.`""
) | Set-Content $buildInfoPath -Encoding UTF8

Write-Host ""
Write-Host "Deployed Portal Station $modVersion ($buildLabel) to:"
Write-Host "  Profile: $DeployProfile"
Write-Host "  Plugins: $pluginsRoot"
Write-Host "  Mod DLL: $destDll"
Write-Host "Wrote $buildInfoPath"
Write-Host "Restart Valheim fully, then check BepInEx\LogOutput.log for the startup message."

# Thunderstore / Hexium package (flat zip: manifest, icon, README, DLL at root)
$team = "Hardwire99"
$packageName = "{0}-{1}-{2}.zip" -f $team, "PortalStation", $modVersion
$packagePath = Join-Path $artifactDir $packageName
$stagingDir = Join-Path $artifactDir "thunderstore-staging"

Write-Host ""
Write-Host "Packaging for Thunderstore and Hexium..."

if (Test-Path $stagingDir) { Remove-Item $stagingDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $stagingDir | Out-Null

Copy-Item $sourceDll (Join-Path $stagingDir "PortalStation.dll") -Force
Copy-Item (Join-Path $root "manifest.json") (Join-Path $stagingDir "manifest.json") -Force
Copy-Item (Join-Path $root "README.md") (Join-Path $stagingDir "README.md") -Force

$changelog = Join-Path $root "CHANGELOG.md"
if (Test-Path $changelog) {
    Copy-Item $changelog (Join-Path $stagingDir "CHANGELOG.md") -Force
}

$readmeScreenshots = @(
    "Portal_Name.png",
    "Portal_display_Name.png",
    "Station.png",
    "Hammer.png",
    "Station_config.png",
    "Change_Name.png"
)
$stagingArtDir = Join-Path $stagingDir "art"
New-Item -ItemType Directory -Force -Path $stagingArtDir | Out-Null
foreach ($screenshot in $readmeScreenshots) {
    $screenshotSource = Join-Path $root "art\$screenshot"
    if (-not (Test-Path $screenshotSource)) {
        throw "Missing README screenshot: $screenshotSource"
    }
    Copy-Item $screenshotSource (Join-Path $stagingArtDir $screenshot) -Force
}
Write-Host "Bundled $($readmeScreenshots.Count) README screenshots into package art/"

$iconSource = Join-Path $root "icon.png"
$iconDest = Join-Path $stagingDir "icon.png"
if (Test-Path $iconSource) {
    Add-Type -AssemblyName System.Drawing
    $srcImage = [System.Drawing.Image]::FromFile((Resolve-Path $iconSource))
    try {
        $dstImage = New-Object System.Drawing.Bitmap(256, 256)
        $graphics = [System.Drawing.Graphics]::FromImage($dstImage)
        try {
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.DrawImage($srcImage, 0, 0, 256, 256)
            $dstImage.Save($iconDest, [System.Drawing.Imaging.ImageFormat]::Png)
        }
        finally {
            $graphics.Dispose()
            $dstImage.Dispose()
        }
    }
    finally {
        $srcImage.Dispose()
    }
    Write-Host "Wrote 256x256 Thunderstore icon to $iconDest"
}
else {
    throw "Missing package icon: $iconSource"
}

if (Test-Path $packagePath) { Remove-Item $packagePath -Force }
Compress-Archive -Path (Join-Path $stagingDir "*") -DestinationPath $packagePath -Force
Remove-Item $stagingDir -Recurse -Force

Write-Host "Package created: $packagePath"
Write-Host "Upload to Thunderstore (team $team) and Hexium using the same zip."
