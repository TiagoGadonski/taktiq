# Force a new deployment with latest code
# This ensures all fixes (double-startup prevention + database warmup) are deployed

Write-Host "=== FORCE NEW DEPLOYMENT ===" -ForegroundColor Cyan
Write-Host ""

Write-Host "This script will:" -ForegroundColor Yellow
Write-Host "1. Make a minor update to trigger a new deployment" -ForegroundColor White
Write-Host "2. Commit and push the change" -ForegroundColor White
Write-Host "3. GitHub Actions will deploy the latest code to Azure" -ForegroundColor White
Write-Host ""

$response = Read-Host "Continue? (Y/N)"
if ($response -ne 'Y' -and $response -ne 'y') {
    Write-Host "Cancelled." -ForegroundColor Yellow
    exit 0
}

# Update the deployment version in Program.cs to trigger rebuild
$programFile = "src\GymHero.Api\Program.cs"
$content = Get-Content $programFile -Raw

# Update version comment
$timestamp = Get-Date -Format "yyyy-MM-dd-HHmmss"
$newContent = $content -replace "Deployment Version: .*", "Deployment Version: $timestamp - Force deploy with all fixes"

Set-Content -Path $programFile -Value $newContent -NoNewline

Write-Host "Updated deployment version in Program.cs" -ForegroundColor Green
Write-Host ""

# Git operations
Write-Host "Committing changes..." -ForegroundColor Yellow
git add $programFile
git add "frontend\apps\web\src\app\(app)\gyms\page.tsx"
git commit -m "fix: Force deployment with double-startup fix and nearby gym update

- Force redeploy to ensure latest fixes are applied
- Removed 'coming soon' banner from nearby gym feature
- This deployment includes:
  * Oryx double-startup prevention
  * Database connection warmup
  * DataProtection keys persistence to /home/site
  * Increased DB timeouts for cold starts

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to commit" -ForegroundColor Red
    exit 1
}

Write-Host "Commit created successfully!" -ForegroundColor Green
Write-Host ""

Write-Host "Pushing to GitHub..." -ForegroundColor Yellow
git push

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to push to GitHub" -ForegroundColor Red
    exit 1
}

Write-Host "Pushed successfully!" -ForegroundColor Green
Write-Host ""

Write-Host "=== DEPLOYMENT TRIGGERED ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "GitHub Actions is now deploying your code." -ForegroundColor Green
Write-Host ""
Write-Host "Monitor deployment:" -ForegroundColor Yellow
Write-Host "1. GitHub Actions: https://github.com/YOUR_USERNAME/YOUR_REPO/actions" -ForegroundColor Cyan
Write-Host "2. Azure Logs: https://taktiq-api.scm.azurewebsites.net/api/logs/docker" -ForegroundColor Cyan
Write-Host ""
Write-Host "Wait 5-10 minutes for deployment to complete, then test login again." -ForegroundColor Yellow
Write-Host ""
Write-Host "What to look for in Azure logs:" -ForegroundColor Yellow
Write-Host "  ✓ '=== Application Starting ==='" -ForegroundColor Green
Write-Host "  ✓ 'Warming up database connection...'" -ForegroundColor Green
Write-Host "  ✓ 'Database connection successful'" -ForegroundColor Green
Write-Host "  ✓ 'DataProtection keys will be persisted to: /home/site/DataProtection-Keys'" -ForegroundColor Green
Write-Host "  ✓ Only ONE 'Now listening on: http://[::]:8080'" -ForegroundColor Green
Write-Host "  ✗ NO 'address already in use' errors" -ForegroundColor Red
Write-Host "  ✗ NO Oryx create-script running" -ForegroundColor Red
Write-Host ""

Read-Host "Press Enter to close"
