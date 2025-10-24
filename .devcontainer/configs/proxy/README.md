# Proxy Configuration

This directory contains the Squid proxy server configuration files for network filtering.

## 📄 Files

### `squid.conf`
Main Squid proxy server configuration file.

**Purpose:** Defines how the proxy server operates, including:
- Port configuration (3128)
- Access control rules
- Logging settings
- Cache configuration
- SSL/TLS handling

**When to edit:**
- Changing proxy port
- Modifying logging verbosity
- Adjusting cache settings
- Changing security rules

**After editing:** Rebuild container
```bash
# From host
podman compose -f .devcontainer/docker-compose.yml build proxy --no-cache
podman compose -f .devcontainer/docker-compose.yml up -d
```

### `allowed-domains.txt`
Network access whitelist - controls which domains can be accessed from the container.

**Format:**
```text
# Comment lines start with #

# Match all subdomains (recommended)
.github.com              # Matches: github.com, api.github.com, raw.githubusercontent.com

# Match exact domain only
example.com              # Only matches: example.com
```

**Current categories:**
- AI Services (OpenAI, Anthropic, Google, etc.)
- Development Tools (GitHub, VS Code, etc.)
- Package Managers (NuGet, NPM, PyPI)
- Infrastructure (Docker Hub, CDNs, etc.)
- Certificate Authorities

**When to edit:**
- Adding new AI service APIs
- Allowing access to new development tools
- Enabling additional package repositories

**After editing:** Just restart the proxy (no rebuild needed!)
```bash
# From host
podman compose -f .devcontainer/docker-compose.yml restart proxy
```

## 🚀 Quick Actions

### Add a New Domain

1. **Edit the file:**
   ```bash
   code .devcontainer/configs/proxy/allowed-domains.txt
   ```

2. **Add your domain:**
   ```text
   # My new service
   .mynewservice.com
   ```

3. **Restart proxy:**
   ```bash
   podman compose -f .devcontainer/docker-compose.yml restart proxy
   ```

4. **Test access:**
   ```bash
   curl -I -x http://localhost:3128 https://mynewservice.com
   ```

### Remove a Domain

1. Comment out or delete the line in `allowed-domains.txt`
2. Restart proxy: `podman compose restart proxy`

### View Current Allowed Domains

```bash
# From host
cat .devcontainer/configs/proxy/allowed-domains.txt | grep -v '^#' | grep -v '^$'

# From container
grep -v '^#' /workspace/.devcontainer/configs/proxy/allowed-domains.txt | grep -v '^$'
```

## 🔍 Testing & Monitoring

### Test if Domain is Allowed

```bash
# From host
curl -I -x http://localhost:3128 https://example.com

# From container
curl -I -x http://proxy:3128 https://example.com
```

**Expected responses:**
- ✅ `HTTP/1.1 200 Connection established` = Allowed
- ❌ `HTTP/1.1 403 Forbidden` = Blocked

### Monitor Proxy Traffic

```bash
# View all proxy logs
podman logs -f chonkienet_devcontainer-proxy-1

# View access log only
podman exec chonkienet_devcontainer-proxy-1 tail -f /var/log/squid/access.log

# View cache log
podman exec chonkienet_devcontainer-proxy-1 tail -f /var/log/squid/cache.log
```

### Check Proxy Status

```bash
# From host
podman ps | grep proxy

# Check health
podman inspect chonkienet_devcontainer-proxy-1 | grep -A 10 Health
```

## 🐛 Troubleshooting

### Domain Not Accessible After Adding

1. **Check syntax in allowed-domains.txt:**
   - No spaces before/after domain
   - Use `.domain.com` for subdomains
   - No protocol (http://, https://)

2. **Verify proxy restarted:**
   ```bash
   podman compose restart proxy
   ```

3. **Check proxy logs for errors:**
   ```bash
   podman logs chonkienet_devcontainer-proxy-1
   ```

4. **Test manually:**
   ```bash
   curl -v -x http://localhost:3128 https://yourdomain.com
   ```

### All Domains Blocked

1. **Check squid.conf syntax:**
   ```bash
   podman exec chonkienet_devcontainer-proxy-1 squid -k parse
   ```

2. **Verify file is mounted:**
   ```bash
   podman exec chonkienet_devcontainer-proxy-1 cat /etc/squid/allowed-domains.txt
   ```

3. **Check ACL configuration in squid.conf:**
   Look for: `acl allowed_domains dstdomain "/etc/squid/allowed-domains.txt"`

## 📚 Advanced Configuration

### Temporarily Disable Filtering

Comment out the ACL rule in `squid.conf`:
```conf
# http_access deny !allowed_domains
http_access allow localnet
```

Then rebuild the container.

### Enable HTTPS Inspection (SSL Bump)

⚠️ **Warning:** Requires SSL certificate generation and installation.

See: `../../docs/troubleshooting/PROXY_ISSUES.md` for detailed instructions.

### Change Proxy Port

1. Edit `squid.conf`: Change `http_port 3128` to desired port
2. Update all tool configs in `../tools/` with new port
3. Update `docker-compose.yml` proxy environment variables
4. Update port mapping in `docker-compose.yml` ports section
5. Rebuild container

## 🔐 Security Notes

- **Whitelist-only:** Container can ONLY access domains in `allowed-domains.txt`
- **No wildcards:** Use `.domain.com` for subdomain matching (Squid's format)
- **HTTPS support:** CONNECT method allows encrypted traffic (no inspection by default)
- **Logging:** All requests are logged to `/var/log/squid/access.log`

## 📖 Related Documentation

- [Main Proxy Troubleshooting Guide](../../docs/troubleshooting/PROXY_ISSUES.md)
- [Configuration Files Overview](../README.md)
- [Quick Start Guide](../../docs/00-QUICK_START.md)

---

*For detailed proxy troubleshooting, see: `../../docs/troubleshooting/PROXY_ISSUES.md`*
