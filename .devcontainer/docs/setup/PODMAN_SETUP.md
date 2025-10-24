# Podman Setup and Configuration

## Overview

This Dev Container is configured to work with **Podman** instead of Docker. Podman is a daemonless container engine that provides enhanced security through rootless containers and is compatible with Docker/OCI standards.

## Why Podman?

- **Rootless Containers**: Enhanced security by running containers without root privileges
- **Daemonless**: No background daemon required, reducing attack surface
- **Docker Compatible**: Drop-in replacement for Docker commands
- **OCI Compliant**: Works with standard container images and tools
- **Better Resource Isolation**: Improved security through user namespaces

## Installation

### Windows

1. **Install Podman Desktop**:
   - Download from: https://podman-desktop.io/downloads
   - Or use winget: `winget install RedHat.Podman-Desktop`

2. **Install Podman CLI** (if not included):
   - Download from: https://github.com/containers/podman/releases
   - Or via Chocolatey: `choco install podman`

3. **Initialize Podman Machine** (Windows/Mac only):
   ```powershell
   podman machine init
   podman machine start
   ```

4. **Install podman-compose**:
   ```powershell
   pip install podman-compose
   ```

### Linux

1. **Install Podman**:
   ```bash
   # Ubuntu/Debian
   sudo apt-get update
   sudo apt-get install podman
   
   # Fedora
   sudo dnf install podman
   
   # Arch
   sudo pacman -S podman
   ```

2. **Install podman-compose**:
   ```bash
   pip install podman-compose
   ```

3. **Enable rootless mode** (recommended):
   ```bash
   # Check subuid/subgid are configured
   grep $USER /etc/subuid /etc/subgid
   
   # If not, add entries (requires root)
   echo "$USER:100000:65536" | sudo tee -a /etc/subuid
   echo "$USER:100000:65536" | sudo tee -a /etc/subgid
   ```

## VS Code Configuration

### Dev Containers Extension

The **Remote - Containers** extension (ms-vscode-remote.remote-containers) works with both Docker and Podman.

### Configure VS Code to Use Podman

1. **Open VS Code Settings** (F1 → "Preferences: Open Settings (JSON)")

2. **Add Podman configuration**:
   ```json
   {
     "dev.containers.dockerPath": "podman",
     "dev.containers.dockerComposePath": "podman-compose"
   }
   ```

   Or for Windows with full paths:
   ```json
   {
     "dev.containers.dockerPath": "C:\\Program Files\\RedHat\\Podman\\podman.exe",
     "dev.containers.dockerComposePath": "podman-compose"
   }
   ```

3. **Reload VS Code** for settings to take effect

## Docker Compose Compatibility

This project uses `docker-compose.yml` which is compatible with both Docker Compose and podman-compose. The syntax is identical.

### Command Equivalents

| Docker Command | Podman Equivalent |
|---------------|-------------------|
| `docker ps` | `podman ps` |
| `docker images` | `podman images` |
| `docker build` | `podman build` |
| `docker run` | `podman run` |
| `docker-compose up` | `podman-compose up` |
| `docker-compose down` | `podman-compose down` |
| `docker-compose logs` | `podman-compose logs` |

## Troubleshooting

### Podman Machine Not Running (Windows/Mac)

```powershell
# Check status
podman machine list

# Start machine if stopped
podman machine start

# If issues persist, recreate
podman machine stop
podman machine rm
podman machine init --cpus 4 --memory 8192
podman machine start
```

### Permission Issues

```bash
# Linux: Verify rootless setup
podman unshare cat /proc/self/uid_map

# Should show mappings like: 0 1000 1 / 1 100000 65536
```

### VS Code Not Using Podman

1. Verify `dev.containers.dockerPath` is set to `podman`
2. Restart VS Code completely
3. Check Podman is in PATH: `podman --version`
4. Try: F1 → "Dev Containers: Rebuild Container"

### Network Issues with Rootless Podman

```bash
# Linux: Enable slirp4netns for better networking
sudo apt-get install slirp4netns

# Or use CNI networking
sudo apt-get install containernetworking-plugins
```

### Volume Mount Issues (Linux)

```bash
# If SELinux is enabled
# Add :Z flag to volumes in docker-compose.yml
volumes:
  - ../:/workspace:Z

# Or disable SELinux for testing (not recommended for production)
sudo setenforce 0
```

## Advanced Configuration

### Increase Resources (Windows/Mac)

Edit Podman machine resources:
```powershell
podman machine stop
podman machine rm
podman machine init --cpus 4 --memory 8192 --disk-size 50
podman machine start
```

### Rootless vs Rootful (Linux)

By default, Podman runs rootless. To run rootful:
```bash
# Rootful mode (requires sudo)
sudo podman run ...

# But rootless is recommended for security
podman run ...
```

### Docker Socket Compatibility (Optional)

To provide Docker socket compatibility:

**Linux:**
```bash
# Enable Podman socket
systemctl --user enable --now podman.socket

# Create Docker socket symlink
sudo ln -s /run/user/$UID/podman/podman.sock /var/run/docker.sock
```

**Windows/Mac:**
Podman Desktop creates a Docker-compatible socket automatically.

## Migration from Docker

If you previously used Docker:

1. **Stop Docker Desktop**
2. **Install Podman** (see above)
3. **Configure VS Code** to use Podman (see above)
4. **Rebuild containers**: F1 → "Dev Containers: Rebuild Container"

Existing `docker-compose.yml` files work without modification!

## Performance Tips

1. **Use Podman Machine with sufficient resources**:
   - CPUs: 4 or more
   - Memory: 8GB or more
   - Disk: 50GB or more

2. **Enable caching**:
   ```bash
   # In docker-compose.yml, volumes use :cached flag
   - ../:/workspace:cached
   ```

3. **Use volumes for build cache**:
   Named volumes persist between rebuilds for faster builds.

## Security Considerations

### Rootless Benefits
- Containers run as regular user
- No root access on host
- Process isolation via user namespaces
- Reduced attack surface

### Best Practices
1. Always run rootless when possible
2. Regularly update Podman: `podman version`
3. Scan images: `podman scan <image>`
4. Use minimal base images
5. Review security contexts in compose file

## Useful Commands

```bash
# Check Podman info
podman info

# List running containers
podman ps

# View container logs
podman logs <container-name>

# Execute command in container
podman exec -it <container-name> bash

# Clean up unused resources
podman system prune -a

# Check Podman machine status (Windows/Mac)
podman machine list
podman machine inspect

# View resource usage
podman stats
```

## Troubleshooting

### "Read-only file system" Errors

**Problem**: VS Code cannot create directories when container uses `read_only: true`

```
mkdir: cannot create directory '/var/devcontainer': Read-only file system
mkdir: cannot create directory '/vscode/vscode-server': Permission denied
```

**Solution**: The docker-compose.yml now includes:
- Persistent volumes for VS Code data
- tmpfs mounts for temporary directories

If you see these errors, rebuild:
```powershell
# In project root
podman compose -f .devcontainer/docker-compose.yml down -v
```

Then: F1 → "Dev Containers: Rebuild Container"

### WSL Wayland Socket Mount Error (Windows)

**Problem**: `unsupported UNC path \\wsl.localhost\...\wayland-0`

**Solution**: Disable Wayland socket mount in VS Code settings:
```json
{
  "dev.containers.mountWaylandSocket": false
}
```

Or add to devcontainer.json:
```json
{
  "hostRequirements": {
    "gpu": false
  }
}
```

### Git "dubious ownership" Warning

**Problem**: `fatal: detected dubious ownership in repository`

**Solution**: This is expected with read-only mounts. To suppress:
```bash
git config --global --add safe.directory /workspace
```

Or add to `.devcontainer/post-create.sh`.

## Known Limitations

1. **Docker Desktop Features**: Podman doesn't include a GUI by default
   - Solution: Use Podman Desktop

2. **Kubernetes**: Docker Desktop includes k8s
   - Solution: Use minikube or kind with Podman

3. **Some Compose Features**: Minor differences in docker-compose vs podman-compose
   - Most features work identically
   - Check: https://github.com/containers/podman-compose

4. **Windows WSL Path Handling**: Podman handles WSL UNC paths differently than Docker
   - GPU/Wayland features may need to be disabled
   - File permissions may require volume mounts instead of bind mounts

## Getting Help

- **Podman Docs**: https://docs.podman.io/
- **Podman Desktop**: https://podman-desktop.io/docs
- **GitHub Issues**: https://github.com/containers/podman/issues
- **Community**: https://podman.io/community/

## Verification Checklist

After setup, verify everything works:

- [ ] `podman --version` shows version
- [ ] `podman-compose --version` shows version
- [ ] `podman ps` runs without error
- [ ] VS Code settings point to Podman
- [ ] Can open workspace in Dev Container
- [ ] Container builds successfully
- [ ] Network access works (test with curl)

## Summary

Podman provides a secure, Docker-compatible container runtime that works seamlessly with VS Code Dev Containers. The configuration in this project supports both Docker and Podman with no modifications needed to the compose file.

**Key Takeaway**: Install Podman, configure VS Code, and everything else works the same!
