#!/bin/bash
# Test proxy connectivity

echo "Testing proxy connectivity..."
echo ""

# Test 1: Can we reach the proxy?
echo "1. Testing proxy port connectivity..."
if nc -z proxy 3128 2>/dev/null; then
    echo "   ✅ Proxy port 3128 is reachable"
else
    echo "   ❌ Proxy port 3128 is NOT reachable"
    exit 1
fi

# Test 2: Can we make HTTP requests through the proxy?
echo ""
echo "2. Testing HTTP request through proxy..."
HTTP_CODE=$(curl -x http://proxy:3128 -s -o /dev/null -w "%{http_code}" http://www.google.com 2>&1)
if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "301" ] || [ "$HTTP_CODE" = "302" ]; then
    echo "   ✅ HTTP requests work (status: $HTTP_CODE)"
else
    echo "   ❌ HTTP requests failed (status: $HTTP_CODE)"
fi

# Test 3: Can we make HTTPS requests through the proxy?
echo ""
echo "3. Testing HTTPS request through proxy..."
HTTPS_CODE=$(curl -x http://proxy:3128 -s -o /dev/null -w "%{http_code}" https://github.com 2>&1)
if [ "$HTTPS_CODE" = "200" ] || [ "$HTTPS_CODE" = "301" ] || [ "$HTTPS_CODE" = "302" ]; then
    echo "   ✅ HTTPS requests work (status: $HTTPS_CODE)"
else
    echo "   ❌ HTTPS requests failed (status: $HTTPS_CODE)"
fi

# Test 4: Can pip access PyPI through the proxy?
echo ""
echo "4. Testing PyPI access through proxy..."
PYPI_CODE=$(curl -x http://proxy:3128 -s -o /dev/null -w "%{http_code}" https://pypi.org/simple/ 2>&1)
if [ "$PYPI_CODE" = "200" ]; then
    echo "   ✅ PyPI is accessible (status: $PYPI_CODE)"
else
    echo "   ❌ PyPI is NOT accessible (status: $PYPI_CODE)"
fi

# Test 5: Check proxy environment variables
echo ""
echo "5. Checking proxy environment variables..."
if [ ! -z "$http_proxy" ]; then
    echo "   http_proxy=$http_proxy"
else
    echo "   ❌ http_proxy not set"
fi

if [ ! -z "$https_proxy" ]; then
    echo "   https_proxy=$https_proxy"
else
    echo "   ❌ https_proxy not set"
fi

echo ""
echo "Proxy test complete!"
