# Restarting the Proxy Service

## Changes Made

Added `huggingface.co` to the allowed domains list in:
- `/workspace/.devcontainer/configs/proxy/allowed-domains.txt`

## How to Apply Changes

Since the proxy service runs outside the devcontainer, you need to restart it from your **host machine** (not from inside the devcontainer).

### Option 1: Restart Just the Proxy (Faster)

From your host machine terminal, navigate to the `.devcontainer` directory and run:

```bash
cd /workspace/.devcontainer
docker-compose restart proxy
```

Or if using Podman:

```bash
cd /workspace/.devcontainer
podman-compose restart proxy
```

### Option 2: Rebuild the Entire Dev Container (Complete Refresh)

In VS Code, use the Command Palette (Ctrl+Shift+P / Cmd+Shift+P):
1. Select: **Dev Containers: Rebuild Container**

This will restart both the devcontainer and proxy services with the new configuration.

### Option 3: Stop and Start Services Manually

From your host machine:

```bash
cd /workspace/.devcontainer

# Stop services
docker-compose down

# Start services
docker-compose up -d
```

## Verification

After restarting the proxy, test HuggingFace access:

```bash
# Test connection
curl -I https://huggingface.co

# Try the model conversion again
uv run scripts/convert_model.py --list
uv run scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2 ./models/all-MiniLM-L6-v2
```

## What Was Added

The following line was added to `allowed-domains.txt`:

```
# HuggingFace
.huggingface.co
```

This allows access to:
- `huggingface.co` (main site)
- `cdn.huggingface.co` (CDN)
- `api.huggingface.co` (API)
- And all other `*.huggingface.co` subdomains

## Troubleshooting

If the connection still fails after restart:

1. **Check proxy logs**:
   ```bash
   docker-compose logs proxy
   ```

2. **Verify the configuration was loaded**:
   ```bash
   docker-compose exec proxy cat /etc/squid/allowed-domains.txt | grep huggingface
   ```

3. **Check devcontainer proxy settings**:
   The devcontainer is configured to use `http://proxy:3128` for all HTTP/HTTPS traffic.

4. **Test proxy directly**:
   ```bash
   curl -x http://proxy:3128 -I https://huggingface.co
   ```
