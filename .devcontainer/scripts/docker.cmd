@echo off
REM Docker → Podman wrapper for Visual Studio
REM This batch file wraps Podman to act as docker.exe

setlocal enabledelayedexpansion

REM Check common Podman installation paths
set "PODMAN_PATH="

if exist "C:\Program Files\RedHat\Podman\podman.exe" (
    set "PODMAN_PATH=C:\Program Files\RedHat\Podman\podman.exe"
) else if exist "C:\Program Files (x86)\RedHat\Podman\podman.exe" (
    set "PODMAN_PATH=C:\Program Files (x86)\RedHat\Podman\podman.exe"
) else (
    REM Try to find in PATH
    where podman.exe >nul 2>&1
    if !errorlevel! equ 0 (
        for /f "tokens=*" %%i in ('where podman.exe') do set "PODMAN_PATH=%%i"
    )
)

if not defined PODMAN_PATH (
    echo Error: Podman executable not found. Please install Podman.
    exit /b 1
)

REM Execute podman with all arguments
"%PODMAN_PATH%" %*
exit /b %ERRORLEVEL%
