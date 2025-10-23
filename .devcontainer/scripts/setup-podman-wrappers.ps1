#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Setup Podman wrappers for Visual Studio compatibility
.DESCRIPTION
    This script creates docker/docker-compose wrapper scripts in C:\bin
    that redirect to Podman, allowing Visual Studio to work with Podman.
.PARAMETER WrapperDirectory
    Directory where wrappers will be created (default: C:\bin)
.PARAMETER AddToPath
    Add wrapper directory to system PATH (requires admin)
.EXAMPLE
    .\setup-podman-wrappers.ps1
.EXAMPLE
    .\setup-podman-wrappers.ps1 -WrapperDirectory "C:\Tools" -AddToPath
#>

[CmdletBinding()]
param(
    [string]$WrapperDirectory = "C:\bin",
    [switch]$AddToPath
)

# Check if running as administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")

if ($AddToPath -and -not $isAdmin) {
    Write-Warning "Adding to PATH requires administrator privileges. Please run as administrator or add manually."
    $AddToPath = $false
}

# Create wrapper directory if it doesn't exist
if (-not (Test-Path $WrapperDirectory)) {
    Write-Host "Creating wrapper directory: $WrapperDirectory" -ForegroundColor Green
    New-Item -ItemType Directory -Force -Path $WrapperDirectory | Out-Null
}

# Source directory containing wrapper scripts
$scriptDir = $PSScriptRoot

# Copy wrapper scripts
$wrappers = @(
    @{ Source = "docker.ps1"; Dest = "docker.ps1" },
    @{ Source = "docker-compose.ps1"; Dest = "docker-compose.ps1" },
    @{ Source = "docker.cmd"; Dest = "docker.cmd" },
    @{ Source = "docker-compose.cmd"; Dest = "docker-compose.cmd" }
)

foreach ($wrapper in $wrappers) {
    $sourcePath = Join-Path $scriptDir $wrapper.Source
    $destPath = Join-Path $WrapperDirectory $wrapper.Dest
    
    if (Test-Path $sourcePath) {
        Write-Host "Copying $($wrapper.Source) to $destPath" -ForegroundColor Cyan
        Copy-Item -Path $sourcePath -Destination $destPath -Force
    } else {
        Write-Warning "Source file not found: $sourcePath"
    }
}

# Create .exe wrappers (copies of .cmd files)
Write-Host "Creating .exe wrappers..." -ForegroundColor Cyan
Copy-Item -Path (Join-Path $WrapperDirectory "docker.cmd") -Destination (Join-Path $WrapperDirectory "docker.exe") -Force
Copy-Item -Path (Join-Path $WrapperDirectory "docker-compose.cmd") -Destination (Join-Path $WrapperDirectory "docker-compose.exe") -Force

# Add to PATH if requested
if ($AddToPath) {
    $currentPath = [Environment]::GetEnvironmentVariable("Path", "Machine")
    if ($currentPath -notlike "*$WrapperDirectory*") {
        Write-Host "Adding $WrapperDirectory to system PATH..." -ForegroundColor Green
        [Environment]::SetEnvironmentVariable("Path", "$currentPath;$WrapperDirectory", "Machine")
        Write-Host "PATH updated. Please restart your terminal/Visual Studio for changes to take effect." -ForegroundColor Yellow
    } else {
        Write-Host "$WrapperDirectory is already in PATH" -ForegroundColor Green
    }
}

# Verify Podman installation
Write-Host "`nVerifying Podman installation..." -ForegroundColor Green
$podmanPath = (Get-Command podman -ErrorAction SilentlyContinue).Source
if ($podmanPath) {
    Write-Host "✓ Podman found at: $podmanPath" -ForegroundColor Green
    & podman --version
} else {
    Write-Warning "× Podman not found in PATH. Please install Podman or Podman Desktop."
}

# Test wrappers
Write-Host "`nTesting wrappers..." -ForegroundColor Green
$testCmd = Join-Path $WrapperDirectory "docker.cmd"
if (Test-Path $testCmd) {
    Write-Host "Testing docker wrapper:" -ForegroundColor Cyan
    & $testCmd --version
} else {
    Write-Warning "Wrapper not found: $testCmd"
}

Write-Host "`n✓ Setup complete!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Ensure $WrapperDirectory is in your PATH" -ForegroundColor White
Write-Host "2. Restart Visual Studio and any open terminals" -ForegroundColor White
Write-Host "3. Test with: docker --version" -ForegroundColor White
Write-Host "4. Test with: docker-compose --version" -ForegroundColor White
Write-Host "`nFor Visual Studio integration, see: VISUAL_STUDIO_PODMAN_SETUP.md" -ForegroundColor Cyan
