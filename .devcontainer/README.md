# DevContainer Setup for Secure AI Agent Development

This devcontainer provides a secure, isolated development environment for testing AI coding assistants like GitHub Copilot, Gemini CLI, and others.

## Features

### Security
- **Network Isolation**: All outbound traffic routes through a Squid proxy
- **Allow-List Configuration**: Only approved AI service domains are accessible
- **Logging**: All network requests are logged for audit

### Development Tools
- .NET SDK 9.0
- Node.js 20
- Git & GitHub CLI
- VS Code with Copilot extensions pre-installed

### Supported Platforms
- ✅ **VS Code** - Full support via Dev Containers extension
- ✅ **CLI** - Direct Docker Compose / Podman Compose usage
- ✅ **Visual Studio 2022** - Full support with Podman wrappers (see [`QUICKSTART_VS_PODMAN.md`](QUICKSTART_VS_PODMAN.md))
- ✅ **Podman** - Alternative to Docker Desktop (no licensing fees)

## Usage

### VS Code (Recommended)

1. Install the [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)
2. Open the project folder
3. Press `F1` → "Dev Containers: Reopen in Container"
4. Wait for the container to build and start

### CLI (Command Line)

```powershell
# Navigate to project root
cd c:\Projects\Personal\Chonkie.Net

# Start the containers
docker-compose -f .devcontainer/docker-compose.yml up -d

# Attach to the development container
docker exec -it chonkienet-dotnet-app-1 bash

# Work in the container
cd /workspace
dotnet build
```

### Visual Studio 2022 with Podman

Visual Studio 2022 supports Docker Compose debugging. When using Podman, you need wrapper scripts.

**Quick Setup (5 minutes):**

```powershell
# 1. Run setup script (as administrator)
cd .devcontainer\scripts
.\setup-podman-wrappers.ps1 -AddToPath

# 2. Restart Visual Studio
# 3. Open Chonkie.Net.sln
# 4. Set docker-compose as startup project
# 5. Press F5 to debug
```

**Full Documentation:**
- [`QUICKSTART_VS_PODMAN.md`](QUICKSTART_VS_PODMAN.md) - 5-minute setup guide
- [`VISUAL_STUDIO_PODMAN_SETUP.md`](VISUAL_STUDIO_PODMAN_SETUP.md) - Complete reference

**Alternative: Use with Docker Desktop**

If you have Docker Desktop installed, no additional setup needed:
1. Open `Chonkie.Net.sln` in Visual Studio
2. Set `docker-compose` as startup project
3. Press F5

**Hybrid Approach (Best of Both Worlds)**
- Use **VS Code** for AI assistant interactions (Copilot, chat, code generation)
- Use **Visual Studio** for heavy lifting (refactoring, debugging, profiling)
- Both can work simultaneously with the same codebase via volume mounts

## Network Configuration

### Inbound Rules (Exposed Ports)
- `3000` - Application services
- `5000` - HTTP endpoints
- `5001` - HTTPS endpoints
- `3128` - Squid proxy (for debugging)

### Outbound Rules (Proxy Allow-List)
Allowed domains are configured in `squid.conf`:
- GitHub & Copilot services
- OpenAI API
- Azure OpenAI
- Google AI (Gemini, Vertex)
- Anthropic (Claude)
- Microsoft authentication
- NuGet, NPM package repositories
- VS Code marketplace

**All other domains are blocked by default.**

### Adding New Domains

If you need to allow additional domains:

1. Edit `.devcontainer/squid.conf`
2. Add ACL entry: `acl ai_services_domains dstdomain .example.com`
3. Rebuild: `docker-compose -f .devcontainer/docker-compose.yml up -d --build`
4. Check logs: `docker-compose -f .devcontainer/docker-compose.yml logs proxy`

### Viewing Blocked Requests

```bash
# View proxy logs
docker exec chonkienet-proxy-1 tail -f /var/log/squid/access.log

# Or via compose
docker-compose -f .devcontainer/docker-compose.yml logs -f proxy
```

## Installing Additional AI Agents

### Gemini CLI

```bash
# Inside the container
npm config set proxy http://proxy:3128
npm config set https-proxy http://proxy:3128
npm install -g @google/gemini-cli
```

### Other CLIs

Add installation commands to `devcontainer.json` → `postCreateCommand`:

```json
"postCreateCommand": "npm install -g @some/ai-cli && echo 'Setup complete!'"
```

## Troubleshooting

### Container Won't Build
```powershell
# Clean and rebuild
docker-compose -f .devcontainer/docker-compose.yml down -v
docker-compose -f .devcontainer/docker-compose.yml build --no-cache
docker-compose -f .devcontainer/docker-compose.yml up -d
```

### Network Access Issues
```bash
# Test proxy connectivity
docker exec chonkienet-dotnet-app-1 curl -x http://proxy:3128 https://api.github.com

# Check if domain is allowed
docker exec chonkienet-proxy-1 grep "github.com" /etc/squid/squid.conf
```

### VS Code Extensions Not Working
1. Check proxy settings in `devcontainer.json` → `customizations.vscode.settings`
2. Ensure container has internet access through proxy
3. Try reinstalling extensions: `F1` → "Developer: Reload Window"

### Volumes Not Persisting
```powershell
# List volumes
docker volume ls

# Inspect volume
docker volume inspect chonkienet_dotnet-packages
```

## File Structure

```
.devcontainer/
├── docker-compose.yml      # Multi-container setup
├── devcontainer.json       # VS Code configuration
├── Dockerfile.dotnet       # .NET development image
├── squid.conf              # Proxy allow-list
└── README.md               # This file
```

## Security Considerations

### What This Protects Against
✅ Unauthorized network access by AI agents  
✅ Data exfiltration to unknown domains  
✅ Accidental exposure of sensitive data  
✅ Malicious code execution attempting network access  

### What This Doesn't Protect Against
❌ Local file system access (agents have full access)  
❌ Execution of malicious code that doesn't require network  
❌ Social engineering attacks  
❌ Compromised allowed domains  

### Best Practices
1. **Review AI suggestions** before accepting
2. **Monitor proxy logs** regularly
3. **Keep allow-list minimal** - only add trusted domains
4. **Use version control** - track all AI-generated changes
5. **Never commit secrets** - use environment variables

## Performance Tips

### Speed Up Rebuilds
Volumes persist data between rebuilds:
- `dotnet-packages` - NuGet packages
- `vscode-extensions` - VS Code extensions
- `squid-cache` - Proxy cache

### Reduce Build Time
Comment out unused tools in `Dockerfile.dotnet`

### Optimize Proxy
Squid caches responses. Increase cache size in `squid.conf`:
```conf
cache_dir ufs /var/spool/squid 500 16 256  # 500 MB cache
```

## Additional Resources

- [VS Code Dev Containers Documentation](https://code.visualstudio.com/docs/devcontainers/containers)
- [Squid Proxy Documentation](http://www.squid-cache.org/Doc/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [GitHub Copilot Documentation](https://docs.github.com/copilot)

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Review container logs: `docker-compose -f .devcontainer/docker-compose.yml logs`
3. Verify network configuration in `squid.conf`
4. Test connectivity manually using `curl` inside the container
