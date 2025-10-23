#!/bin/bash
# Post-start script - runs every time the container starts
set -e

echo "🔄 Running post-start tasks..."

# Check network connectivity
echo "🌐 Checking network connectivity..."
if curl -s -I --max-time 5 https://api.github.com > /dev/null 2>&1; then
    echo "✅ Internet access is working"
else
    echo "⚠️  Network check failed - you may have limited connectivity"
fi

# Check if proxy is enabled
if [ ! -z "$http_proxy" ]; then
    echo "🔒 Proxy is configured: $http_proxy"
    if nc -z proxy 3128 2>/dev/null; then
        echo "✅ Proxy service is reachable"
        if curl -s -I -x http://proxy:3128 --max-time 5 https://www.google.com > /dev/null 2>&1; then
            echo "✅ Internet access via proxy is working"
        else
            echo "⚠️  Proxy is up but internet access may be limited (check allowed-domains.txt)"
        fi
    else
        echo "⚠️  Proxy service not reachable"
    fi
else
    echo "ℹ️  Proxy is disabled - using direct internet connection"
fi

# Display environment info
echo ""
echo "📋 Environment Information:"
echo "  - .NET SDK: $(dotnet --version)"
echo "  - Python: $(python3 --version)"
echo "  - Node.js: $(node --version)"
echo "  - Git: $(git --version)"
echo ""
echo "🔐 Security Features Active:"
echo "  - Network filtering: $([ ! -z "$http_proxy" ] && echo "Yes ($http_proxy)" || echo "No (direct connection)")"
echo "  - Running as non-root user: $(whoami)"
echo "  - All capabilities dropped: Yes"
echo "  - No new privileges: Yes"
echo ""
echo "✅ Container is ready for development!"
