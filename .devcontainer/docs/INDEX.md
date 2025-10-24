# Dev Container Files Index

This directory contains the complete VS Code Dev Container configuration for a secure, sandboxed development environment with AI tool support.

## 📑 File Directory

### Core Configuration Files
- **`devcontainer.json`** - Main VS Code Dev Container configuration
  - Defines container settings, VS Code extensions, user preferences
  - Configures lifecycle scripts and environment variables
  
- **`docker-compose.yml`** - Podman Compose orchestration
  - Defines services (devcontainer + proxy)
  - Configures volume mounts and network settings
  - Sets security options and resource limits
  
- **`Dockerfile`** - Container image definition
  - Base image: .NET 8.0 SDK
  - Installs development tools and AI CLI packages
  - Sets up non-root user and permissions

### Security & Network
- **`squid.conf`** - Squid proxy server configuration
  - Whitelist-based domain filtering
  - HTTP/HTTPS proxy on port 3128
  - Logging and caching settings
  
- **`allowed-domains.txt`** - Network access whitelist
  - List of permitted domains for network requests
  - Includes AI service APIs, package managers, common dev tools
  - Edit this file to add/remove allowed domains

### Configuration & Setup
- **`.env.example`** - Environment variable template
  - Template for API keys and configuration
  - Copy to `.env` and fill in your values
  - Never commit `.env` to version control
  
- **`.env`** - Your actual environment configuration
  - Created from `.env.example`
  - Contains your API keys (Git-ignored)
  
- **`.gitignore`** - Git ignore rules
  - Prevents committing sensitive files (.env, logs)

### Lifecycle Scripts
- **`scripts/post-create.sh`** - Post-creation script
  - Runs once after container is created
  - Installs AI CLI tools (OpenAI, Claude, Gemini, etc.)
  - Sets up helper scripts and aliases
  
- **`scripts/post-start.sh`** - Post-start script
  - Runs every time container starts
  - Checks network connectivity
  - Displays environment information

### Setup & Usage
- **`setup.ps1`** - PowerShell setup script
  - Interactive setup wizard
  - Creates .env file
  - Checks prerequisites
  - Optionally opens in container
  
- **`CHECKLIST.md`** - Setup verification checklist
  - Step-by-step setup tasks
  - Verification procedures
  - Troubleshooting checks

### Documentation
- **`README.md`** - Comprehensive documentation
  - Complete setup guide
  - Security features explained
  - Usage examples and troubleshooting
  
- **`QUICK_START.md`** - Quick reference guide
  - Common commands and tasks
  - Quick configuration examples
  - Workflow examples
  
- **`ARCHITECTURE.md`** - Architecture diagrams
  - Visual system architecture
  - Network flow diagrams
  - Security layers explained
  
- **`SETUP_SUMMARY.md`** - Setup summary
  - High-level overview
  - What was created
  - Quick start instructions

- **`PODMAN_SETUP.md`** - Podman configuration guide
  - Podman installation and setup
  - VS Code configuration for Podman
  - Migration from Docker
  - Troubleshooting Podman-specific issues
  
- **`INDEX.md`** - This file
  - Directory of all files
  - Purpose and relationships

## 🚀 Quick Start

### First Time Setup

1. **Run setup script:**
   ```powershell
   .\.devcontainer\setup.ps1
   ```

2. **Or manually:**
   ```powershell
   Copy-Item .devcontainer\.env.example .devcontainer\.env
   code .devcontainer\.env  # Add your API keys
   ```

3. **Open in container:**
   - Press `F1` → "Dev Containers: Reopen in Container"

### Daily Usage

- **Build project:** `dotnet build`
- **Run tests:** `dotnet test`
- **Ask AI:** `ask-openai "your question"`
- **Check network:** `proxy-status`

## 📋 File Relationships

```
.devcontainer/
│
├── Core Config (must understand)
│   ├── devcontainer.json    → References docker-compose.yml
│   ├── docker-compose.yml   → References Dockerfile, squid.conf
│   └── Dockerfile          → Base container image
│
├── Security (configure for your needs)
│   ├── squid.conf          → Used by proxy container
│   ├── allowed-domains.txt → Used by squid.conf
│   └── .env               → Used by docker-compose.yml
│
├── Automation (runs automatically)
│   └── scripts/
│       ├── post-create.sh  → Called by devcontainer.json
│       └── post-start.sh   → Called by devcontainer.json
│
├── Setup (run once)
│   ├── setup.ps1           → Interactive setup
│   ├── .env.example        → Template for .env
│   └── CHECKLIST.md        → Verification steps
│
└── Documentation (read as needed)
    ├── README.md           → Start here
    ├── QUICK_START.md      → Common tasks
    ├── ARCHITECTURE.md     → System design
    ├── SETUP_SUMMARY.md    → Overview
    └── INDEX.md            → This file
```

## 🎯 When to Edit Each File

| File | Edit When... |
|------|-------------|
| `.env` | Setting up for first time, rotating API keys |
| `docker-compose.yml` | Adding external folder mounts, changing resource limits |
| `allowed-domains.txt` | AI tools need access to new domains |
| `Dockerfile` | Need to install system packages |
| `post-create.sh` | Adding new AI CLI tools |
| `squid.conf` | Changing proxy behavior (advanced) |
| `devcontainer.json` | Adding VS Code extensions, changing settings |

## 🔐 Security Configuration Matrix

| Security Level | Network | Root FS | External Mounts |
|---------------|---------|---------|-----------------|
| **Maximum** | `internal: true` | Read-only | None or read-only only |
| **High** (default) | Proxy filtered | Read-only | Selective read-only |
| **Medium** | Proxy filtered | Read-only | Read-write allowed |
| **Development** | No proxy | Read-only | Full access |

Edit `docker-compose.yml` to change security levels.

## 📦 What Gets Installed

### In Dockerfile (build time)
- .NET 8.0 SDK
- Python 3.11 + pip
- Node.js 20 + npm
- Git, curl, wget, etc.
- Base Python packages

### In post-create.sh (container creation)
- OpenAI CLI
- Anthropic Claude CLI
- Google Gemini CLI
- GitHub Copilot CLI
- Additional Python tools
- Helper scripts

## 🔄 Rebuild Scenarios

| Change Made | Action Required |
|-------------|----------------|
| Edit `.env` | Recreate container |
| Edit `allowed-domains.txt` | Restart proxy: `podman-compose restart proxy` |
| Edit `Dockerfile` | Rebuild: F1 → "Rebuild Container" |
| Edit `docker-compose.yml` | Rebuild: F1 → "Rebuild Container" |
| Edit `post-create.sh` | Rebuild: F1 → "Rebuild Container" |
| Edit `post-start.sh` | Restart container |
| Edit `devcontainer.json` | Reload window or rebuild |

## 📚 Documentation Reading Order

1. **First time setup:**
   - `PODMAN_SETUP.md` - Install and configure Podman
   - `SETUP_SUMMARY.md` - What this is
   - `CHECKLIST.md` - Step by step setup
   - `README.md` - Detailed guide

2. **Daily usage:**
   - `QUICK_START.md` - Command reference
   - `README.md` (troubleshooting section)

3. **Understanding the system:**
   - `ARCHITECTURE.md` - How it works
   - `README.md` (security section)

4. **Customization:**
   - `README.md` (advanced configuration)
   - Source files (with inline comments)

## 🆘 Quick Troubleshooting

| Problem | Check File |
|---------|-----------|
| Container won't start | `README.md` → Troubleshooting |
| Network not working | `allowed-domains.txt`, `squid.conf` |
| API keys not found | `.env` |
| Build errors | `Dockerfile`, `docker-compose.yml` |
| Permission errors | `docker-compose.yml` (UID/GID in .env) |
| Missing AI tools | `scripts/post-create.sh` |

## 🔗 External Resources

- [VS Code Dev Containers Docs](https://code.visualstudio.com/docs/devcontainers/containers)
- [Podman Desktop](https://podman-desktop.io/)
- [Podman Compose Reference](https://github.com/containers/podman-compose)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [Squid Proxy Documentation](http://www.squid-cache.org/Doc/)
- [.NET SDK Documentation](https://docs.microsoft.com/en-us/dotnet/)

## ✅ Success Checklist

- [ ] Read `PODMAN_SETUP.md` and install Podman
- [ ] Configure VS Code to use Podman
- [ ] Read `SETUP_SUMMARY.md`
- [ ] Run `setup.ps1` or manually create `.env`
- [ ] Add API keys to `.env`
- [ ] Review `allowed-domains.txt`
- [ ] Open in container
- [ ] Follow `CHECKLIST.md` for verification
- [ ] Test AI tools
- [ ] Build and test project

---

**Need Help?**
- Start with `PODMAN_SETUP.md` for Podman installation
- Use `README.md` for comprehensive documentation
- Use `QUICK_START.md` for common commands
- Follow `CHECKLIST.md` for systematic verification
- Review `ARCHITECTURE.md` to understand the system
