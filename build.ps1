param(
    [string]$ValheimPath = "D:\SteamLibrary\steamapps\common\Valheim",
    [string]$DeployProfile = "C:\Users\cdjen\AppData\Roaming\r2modmanPlus-local\Valheim\profiles\Portals"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$solution = Join-Path $root "PortalStation.sln"
$modConstantsPath = Join-Path $root "src\PortalStation\ModConstants.cs"
$modConstants = Get-Content $modConstantsPath -Raw
$modVersion = if ($modConstants -match 'ModVersion = "([^"]+)"') { $Matches[1] } else { "unknown" }
$buildLabel = if ($modConstants -match 'BuildLabel = "([^"]+)"') { $Matches[1] } else { "unknown" }

Write-Host "Building Portal Station $modVersion ($buildLabel)..."

dotnet build $solution -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$artifactDir = Join-Path $root "artifacts"
$pluginsRoot = Join-Path $DeployProfile "BepInEx\plugins"
$pluginDir = Join-Path $pluginsRoot "PortalStation"
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
    "Verify in BepInEx log - should show `"$modVersion ($buildLabel) loaded.`""
) | Set-Content $buildInfoPath -Encoding UTF8

Write-Host ""
Write-Host "Deployed Portal Station $modVersion ($buildLabel) to:"
Write-Host "  Profile: $DeployProfile"
Write-Host "  Plugins: $pluginsRoot"
Write-Host "  Mod DLL: $destDll"
Write-Host "Wrote $buildInfoPath"
Write-Host "Restart Valheim fully, then check BepInEx\LogOutput.log for the startup message."
