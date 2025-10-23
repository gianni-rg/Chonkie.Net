# Test network connectivity from host machine

Write-Host "🔍 Testing DevContainer Network Configuration..." -ForegroundColor Cyan
Write-Host ""

# Check if containers are running
$dotnetContainer = docker ps --filter "name=chonkienet-dotnet-app" --format "{{.Names}}"
$proxyContainer = docker ps --filter "name=chonkienet-proxy" --format "{{.Names}}"

if (-not $dotnetContainer) {
    Write-Host "❌ DevContainer is not running!" -ForegroundColor Red
    Write-Host "   Start it with: docker-compose -f .devcontainer/docker-compose.yml up -d" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Containers are running" -ForegroundColor Green
Write-Host "   - App container: $dotnetContainer"
Write-Host "   - Proxy container: $proxyContainer"
Write-Host ""

# Run network test script inside container
Write-Host "📊 Running connectivity tests..." -ForegroundColor Cyan
docker exec $dotnetContainer bash -c "
    echo '=== Testing Network Configuration ==='
    echo ''
    echo 'Testing GitHub API...'
    curl -s -o /dev/null -w 'HTTP Status: %{http_code}\n' -x http://proxy:3128 https://api.github.com
    echo ''
    echo 'Testing NuGet...'
    curl -s -o /dev/null -w 'HTTP Status: %{http_code}\n' -x http://proxy:3128 https://api.nuget.org/v3/index.json
    echo ''
    echo 'Testing NPM...'
    curl -s -o /dev/null -w 'HTTP Status: %{http_code}\n' -x http://proxy:3128 https://registry.npmjs.org
    echo ''
    echo 'Testing blocked site (should fail)...'
    curl -s -o /dev/null -w 'HTTP Status: %{http_code}\n' -x http://proxy:3128 https://example.com || echo 'Blocked (expected)'
"

Write-Host ""
Write-Host "📜 Recent proxy logs:" -ForegroundColor Cyan
docker logs --tail 20 $proxyContainer 2>&1 | Select-Object -Last 20

Write-Host ""
Write-Host "✅ Network test complete!" -ForegroundColor Green
