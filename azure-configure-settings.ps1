# Azure Configuration Script for GymHero/Taktiq (PowerShell)
# This script configures all application settings in Azure App Service using Az PowerShell module

# Variables
$ResourceGroup = "taktiqRecursos"
$BackendApp = "taktiq-api"
$FrontendApp = "taktiq-web-frontend"

Write-Host "=========================================="
Write-Host "Configuring Azure App Service Settings"
Write-Host "=========================================="

# Check if Az module is installed
if (-not (Get-Module -ListAvailable -Name Az.Websites)) {
    Write-Host "Az.Websites module not found. Installing..."
    Install-Module -Name Az.Websites -Scope CurrentUser -Force
}

# Login to Azure (if not already logged in)
try {
    $context = Get-AzContext
    if (-not $context) {
        Write-Host "Please login to Azure..."
        Connect-AzAccount
    }
} catch {
    Write-Host "Please login to Azure..."
    Connect-AzAccount
}

# 1. Configure Google Places API
Write-Host ""
Write-Host "1. Configuring Google Places API..."
$settings = @{
    "GooglePlaces__ApiKey" = "AIzaSyDw18sH8dG1Hj-39KbtB-OJbMNBq3Ajfkk"
}
Set-AzWebAppSettings -ResourceGroupName $ResourceGroup -Name $BackendApp -AppSettings $settings

# 2. Configure Stripe
Write-Host ""
Write-Host "2. Configuring Stripe..."
Write-Host "IMPORTANT: Get your Stripe keys from: https://dashboard.stripe.com/apikeys"
$StripeSecretKey = Read-Host "Enter your Stripe Secret Key (sk_test_...)"
$StripeWebhookSecret = Read-Host "Enter your Stripe Webhook Secret (whsec_...)"
$StripePublishableKey = Read-Host "Enter your Stripe Publishable Key (pk_test_...)"

$stripeSettings = @{
    "Stripe__SecretKey" = $StripeSecretKey
    "Stripe__WebhookSecret" = $StripeWebhookSecret
}
Set-AzWebAppSettings -ResourceGroupName $ResourceGroup -Name $BackendApp -AppSettings $stripeSettings

# 3. Enable Marketplace Payments
Write-Host ""
Write-Host "3. Enabling Marketplace Payments..."
$paymentSettings = @{
    "Marketplace__PaymentsEnabled" = "true"
}
Set-AzWebAppSettings -ResourceGroupName $ResourceGroup -Name $BackendApp -AppSettings $paymentSettings

# 4. Configure Video Compression (FFmpeg settings)
Write-Host ""
Write-Host "4. Configuring Video Compression..."
$videoSettings = @{
    "VideoCompression__Enabled" = "true"
    "VideoCompression__DefaultQuality" = "Medium"
    "VideoCompression__MaxResolution" = "1920"
    "VideoCompression__AutoCompress" = "true"
}
Set-AzWebAppSettings -ResourceGroupName $ResourceGroup -Name $BackendApp -AppSettings $videoSettings

Write-Host ""
Write-Host "=========================================="
Write-Host "Configuration Complete!"
Write-Host "=========================================="
Write-Host ""
Write-Host "Next Steps:"
Write-Host "1. Restart the App Service:"
Write-Host "   Restart-AzWebApp -ResourceGroupName $ResourceGroup -Name $BackendApp"
Write-Host ""
Write-Host "2. Configure Frontend Stripe Key manually in Azure Portal:"
Write-Host "   - Navigate to: $FrontendApp > Configuration > Application settings"
Write-Host "   - Add: NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY = $StripePublishableKey"
Write-Host ""
Write-Host "3. Test the APIs:"
Write-Host "   - Google Places: https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/places/nearby"
Write-Host "   - Stripe Connect: https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/stripe-connect"
