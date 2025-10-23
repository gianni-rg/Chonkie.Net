# Proxy Configuration Guide

## Overview
The devcontainer uses a Squid proxy for network filtering. All tools need to be configured to use the proxy at `http://proxy:3128`.

## Configuration Files

### 1. pip (Python Package Manager)
**File:** `.devcontainer/pip.conf`
```ini
[global]
proxy = http://proxy:3128

[install]
trusted-host = pypi.org
               pypi.python.org
               files.pythonhosted.org
```
**Installed to:** `/etc/pip.conf`

### 2. npm (Node Package Manager)
**File:** `.devcontainer/npmrc`
```ini
proxy=http://proxy:3128
https-proxy=http://proxy:3128
strict-ssl=false
```
**Installed to:** `/home/vscode/.npmrc`

### 3. Environment Variables
**Set in:** `docker-compose.yml` and `devcontainer.json`
```bash
http_proxy=http://proxy:3128
https_proxy=http://proxy:3128
HTTP_PROXY=http://proxy:3128
HTTPS_PROXY=http://proxy:3128
no_proxy=localhost,127.0.0.1,devcontainer
NO_PROXY=localhost,127.0.0.1,devcontainer
```

### 4. VS Code Settings
**Set in:** `devcontainer.json`
```json
{
  "http.proxy": "http://proxy:3128",
  "http.proxyStrictSSL": false,
  "http.proxySupport": "on"
}
```

### 5. Git Configuration
**File:** `.devcontainer/gitconfig-proxy`
```ini
[http]
    proxy = http://proxy:3128
[https]
    proxy = http://proxy:3128
```
**Applied to:** `/home/vscode/.gitconfig`

### 6. NuGet Configuration
**File:** `.devcontainer/nuget.config`
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <config>
    <add key="http_proxy" value="http://proxy:3128" />
    <add key="https_proxy" value="http://proxy:3128" />
  </config>
</configuration>
```
**Installed to:** `/home/vscode/.nuget/NuGet/NuGet.Config`

### 7. wget Configuration
**File:** `.devcontainer/wgetrc`
```ini
use_proxy = on
http_proxy = http://proxy:3128
https_proxy = http://proxy:3128
```
**Installed to:** `/etc/wgetrc`

### 8. curl Configuration
**File:** `.devcontainer/curlrc`
```ini
proxy = http://proxy:3128
```
**Installed to:** `/home/vscode/.curlrc`

### 9. APT Configuration
**File:** `.devcontainer/apt-proxy.conf`
```ini
Acquire::http::Proxy "http://proxy:3128";
Acquire::https::Proxy "http://proxy:3128";
```
**Installed to:** `/etc/apt/apt.conf.d/99proxy`

### 10. Docker CLI Configuration
**File:** `.devcontainer/docker-config.json`
```json
{
  "proxies": {
    "default": {
      "httpProxy": "http://proxy:3128",
      "httpsProxy": "http://proxy:3128",
      "noProxy": "localhost,127.0.0.1"
    }
  }
}
```
**Installed to:** `/home/vscode/.docker/config.json`

## Testing Proxy Configuration

### From Host (PowerShell)
```powershell
# Test proxy is running
curl -x http://localhost:3128 -I https://github.com

# View proxy logs
podman logs chonkienet_devcontainer-proxy-1 --follow

# View access log
podman exec chonkienet_devcontainer-proxy-1 tail -f /var/log/squid/access.log
```

### From Container (Bash)
```bash
# Test network connectivity
bash /workspace/.devcontainer/scripts/test-proxy.sh

# Test proxy directly
curl -v -x http://proxy:3128 https://pypi.org/simple/

# Check environment variables
env | grep -i proxy

# Check pip configuration
pip3 config list

# Check npm configuration
npm config get proxy
npm config get https-proxy

# Test pip with proxy
pip3 install --user --break-system-packages requests

# Test npm with proxy
npm install -g cowsay

# Test dotnet/NuGet with proxy
dotnet restore /workspace/Chonkie.Net.sln

# Test git with proxy
git ls-remote https://github.com/microsoft/vscode.git

# Test wget with proxy
wget -O /dev/null https://github.com

# Test curl with proxy (uses .curlrc)
curl -I https://github.com

# Test apt with proxy (requires sudo)
sudo apt-get update
```

## Troubleshooting

### pip fails to connect
```bash
# Check pip configuration
pip3 config list

# Verify proxy environment variables
echo $http_proxy
echo $https_proxy

# Test proxy manually
curl -x http://proxy:3128 https://pypi.org/simple/

# Try with explicit proxy
pip3 install --user --break-system-packages --proxy http://proxy:3128 package-name
```

### VS Code Extensions fail to install/update
1. Check VS Code settings: `http.proxy` should be `http://proxy:3128`
2. Check proxy logs: `podman exec chonkienet_devcontainer-proxy-1 tail -f /var/log/squid/access.log`
3. Verify allowed domains include VS Code domains in `.devcontainer/allowed-domains.txt`

### GitHub Copilot doesn't work
1. Verify GitHub domains are in allowed list: `.github.com`, `.githubusercontent.com`
2. Check VS Code proxy settings are correct
3. Check Copilot logs in VS Code: View → Output → GitHub Copilot
4. Test GitHub connectivity: `curl -v -x http://proxy:3128 https://api.github.com`

### npm packages fail to install
```bash
# Verify npm config
npm config get proxy
npm config get https-proxy

# Test with explicit proxy
npm install --proxy http://proxy:3128 package-name

# Check if registry is accessible
curl -x http://proxy:3128 https://registry.npmjs.org/
```

## Adding New Domains
Edit `.devcontainer/allowed-domains.txt` and add the domain:
```
.example.com
```

Then restart the proxy:
```bash
podman compose restart proxy
```

## Monitoring Traffic
Real-time monitoring of all proxy requests:
```bash
podman exec -it chonkienet_devcontainer-proxy-1 tail -f /var/log/squid/access.log
```

Look for:
- `TCP_DENIED` - Blocked requests
- `TCP_MISS` - Allowed requests (cache miss)
- `TCP_TUNNEL` - HTTPS connections
- `403` - Forbidden responses

---
*Last Updated: October 23, 2025*
