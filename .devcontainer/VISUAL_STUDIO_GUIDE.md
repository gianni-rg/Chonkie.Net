# Visual Studio 2022 Setup Guide for DevContainer

## Overview

Visual Studio 2022 doesn't have native DevContainer support like VS Code. However, you can still work with the secure DevContainer environment using one of these approaches.

## Recommended Approach: Hybrid Development

Use **both** VS Code and Visual Studio together:

### Setup Steps

1. **Start the DevContainer using VS Code or CLI**

   **Option A - VS Code:**
   ```
   1. Install "Dev Containers" extension
   2. F1 → "Dev Containers: Reopen in Container"
   ```

   **Option B - PowerShell:**
   ```powershell
   cd c:\Projects\Personal\Chonkie.Net
   .\.devcontainer\scripts\start-devcontainer.ps1
   ```

2. **Open in Visual Studio 2022**
   
   - Launch Visual Studio 2022
   - File → Open → Project/Solution
   - Navigate to `c:\Projects\Personal\Chonkie.Net\Chonkie.Net.sln`
   - Open the solution normally

3. **Workflow**

   - **Visual Studio**: Use for development, IntelliSense, refactoring, debugging
   - **VS Code**: Use for AI assistants (Copilot, chat) and terminal work
   - Files sync automatically via Docker volume mount

### Why This Works

- DevContainer runs in background
- Your files are mounted at `/workspace` in container
- Changes in Visual Studio appear immediately in container
- Both IDEs work with the same files simultaneously

## Alternative: Container Tools for Visual Studio

Visual Studio has built-in Docker/Container support:

### Setup

1. **Install Container Tools**
   - Visual Studio Installer → Modify
   - Check "Container development tools"

2. **Configure Docker Compose**

   Visual Studio can use the root `docker-compose.yml`, but NOT the `.devcontainer/docker-compose.yml` directly. You need to merge them.

3. **Limitations**
   - No automatic extension installation
   - No devcontainer.json support
   - Proxy configuration must be manual

## Network Isolation with Visual Studio

The DevContainer's proxy security works regardless of which IDE you use:

### How It Works

```
Your IDE (VS/VSCode)
      ↓
Files on disk (c:\Projects\...)
      ↓
Docker volume mount
      ↓
Container filesystem (/workspace)
      ↓
AI Agents/Tools running in container
      ↓
Squid Proxy (network filter)
      ↓
Only allowed domains
```

### Important Notes

- ✅ Network isolation applies to **code running in the container**
- ❌ Network isolation does NOT apply to **Visual Studio itself**
- ❌ Network isolation does NOT apply to **NuGet package restore in VS**

### Security Implications

If you run/debug from Visual Studio:
- The app runs on **your host machine** (outside container)
- No network restrictions apply
- AI agents must be installed and run **inside the container**

If you want full security:
- Run/debug from within the container
- Use `docker exec` or VS Code terminal
- Don't use F5/Debug in Visual Studio

## Remote Debugging (Advanced)

You can debug container processes from Visual Studio:

### Setup Remote Debugging

1. **Install Remote Debugger in Container**

   Add to `.devcontainer/Dockerfile.dotnet`:
   ```dockerfile
   # Install VS Remote Debugger
   RUN curl -sSL https://aka.ms/getvsdbgsh | \
       bash /dev/stdin -v latest -l /vsdbg
   ```

2. **Attach to Process**
   
   - Debug → Attach to Process
   - Connection Type: Docker (Linux Container)
   - Select your container
   - Find your .NET process
   - Attach

3. **Configure Launch Settings**

   Modify `launchSettings.json` to run in container

### Limitations

- More complex setup
- Slower than native debugging
- Some features may not work

## Recommended Workflow Patterns

### Pattern 1: AI-Assisted Development

```
1. Use VS Code with Copilot to generate/refactor code
2. Switch to Visual Studio for detailed implementation
3. Run/test in container via VS Code terminal
4. Iterate
```

### Pattern 2: Visual Studio Primary

```
1. Develop primarily in Visual Studio
2. When you need AI assistance:
   - Open VS Code (same project)
   - Use Copilot/AI chat
   - Close VS Code
3. Continue in Visual Studio
```

### Pattern 3: Container-First

```
1. Start container: .devcontainer/scripts/start-devcontainer.ps1
2. Attach VS Code to container
3. Use Visual Studio for file editing only
4. Run all builds/tests in container via VS Code
```

## Comparison Matrix

| Feature | VS Code + DevContainer | Visual Studio + DevContainer (Hybrid) | Visual Studio Only |
|---------|----------------------|-----------------------------------|-------------------|
| AI Assistants (Copilot) | ✅ Built-in | ✅ In VS Code | ✅ Built-in |
| Network Isolation | ✅ Full | ⚠️ Only for container processes | ❌ No |
| IntelliSense | ✅ Good | ✅ Excellent | ✅ Excellent |
| Debugging | ✅ Good | ✅ Excellent | ✅ Excellent |
| Refactoring | ⚠️ Basic | ✅ Advanced | ✅ Advanced |
| Performance | ⚠️ Slower | ✅ Fast | ✅ Fast |
| Setup Complexity | ⚠️ Medium | ⚠️ Medium | ✅ Easy |

## Troubleshooting

### "Cannot connect to container"

```powershell
# Check container status
docker ps -a

# Restart container
.\.devcontainer\scripts\stop-devcontainer.ps1
.\.devcontainer\scripts\start-devcontainer.ps1
```

### "File changes not syncing"

- Ensure container is running
- Check volume mounts: `docker inspect chonkienet-dotnet-app-1`
- Try closing and reopening files

### "Build fails in Visual Studio"

- Visual Studio builds on host, not in container
- Container has isolated dependencies
- Solution: Use `dotnet build` in container terminal

### "Proxy blocks NuGet restore"

If you run `dotnet restore` in Visual Studio:
- It uses your host network (bypasses proxy)
- This is expected and safe for package restore

If you run it in container:
- Uses proxy
- NuGet.org is in the allow-list
- Should work automatically

## Quick Reference

### Start Container
```powershell
.\.devcontainer\scripts\start-devcontainer.ps1
```

### Stop Container
```powershell
.\.devcontainer\scripts\stop-devcontainer.ps1
```

### Test Network
```powershell
.\.devcontainer\scripts\test-network.ps1
```

### Access Container Shell
```powershell
docker exec -it chonkienet-dotnet-app-1 bash
```

### View Logs
```powershell
docker-compose -f .devcontainer/docker-compose.yml logs -f
```

## Summary

**Best Practice:**
1. Start DevContainer once at beginning of day
2. Use Visual Studio for development work
3. Switch to VS Code when you need AI assistance
4. Run/test code in container to ensure network isolation
5. Stop container at end of day

This gives you the best of both worlds:
- Visual Studio's powerful IDE features
- VS Code's AI integration
- Container's security and isolation
