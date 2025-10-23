#!/bin/bash
#
# Test network connectivity and proxy configuration
#

set -e

echo "🔍 Testing Network Configuration..."
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Test function
test_connection() {
    local url=$1
    local description=$2
    
    echo -n "Testing $description... "
    
    if curl -s -o /dev/null -w "%{http_code}" --max-time 10 -x http://proxy:3128 "$url" | grep -q "^[23]"; then
        echo -e "${GREEN}✓ OK${NC}"
        return 0
    else
        echo -e "${RED}✗ FAILED${NC}"
        return 1
    fi
}

# Proxy connectivity
echo "=== Proxy Connectivity ==="
if curl -s http://proxy:3128 > /dev/null 2>&1; then
    echo -e "${GREEN}✓ Proxy is reachable${NC}"
else
    echo -e "${RED}✗ Cannot reach proxy${NC}"
    exit 1
fi
echo ""

# Test allowed domains
echo "=== Testing Allowed Domains ==="
test_connection "https://api.github.com" "GitHub API"
test_connection "https://www.microsoft.com" "Microsoft"
test_connection "https://www.nuget.org/api/v2" "NuGet"
test_connection "https://registry.npmjs.org" "NPM Registry"
test_connection "https://generativelanguage.googleapis.com" "Google Gemini API"
test_connection "https://marketplace.visualstudio.com" "VS Code Marketplace"
echo ""

# Test blocked domains (should fail)
echo "=== Testing Blocked Domains (should fail) ==="
echo -n "Testing example.com... "
if curl -s -o /dev/null --max-time 5 -x http://proxy:3128 "https://example.com" 2>&1 | grep -q "Forbidden\|403\|denied"; then
    echo -e "${GREEN}✓ Correctly blocked${NC}"
else
    if curl -s -o /dev/null -w "%{http_code}" --max-time 5 -x http://proxy:3128 "https://example.com" | grep -q "^[23]"; then
        echo -e "${YELLOW}⚠ WARNING: Domain should be blocked but is accessible${NC}"
    else
        echo -e "${GREEN}✓ Blocked (timeout/error)${NC}"
    fi
fi
echo ""

# Check environment variables
echo "=== Environment Variables ==="
echo "http_proxy: ${http_proxy:-not set}"
echo "https_proxy: ${https_proxy:-not set}"
echo "HTTP_PROXY: ${HTTP_PROXY:-not set}"
echo "HTTPS_PROXY: ${HTTPS_PROXY:-not set}"
echo "NO_PROXY: ${NO_PROXY:-not set}"
echo ""

# Check DNS resolution
echo "=== DNS Resolution ==="
if nslookup github.com > /dev/null 2>&1; then
    echo -e "${GREEN}✓ DNS working${NC}"
else
    echo -e "${RED}✗ DNS not working${NC}"
fi
echo ""

# View recent proxy logs
echo "=== Recent Proxy Activity ==="
if docker exec chonkienet-proxy-1 tail -20 /var/log/squid/access.log 2>/dev/null; then
    echo ""
else
    echo -e "${YELLOW}⚠ Cannot access proxy logs${NC}"
fi

echo ""
echo "✅ Network configuration test complete!"
