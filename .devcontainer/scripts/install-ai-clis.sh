#!/bin/bash
#
# Install AI Agent CLIs in the devcontainer
# This script is designed to run inside the container with proxy configured
#

set -e  # Exit on error

echo "🤖 Installing AI Agent CLIs..."

# Configure npm to use proxy
echo "📦 Configuring npm..."
npm config set proxy http://proxy:3128
npm config set https-proxy http://proxy:3128

# Install Gemini CLI
echo "🔮 Installing Gemini CLI..."
if npm install -g @google/gemini-cli; then
    echo "✅ Gemini CLI installed successfully"
    gemini --version || echo "⚠️ Gemini CLI installed but not configured"
else
    echo "❌ Failed to install Gemini CLI"
fi

# Install GitHub CLI (if not already installed via features)
echo "🐙 Checking GitHub CLI..."
if ! command -v gh &> /dev/null; then
    echo "📥 Installing GitHub CLI..."
    curl -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg | dd of=/usr/share/keyrings/githubcli-archive-keyring.gpg
    chmod go+r /usr/share/keyrings/githubcli-archive-keyring.gpg
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main" | tee /etc/apt/sources.list.d/github-cli.list > /dev/null
    apt-get update
    apt-get install -y gh
    echo "✅ GitHub CLI installed successfully"
else
    echo "✅ GitHub CLI already installed"
    gh --version
fi

# Note: OpenCode installation removed as it requires verification
# Users can install it manually if needed

echo ""
echo "✨ Installation complete!"
echo ""
echo "Next steps:"
echo "1. Authenticate with GitHub: gh auth login"
echo "2. Configure Gemini: gemini config set apiKey YOUR_API_KEY"
echo "3. Test connectivity: curl -x http://proxy:3128 https://api.github.com"
echo ""
