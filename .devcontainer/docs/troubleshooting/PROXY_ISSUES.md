# Proxy Configuration & Troubleshooting Guide

## 📋 Overview
The devcontainer uses a Squid proxy for network filtering. All tools are configured to use the proxy at `http://proxy:3128`.

---

## ✅ Current Status

```bash
✅ Proxy listening on: localhost:3128
✅ Network filtering: ACTIVE
✅ Allowed domains: ~60+ development domains
✅ DNS servers: 8.8.8.8, 1.1.1.1
✅ Cache size: 1000 MB disk, 256 MB memory
```

### Test Results

**Allowed Domain (GitHub):**
```bash
$ curl -x http://localhost:3128 -I https://github.com
HTTP/1.1 200 Connection established
HTTP/1.1 200 OK
✅ SUCCESS
```

**Blocked Domain (example.org):**
```bash
$ curl -x http://localhost:3128 -I https://example.org
HTTP/1.1 403 Forbidden
X-Squid-Error: ERR_ACCESS_DENIED 0
✅ CORRECTLY BLOCKED
```

---

## 🔧 Configuration Files

All configuration files are located in `.devcontainer/configs/` directory.

### 1. Proxy Server Configuration
**File:** `.devcontainer/configs/proxy/squid.conf`
- Main Squid proxy server configuration
- Defines ACLs and access rules
- Controls logging and caching behavior

**File:** `.devcontainer/configs/proxy/allowed-domains.txt`
- Whitelist of allowed domains
- Use leading dot (`.domain.com`) to match all subdomains
- Without leading dot matches only exact domain

### 2. Tool Configurations

#### pip (Python Package Manager)
**File:** `.devcontainer/configs/tools/pip.conf`
```ini
[global]
proxy = http://proxy:3128

[install]
trusted-host = pypi.org
               pypi.python.org
               files.pythonhosted.org
```
**Installed to:** `/etc/pip.conf`

#### npm (Node Package Manager)
**File:** `.devcontainer/configs/tools/npmrc`
```ini
proxy=http://proxy:3128
https-proxy=http://proxy:3128
strict-ssl=false
```
**Installed to:** `/home/vscode/.npmrc`

#### Git Configuration
**File:** `.devcontainer/configs/tools/gitconfig-proxy`
```ini
[http]
    proxy = http://proxy:3128
[https]
    proxy = http://proxy:3128
```
**Applied to:** `/home/vscode/.gitconfig`

#### NuGet Configuration
**File:** `.devcontainer/configs/tools/nuget.config`
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

#### wget Configuration
**File:** `.devcontainer/configs/tools/wgetrc`
```ini
use_proxy = on
http_proxy = http://proxy:3128
https_proxy = http://proxy:3128
```
**Installed to:** `/etc/wgetrc`

#### curl Configuration
**File:** `.devcontainer/configs/tools/curlrc`
```ini
proxy = http://proxy:3128
```
**Installed to:** `/home/vscode/.curlrc`

#### APT Configuration
**File:** `.devcontainer/configs/tools/apt-proxy.conf`
```ini
Acquire::http::Proxy "http://proxy:3128";
Acquire::https::Proxy "http://proxy:3128";
```
**Installed to:** `/etc/apt/apt.conf.d/99proxy`

#### Docker CLI Configuration
**File:** `.devcontainer/configs/tools/docker-config.json`
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

---

## 🔍 Testing Proxy Configuration

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

---

## 🐛 Troubleshooting

### pip Fails to Connect

**Symptoms:**
- Timeout when installing packages
- SSL verification errors
- Connection refused errors

**Solutions:**
```bash
# 1. Check pip configuration
pip3 config list

# 2. Verify proxy environment variables
echo $http_proxy
echo $https_proxy

# 3. Test proxy manually
curl -x http://proxy:3128 https://pypi.org/simple/

# 4. Try with explicit proxy
pip3 install --user --break-system-packages --proxy http://proxy:3128 package-name

# 5. Check if PyPI domains are allowed
grep -E 'pypi|pythonhosted' /workspace/.devcontainer/configs/proxy/allowed-domains.txt
```

### VS Code Extensions Fail to Install/Update

**Solutions:**
1. Check VS Code settings: `http.proxy` should be `http://proxy:3128`
2. Check proxy logs: `podman exec chonkienet_devcontainer-proxy-1 tail -f /var/log/squid/access.log`
3. Verify allowed domains include VS Code domains in `.devcontainer/configs/proxy/allowed-domains.txt`:
   - `.visualstudio.com`
   - `.microsoft.com`
   - `.vscode-cdn.net`

### GitHub Copilot Doesn't Work

**Solutions:**
1. Verify GitHub domains are in allowed list:
   ```bash
   grep github /workspace/.devcontainer/configs/proxy/allowed-domains.txt
   ```
   Should include: `.github.com`, `.githubusercontent.com`

2. Check VS Code proxy settings are correct

3. Check Copilot logs in VS Code: View → Output → GitHub Copilot

4. Test GitHub connectivity:
   ```bash
   curl -v -x http://proxy:3128 https://api.github.com
   ```

### npm Packages Fail to Install

**Solutions:**
```bash
# 1. Verify npm config
npm config get proxy
npm config get https-proxy

# 2. Test with explicit proxy
npm install --proxy http://proxy:3128 package-name

# 3. Check if registry is accessible
curl -x http://proxy:3128 https://registry.npmjs.org/

# 4. Check npm domains are allowed
grep npm /workspace/.devcontainer/configs/proxy/allowed-domains.txt
```

### NuGet Restore Fails

**Solutions:**
```bash
# 1. Test NuGet connectivity
curl -x http://proxy:3128 https://api.nuget.org/v3/index.json

# 2. Check environment variables
echo $http_proxy
echo $https_proxy

# 3. Verify NuGet config
cat ~/.nuget/NuGet/NuGet.Config

# 4. Try clearing NuGet cache
dotnet nuget locals all --clear

# 5. Restore with verbose logging
dotnet restore -v detailed
```

### ICMP Pinger Error (Non-Critical)

**Symptoms:**
```
ERROR: Unable to start ICMP pinger.
FATAL: Unable to open any ICMP sockets.
```

**Explanation:**
- **Impact:** None for HTTP/HTTPS proxying
- **Cause:** Container doesn't have CAP_NET_RAW capability (by design for security)
- **Note:** ICMP is only used for ping functionality, not required for web traffic
- **Action:** Safe to ignore

---

## ➕ Adding New Domains

To allow additional domains:

1. **Edit the allowed domains file:**
   ```bash
   code /workspace/.devcontainer/configs/proxy/allowed-domains.txt
   ```

2. **Add your domain:**
   ```txt
   # Use leading dot to match all subdomains
   .example.com          # Matches: example.com, api.example.com, www.example.com

   # Without leading dot, only matches exact domain
   example.com           # Only matches: example.com
   ```

3. **Restart the proxy:**
   ```bash
   cd /workspace/.devcontainer
   podman compose restart proxy
   ```

4. **Verify the domain is accessible:**
   ```bash
   curl -I -x http://proxy:3128 https://example.com
   ```

---

## 📊 Monitoring Traffic

Real-time monitoring of all proxy requests:

```bash
# View access log
podman exec -it chonkienet_devcontainer-proxy-1 tail -f /var/log/squid/access.log

# View cache log
podman exec -it chonkienet_devcontainer-proxy-1 tail -f /var/log/squid/cache.log

# View all logs
podman logs -f chonkienet_devcontainer-proxy-1
```

### Understanding Log Entries

Look for:
- `TCP_DENIED` - Blocked requests (domain not whitelisted)
- `TCP_MISS` - Allowed requests (cache miss)
- `TCP_TUNNEL` - HTTPS CONNECT connections
- `TCP_HIT` - Served from cache
- `403` - Forbidden responses

Example log entry:
```
1698012345.678    123 172.18.0.3 TCP_TUNNEL/200 4567 CONNECT api.github.com:443 - HIER_DIRECT/140.82.121.6 -
```

---

## 🔐 Security Features

1. **Network Filtering:** Only whitelisted domains are accessible
2. **SSL/TLS Inspection:** HTTPS CONNECT method properly handled
3. **Safe Ports:** Only ports 80, 443, and 1025-65535 allowed
4. **Privacy Headers:** X-Forwarded-For and Via headers removed
5. **Access Control:** Localnet-only access (container network)

---

## 🛠️ Advanced Configuration

### Disable Proxy Temporarily (for debugging)

Edit `.devcontainer/docker-compose.yml` and comment out proxy environment variables:
```yaml
# environment:
#   - http_proxy=http://proxy:3128
#   - https_proxy=http://proxy:3128
```

Then restart: `podman compose restart devcontainer`

### View Current Configuration Changes

```bash
# View configuration history
cd /workspace/.devcontainer
git log --oneline -- configs/proxy/
```

---

## 📚 Current Allowed Domains

The proxy allows access to:
- **AI Services:** OpenAI, Anthropic (Claude), Google (Gemini), Cohere, Voyage AI, Jina AI
- **Development Tools:** GitHub, VS Code Marketplace, Microsoft, Azure
- **Package Managers:** NuGet, NPM, PyPI
- **Infrastructure:** Docker Hub, CloudFlare, Fastly, Akamai, CloudFront
- **Certificate Authorities:** DigiCert, Let's Encrypt, GlobalSign

Full list: See `.devcontainer/configs/proxy/allowed-domains.txt`

---

*Last Updated: October 24, 2025*
