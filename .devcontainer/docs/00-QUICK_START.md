# Dev Container Quick Reference

## 🚀 Quick Setup Commands

```bash
# 1. Copy environment template
cp .devcontainer/.env.example .devcontainer/.env

# 2. Edit with your API keys
code .devcontainer/.env

# 3. (Optional) Configure external mounts
code .devcontainer/docker-compose.yml

# 4. Open in Dev Container
# Press F1 → "Dev Containers: Reopen in Container"
```

## 🔧 Common Tasks

### Building and Testing
```bash
# Build entire solution
dotnet build Chonkie.Net.sln

# Run all tests
dotnet test

# Run specific project
cd samples/Chonkie.Sample
dotnet run
```

### Using AI Tools
```bash
# Ask questions
ask-openai "How do I implement semantic chunking?"
ask-claude "Review my code changes"
ask-gemini "Optimize this algorithm"

# GitHub Copilot (after: gh auth login)
gh copilot suggest "Write unit test for TokenChunker"
gh copilot explain "What does this function do?"
```

### Network Management
```bash
# Test connectivity
curl https://api.openai.com
curl https://api.anthropic.com

# Check proxy status
proxy-status

# View allowed domains
cat .devcontainer/allowed-domains.txt

# Add new domain
echo ".example.com" >> .devcontainer/allowed-domains.txt
# Then restart proxy: docker-compose restart proxy
```

### Filesystem Access
```bash
# Workspace (read-write)
ls /workspace

# External mounts (configure in docker-compose.yml)
ls /mnt/data      # Your external data (read-only example)
ls /mnt/libs      # Your external libraries (read-only example)
ls /mnt/output    # Your output folder (read-write example)

# Temp storage (writable, cleared on restart)
ls /tmp
```

## 🔐 Security Commands

```bash
# Check current user (should be 'vscode')
whoami
id

# Check filesystem is read-only
touch /test.txt  # Should fail with "Read-only file system"

# Verify writable directories
touch /tmp/test.txt  # Should succeed
touch /workspace/test.txt  # Should succeed
```

## 🐛 Troubleshooting

### Container won't build
```bash
# Clean rebuild
podman-compose down -v
podman-compose build --no-cache
podman-compose up -d
```

### Network issues
```bash
# Check proxy is running
podman-compose ps

# View proxy logs
podman-compose logs proxy

# Test without proxy (temporarily)
unset http_proxy https_proxy
curl https://google.com
```

### API keys not working
```bash
# Check environment variables are set
env | grep API_KEY

# Reload .env file
podman-compose down
podman-compose up -d
```

## 📂 File Structure

```
.devcontainer/
├── devcontainer.json      # Main VS Code config
├── docker-compose.yml     # Container orchestration
├── Dockerfile            # Container image definition
├── squid.conf            # Proxy configuration
├── allowed-domains.txt   # Network whitelist
├── .env.example          # Template for API keys
├── .env                  # Your API keys (git-ignored)
├── .gitignore            # Ignore local files
├── README.md             # Full documentation
├── QUICK_START.md        # This file
└── scripts/
    ├── post-create.sh    # Run after container creation
    └── post-start.sh     # Run on container start
```

## 🔄 Rebuilding After Changes

```bash
# After editing Dockerfile or docker-compose.yml
# In VS Code: F1 → "Dev Containers: Rebuild Container"

# Or manually:
podman-compose build
podman-compose up -d
# Then: F1 → "Dev Containers: Reopen in Container"
```

## 📝 Configuration Files to Edit

| File | Purpose | When to Edit |
|------|---------|-------------|
| `.env` | API keys and secrets | Always (first time setup) |
| `docker-compose.yml` | Volume mounts, resources | When adding external folders |
| `allowed-domains.txt` | Network whitelist | When AI tools need new domains |
| `post-create.sh` | Tool installation | When adding new CLI tools |
| `Dockerfile` | Container base setup | Rarely (only for system packages) |

## 🎯 Example Workflows

### Adding External Data Folder
1. Edit `docker-compose.yml`:
   ```yaml
   volumes:
     - ~/my-data:/mnt/mydata:ro
   ```
2. Rebuild: F1 → "Rebuild Container"
3. Access: `ls /mnt/mydata`

### Adding New AI Service Domain
1. Edit `allowed-domains.txt`:
   ```
   .newai-service.com
   api.newai-service.com
   ```
2. Restart proxy: `podman-compose restart proxy`
3. Test: `curl https://api.newai-service.com`

### Installing New AI CLI Tool
1. Edit `scripts/post-create.sh`:
   ```bash
   pip3 install --user new-ai-cli
   ```
2. Rebuild container
3. Use: `new-ai-cli --help`

## 💡 Tips

- **Persistent Storage**: NuGet packages, VS Code extensions persist across rebuilds
- **Performance**: First build is slow (~10min), rebuilds are faster
- **Security**: Never commit `.env` file - it's git-ignored
- **Debugging**: Check logs with `podman-compose logs` if issues occur
- **Updates**: Regularly update allowed domains as you discover what's needed
- **Podman**: Works seamlessly with VS Code Dev Containers extension
