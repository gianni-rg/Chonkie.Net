# VS Code Dev Container - Secure Sandboxed Environment

This Dev Container provides a secure, sandboxed environment for running AI-assisted tools and .NET development with controlled network access and filesystem isolation.

## � Documentation Structure

The devcontainer documentation is organized in the `docs/` folder:

- **[Quick Start Guide](docs/00-QUICK_START.md)** - Common commands and quick reference
- **[Setup Checklist](docs/01-SETUP_CHECKLIST.md)** - Step-by-step setup verification
- **[Architecture](docs/02-ARCHITECTURE.md)** - System design and diagrams
- **[Setup Guides](docs/setup/)** - Installation and configuration guides
  - [Podman Setup](docs/setup/PODMAN_SETUP.md)
  - [Setup Summary](docs/setup/SETUP_SUMMARY.md)
- **[Troubleshooting](docs/troubleshooting/)** - Problem-solving guides
  - [Proxy Issues](docs/troubleshooting/PROXY_ISSUES.md) - Complete proxy configuration & troubleshooting
  - [Migration to Podman](docs/troubleshooting/MIGRATION_TO_PODMAN.md)
  - [PIP Proxy Fixes](docs/troubleshooting/PIP_PROXY_FIX.md)

Configuration files are organized in the `configs/` folder:
- **[Proxy Configuration](configs/proxy/)** - Network filtering settings
- **[Tool Configuration](configs/tools/)** - Development tool proxy settings

## �🔐 Security Features

### Network Security
- **Controlled Internet Access**: All network traffic routes through a Squid proxy
- **Whitelist-based filtering**: Only approved domains in `configs/proxy/allowed-domains.txt` are accessible
- **Transparent HTTPS**: Optional SSL inspection for monitoring encrypted traffic
- **Isolated network**: Container runs on a dedicated Docker network

### Filesystem Security
- **Read-only root filesystem**: Prevents unauthorized modifications to the system
- **Workspace isolation**: Only the workspace and explicitly mounted folders are accessible
- **Selective mounting**: External folders can be mounted as read-only or read-write
- **Temporary storage**: Writable temp directories with `noexec` flag

### Runtime Security
- **Non-root user**: Container runs as `vscode` user (UID 1000)
- **Dropped capabilities**: Minimal Linux capabilities with `CAP_DROP ALL`
- **No new privileges**: Prevents privilege escalation
- **Resource limits**: CPU and memory limits configured in docker-compose

## 🚀 Quick Start

### Prerequisites
- Podman Desktop (Windows/Mac) or Podman (Linux)
- VS Code with Remote - Containers extension
- Git

### First-Time Setup

1. **Copy the environment file**:
   ```bash
   cp .devcontainer/.env.example .devcontainer/.env
   ```

2. **Configure your API keys** in `.devcontainer/.env`:
   ```env
   OPENAI_API_KEY=sk-your-key-here
   ANTHROPIC_API_KEY=sk-ant-your-key-here
   GOOGLE_API_KEY=your-gemini-key-here
   GITHUB_TOKEN=ghp_your-token-here
   ```

3. **Configure external folder mounts** (optional):
   Edit `.devcontainer/docker-compose.yml` and uncomment/modify the volume mounts:
   ```yaml
   volumes:
     - ${EXTERNAL_DATA_PATH:-~/data}:/mnt/data:ro
     - ${EXTERNAL_LIBS_PATH:-~/libs}:/mnt/libs:ro
     - ${EXTERNAL_OUTPUT_PATH:-~/output}:/mnt/output:rw
   ```

4. **Update allowed domains** (if needed):
   Edit `.devcontainer/configs/proxy/allowed-domains.txt` to add domains you need to access

5. **Open in Dev Container**:
   - Open VS Code in this workspace
   - Press `F1` and select "Dev Containers: Reopen in Container"
   - Wait for the container to build (first time takes ~5-10 minutes)
   - Note: VS Code will use Podman automatically if configured

## 📦 Installed Tools

### .NET Development
- .NET 10.0 SDK
- dotnet-ef (Entity Framework tools)
- dotnet-format (code formatter)
- dotnet-outdated-tool

### AI CLI Tools
- OpenAI CLI
- Anthropic Claude CLI
- Google Gemini CLI
- GitHub Copilot CLI (requires `gh auth login`)

### Other Tools
- Python 3.11 with pip
- Node.js 20 with npm
- Git and GitHub CLI
- Various utilities (curl, wget, jq, tree, etc.)

## 🎯 Usage

### Using AI Tools

The container provides a unified `ai-tools` wrapper:

```bash
# OpenAI
ai-tools openai chat "Explain this code"
ask-openai "What is this function doing?"

# Claude
ai-tools claude chat "Help me debug"
ask-claude "Review this code"

# Gemini
ai-tools gemini generate "Optimize this algorithm"
ask-gemini "Suggest improvements"

# GitHub Copilot
ai-tools copilot suggest "Write a unit test for..."
ask-copilot "Explain this error"
```

### .NET Development

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Run a project
cd samples/Chonkie.Sample
dotnet run
```

### Managing Network Access

**View current allowed domains**:
```bash
cat .devcontainer/configs/proxy/allowed-domains.txt
```

**Add a new domain**:
1. Edit `.devcontainer/configs/proxy/allowed-domains.txt`
2. Add the domain (e.g., `.example.com`)
3. Rebuild the container or restart the proxy

**Test network connectivity**:
```bash
# Check general connectivity
check-network

# Check proxy status
proxy-status

# Test specific domain
curl -I https://api.openai.com
```

**Disable network filtering** (for debugging):
Edit `.devcontainer/docker-compose.yml` and comment out proxy environment variables:
```yaml
# environment:
#   - http_proxy=http://proxy:3128
#   - https_proxy=http://proxy:3128
```
Then restart: `podman-compose restart devcontainer`

## 📁 Filesystem Layout

```
/workspace/              # Your project (read-write)
/mnt/data/              # External data folder (configure mount)
/mnt/libs/              # External libraries (configure mount)
/mnt/output/            # External output folder (configure mount)
/home/vscode/.nuget/    # NuGet packages cache (persistent)
/home/vscode/.vscode-server/  # VS Code extensions (persistent)
/tmp/                   # Writable temp (noexec, cleared on restart)
```

## 🔧 Configuration Files

### `.devcontainer/devcontainer.json`
Main Dev Container configuration. Controls:
- VS Code settings and extensions
- Container user and environment
- Port forwarding
- Lifecycle scripts

### `.devcontainer/docker-compose.yml`
Podman Compose services configuration. Controls:
- Volume mounts (workspace and external folders)
- Network settings
- Resource limits
- Security options

### `.devcontainer/Dockerfile`
Container image definition. Controls:
- Base image and installed packages
- User setup
- Pre-installed tools

### `.devcontainer/configs/proxy/squid.conf`
Proxy configuration. Controls:
- Network filtering rules
- Caching behavior
- Logging

### `.devcontainer/configs/proxy/allowed-domains.txt`
Whitelist of allowed domains for network access.

## 🛡️ Advanced Security Configuration

### Stricter Network Isolation

For complete network isolation (container can't access internet at all):

Edit `.devcontainer/docker-compose.yml`:
```yaml
networks:
  devcontainer-network:
    driver: bridge
    internal: true  # Change from false to true
```

### Read-Only External Mounts

Mount external folders as read-only:
```yaml
volumes:
  - /path/to/sensitive/data:/mnt/data:ro  # :ro = read-only
```

### Custom UID/GID

Match your host user for better file permissions:
```env
# In .env file
USER_UID=1001
USER_GID=1001
```

### Enable HTTPS Inspection

To inspect HTTPS traffic (for monitoring):

1. Generate SSL certificates:
   ```bash
   openssl req -new -newkey rsa:2048 -days 365 -nodes -x509 \
     -keyout .devcontainer/ssl/key.pem \
     -out .devcontainer/ssl/cert.pem
   ```

2. Uncomment SSL bump configuration in `.devcontainer/configs/proxy/squid.conf`

3. Rebuild the container

## 🐛 Troubleshooting

### Container won't start
- Check Podman Desktop is running
- Verify docker-compose.yml syntax
- Check logs: `podman-compose logs devcontainer`

### Network not working
- Verify proxy is running: `podman-compose ps`
- Check allowed-domains.txt contains the domain you need: `cat .devcontainer/configs/proxy/allowed-domains.txt`
- Test proxy directly: `curl -x http://proxy:3128 https://google.com`
- Check proxy logs: `podman-compose logs proxy`

### Permission denied errors
- Verify UID/GID matches your host user
- Check volume mount permissions
- Ensure directories are owned by vscode user

### AI tools not working
- Verify API keys are set in `.env`
- Check network access to AI service domains
- Test with curl: `curl https://api.openai.com`

### Read-only filesystem errors
- Ensure you're writing to allowed directories (/tmp, /workspace, /home/vscode)
- Check tmpfs mounts are configured correctly
- Use /tmp for temporary files

## 📚 Additional Resources

- [VS Code Dev Containers Documentation](https://code.visualstudio.com/docs/devcontainers/containers)
- [Podman Desktop Documentation](https://podman-desktop.io/docs)
- [Podman Compose Documentation](https://github.com/containers/podman-compose)
- [Docker Security Best Practices](https://docs.docker.com/engine/security/)
- [Squid Proxy Documentation](http://www.squid-cache.org/Doc/)

## 🔄 Updating the Container

When you make changes to the container configuration:

1. **Rebuild the container**:
   - Press `F1` → "Dev Containers: Rebuild Container"
   - Or: `podman-compose build --no-cache`

2. **Update running container**:
   - Press `F1` → "Dev Containers: Rebuild and Reopen in Container"

## 📝 Notes

- First build takes 5-10 minutes (subsequent builds are faster with caching)
- API keys are passed as environment variables (never committed to Git)
- Container state persists across restarts (NuGet cache, extensions, etc.)
- Network filtering may cause some legitimate requests to fail - add domains as needed
- Read-only filesystem prevents some tools from working - use /tmp or /workspace
- VS Code Dev Containers extension works with both Docker and Podman
- Podman provides rootless containers for enhanced security

## 🤝 Contributing

When adding new AI tools or domains:

1. Add the tool installation to `scripts/post-create.sh`
2. Add required domains to `allowed-domains.txt`
3. Update this README with usage instructions
4. Test in a clean container build

## ⚠️ Security Considerations

- **API Keys**: Never commit `.env` file to version control
- **Sensitive Data**: Don't mount sensitive folders as read-write
- **Network Access**: Regularly review and audit `allowed-domains.txt`
- **Updates**: Keep base images updated for security patches
- **Logs**: Proxy logs may contain sensitive URLs - secure appropriately
