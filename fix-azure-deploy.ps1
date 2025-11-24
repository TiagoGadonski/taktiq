# Fix Azure Deployment Issues
# This script fixes the port conflict and deploys the updated code

$ResourceGroup = "taktiqRecursos"
$BackendApp = "taktiq-api"
$FrontendApp = "taktiq-web-frontend"

Write-Host "=========================================="
Write-Host "Fixing Azure Deployment Issues"
Write-Host "=========================================="

# Step 1: Stop the backend app to release port 8080
Write-Host ""
Write-Host "Step 1: Stopping backend app to release port..."
az webapp stop --name $BackendApp --resource-group $ResourceGroup

Start-Sleep -Seconds 10

# Step 2: Configure CORS settings
Write-Host ""
Write-Host "Step 2: Configuring CORS for production domains..."
az webapp cors add --name $BackendApp --resource-group $ResourceGroup --allowed-origins "https://taktiq.app"
az webapp cors add --name $BackendApp --resource-group $ResourceGroup --allowed-origins "https://www.taktiq.app"
az webapp cors add --name $BackendApp --resource-group $ResourceGroup --allowed-origins "https://taktiq-web-frontend.azurestaticapps.net"

# Step 3: Configure Google Places API key
Write-Host ""
Write-Host "Step 3: Configuring Google Places API..."
az webapp config appsettings set `
  --name $BackendApp `
  --resource-group $ResourceGroup `
  --settings GooglePlaces__ApiKey="AIzaSyDw18sH8dG1Hj-39KbtB-OJbMNBq3Ajfkk"

# Step 4: Build and deploy backend
Write-Host ""
Write-Host "Step 4: Building backend..."
cd src/GymHero.Api
dotnet publish -c Release -o ../../publish

Write-Host ""
Write-Host "Step 5: Creating deployment package..."
cd ../../publish
Compress-Archive -Path * -DestinationPath ../backend-deploy.zip -Force

Write-Host ""
Write-Host "Step 6: Deploying to Azure..."
cd ..
az webapp deployment source config-zip `
  --resource-group $ResourceGroup `
  --name $BackendApp `
  --src backend-deploy.zip

# Step 7: Start the app
Write-Host ""
Write-Host "Step 7: Starting backend app..."
az webapp start --name $BackendApp --resource-group $ResourceGroup

# Step 8: Configure frontend environment
Write-Host ""
Write-Host "Step 8: Configuring frontend Static Web App..."
Write-Host "Please manually add this environment variable in Azure Portal:"
Write-Host "Navigate to: $FrontendApp > Configuration > Application settings"
Write-Host "Add: NEXT_PUBLIC_API_BASE_URL = https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api"

Write-Host ""
Write-Host "Step 9: Building and deploying frontend..."
cd frontend
pnpm install
pnpm --filter @gymhero/web run build

Write-Host ""
Write-Host "=========================================="
Write-Host "Deployment Complete!"
Write-Host "=========================================="
Write-Host ""
Write-Host "Next Steps:"
Write-Host "1. Check the logs: az webapp log tail --name $BackendApp --resource-group $ResourceGroup"
Write-Host "2. Test the API: https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/places?lat=-23.5505&lng=-46.6333&type=gym&radius=5000"
Write-Host "3. Test the frontend: https://taktiq.app"
