# Pip Proxy Configuration Fix

## Issue
Pip was unable to connect to PyPI through the proxy, causing package installation failures.

## Root Causes Identified

1. **Incomplete pip.conf**: The original configuration only had proxy and trusted-host settings, but was missing:
   - `index-url` directive
   - `timeout` configuration
   - Proper placement of trusted-host in both `[global]` and `[install]` sections

2. **Missing Environment Variables**: Pip-specific environment variables were not set:
   - `PIP_DEFAULT_TIMEOUT`
   - `PIP_TRUSTED_HOST`

3. **Single Configuration Location**: pip.conf was only in `/etc/`, not also in user's home directory (`~/.config/pip/pip.conf`)

4. **Pip Install During Build**: The Dockerfile was trying to install Python packages during the build phase, but the proxy container doesn't exist yet at that time

5. **APT Proxy During Build**: The `apt-proxy.conf` was being applied during Docker build, causing "Temporary failure resolving 'proxy'" errors because the proxy container doesn't exist during build time

6. **Pip Config During Build**: The `pip.conf` was being copied to `/etc/pip.conf` and `~/.config/pip/pip.conf` during build, causing the **Python devcontainer feature** to fail when trying to update pip. The feature reads the pip config and tries to use the non-existent proxy.

## Fixes Applied

### 1. Updated pip.conf
Location: `.devcontainer/pip.conf`

```ini
[global]
proxy = http://proxy:3128
trusted-host = pypi.org
               pypi.python.org
               files.pythonhosted.org
index-url = https://pypi.org/simple
timeout = 60

[install]
trusted-host = pypi.org
               pypi.python.org
               files.pythonhosted.org
```

**Key additions:**
- Added `index-url = https://pypi.org/simple` to explicitly specify PyPI index
- Added `timeout = 60` to prevent premature connection timeouts
- Moved `trusted-host` to `[global]` section for broader coverage

### 2. Updated Dockerfile
Location: `.devcontainer/Dockerfile`

**Changes:**
- Added environment variables:
  ```dockerfile
  PIP_DEFAULT_TIMEOUT=100
  PIP_TRUSTED_HOST="pypi.org pypi.python.org files.pythonhosted.org"
  ```

**Enhanced pip.conf deployment:**
  ```dockerfile
  # Copy to both system and user locations
  COPY pip.conf /etc/pip.conf
  RUN mkdir -p /home/vscode/.config/pip && \
      cp /etc/pip.conf /home/vscode/.config/pip/pip.conf && \
      chown -R vscode:vscode /home/vscode/.config
  ```

- **Removed pip install from Dockerfile build phase:**
  
  The Dockerfile was trying to install packages during build:
  ```dockerfile
  # OLD - FAILS during build because proxy isn't running
  RUN python3 -m pip install --user --no-cache-dir --break-system-packages \
      openai anthropic google-generativeai requests rich
  ```
  
  This fails because:
  - Docker build happens **before** `docker-compose up`
  - The proxy container doesn't exist during build
  - No network access to PyPI during build phase
  
  **Solution:** Moved all pip installations to `post-create.sh` which runs **after** the proxy is up

- **Removed APT proxy from Dockerfile build phase:**
  
  The Dockerfile was copying APT proxy config during build:
  ```dockerfile
  # OLD - FAILS during build because proxy isn't running
  COPY apt-proxy.conf /etc/apt/apt.conf.d/99proxy
  ```
  
  This caused errors like:
  ```
  W: Failed to fetch http://archive.ubuntu.com/ubuntu/dists/noble/InRelease
     Temporary failure resolving 'proxy'
  E: Unable to locate package icu-devtools
  ```
  
  **Solution:** Copy to `/tmp/` during build, apply in post-create when proxy is running:
  ```dockerfile
  # NEW - Copy to temp, apply later
  COPY apt-proxy.conf /tmp/apt-proxy.conf
  ```

- **Removed pip.conf from Dockerfile build phase:**
  
  The Dockerfile was copying pip.conf during build:
  ```dockerfile
  # OLD - FAILS because Python feature reads this config
  COPY pip.conf /etc/pip.conf
  RUN mkdir -p /home/vscode/.config/pip && \
      cp /etc/pip.conf /home/vscode/.config/pip/pip.conf
  ```
  
  This caused the **Python devcontainer feature** to fail:
  ```
  WARNING: Retrying (Retry(total=4...)) after connection broken by 
  'ProxyError('Cannot connect to proxy.', NewConnectionError...
  Failed to establish a new connection: [Errno -3] Temporary failure in name resolution'))
  ```
  
  **Why it fails:**
  1. Dockerfile copies pip.conf to `/etc/pip.conf` during build
  2. Python feature (from `devcontainer.json`) installs after this
  3. Python feature runs `pip install --upgrade pip`
  4. Pip reads `/etc/pip.conf` and tries to use proxy:3128
  5. Proxy container doesn't exist yet → connection fails
  6. Build fails!
  
  **Solution:** Copy to `/tmp/` during build, apply in post-create:
  ```dockerfile
  # NEW - Copy to temp, apply after proxy is running
  COPY pip.conf /tmp/pip.conf
  ```

### 3. Updated docker-compose.yml
Location: `.devcontainer/docker-compose.yml`

**Added environment variables:**
```yaml
- PIP_DEFAULT_TIMEOUT=100
- PIP_TRUSTED_HOST=pypi.org pypi.python.org files.pythonhosted.org
```

### 4. Updated devcontainer.json
Location: `.devcontainer/devcontainer.json`

**Added to remoteEnv:**
```json
"PIP_DEFAULT_TIMEOUT": "100",
"PIP_TRUSTED_HOST": "pypi.org pypi.python.org files.pythonhosted.org"
```

### 5. Enhanced post-create.sh
Location: `.devcontainer/scripts/post-create.sh`

**Added diagnostics:**
- Display both system and user pip.conf files
- Show all pip environment variables
- Test pip connectivity with dry-run before actual installations
- Provide verbose output on failures

**Moved Python package installations here:**
- All `pip install` commands now run in post-create (not during Docker build)
- Base packages: `openai`, `anthropic`, `google-generativeai`, `requests`, `rich`
- CLI tools and additional packages
- This ensures the proxy is available when pip needs network access

**Apply APT proxy configuration:**
```bash
# Apply APT proxy configuration
if [ -f "/tmp/apt-proxy.conf" ]; then
    sudo cp /tmp/apt-proxy.conf /etc/apt/apt.conf.d/99proxy
    echo "✅ APT proxy configured"
fi
```

**Apply pip proxy configuration:**
```bash
# Apply pip proxy configuration
if [ -f "/tmp/pip.conf" ]; then
    sudo cp /tmp/pip.conf /etc/pip.conf
    mkdir -p /home/vscode/.config/pip
    cp /tmp/pip.conf /home/vscode/.config/pip/pip.conf
    echo "✅ Pip proxy configured"
fi
```

### 6. Created Diagnostic Script
Location: `.devcontainer/scripts/test-pip-connectivity.sh`

**New utility script** that tests:
- Environment variables
- Configuration files
- Proxy connectivity
- PyPI domain access
- Actual pip package installation

## How to Apply Fixes

### Option 1: Rebuild Container (Recommended)
```bash
# In VS Code Command Palette (Ctrl+Shift+P)
Dev Containers: Rebuild Container
```

### Option 2: Rebuild via Command Line
```powershell
# Stop and remove existing containers
podman compose -f .devcontainer/docker-compose.yml down

# Rebuild with no cache
podman compose -f .devcontainer/docker-compose.yml build --no-cache

# Start containers
podman compose -f .devcontainer/docker-compose.yml up -d
```

## Verification

### Inside the Container
```bash
# Run the diagnostic script
bash /workspace/.devcontainer/scripts/test-pip-connectivity.sh

# Test manual installation
pip3 install --no-cache-dir requests

# Verify pip configuration
pip3 config list
```

### Expected Output
```
✅ Environment variables set correctly
✅ pip.conf files present in both locations
✅ Proxy is reachable
✅ Can reach PyPI through proxy
✅ Pip can successfully query PyPI
```

## Troubleshooting

### If pip still fails after rebuild:

1. **Check proxy is running:**
   ```powershell
   podman ps | grep proxy
   ```

2. **Check proxy logs:**
   ```powershell
   podman logs <proxy-container-name>
   ```

3. **Verify allowed domains:**
   Check `.devcontainer/allowed-domains.txt` includes:
   - `.pypi.org`
   - `.python.org`
   - `.pythonhosted.org`

4. **Test proxy manually:**
   ```bash
   curl -I -x http://proxy:3128 https://pypi.org
   ```

5. **Use explicit proxy flag:**
   ```bash
   pip3 install --proxy http://proxy:3128 <package>
   ```

6. **Check network connectivity:**
   ```bash
   nc -zv proxy 3128
   ping proxy
   ```

## Alternative Solutions

### Temporary: Skip proxy for pip
If you need to bypass the proxy temporarily:

```bash
# In container terminal
export http_proxy=""
export https_proxy=""
pip3 install <package>
```

### Disable proxy globally
Edit `.devcontainer/docker-compose.yml` and comment out proxy environment variables.

## Related Files
- `.devcontainer/pip.conf` - Main pip configuration
- `.devcontainer/Dockerfile` - Container build configuration
- `.devcontainer/docker-compose.yml` - Container runtime configuration
- `.devcontainer/devcontainer.json` - VS Code dev container settings
- `.devcontainer/allowed-domains.txt` - Proxy whitelist
- `.devcontainer/scripts/post-create.sh` - Container initialization
- `.devcontainer/scripts/test-pip-connectivity.sh` - Diagnostic tool

## References
- [Pip Configuration](https://pip.pypa.io/en/stable/topics/configuration/)
- [Using pip behind a proxy](https://pip.pypa.io/en/stable/user_guide/#using-a-proxy-server)
- [Squid Proxy Configuration](http://www.squid-cache.org/Doc/config/)

---

**Last Updated:** 2025-10-23
**Status:** Fixed and tested
