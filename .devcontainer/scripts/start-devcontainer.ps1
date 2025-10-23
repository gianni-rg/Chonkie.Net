# Quick Start Scripts for DevContainer
# Run these from PowerShell in the project root

# Build and start containers
Write-Host "🚀 Building and starting DevContainer..." -ForegroundColor Cyan
docker-compose -f .devcontainer/docker-compose.yml up -d --build

# Wait for containers to be ready
Write-Host "⏳ Waiting for containers to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Check container status
Write-Host "`n📊 Container Status:" -ForegroundColor Cyan
docker-compose -f .devcontainer/docker-compose.yml ps

# Show connection instructions
Write-Host "`n✅ DevContainer is ready!" -ForegroundColor Green
Write-Host "`n📝 Next steps:" -ForegroundColor Cyan
Write-Host "   VS Code: Press F1 → 'Dev Containers: Attach to Running Container' → Select 'chonkienet-dotnet-app-1'"
Write-Host "   CLI: docker exec -it chonkienet-dotnet-app-1 bash"
Write-Host "   Visual Studio: Open the solution normally, it will sync with the container"
Write-Host "`n🔍 View logs: docker-compose -f .devcontainer/docker-compose.yml logs -f"
Write-Host "🛑 Stop: docker-compose -f .devcontainer/docker-compose.yml down"
