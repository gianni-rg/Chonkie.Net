# Configuration Files

This directory contains all configuration files for the dev container.

## 📁 Directory Structure

```
configs/
├── proxy/          # Proxy server configurations
│   ├── squid.conf               # Squid proxy server config
│   ├── allowed-domains.txt      # Network access whitelist
│   └── README.md                # Proxy config documentation
└── tools/          # Tool-specific configurations
    ├── pip.conf                 # Python pip proxy config
    ├── npmrc                    # Node.js npm proxy config
    ├── curlrc                   # curl proxy config
    ├── wgetrc                   # wget proxy config
    ├── apt-proxy.conf           # APT package manager proxy config
    ├── docker-config.json       # Docker CLI proxy config
    ├── gitconfig-proxy          # Git proxy config
    └── nuget.config             # NuGet proxy config
```

## 🔧 Configuration Categories

### Proxy Configuration (`proxy/`)
Files that control the network filtering proxy server:
- **`squid.conf`**: Main Squid proxy server configuration
- **`allowed-domains.txt`**: Whitelist of domains accessible from the container

### Tool Configuration (`tools/`)
Files that configure development tools to use the proxy:
- **`pip.conf`**: Python package installer
- **`npmrc`**: Node.js package manager
- **`curlrc`**: Command-line HTTP client
- **`wgetrc`**: File downloader
- **`apt-proxy.conf`**: Debian package manager
- **`docker-config.json`**: Docker CLI
- **`gitconfig-proxy`**: Git version control
- **`nuget.config`**: .NET package manager

## 📝 How These Files Are Used

### During Container Build (Dockerfile)
All tool configuration files are copied from `configs/tools/` to their respective locations in the container:
- `/etc/pip.conf`
- `/home/vscode/.npmrc`
- `/home/vscode/.curlrc`
- `/etc/wgetrc`
- `/home/vscode/.docker/config.json`
- (Others are applied in post-create script)

### During Container Runtime (docker-compose.yml)
Proxy configuration files are mounted into the proxy container:
- `configs/proxy/squid.conf` → `/etc/squid/squid.conf`
- `configs/proxy/allowed-domains.txt` → `/etc/squid/allowed-domains.txt`

## 🔄 When to Edit

| File | Edit When... | Action After Editing |
|------|-------------|---------------------|
| `proxy/squid.conf` | Changing proxy behavior | Rebuild container |
| `proxy/allowed-domains.txt` | Adding/removing allowed domains | Restart proxy: `podman compose restart proxy` |
| `tools/*.conf` | Changing tool proxy settings | Rebuild container |

## 📚 Quick Reference

### Allow a New Domain
1. Edit `configs/proxy/allowed-domains.txt`
2. Add the domain (use `.domain.com` to match all subdomains)
3. Run: `podman compose restart proxy`

### Change Proxy Port
1. Edit `configs/proxy/squid.conf` (change `http_port`)
2. Update all `tools/*.conf` files with new port
3. Update `docker-compose.yml` environment variables
4. Rebuild container

### Disable Proxy for a Tool
1. Edit the tool's config file in `tools/`
2. Comment out or remove proxy settings
3. Rebuild container

## 🔍 Troubleshooting

If tools can't connect to the internet:
1. Check proxy is running: `podman ps | grep proxy`
2. Check domain is allowed: `grep domain configs/proxy/allowed-domains.txt`
3. Check tool config is correct: `cat /path/to/installed/config`
4. Check proxy logs: `podman logs -f chonkienet_devcontainer-proxy-1`

For detailed troubleshooting, see: `../docs/troubleshooting/PROXY_ISSUES.md`

---

*See also: [Proxy Configuration Guide](proxy/README.md)*
