# Dev Container Setup Checklist

Use this checklist to set up your secure sandboxed development environment.

## ☐ Pre-Setup Tasks

- [ ] Ensure Podman Desktop is installed and running
- [ ] Ensure VS Code has "Remote - Containers" extension installed
- [ ] Close any running containers that might conflict
- [ ] Have your API keys ready (OpenAI, Anthropic, Google, GitHub, etc.)

## ☐ Initial Configuration

### 1. Environment Variables
- [ ] Copy `.devcontainer/.env.example` to `.devcontainer/.env`
  ```powershell
  Copy-Item .devcontainer\.env.example .devcontainer\.env
  ```
- [ ] Edit `.devcontainer/.env` and add your API keys:
  - [ ] `OPENAI_API_KEY=sk-...`
  - [ ] `ANTHROPIC_API_KEY=sk-ant-...`
  - [ ] `GOOGLE_API_KEY=...`
  - [ ] `GITHUB_TOKEN=ghp_...`
  - [ ] `COHERE_API_KEY=...` (optional)
  - [ ] `VOYAGE_API_KEY=...` (optional)

### 2. External Folder Mounts (Optional)
- [ ] Decide which external folders you want to mount
- [ ] Edit `.devcontainer/docker-compose.yml`
- [ ] Uncomment and configure volume mounts:
  ```yaml
  - D:/MyData:/mnt/data:ro      # Read-only data
  - D:/MyLibs:/mnt/libs:ro      # Read-only libraries  
  - D:/Output:/mnt/output:rw    # Read-write output
  ```
- [ ] Update the `.env` file with paths if using environment variables

### 3. Network Configuration
- [ ] Review `.devcontainer/allowed-domains.txt`
- [ ] Add any additional domains your AI tools need
- [ ] Verify all required AI service domains are listed:
  - [ ] `.openai.com`
  - [ ] `.anthropic.com`
  - [ ] `.googleapis.com`
  - [ ] `.github.com`
  - [ ] `.nuget.org`

### 4. Security Settings (Optional Adjustments)
- [ ] Review resource limits in `docker-compose.yml`:
  - [ ] CPU limit (default: 4 cores)
  - [ ] Memory limit (default: 8GB)
- [ ] Decide on network isolation level:
  - [ ] Proxy-filtered (default)
  - [ ] Complete isolation (set `internal: true`)
  - [ ] No filtering (comment out proxy env vars)

## ☐ First Build

- [ ] Open VS Code in the workspace folder
- [ ] Press `F1` (or `Ctrl+Shift+P`)
- [ ] Type and select: "Dev Containers: Reopen in Container"
- [ ] Wait for initial build (5-10 minutes)
- [ ] Watch for any build errors in the output
- [ ] Verify container starts successfully
- [ ] Note: VS Code will use Podman automatically if configured

## ☐ Post-Build Verification

### Test Container Basics
- [ ] Open integrated terminal in VS Code
- [ ] Verify you're the correct user:
  ```bash
  whoami  # Should show: vscode
  id      # Should show: uid=1000(vscode) gid=1000(vscode)
  ```
- [ ] Check .NET is installed:
  ```bash
  dotnet --version  # Should show: 8.0.x
  ```
- [ ] Check Python is installed:
  ```bash
  python3 --version  # Should show: 3.11.x
  ```

### Test Network Access
- [ ] Test general connectivity:
  ```bash
  check-network
  ```
- [ ] Test proxy connectivity:
  ```bash
  proxy-status
  ```
- [ ] Test allowed domain access:
  ```bash
  curl -I https://api.openai.com
  curl -I https://api.anthropic.com
  curl -I https://github.com
  ```
- [ ] Verify blocked domain is rejected:
  ```bash
  curl -I https://facebook.com  # Should timeout or fail
  ```

### Test Filesystem Security
- [ ] Verify read-only root filesystem:
  ```bash
  touch /test.txt  # Should fail: Read-only file system
  ```
- [ ] Verify writable areas work:
  ```bash
  touch /tmp/test.txt        # Should succeed
  touch /workspace/test.txt  # Should succeed
  rm /tmp/test.txt /workspace/test.txt
  ```
- [ ] Check workspace is mounted:
  ```bash
  ls -la /workspace  # Should show your project files
  ```
- [ ] Check external mounts (if configured):
  ```bash
  ls -la /mnt/data    # Should show your external data
  ls -la /mnt/libs    # Should show your external libraries
  ls -la /mnt/output  # Should show your output folder
  ```

### Test Development Tools
- [ ] Build the solution:
  ```bash
  cd /workspace
  dotnet restore Chonkie.Net.sln
  dotnet build Chonkie.Net.sln
  ```
- [ ] Run tests:
  ```bash
  dotnet test
  ```
- [ ] Verify AI tools wrapper exists:
  ```bash
  which ai-tools  # Should show: /home/vscode/bin/ai-tools
  ```

## ☐ AI Tools Setup

### GitHub Copilot
- [ ] Authenticate with GitHub:
  ```bash
  gh auth login
  ```
- [ ] Follow the prompts to authenticate
- [ ] Test Copilot:
  ```bash
  gh copilot suggest "write a hello world in C#"
  ```

### OpenAI
- [ ] Verify API key is set:
  ```bash
  echo $OPENAI_API_KEY  # Should show your key
  ```
- [ ] Test OpenAI CLI:
  ```bash
  ask-openai "Say hello"
  ```

### Anthropic Claude
- [ ] Verify API key is set:
  ```bash
  echo $ANTHROPIC_API_KEY  # Should show your key
  ```
- [ ] Test Claude CLI:
  ```bash
  ask-claude "Say hello"
  ```

### Google Gemini
- [ ] Verify API key is set:
  ```bash
  echo $GOOGLE_API_KEY  # Should show your key
  ```
- [ ] Test Gemini CLI:
  ```bash
  ask-gemini "Say hello"
  ```

## ☐ VS Code Integration

- [ ] Verify C# extension is working
- [ ] Open a .cs file and check IntelliSense works
- [ ] Verify GitHub Copilot extension is active
- [ ] Test Copilot suggestions in code
- [ ] Check terminal integration works
- [ ] Verify debugging works (set breakpoint, F5)

## ☐ Security Audit

- [ ] Verify `.env` is not committed to Git:
  ```bash
  git status  # Should show .env as untracked/ignored
  ```
- [ ] Check proxy logs for unwanted traffic:
  ```bash
  docker-compose logs proxy | tail -50
  ```
- [ ] Review mounted volumes:
  ```bash
  mount | grep /mnt
  mount | grep /workspace
  ```
- [ ] Confirm no unnecessary capabilities:
  ```bash
  podman inspect <container-id> | grep -A20 CapAdd
  ```

## ☐ Performance Tuning (Optional)

- [ ] Monitor resource usage:
  ```bash
  htop  # Inside container
  # Or from host: podman stats
  ```
- [ ] Adjust CPU/memory limits if needed in `docker-compose.yml`
- [ ] Configure squid cache size if needed in `squid.conf`

## ☐ Documentation Review

- [ ] Read `.devcontainer/README.md` (comprehensive guide)
- [ ] Review `.devcontainer/QUICK_START.md` (quick reference)
- [ ] Check `.devcontainer/ARCHITECTURE.md` (architecture diagrams)
- [ ] Bookmark this checklist for future rebuilds

## ☐ Ongoing Maintenance

### Weekly
- [ ] Review proxy logs for errors
- [ ] Check for failed requests to needed domains
- [ ] Update `allowed-domains.txt` as needed

### Monthly
- [ ] Update base Podman images:
  ```bash
  podman-compose pull
  podman-compose build --no-cache
  ```
- [ ] Review and update AI CLI tools
- [ ] Check for security updates in packages

### As Needed
- [ ] Add new allowed domains when required
- [ ] Configure additional external mounts
- [ ] Adjust resource limits based on usage
- [ ] Update API keys when they rotate

## ☐ Troubleshooting Checklist

If something doesn't work:

- [ ] Check Podman Desktop is running
- [ ] Verify `.env` file exists and has correct API keys
- [ ] Review container logs: `podman-compose logs devcontainer`
- [ ] Review proxy logs: `podman-compose logs proxy`
- [ ] Try rebuilding: F1 → "Dev Containers: Rebuild Container"
- [ ] Check network connectivity inside container
- [ ] Verify allowed-domains.txt has required domains
- [ ] Check file permissions if mounting issues
- [ ] Review this checklist for missed steps

## ✅ Setup Complete!

Once all items are checked:
- Your secure, sandboxed dev environment is ready
- AI tools are configured and working
- Network access is controlled and filtered
- Filesystem is isolated and protected
- Development tools are functional

**Next Steps:**
1. Start coding with AI assistance!
2. Test your chunking algorithms
3. Run integration tests with AI services
4. Monitor network and resource usage
5. Adjust configuration as needed

---

**Need Help?**
- Check `.devcontainer/README.md` for detailed docs
- Review `.devcontainer/QUICK_START.md` for commands
- See `.devcontainer/ARCHITECTURE.md` for system design
- Check troubleshooting section above
