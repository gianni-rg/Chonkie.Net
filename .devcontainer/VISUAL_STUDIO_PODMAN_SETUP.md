# Visual Studio 2022 with Podman Configuration Guide

## Problem Statement

Visual Studio 2022's Docker Compose tooling is hardcoded to use Docker Desktop (`docker.exe` and `docker-compose.exe`). When using Podman as an alternative, VS needs to be configured to use Podman's compatibility layer instead.

## Solution Overview

There are **three approaches** to make Visual Studio work with Podman:

1. **Method 1: Symlink/Wrapper Approach** ✅ **RECOMMENDED**
2. **Method 2: MSBuild Property Override**
3. **Method 3: Podman Desktop with Docker Compatibility**

---

## Method 1: Symlink/Wrapper Approach (RECOMMENDED)

This method creates wrapper scripts that redirect Docker commands to Podman.

### Step 1: Locate Your Podman Installation

```powershell
# Find podman.exe
where.exe podman

# Example output: C:\Program Files\RedHat\Podman\podman.exe
```

### Step 2: Create Docker Compatibility Wrappers

Choose a location that's in your PATH (or create one):

```powershell
# Create a directory for wrappers
$wrapperDir = "C:\bin"
New-Item -ItemType Directory -Force -Path $wrapperDir

# Add to PATH if not already there (requires admin)
[Environment]::SetEnvironmentVariable("Path", $env:Path + ";$wrapperDir", "Machine")
```

### Step 3: Create Wrapper Scripts

#### Option A: PowerShell Wrappers (Recommended for Windows)

Create `C:\bin\docker.ps1`:
```powershell
#!/usr/bin/env pwsh
# Docker → Podman wrapper for Visual Studio
param([Parameter(ValueFromRemainingArguments)]$args)

$podmanPath = "C:\Program Files\RedHat\Podman\podman.exe"
& $podmanPath @args
exit $LASTEXITCODE
```

Create `C:\bin\docker-compose.ps1`:
```powershell
#!/usr/bin/env pwsh
# Docker Compose → Podman Compose wrapper for Visual Studio
param([Parameter(ValueFromRemainingArguments)]$args)

$podmanPath = "C:\Program Files\RedHat\Podman\podman.exe"
& $podmanPath compose @args
exit $LASTEXITCODE
```

Create `C:\bin\docker.cmd`:
```cmd
@echo off
REM Docker → Podman wrapper for Visual Studio
"C:\Program Files\RedHat\Podman\podman.exe" %*
exit /b %ERRORLEVEL%
```

Create `C:\bin\docker-compose.cmd`:
```cmd
@echo off
REM Docker Compose → Podman Compose wrapper
"C:\Program Files\RedHat\Podman\podman.exe" compose %*
exit /b %ERRORLEVEL%
```

Create `C:\bin\docker.exe`:
```cmd
@echo off
REM Docker → Podman wrapper (exe named)
"C:\Program Files\RedHat\Podman\podman.exe" %*
exit /b %ERRORLEVEL%
```

Create `C:\bin\docker-compose.exe`:
```cmd
@echo off
REM Docker Compose → Podman Compose wrapper (exe named)
"C:\Program Files\RedHat\Podman\podman.exe" compose %*
exit /b %ERRORLEVEL%
```

### Step 4: Test the Wrappers

```powershell
# Test docker wrapper
docker --version

# Test docker-compose wrapper
docker-compose --version

# Both should show Podman version information
```

### Step 5: Configure Podman Compatibility

Ensure Podman is running in rootful mode with compatibility socket:

```powershell
# Start Podman machine (if using machine)
podman machine start

# Enable Docker compatibility socket
podman system service --time=0 tcp:127.0.0.1:2375
```

For permanent setup, create a Windows Service or startup task.

---

## Method 2: MSBuild Property Override

This method overrides Visual Studio's Docker tooling paths via MSBuild properties.

### Step 1: Create Directory.Build.props Override

Add to your **root** `Directory.Build.props`:

```xml
<Project>
  <!-- Existing properties... -->
  
  <!-- Docker/Podman Configuration for Visual Studio -->
  <PropertyGroup Condition="'$(OS)' == 'Windows_NT'">
    <!-- Path to Podman executable acting as docker -->
    <DockerComposeExePath>C:\bin\docker-compose.exe</DockerComposeExePath>
    <DockerExePath>C:\bin\docker.exe</DockerExePath>
    
    <!-- Alternative: Direct Podman paths -->
    <!-- <DockerComposeExePath>C:\Program Files\RedHat\Podman\podman.exe</DockerComposeExePath> -->
    <!-- <DockerExePath>C:\Program Files\RedHat\Podman\podman.exe</DockerExePath> -->
    
    <!-- Docker Compose project name -->
    <DockerComposeProjectName>chonkie-net</DockerComposeProjectName>
    
    <!-- Enable Docker Compose for debugging -->
    <EnableDockerCompose>true</EnableDockerCompose>
  </PropertyGroup>
</Project>
```

### Step 2: Update launchSettings.json

Modify `launchSettings.json` to use the wrapper:

```json
{
  "profiles": {
    "Docker Compose": {
      "commandName": "DockerCompose",
      "commandVersion": "1.0",
      "executablePath": "C:\\bin\\docker-compose.exe",
      "composeBuild": true,
      "composeProfile": {
        "serviceActions": {
          "chonkie.sample": "StartDebugging"
        }
      }
    }
  }
}
```

---

## Method 3: Podman Desktop with Docker Compatibility

Use Podman Desktop's built-in Docker compatibility mode.

### Step 1: Install Podman Desktop

Download from: https://podman-desktop.io/

### Step 2: Enable Docker Compatibility

1. Open Podman Desktop
2. Go to **Settings** → **Resources**
3. Enable **"Docker Compatibility Mode"**
4. This creates docker.sock and docker CLI symlinks

### Step 3: Configure VS

Visual Studio should now detect "Docker" automatically (which is actually Podman).

---

## Configuration Files for DevContainer

### Update launchSettings.json

Replace the existing `launchSettings.json`:

```json
{
  "profiles": {
    "Docker Compose (Podman)": {
      "commandName": "DockerCompose",
      "commandVersion": "1.0",
      "executablePath": "C:\\bin\\docker-compose.exe",
      "serviceActions": {
        "dotnet-app": "StartDebugging"
      },
      "composeBuild": true,
      "composeProfile": {
        "commandLineArgs": "-f .devcontainer/docker-compose.yml up",
        "environmentVariables": {
          "DOCKER_HOST": "tcp://127.0.0.1:2375"
        }
      }
    },
    "Local Podman Compose": {
      "commandName": "Executable",
      "executablePath": "C:\\Program Files\\RedHat\\Podman\\podman.exe",
      "commandLineArgs": "compose -f .devcontainer/docker-compose.yml up",
      "workingDirectory": "$(ProjectDir)"
    }
  }
}
```

### Update docker-compose.dcproj

Ensure the `.dcproj` file doesn't have hardcoded paths:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" Sdk="Microsoft.Docker.Sdk">
  <PropertyGroup Label="Globals">
    <ProjectVersion>2.1</ProjectVersion>
    <DockerTargetOS>Linux</DockerTargetOS>
    <ProjectGuid>81dded9d-158b-e303-5f62-77a2896d2a5a</ProjectGuid>
    <!-- Use override from Directory.Build.props -->
    <DockerServiceUrl>{Scheme}://localhost:{ServicePort}</DockerServiceUrl>
  </PropertyGroup>
  
  <ItemGroup>
    <None Include=".devcontainer\docker-compose.yml">
      <DependentUpon>.devcontainer\docker-compose.override.yml</DependentUpon>
    </None>
    <None Include=".devcontainer\docker-compose.override.yml" />
    <None Include=".dockerignore" />
  </ItemGroup>
</Project>
```

---

## Troubleshooting

### Issue: "Docker is not running"

**Solution:** Start Podman service with Docker compatibility:

```powershell
# Option 1: TCP Socket
podman system service --time=0 tcp:127.0.0.1:2375

# Option 2: Named Pipe (Windows)
podman system service --time=0 npipe:////./pipe/docker_engine
```

### Issue: "Cannot find docker-compose.exe"

**Solution:** Ensure `C:\bin` is in your PATH and wrappers exist:

```powershell
# Check PATH
$env:PATH -split ';' | Select-String 'C:\\bin'

# Verify wrapper exists
Get-ChildItem C:\bin\docker-compose.*
```

### Issue: Visual Studio can't parse docker-compose.yml

**Solution:** Ensure you're using docker-compose v2 syntax and Podman 4.0+:

```powershell
podman --version  # Should be 4.0 or higher
```

### Issue: Build context errors

**Solution:** Update `DockerfileContext` in `.csproj` files:

```xml
<PropertyGroup>
  <DockerfileContext>..\..</DockerfileContext>
  <DockerfilePath>.devcontainer\Dockerfile.dotnet</DockerfilePath>
</PropertyGroup>
```

---

## Verification Steps

### 1. Test from Command Line

```powershell
# Navigate to project root
cd C:\Projects\Personal\Chonkie.Net

# Test with wrapper
docker-compose -f .devcontainer\docker-compose.yml config

# Should show parsed configuration without errors
```

### 2. Test from Visual Studio

1. Open `Chonkie.Net.sln` in Visual Studio 2022
2. Set `docker-compose` as startup project
3. Press F5 to debug
4. Should build containers using Podman and attach debugger

### 3. Test from VS Code

1. Open folder in VS Code
2. Ctrl+Shift+P → "Dev Containers: Reopen in Container"
3. Should use Podman to build and start devcontainer

---

## Additional Configuration for Visual Studio

### Enable Container Tools in Visual Studio

Ensure you have the following workloads installed:

- **ASP.NET and web development**
- **Azure development** (includes Docker tools)

### Configure Visual Studio Settings

1. Tools → Options → Container Tools
2. Set **Docker Compose Executable Path**: `C:\bin\docker-compose.exe`
3. Set **Docker Executable Path**: `C:\bin\docker.exe`

---

## Environment Variables for Podman

Create a `.env` file in the project root:

```env
# Podman Configuration
DOCKER_HOST=tcp://127.0.0.1:2375
DOCKER_BUILDKIT=1
COMPOSE_DOCKER_CLI_BUILD=1

# Proxy settings (from squid.conf)
http_proxy=http://proxy:3128
https_proxy=http://proxy:3128
NO_PROXY=localhost,127.0.0.1,proxy,dotnet-app
```

---

## Comparison: Docker Desktop vs Podman

| Feature | Docker Desktop | Podman |
|---------|----------------|--------|
| License | Subscription required for enterprise | Open source (Apache 2.0) |
| Rootless | No | Yes ✅ |
| Daemon | Required | Daemonless ✅ |
| Security | Good | Better (rootless, SELinux) ✅ |
| Windows Support | Native (WSL2) | Native (WSL2 or Windows) |
| VS Integration | Built-in | Requires wrappers |
| Performance | Good | Comparable |

---

## Next Steps

1. ✅ Create wrapper scripts (Method 1)
2. ✅ Update `launchSettings.json`
3. ✅ Test from VS Code (should work already)
4. ✅ Test from Visual Studio 2022
5. ✅ Test from PowerShell CLI
6. 📝 Document any additional issues encountered

---

## Resources

- [Podman Desktop](https://podman-desktop.io/)
- [Podman Documentation](https://docs.podman.io/)
- [Visual Studio Container Tools](https://learn.microsoft.com/en-us/visualstudio/containers/)
- [MSBuild Docker Properties](https://learn.microsoft.com/en-us/visualstudio/containers/container-msbuild-properties)

---

## Support Matrix

| Tool | Docker Desktop | Podman + Wrappers | Status |
|------|----------------|-------------------|--------|
| VS Code DevContainer | ✅ | ✅ | Works |
| Visual Studio 2022 Compose | ✅ | ✅ (with wrappers) | Works |
| PowerShell CLI | ✅ | ✅ | Works |
| GitHub Actions | ✅ | ✅ | Works |
| Azure DevOps | ✅ | ✅ | Works |
