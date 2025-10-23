# Quick Start: Visual Studio 2022 with Podman

This guide will get you up and running with Visual Studio 2022 using Podman instead of Docker Desktop.

## Prerequisites

1. **Podman Desktop** or **Podman CLI** installed
   - Download from: <https://podman-desktop.io/>
   - Or install via winget: `winget install RedHat.Podman-Desktop`

2. **Visual Studio 2022** (17.8 or later)
   - With "ASP.NET and web development" workload
   - With "Azure development" workload (for container tools)

3. **Administrative privileges** (for one-time setup)

## Installation Steps

### Step 1: Install Wrapper Scripts

Run the setup script as administrator:

```powershell
# Navigate to the project
cd C:\Projects\Personal\Chonkie.Net\.devcontainer\scripts

# Run setup script (as administrator)
.\setup-podman-wrappers.ps1 -AddToPath
```

This will:

- Create `C:\bin` directory
- Copy wrapper scripts to redirect docker/docker-compose → podman
- Add `C:\bin` to your system PATH

### Step 2: Verify Installation

Open a **new** PowerShell window (to pick up PATH changes):

```powershell
# Test docker wrapper
docker --version
# Should show: podman version X.Y.Z

# Test docker-compose wrapper
docker-compose --version
# Should show: podman compose version X.Y.Z
```

### Step 3: Configure Podman Service (Optional)

For best compatibility, start Podman's Docker-compatible service:

```powershell
# Start Podman machine (if using machine mode)
podman machine start

# Start Docker-compatible API service
podman system service --time=0 tcp:127.0.0.1:2375
```

**For permanent service:**

Create a Windows Scheduled Task:

```powershell
# Create startup task (as administrator)
$action = New-ScheduledTaskAction -Execute "podman" -Argument "system service --time=0 tcp:127.0.0.1:2375"
$trigger = New-ScheduledTaskTrigger -AtStartup
$principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest
Register-ScheduledTask -TaskName "PodmanDockerService" -Action $action -Trigger $trigger -Principal $principal
```

### Step 4: Test with Visual Studio

1. Open `Chonkie.Net.sln` in Visual Studio 2022
2. In Solution Explorer, right-click on `docker-compose` project
3. Select **Set as Startup Project**
4. Press **F5** to start debugging

Visual Studio should now:

- ✅ Build containers using Podman
- ✅ Start the devcontainer services
- ✅ Attach the debugger to your .NET application

## Alternative: VS Code

If you prefer VS Code, it should work out of the box:

1. Open project folder in VS Code
2. Press `Ctrl+Shift+P`
3. Select **"Dev Containers: Reopen in Container"**
4. VS Code will use Podman automatically

## Troubleshooting

### "Docker is not running"

**Solution:** Start Podman service:

```powershell
podman machine start
podman system service --time=0 tcp:127.0.0.1:2375
```

### "Cannot find docker-compose.exe"

**Solution:** Verify PATH contains `C:\bin`:

```powershell
$env:PATH -split ';' | Select-String 'C:\\bin'
```

If not found, restart your computer or manually add to PATH.

### "Access Denied" when building containers

**Solution:** Run Podman machine as administrator once:

```powershell
# As administrator
podman machine stop
podman machine start
```

### Visual Studio can't attach debugger

**Solution:** Ensure service is running on correct port:

```powershell
# Check if service is listening
netstat -an | findstr "2375"

# Should show: TCP    127.0.0.1:2375    LISTENING
```

## What's Different from Docker Desktop?

| Aspect | Docker Desktop | Podman |
|--------|---------------|--------|
| **Installation** | GUI installer | CLI or Desktop app |
| **Daemon** | Required | Daemonless (runs as service) |
| **Root Access** | Requires admin | Can run rootless |
| **VS Integration** | Native | Via wrappers ✅ |
| **Performance** | WSL2 backend | Native or WSL2 |
| **License** | Commercial use fee | Free & Open Source |

## Next Steps

- ✅ Read full setup guide: `VISUAL_STUDIO_PODMAN_SETUP.md`
- ✅ Review devcontainer config: `devcontainer.json`
- ✅ Explore AI agent isolation: `README.md`

## Support

If you encounter issues:

1. Check Podman version: `podman --version` (should be 4.0+)
2. Check wrapper scripts: `Get-ChildItem C:\bin\docker*`
3. Review Visual Studio output window for detailed errors
4. See troubleshooting section in `VISUAL_STUDIO_PODMAN_SETUP.md`

---

**Pro Tip:** You can use both Docker Desktop AND Podman simultaneously by controlling which one the wrappers point to. Just update the paths in the wrapper scripts.
