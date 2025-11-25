# Complete Azure Configuration Fix for Double-Startup Issue
# This script will properly configure Azure to prevent Oryx from running twice

Write-Host "=== FIXING AZURE DOUBLE-STARTUP ISSUE ===" -ForegroundColor Cyan
Write-Host ""

# Check if Azure CLI is installed
Write-Host "Checking Azure CLI..." -ForegroundColor Yellow
$azCommand = Get-Command az -ErrorAction SilentlyContinue

if (-not $azCommand) {
    Write-Host "ERROR: Azure CLI is not installed!" -ForegroundColor Red
    Write-Host "Please install it from: https://aka.ms/installazurecliwindows" -ForegroundColor Yellow
    Write-Host "After installation, restart PowerShell and run this script again." -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "Azure CLI found!" -ForegroundColor Green
Write-Host ""

# Login to Azure
Write-Host "Step 1: Logging in to Azure..." -ForegroundColor Yellow
az login --output none

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to login to Azure" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "Login successful!" -ForegroundColor Green
Write-Host ""

# Stop the app
Write-Host "Step 2: Stopping the API..." -ForegroundColor Yellow
az webapp stop --resource-group taktiqRecursos --name taktiq-api --output none

if ($LASTEXITCODE -eq 0) {
    Write-Host "API stopped successfully!" -ForegroundColor Green
} else {
    Write-Host "Warning: Failed to stop API (it might already be stopped)" -ForegroundColor Yellow
}
Write-Host ""

# Wait a moment
Write-Host "Waiting 10 seconds for app to fully stop..." -ForegroundColor Gray
Start-Sleep -Seconds 10

# Clear the startup command
Write-Host "Step 3: Clearing startup command..." -ForegroundColor Yellow
az webapp config set `
    --resource-group taktiqRecursos `
    --name taktiq-api `
    --startup-file "" `
    --output none

if ($LASTEXITCODE -eq 0) {
    Write-Host "Startup command cleared!" -ForegroundColor Green
} else {
    Write-Host "ERROR: Failed to clear startup command" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host ""

# Configure app settings to prevent Oryx interference
Write-Host "Step 4: Configuring app settings to prevent Oryx..." -ForegroundColor Yellow
az webapp config appsettings set `
    --resource-group taktiqRecursos `
    --name taktiq-api `
    --settings `
        SCM_DO_BUILD_DURING_DEPLOYMENT=false `
        WEBSITE_DISABLE_SCM_SEPARATION=false `
        WEBSITE_RUN_FROM_PACKAGE=0 `
    --output none

if ($LASTEXITCODE -eq 0) {
    Write-Host "App settings configured!" -ForegroundColor Green
} else {
    Write-Host "ERROR: Failed to configure app settings" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host ""

# Verify configuration
Write-Host "Step 5: Verifying configuration..." -ForegroundColor Yellow
Write-Host ""
Write-Host "Startup command:" -ForegroundColor Cyan
$startupCmd = az webapp config show --resource-group taktiqRecursos --name taktiq-api --query "appCommandLine" -o tsv
if ([string]::IsNullOrWhiteSpace($startupCmd)) {
    Write-Host "  (empty) ✓" -ForegroundColor Green
} else {
    Write-Host "  $startupCmd" -ForegroundColor Yellow
    Write-Host "  WARNING: Startup command is not empty!" -ForegroundColor Yellow
}
Write-Host ""

Write-Host "App settings:" -ForegroundColor Cyan
az webapp config appsettings list `
    --resource-group taktiqRecursos `
    --name taktiq-api `
    --query "[?name=='SCM_DO_BUILD_DURING_DEPLOYMENT' || name=='WEBSITE_RUN_FROM_PACKAGE' || name=='WEBSITE_DISABLE_SCM_SEPARATION'].{Name:name, Value:value}" `
    --output table
Write-Host ""

# Start the app
Write-Host "Step 6: Starting the API..." -ForegroundColor Yellow
az webapp start --resource-group taktiqRecursos --name taktiq-api --output none

if ($LASTEXITCODE -eq 0) {
    Write-Host "API started successfully!" -ForegroundColor Green
} else {
    Write-Host "ERROR: Failed to start API" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host ""

# Wait for startup
Write-Host "Waiting 30 seconds for app to start..." -ForegroundColor Gray
Start-Sleep -Seconds 30

# Check logs
Write-Host "Step 7: Checking logs for double-startup issue..." -ForegroundColor Yellow
Write-Host ""
Write-Host "Opening log stream in browser..." -ForegroundColor Gray
Write-Host "URL: https://taktiq-api.scm.azurewebsites.net/api/logs/docker" -ForegroundColor Cyan
Write-Host ""
Write-Host "Look for:" -ForegroundColor Yellow
Write-Host "  ✓ ONE instance of 'Now listening on: http://[::]:8080'" -ForegroundColor Green
Write-Host "  ✓ 'Database connection successful' (from our warmup code)" -ForegroundColor Green
Write-Host "  ✗ Should NOT see 'address already in use'" -ForegroundColor Red
Write-Host "  ✗ Should NOT see Oryx running create-script" -ForegroundColor Red
Write-Host ""

Start-Process "https://taktiq-api.scm.azurewebsites.net/api/logs/docker"

# Test the API
Write-Host "Step 8: Testing API health endpoint..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

try {
    $response = Invoke-RestMethod -Uri "https://api.taktiq.app/health" -Method Get -ErrorAction Stop
    Write-Host "API is responding!" -ForegroundColor Green
    Write-Host "Response: $($response | ConvertTo-Json -Compress)" -ForegroundColor Cyan
} catch {
    Write-Host "Warning: API is not responding yet (might still be starting up)" -ForegroundColor Yellow
    Write-Host "Error: $_" -ForegroundColor Gray
}
Write-Host ""

Write-Host "=== CONFIGURATION COMPLETE ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Monitor the logs in the browser window for 2-3 minutes" -ForegroundColor White
Write-Host "2. Verify only ONE startup happens (no Oryx create-script)" -ForegroundColor White
Write-Host "3. Test logging in with both Student and Personal Trainer accounts" -ForegroundColor White
Write-Host "4. If issue persists, we need to trigger a new deployment from GitHub" -ForegroundColor White
Write-Host ""

Read-Host "Press Enter to close"
