#!/bin/bash

# Azure Configuration Script for GymHero/Taktiq
# This script configures all application settings in Azure App Service

# Variables
RESOURCE_GROUP="taktiqRecursos"
BACKEND_APP="taktiq-api"
FRONTEND_APP="taktiq-web-frontend"

echo "=========================================="
echo "Configuring Azure App Service Settings"
echo "=========================================="

# 1. Configure Google Places API
echo ""
echo "1. Configuring Google Places API..."
az webapp config appsettings set \
  --name $BACKEND_APP \
  --resource-group $RESOURCE_GROUP \
  --settings GooglePlaces__ApiKey="AIzaSyDw18sH8dG1Hj-39KbtB-OJbMNBq3Ajfkk"

# 2. Configure Stripe (you'll need to replace these with your actual Stripe keys)
echo ""
echo "2. Configuring Stripe..."
echo "IMPORTANT: Replace the placeholder values below with your actual Stripe keys"
echo "Get your keys from: https://dashboard.stripe.com/apikeys"
read -p "Enter your Stripe Secret Key (sk_test_...): " STRIPE_SECRET_KEY
read -p "Enter your Stripe Webhook Secret (whsec_...): " STRIPE_WEBHOOK_SECRET
read -p "Enter your Stripe Publishable Key (pk_test_...): " STRIPE_PUBLISHABLE_KEY

az webapp config appsettings set \
  --name $BACKEND_APP \
  --resource-group $RESOURCE_GROUP \
  --settings \
    Stripe__SecretKey="$STRIPE_SECRET_KEY" \
    Stripe__WebhookSecret="$STRIPE_WEBHOOK_SECRET"

# 3. Enable Marketplace Payments
echo ""
echo "3. Enabling Marketplace Payments..."
az webapp config appsettings set \
  --name $BACKEND_APP \
  --resource-group $RESOURCE_GROUP \
  --settings Marketplace__PaymentsEnabled="true"

# 4. Configure Video Compression (FFmpeg settings)
echo ""
echo "4. Configuring Video Compression..."
az webapp config appsettings set \
  --name $BACKEND_APP \
  --resource-group $RESOURCE_GROUP \
  --settings \
    VideoCompression__Enabled="true" \
    VideoCompression__DefaultQuality="Medium" \
    VideoCompression__MaxResolution="1920" \
    VideoCompression__AutoCompress="true"

# 5. Configure Frontend Environment Variables
echo ""
echo "5. Configuring Frontend..."
az staticwebapp appsettings set \
  --name $FRONTEND_APP \
  --setting-names \
    NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY="$STRIPE_PUBLISHABLE_KEY" \
    NEXT_PUBLIC_API_BASE_URL="https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/v1"

echo ""
echo "=========================================="
echo "Configuration Complete!"
echo "=========================================="
echo ""
echo "Next Steps:"
echo "1. Verify settings in Azure Portal"
echo "2. Restart the App Service: az webapp restart --name $BACKEND_APP --resource-group $RESOURCE_GROUP"
echo "3. Test the Google Places API endpoint: https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/places/nearby"
echo "4. Test Stripe Connect: https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/stripe-connect"
