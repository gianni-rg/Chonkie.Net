# Dev Container Initial Setup Script
# Run this script from the project root to prepare the dev container

Write-Host "🚀 Chonkie.Net Dev Container Setup" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan
Write-Host ""

# Check Podman is running
Write-Host "Checking Podman..." -ForegroundColor Yellow
try {
    $podmanVersion = podman --version
    Write-Host "✓ Podman found: $podmanVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ Podman not found or not running!" -ForegroundColor Red
    Write-Host "  Please install Podman Desktop and ensure it's running." -ForegroundColor Red
    exit 1
}

# Check if .env exists
Write-Host ""
Write-Host "Checking environment configuration..." -ForegroundColor Yellow
if (Test-Path ".devcontainer\.env") {
    Write-Host "✓ .env file already exists" -ForegroundColor Green
    $overwrite = Read-Host "Do you want to recreate it? (y/N)"
    if ($overwrite -eq 'y' -or $overwrite -eq 'Y') {
        Copy-Item ".devcontainer\.env.example" ".devcontainer\.env" -Force
        Write-Host "✓ .env file recreated from template" -ForegroundColor Green
    }
} else {
    Copy-Item ".devcontainer\.env.example" ".devcontainer\.env"
    Write-Host "✓ .env file created from template" -ForegroundColor Green
}

# Prompt for API keys
Write-Host ""
Write-Host "Configuration" -ForegroundColor Cyan
Write-Host "=============" -ForegroundColor Cyan
Write-Host ""
Write-Host "You'll need to manually edit .devcontainer\.env with your API keys." -ForegroundColor Yellow
Write-Host ""
Write-Host "Required API Keys:" -ForegroundColor White
Write-Host "  - OPENAI_API_KEY (from https://platform.openai.com/api-keys)" -ForegroundColor Gray
Write-Host "  - ANTHROPIC_API_KEY (from https://console.anthropic.com/)" -ForegroundColor Gray
Write-Host "  - GOOGLE_API_KEY (from https://makersuite.google.com/app/apikey)" -ForegroundColor Gray
Write-Host "  - GITHUB_TOKEN (from https://github.com/settings/tokens)" -ForegroundColor Gray
Write-Host ""

$openEditor = Read-Host "Open .env file in editor now? (Y/n)"
if ($openEditor -ne 'n' -and $openEditor -ne 'N') {
    code ".devcontainer\.env"
    Write-Host "✓ Opened .env in VS Code" -ForegroundColor Green
}

# Check for VS Code Remote Containers extension
Write-Host ""
Write-Host "Checking VS Code extensions..." -ForegroundColor Yellow
try {
    $extensions = code --list-extensions
    if ($extensions -match "ms-vscode-remote.remote-containers") {
        Write-Host "✓ Remote - Containers extension is installed" -ForegroundColor Green
    } else {
        Write-Host "✗ Remote - Containers extension not found" -ForegroundColor Red
        Write-Host "  Installing extension..." -ForegroundColor Yellow
        code --install-extension ms-vscode-remote.remote-containers
        Write-Host "✓ Extension installed" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠ Could not check VS Code extensions" -ForegroundColor Yellow
    Write-Host "  Please ensure 'Remote - Containers' extension is installed" -ForegroundColor Yellow
}

# Summary
Write-Host ""
Write-Host "Setup Summary" -ForegroundColor Cyan
Write-Host "=============" -ForegroundColor Cyan
Write-Host ""
Write-Host "✓ Podman is running" -ForegroundColor Green
Write-Host "✓ .env file is created" -ForegroundColor Green
Write-Host "✓ VS Code extensions checked" -ForegroundColor Green
Write-Host ""

# Next steps
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "===========" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Edit .devcontainer\.env with your API keys" -ForegroundColor White
Write-Host ""
Write-Host "2. (Optional) Configure external folder mounts:" -ForegroundColor White
Write-Host "   Edit .devcontainer\docker-compose.yml" -ForegroundColor Gray
Write-Host ""
Write-Host "3. (Optional) Review allowed domains:" -ForegroundColor White
Write-Host "   Edit .devcontainer\allowed-domains.txt" -ForegroundColor Gray
Write-Host ""
Write-Host "4. Open in Dev Container:" -ForegroundColor White
Write-Host "   - Press F1 in VS Code" -ForegroundColor Gray
Write-Host "   - Select 'Dev Containers: Reopen in Container'" -ForegroundColor Gray
Write-Host "   - Wait 5-10 minutes for initial build" -ForegroundColor Gray
Write-Host "   - Note: VS Code will use Podman automatically" -ForegroundColor Gray
Write-Host ""
Write-Host "5. See .devcontainer\CHECKLIST.md for detailed verification steps" -ForegroundColor White
Write-Host ""

# Optional: Open in container now
Write-Host ""
$openNow = Read-Host "Open in Dev Container now? (Y/n)"
if ($openNow -ne 'n' -and $openNow -ne 'N') {
    Write-Host ""
    Write-Host "Opening workspace in container..." -ForegroundColor Yellow
    Write-Host "This will close current VS Code window and reopen in container." -ForegroundColor Yellow
    Write-Host "Note: Ensure Podman is set as the container provider in VS Code settings." -ForegroundColor Yellow
    Write-Host ""
    Start-Sleep -Seconds 2
    
    # Open in container
    code . --folder-uri "vscode-remote://dev-container+$(Get-Location | ForEach-Object {$_.Path.Replace('\','/').Replace('C:','/c').Replace(' ','%20')})"
    
    Write-Host "✓ Opening in container..." -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "Setup complete! 🎉" -ForegroundColor Green
    Write-Host "When ready, press F1 and select 'Dev Containers: Reopen in Container'" -ForegroundColor White
}

Write-Host ""
Write-Host "Documentation:" -ForegroundColor Cyan
Write-Host "  - Full Guide: .devcontainer\README.md" -ForegroundColor Gray
Write-Host "  - Quick Start: .devcontainer\QUICK_START.md" -ForegroundColor Gray
Write-Host "  - Checklist: .devcontainer\CHECKLIST.md" -ForegroundColor Gray
Write-Host "  - Architecture: .devcontainer\ARCHITECTURE.md" -ForegroundColor Gray
Write-Host ""
