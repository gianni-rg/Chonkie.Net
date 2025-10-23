#!/bin/bash
# Test script for pip connectivity and configuration
# Run this inside the dev container to diagnose pip issues

echo "==============================================="
echo "  Pip Connectivity Diagnostic Script"
echo "==============================================="
echo ""

# Check environment variables
echo "1️⃣ Checking environment variables..."
echo "   http_proxy: ${http_proxy:-NOT SET}"
echo "   https_proxy: ${https_proxy:-NOT SET}"
echo "   PIP_TRUSTED_HOST: ${PIP_TRUSTED_HOST:-NOT SET}"
echo "   PIP_DEFAULT_TIMEOUT: ${PIP_DEFAULT_TIMEOUT:-NOT SET}"
echo ""

# Check pip configuration files
echo "2️⃣ Checking pip configuration files..."
echo "   System pip.conf (/etc/pip.conf):"
if [ -f /etc/pip.conf ]; then
    cat /etc/pip.conf | sed 's/^/      /'
else
    echo "      ❌ NOT FOUND"
fi
echo ""

echo "   User pip.conf (/home/vscode/.config/pip/pip.conf):"
if [ -f /home/vscode/.config/pip/pip.conf ]; then
    cat /home/vscode/.config/pip/pip.conf | sed 's/^/      /'
else
    echo "      ❌ NOT FOUND"
fi
echo ""

# Check pip config list
echo "3️⃣ Pip configuration (pip3 config list):"
pip3 config list | sed 's/^/   /' || echo "   ❌ Failed to get pip config"
echo ""

# Test proxy connectivity
echo "4️⃣ Testing proxy connectivity..."
if nc -z proxy 3128 2>/dev/null; then
    echo "   ✅ Proxy is reachable at proxy:3128"
else
    echo "   ❌ Proxy is NOT reachable at proxy:3128"
fi
echo ""

# Test internet via proxy
echo "5️⃣ Testing internet via proxy..."
if curl -s -I -x http://proxy:3128 --max-time 10 https://pypi.org > /dev/null 2>&1; then
    echo "   ✅ Can reach PyPI through proxy"
else
    echo "   ❌ Cannot reach PyPI through proxy"
fi
echo ""

# Test PyPI domains
echo "6️⃣ Testing PyPI domains..."
for domain in pypi.org pypi.python.org files.pythonhosted.org; do
    if curl -s -I -x http://proxy:3128 --max-time 10 https://$domain > /dev/null 2>&1; then
        echo "   ✅ $domain is reachable"
    else
        echo "   ❌ $domain is NOT reachable"
    fi
done
echo ""

# Test pip search (dry run)
echo "7️⃣ Testing pip package lookup (dry run)..."
echo "   Running: pip3 install --dry-run --no-cache-dir requests"
if pip3 install --dry-run --no-cache-dir requests 2>&1 | grep -q -E "(Successfully|Requirement already satisfied|would install)"; then
    echo "   ✅ Pip can successfully query PyPI"
else
    echo "   ❌ Pip cannot query PyPI"
    echo ""
    echo "   Last 10 lines of verbose output:"
    pip3 install --dry-run --no-cache-dir --verbose requests 2>&1 | tail -10 | sed 's/^/      /'
fi
echo ""

# Test actual installation to temp location
echo "8️⃣ Testing actual pip installation..."
TEST_DIR=$(mktemp -d)
echo "   Installing 'certifi' to temporary location: $TEST_DIR"
if pip3 install --target="$TEST_DIR" --no-cache-dir certifi 2>&1 | tail -5; then
    echo "   ✅ Successfully installed test package"
    rm -rf "$TEST_DIR"
else
    echo "   ❌ Failed to install test package"
    rm -rf "$TEST_DIR"
fi
echo ""

# Check squid proxy logs for PyPI requests
echo "9️⃣ Checking recent proxy logs for PyPI access..."
echo "   (showing last 5 PyPI-related requests)"
if command -v docker &> /dev/null || command -v podman &> /dev/null; then
    CONTAINER_CMD=$(command -v podman || command -v docker)
    # Try to get proxy container logs
    PROXY_CONTAINER=$(${CONTAINER_CMD} ps --filter "name=proxy" --format "{{.Names}}" | head -1)
    if [ ! -z "$PROXY_CONTAINER" ]; then
        ${CONTAINER_CMD} exec "$PROXY_CONTAINER" tail -20 /var/log/squid/access.log 2>/dev/null | grep -i "pypi\|python" | tail -5 | sed 's/^/   /' || echo "   ℹ️  No recent PyPI requests in proxy logs"
    else
        echo "   ⚠️  Could not find proxy container"
    fi
else
    echo "   ⚠️  Docker/Podman not available from inside container"
fi
echo ""

echo "==============================================="
echo "  Diagnostic Complete"
echo "==============================================="
echo ""
echo "If you see issues above, try these solutions:"
echo "1. Rebuild the dev container: 'Dev Containers: Rebuild Container'"
echo "2. Check that proxy container is running: 'podman ps'"
echo "3. Check proxy logs: 'podman logs <proxy-container-name>'"
echo "4. Verify allowed-domains.txt includes PyPI domains"
echo "5. Try pip with explicit proxy: 'pip3 install --proxy http://proxy:3128 <package>'"
echo ""
