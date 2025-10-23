# Stop and clean up DevContainer
# Run this from PowerShell in the project root

Write-Host "🛑 Stopping DevContainer..." -ForegroundColor Yellow
docker-compose -f .devcontainer/docker-compose.yml down

Write-Host "✅ DevContainer stopped!" -ForegroundColor Green
Write-Host "`n📝 Notes:"
Write-Host "   - Volumes are preserved (dotnet packages, VS Code extensions)"
Write-Host "   - To remove volumes: docker-compose -f .devcontainer/docker-compose.yml down -v"
Write-Host "   - To remove images: docker-compose -f .devcontainer/docker-compose.yml down --rmi all"
