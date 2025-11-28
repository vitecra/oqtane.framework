# Deploy custom modules and themes to Oqtane.Server
# This script copies module/theme DLLs and asset manifests to the Oqtane.Server bin directory

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ServerBin = Join-Path $ScriptDir "Oqtane.Server\bin\Debug\net10.0"

Write-Host "Deploying custom modules and themes to Oqtane.Server..." -ForegroundColor Cyan

# Deploy AgentHub Module
$AgentHubServer = Join-Path $ScriptDir "AgentHub\Server"
if (Test-Path $AgentHubServer) {
    Write-Host "Deploying AgentHub module..." -ForegroundColor Yellow
    
    # Copy Server DLL
    $srcDll = Join-Path $ScriptDir "AgentHub\Server\bin\Debug\net10.0\AgentHub.Server.dll"
    if (Test-Path $srcDll) {
        Copy-Item $srcDll $ServerBin -Force
    }
    
    # Copy asset manifest
    $srcManifest = Join-Path $ScriptDir "AgentHub\Client\bin\Debug\net10.0\AgentHub.Server.assets.json"
    if (Test-Path $srcManifest) {
        Copy-Item $srcManifest $ServerBin -Force
    }
    
    # Copy Client DLL (may be needed for Blazor)
    $srcClientDll = Join-Path $ScriptDir "AgentHub\Client\bin\Debug\net10.0\AgentHub.Client.dll"
    if (Test-Path $srcClientDll) {
        Copy-Item $srcClientDll $ServerBin -Force
    }
    
    Write-Host "  ✓ AgentHub deployed" -ForegroundColor Green
}

# Deploy FullscreenTheme
$ThemeServer = Join-Path $ScriptDir "Oqtane.Themes.FullscreenTheme\Server"
if (Test-Path $ThemeServer) {
    Write-Host "Deploying FullscreenTheme..." -ForegroundColor Yellow
    
    # Copy Server DLL
    $srcDll = Join-Path $ScriptDir "Oqtane.Themes.FullscreenTheme\Server\bin\Debug\net10.0\Oqtane.Themes.FullscreenTheme.dll"
    if (Test-Path $srcDll) {
        Copy-Item $srcDll $ServerBin -Force
    }
    
    # Copy asset manifest
    $srcManifest = Join-Path $ScriptDir "Oqtane.Themes.FullscreenTheme\Client\bin\Debug\net10.0\Oqtane.Themes.FullscreenTheme.assets.json"
    if (Test-Path $srcManifest) {
        Copy-Item $srcManifest $ServerBin -Force
    }
    
    # Copy Client DLL (may be needed for Blazor)
    $srcClientDll = Join-Path $ScriptDir "Oqtane.Themes.FullscreenTheme\Client\bin\Debug\net10.0\Oqtane.Themes.FullscreenTheme.Client.dll"
    if (Test-Path $srcClientDll) {
        Copy-Item $srcClientDll $ServerBin -Force
    }
    
    Write-Host "  ✓ FullscreenTheme deployed" -ForegroundColor Green
}

Write-Host ""
Write-Host "Deployment complete! Deployed files:" -ForegroundColor Cyan
$manifests = Get-ChildItem -Path $ServerBin -Filter "*.assets.json" -ErrorAction SilentlyContinue
if ($manifests) {
    $manifests | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor White }
} else {
    Write-Host "  No asset manifests found" -ForegroundColor Red
}

Write-Host ""
Write-Host "To verify deployment, check that the following files exist:" -ForegroundColor Cyan
Write-Host "  - AgentHub.Server.dll" -ForegroundColor White
Write-Host "  - AgentHub.Server.assets.json" -ForegroundColor White
Write-Host "  - Oqtane.Themes.FullscreenTheme.dll" -ForegroundColor White
Write-Host "  - Oqtane.Themes.FullscreenTheme.assets.json" -ForegroundColor White
