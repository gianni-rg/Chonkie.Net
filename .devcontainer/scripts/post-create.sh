#!/bin/bash
# Post-create script - runs after the container is created
set -e

echo "🚀 Running post-create setup..."

# Wait for proxy to be ready (with timeout)
echo "⏳ Waiting for proxy service to be ready..."
PROXY_READY=false
for i in {1..30}; do
    if nc -z proxy 3128 2>/dev/null; then
        PROXY_READY=true
        echo "✅ Proxy is ready!"
        break
    fi
    echo "⏳ Waiting for proxy... ($i/30)"
    sleep 2
done

if [ "$PROXY_READY" = false ]; then
    echo "⚠️  Warning: Proxy service not available. Attempting to install packages without proxy..."
    # Unset proxy environment variables
    unset http_proxy
    unset https_proxy
    unset HTTP_PROXY
    unset HTTPS_PROXY
fi

# Install AI CLI tools
echo "📦 Installing AI CLI tools..."

# Install OpenAI CLI (unofficial)
pip3 install --user --break-system-packages openai-cli

# Install Anthropic Claude CLI (unofficial)
pip3 install --user --break-system-packages anthropic-cli

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

# Set up git safe directory
git config --global --add safe.directory /workspace

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
