#!/bin/bash
# Post-start script - runs every time the container starts
set -e

echo "🔄 Running post-start tasks..."

# Check network connectivity
echo "🌐 Checking network connectivity..."
if curl -s -I --max-time 5 https://api.github.com > /dev/null; then
    echo "✅ Network is accessible"
else
    echo "⚠️  Network check failed - you may have limited connectivity"
fi

# Check proxy
if [ ! -z "$http_proxy" ]; then
    echo "🔒 Proxy is configured: $http_proxy"
    if curl -s -I -x "$http_proxy" --max-time 5 https://www.google.com > /dev/null; then
        echo "✅ Proxy is working"
    else
        echo "⚠️  Proxy check failed"
    fi
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
echo "  - Read-only root filesystem: Yes"
echo "  - Network filtering via proxy: $([ ! -z "$http_proxy" ] && echo "Yes" || echo "No")"
echo "  - Running as non-root user: $(whoami)"
echo ""
echo "✅ Container is ready for development!"
