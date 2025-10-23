#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Docker Compose → Podman Compose wrapper for Visual Studio compatibility
.DESCRIPTION
    This script wraps Podman's compose command to act as docker-compose.exe 
    for Visual Studio 2022 Docker Compose tooling integration.
.NOTES
    Place this in C:\bin and ensure it's in your PATH
#>

param(
    [Parameter(ValueFromRemainingArguments)]
    [string[]]$Arguments
)

# Locate podman executable
$podmanPath = $null

# Common installation paths
$searchPaths = @(
    "C:\Program Files\RedHat\Podman\podman.exe",
    "C:\Program Files (x86)\RedHat\Podman\podman.exe",
    "$env:ProgramFiles\RedHat\Podman\podman.exe",
    "${env:ProgramFiles(x86)}\RedHat\Podman\podman.exe"
)

foreach ($path in $searchPaths) {
    if (Test-Path $path) {
        $podmanPath = $path
        break
    }
}

# Fallback: search PATH
if (-not $podmanPath) {
    $podmanPath = (Get-Command podman -ErrorAction SilentlyContinue).Source
}

if (-not $podmanPath) {
    Write-Error "Podman executable not found. Please install Podman Desktop or Podman CLI."
    exit 1
}

# Execute podman compose with all arguments
& $podmanPath compose @Arguments
exit $LASTEXITCODE
