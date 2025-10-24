#!/bin/bash
# Post-create script - runs after the container is created
set -e

echo "🚀 Running post-create setup..."

# Check if proxy is enabled
if [ ! -z "$http_proxy" ]; then
    echo "⏳ Waiting for proxy service to be ready..."
    PROXY_READY=false

    # Wait up to 120 seconds for proxy to be ready
    for i in {1..60}; do
        # Test both port connectivity and actual HTTP functionality
        if nc -z proxy 3128 2>/dev/null && curl -x http://proxy:3128 -s -o /dev/null -w "%{http_code}" http://www.google.com 2>/dev/null | grep -q "200\|301\|302"; then
            PROXY_READY=true
            echo "✅ Proxy is ready and functional!"
            break
        fi
        echo "⏳ Waiting for proxy to be functional... ($i/60)"
        sleep 2
    done

    if [ "$PROXY_READY" = false ]; then
        echo "⚠️  Warning: Proxy service not available after 120 seconds."
        echo "⚠️  Package installation may fail or be incomplete."
        echo "⚠️  Check proxy logs with: podman compose logs proxy"
        echo "⚠️  Continuing anyway - you can manually install packages later."
        # Don't exit - allow container to continue starting
    fi
else
    echo "ℹ️  Proxy is disabled - using direct internet connection"
fi

# Install AI CLI tools
echo "📦 Installing AI CLI tools..."

# Verify pip can see proxy settings
echo "📡 Checking pip proxy configuration..."
echo "System pip.conf:"
cat /etc/pip.conf || true
echo ""
echo "User pip.conf:"
cat /home/vscode/.config/pip/pip.conf || echo "User pip.conf not found"
echo ""
echo "Pip config list:"
pip3 config list || true
echo ""

# Verify environment proxy settings
echo "📡 Proxy environment variables:"
echo "  http_proxy=$http_proxy"
echo "  https_proxy=$https_proxy"
echo "  PIP_TRUSTED_HOST=$PIP_TRUSTED_HOST"
echo "  PIP_DEFAULT_TIMEOUT=$PIP_DEFAULT_TIMEOUT"
echo ""

# Test pip connectivity
echo "🔍 Testing pip connectivity to PyPI..."
if pip3 install --dry-run --no-cache-dir requests 2>&1 | grep -q "Successfully"; then
    echo "✅ Pip can successfully reach PyPI"
else
    echo "⚠️  Pip connectivity test failed. Trying verbose mode..."
    pip3 install --dry-run --no-cache-dir --verbose requests 2>&1 | tail -20
fi
echo ""

# Install .NET global tools
echo "📦 Installing .NET global tools..."
dotnet tool install -g dotnet-ef || echo "⚠️  dotnet-ef installation failed"
dotnet tool install -g dotnet-format || echo "⚠️  dotnet-format installation failed"
dotnet tool install -g dotnet-outdated-tool || echo "⚠️  dotnet-outdated-tool installation failed"
echo ""

# Install base Python packages first
echo "📦 Installing base Python packages..."
pip3 install --user --break-system-packages --no-cache-dir \
    openai \
    anthropic \
    google-generativeai \
    requests \
    rich \
|| echo "⚠️  Some base packages failed to install"
echo ""

# Install OpenAI CLI (unofficial)
pip3 install --user --break-system-packages openai-cli || echo "⚠️  OpenAI CLI installation failed"

# Install Anthropic Claude CLI (unofficial)
pip3 install --user --break-system-packages anthropic-cli || echo "⚠️  Anthropic CLI installation failed"

# Install Google Generative AI CLI tools
pip3 install --user --break-system-packages google-generativeai-cli || echo "⚠️  Google CLI not available via pip"

# Install GitHub Copilot CLI (requires authentication)
if command -v gh &> /dev/null; then
    echo "Installing GitHub Copilot CLI extensions..."
    gh extension install github/gh-copilot || echo "⚠️  Copilot extension already installed or unavailable"
fi

# Install additional Python tools
echo "📦 Installing additional Python tools..."
pip3 install --user --break-system-packages \
    httpx \
    aiohttp \
    python-dotenv \
    tiktoken \
    pyyaml

# Install UV - fast Python package manager
echo "📦 Installing UV package manager..."
pip3 install --user --break-system-packages uv || echo "⚠️  UV installation failed"
# Add UV to PATH if installed
if [ -d "/home/vscode/.local/bin" ]; then
    if ! grep -q "/home/vscode/.local/bin" /home/vscode/.bashrc; then
        echo 'export PATH="$HOME/.local/bin:$PATH"' >> /home/vscode/.bashrc
    fi
fi

# Install Node.js based AI tools
echo "📦 Installing Node.js AI tools..."
npm install -g @anthropic-ai/sdk || echo "⚠️  Could not install Anthropic SDK"

# Restore .NET dependencies
if [ -f "/workspace/Chonkie.Net.sln" ]; then
    echo "📦 Restoring .NET dependencies..."
    cd /workspace
    dotnet restore Chonkie.Net.sln || echo "⚠️  Failed to restore .NET dependencies"
fi

# Create helpful scripts
echo "📝 Creating helper scripts..."
mkdir -p /home/vscode/bin

# Create AI tools wrapper script
cat > /home/vscode/bin/ai-tools << 'EOF'
#!/bin/bash
# Wrapper for AI CLI tools

case "$1" in
    openai)
        shift
        openai "$@"
        ;;
    claude)
        shift
        anthropic "$@"
        ;;
    gemini)
        shift
        python3 -m google.generativeai.cli "$@"
        ;;
    copilot)
        shift
        gh copilot "$@"
        ;;
    *)
        echo "Usage: ai-tools {openai|claude|gemini|copilot} [args]"
        exit 1
        ;;
esac
EOF

chmod +x /home/vscode/bin/ai-tools

# Add bin directory to PATH if not already there
if ! grep -q "/home/vscode/bin" /home/vscode/.bashrc; then
    echo 'export PATH="$HOME/bin:$PATH"' >> /home/vscode/.bashrc
fi

# Set up git safe directory (with retry logic)
echo "📝 Configuring git..."
# We can't write to /home/vscode/.gitconfig because it's bind-mounted read-only.
# Use XDG global config at /home/vscode/.config/git/config instead.
mkdir -p /home/vscode/.config/git
for i in {1..3}; do
    if git config --file /home/vscode/.config/git/config --add safe.directory /workspace 2>/dev/null; then
        echo "✅ Git configured successfully (XDG global config)"
        break
    else
        echo "⏳ Retrying git config... ($i/3)"
        sleep 2
    fi
done

# Ensure line-ending policy is consistent in the container
# Rely on .gitattributes for normalization and keep autocrlf disabled
git config --file /home/vscode/.config/git/config core.autocrlf false || true
git config --file /home/vscode/.config/git/config core.eol lf || true

# Apply git proxy configuration (XDG global config)
if [ -f "/tmp/gitconfig-proxy" ]; then
    echo "📝 Configuring git proxy (XDG)..."
    mkdir -p /home/vscode/.config/git
    cat /tmp/gitconfig-proxy >> /home/vscode/.config/git/config
    echo "✅ Git proxy configured"
fi

# Apply NuGet proxy configuration
if [ -f "/tmp/nuget.config" ]; then
    echo "📝 Configuring NuGet proxy..."
    mkdir -p /home/vscode/.nuget/NuGet
    cp /tmp/nuget.config /home/vscode/.nuget/NuGet/NuGet.Config
    echo "✅ NuGet proxy configured"
fi

# Note: APT proxy configuration skipped in post-create due to no-new-privileges
# The container already has all needed packages installed during build
# If you need apt during runtime, temporarily disable no-new-privileges in docker-compose.yml

# Apply pip proxy configuration (user-level only)
if [ -f "/tmp/pip.conf" ]; then
    echo "📝 Configuring pip proxy..."
    # System-level config requires sudo (skipped due to no-new-privileges)
    # User-level config works fine for all pip operations as vscode user
    mkdir -p /home/vscode/.config/pip
    cp /tmp/pip.conf /home/vscode/.config/pip/pip.conf
    echo "✅ Pip proxy configured (user-level)"
    echo "ℹ️  Pip will now use proxy for package installations"
fi

# Create helpful aliases
cat >> /home/vscode/.bashrc << 'EOF'

# Dev Container Aliases
alias ll='ls -lah'
alias build='dotnet build'
alias test='dotnet test'
alias run='dotnet run'

# AI Tools shortcuts
alias ask-openai='ai-tools openai'
alias ask-claude='ai-tools claude'
alias ask-gemini='ai-tools gemini'
alias ask-copilot='ai-tools copilot'

# Network debugging
alias check-network='curl -I https://www.google.com'
alias proxy-status='curl -I -x http://proxy:3128 https://www.google.com'
EOF

echo "✅ Post-create setup complete!"
echo ""
echo "Available AI tools:"
echo "  - ai-tools openai [command]"
echo "  - ai-tools claude [command]"
echo "  - ai-tools gemini [command]"
echo "  - ai-tools copilot [command]"
echo ""
echo "Or use the aliases: ask-openai, ask-claude, ask-gemini, ask-copilot"
