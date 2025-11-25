# Fix Azure API Startup Port Conflict Issue
# This script fixes the "address already in use" error on Azure App Service

Write-Host "=== Azure API Startup Fix ===" -ForegroundColor Cyan
Write-Host ""

# Check if Azure CLI is installed
$azInstalled = Get-Command az -ErrorAction SilentlyContinue
if (-not $azInstalled) {
    Write-Host "ERROR: Azure CLI not installed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please follow these MANUAL steps in Azure Portal:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Go to: https://portal.azure.com" -ForegroundColor White
    Write-Host "2. Navigate to: App Services -> taktiq-api" -ForegroundColor White
    Write-Host "3. In left menu, click: Configuration" -ForegroundColor White
    Write-Host "4. Go to: General settings tab" -ForegroundColor White
    Write-Host "5. Check 'Startup Command' field:" -ForegroundColor White
    Write-Host "   - If it says 'dotnet GymHero.Api.dll', CLEAR IT (leave empty)" -ForegroundColor Yellow
    Write-Host "   - Azure will auto-detect the DLL, no need for explicit command" -ForegroundColor Yellow
    Write-Host "6. Click 'Save' at the top" -ForegroundColor White
    Write-Host "7. In left menu, click: Overview" -ForegroundColor White
    Write-Host "8. Click 'Restart' button at the top" -ForegroundColor White
    Write-Host ""
    Write-Host "ALTERNATIVE FIX (if above doesn't work):" -ForegroundColor Cyan
    Write-Host "1. In Configuration -> General settings" -ForegroundColor White
    Write-Host "2. Set 'Startup Command' to: sh -c 'dotnet GymHero.Api.dll'" -ForegroundColor Yellow
    Write-Host "3. Save and Restart" -ForegroundColor White
    Write-Host ""
    Write-Host "After restart, check logs at:" -ForegroundColor White
    Write-Host "https://taktiq-api.scm.azurewebsites.net/api/logs/docker" -ForegroundColor Cyan
    Write-Host ""
    exit 1
}

Write-Host "Azure CLI found. Applying fix..." -ForegroundColor Green
Write-Host ""

# Login check
Write-Host "Checking Azure login status..." -ForegroundColor Yellow
$account = az account show 2>$null | ConvertFrom-Json
if (-not $account) {
    Write-Host "Not logged in to Azure. Logging in..." -ForegroundColor Yellow
    az login
}

Write-Host "Logged in as: $($account.user.name)" -ForegroundColor Green
Write-Host ""

# Get current startup command
Write-Host "Checking current startup command..." -ForegroundColor Yellow
$currentCommand = az webapp config show --name taktiq-api --resource-group taktiq-group --query 'appCommandLine' -o tsv 2>$null

Write-Host "Current startup command: '$currentCommand'" -ForegroundColor Cyan
Write-Host ""

# Clear startup command (let Azure auto-detect)
Write-Host "Clearing startup command (Azure will auto-detect)..." -ForegroundColor Yellow
az webapp config set --name taktiq-api --resource-group taktiq-group --startup-file ""

Write-Host "Startup command cleared" -ForegroundColor Green
Write-Host ""

# Restart the app
Write-Host "Restarting API app service..." -ForegroundColor Yellow
az webapp restart --name taktiq-api --resource-group taktiq-group

Write-Host ""
Write-Host "=== Fix Applied ===" -ForegroundColor Green
Write-Host ""
Write-Host "The API should restart cleanly without port conflicts." -ForegroundColor White
Write-Host ""
Write-Host "Monitor the logs at:" -ForegroundColor Yellow
Write-Host "https://taktiq-api.scm.azurewebsites.net/api/logs/docker" -ForegroundColor Cyan
Write-Host ""
Write-Host "Wait 2-3 minutes, then test your app at:" -ForegroundColor Yellow
Write-Host "https://taktiq.app" -ForegroundColor Cyan
Write-Host ""
Write-Host "If issues persist, check the ALTERNATIVE FIX in the manual steps above." -ForegroundColor Yellow
