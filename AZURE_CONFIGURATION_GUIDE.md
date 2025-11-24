# Azure Configuration Guide for GymHero/Taktiq

This guide walks you through configuring Google Places API, FFmpeg, and Stripe in Azure.

## Your Azure Resources

- **Resource Group**: `taktiqRecursos`
- **Backend API**: `taktiq-api`
- **Frontend**: `taktiq-web-frontend`
- **API URL**: `https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net`

---

## Part 1: Configure Google Places API ✅

### Local Configuration (Already Done)
✅ Google Places API key has been added to `appsettings.json`

### Azure Portal Configuration

1. **Navigate to Azure Portal**
   - Go to: https://portal.azure.com
   - Find `taktiq-api` in Resource Group `taktiqRecursos`

2. **Add Application Setting**
   - Click on **Configuration** in the left menu
   - Click **+ New application setting**
   - Add the following:
     - **Name**: `GooglePlaces__ApiKey`
     - **Value**: `AIzaSyDw18sH8dG1Hj-39KbtB-OJbMNBq3Ajfkk`
   - Click **OK**, then **Save**

3. **Restart the App Service**
   - Go back to **Overview**
   - Click **Restart**

### Test the API

```bash
# Test nearby gyms endpoint
curl "https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/places/nearby?latitude=-23.5505&longitude=-46.6333&radius=5000&type=gym"
```

**Expected Response**: List of nearby gyms with names, addresses, ratings, etc.

---

## Part 2: FFmpeg in Azure Container

### Current Status
✅ FFmpeg is **already configured** in your Dockerfile at `src/GymHero.Api/Dockerfile:28-31`

The Dockerfile includes:
```dockerfile
# Install FFmpeg for video thumbnail generation
RUN apt-get update && \
    apt-get install -y ffmpeg && \
    rm -rf /var/lib/apt/lists/*
```

### Deployment Options

#### Option A: Using Azure Portal (Easiest)

1. **Enable Container Deployment**
   - Go to `taktiq-api` in Azure Portal
   - Click **Deployment Center** in the left menu
   - Check if it's already configured for container deployment
   - If not, select **Container Registry** or **Docker Hub**

2. **Build and Deploy**

   Open PowerShell in your project directory:

   ```powershell
   # Build the Docker image
   docker build -f src/GymHero.Api/Dockerfile -t gymhero-api:latest .

   # Test FFmpeg is installed
   docker run --rm gymhero-api:latest ffmpeg -version
   ```

3. **Push to Container Registry**

   You have two options:

   **Option 1: Create Azure Container Registry (Recommended)**
   ```bash
   # Create ACR (choose a unique name)
   az acr create --resource-group taktiqRecursos --name taktiqacr --sku Basic

   # Login to ACR
   az acr login --name taktiqacr

   # Tag and push
   docker tag gymhero-api:latest taktiqacr.azurecr.io/gymhero-api:latest
   docker push taktiqacr.azurecr.io/gymhero-api:latest

   # Update App Service
   az webapp config container set \
       --name taktiq-api \
       --resource-group taktiqRecursos \
       --docker-custom-image-name taktiqacr.azurecr.io/gymhero-api:latest \
       --docker-registry-server-url https://taktiqacr.azurecr.io
   ```

   **Option 2: Use Docker Hub**
   ```bash
   # Login to Docker Hub
   docker login

   # Tag and push
   docker tag gymhero-api:latest yourusername/gymhero-api:latest
   docker push yourusername/gymhero-api:latest

   # Update in Azure Portal: Deployment Center > Settings > Image = yourusername/gymhero-api:latest
   ```

#### Option B: Using Deployment Script (Automated)

Run the provided script:
```bash
bash azure-deploy-with-ffmpeg.sh
```

Or use PowerShell:
```powershell
.\azure-deploy-with-ffmpeg.ps1
```

### Verify FFmpeg Works

After deployment, test video processing:

```bash
# Upload a video through your API
curl -X POST "https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/media/upload" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "file=@test-video.mp4" \
  -F "quality=Medium"
```

---

## Part 3: Configure Stripe

### Step 1: Get Stripe API Keys

1. **Go to Stripe Dashboard**
   - Visit: https://dashboard.stripe.com/apikeys
   - Sign in or create an account

2. **Get Your Keys**
   - **Publishable Key**: Starts with `pk_test_...` (for frontend)
   - **Secret Key**: Starts with `sk_test_...` (for backend)

3. **Create Webhook Endpoint**
   - Go to: https://dashboard.stripe.com/webhooks
   - Click **+ Add endpoint**
   - URL: `https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/stripe/webhook`
   - Select events:
     - `payment_intent.succeeded`
     - `payment_intent.payment_failed`
     - `account.updated`
   - Get the **Webhook Secret** (starts with `whsec_...`)

### Step 2: Configure Backend in Azure Portal

1. **Navigate to taktiq-api**
   - Go to **Configuration** > **Application settings**

2. **Add Stripe Settings**

   Click **+ New application setting** for each:

   | Name | Value |
   |------|-------|
   | `Stripe__SecretKey` | `sk_test_YOUR_SECRET_KEY` |
   | `Stripe__WebhookSecret` | `whsec_YOUR_WEBHOOK_SECRET` |
   | `Marketplace__PaymentsEnabled` | `true` |

3. **Click Save** and **Restart** the app

### Step 3: Configure Frontend

#### Option A: Azure Static Web App Configuration

1. **Navigate to taktiq-web-frontend**
   - Go to **Configuration** > **Application settings**

2. **Add Frontend Setting**
   - **Name**: `NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY`
   - **Value**: `pk_test_YOUR_PUBLISHABLE_KEY`
   - Click **Save**

#### Option B: Local Environment File

Update `frontend/apps/web/.env.local`:
```bash
NEXT_PUBLIC_API_BASE_URL=https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/v1
NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=pk_test_YOUR_PUBLISHABLE_KEY
```

### Step 4: Test Stripe Integration

1. **Test Connect Account Creation**
   ```bash
   curl -X POST "https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/stripe-connect/account" \
     -H "Authorization: Bearer YOUR_TOKEN" \
     -H "Content-Type: application/json"
   ```

2. **Test Payment Intent**
   ```bash
   curl -X POST "https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/payments/create-intent" \
     -H "Authorization: Bearer YOUR_TOKEN" \
     -H "Content-Type: application/json" \
     -d '{"workoutPlanId": "plan-id", "amount": 5000}'
   ```

---

## Quick Configuration Scripts

### Using Azure CLI

```bash
# Install Azure CLI first: https://aka.ms/installazurecli
# Then run:
bash azure-configure-settings.sh
```

### Using PowerShell

```powershell
# Install Az module first: Install-Module -Name Az -Scope CurrentUser
# Then run:
.\azure-configure-settings.ps1
```

### Manual Configuration via Azure Portal

All settings can be configured manually in Azure Portal:
1. Go to your App Service
2. Configuration > Application settings
3. Add each setting as documented above
4. Save and Restart

---

## Verification Checklist

After configuration, verify each service:

- [ ] **Google Places API**
  - [ ] Setting configured in Azure: `GooglePlaces__ApiKey`
  - [ ] Test endpoint returns gym data
  - [ ] No authentication errors in logs

- [ ] **FFmpeg Container**
  - [ ] Docker image built with FFmpeg
  - [ ] Container deployed to Azure
  - [ ] Test video upload and compression works
  - [ ] Check logs for FFmpeg version output

- [ ] **Stripe Integration**
  - [ ] Backend settings: `Stripe__SecretKey`, `Stripe__WebhookSecret`
  - [ ] Frontend setting: `NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY`
  - [ ] Marketplace enabled: `Marketplace__PaymentsEnabled=true`
  - [ ] Webhook endpoint configured in Stripe Dashboard
  - [ ] Test Connect account creation
  - [ ] Test payment intent creation

---

## Troubleshooting

### Google Places API Issues

**Error: "API key not valid"**
- Verify the key is correct in Azure settings
- Check if Places API is enabled in Google Cloud Console
- Restart the App Service after changing settings

### FFmpeg Issues

**Error: "ffmpeg: command not found"**
- Verify you're deploying the Docker container (not just code)
- Check Deployment Center in Azure Portal
- Rebuild and redeploy the container

### Stripe Issues

**Error: "No such customer"**
- Verify Secret Key is correct (starts with `sk_test_`)
- Check you're using the same Stripe account for all keys
- Verify webhook secret matches the endpoint

**Payments not working**
- Ensure `Marketplace__PaymentsEnabled` is set to `true`
- Check webhook endpoint is accessible: `/api/stripe/webhook`
- View webhook logs in Stripe Dashboard

---

## Support

If you encounter issues:
1. Check Azure App Service logs: **Monitoring** > **Log stream**
2. Check Application Insights for detailed errors
3. Verify all environment variables are set correctly
4. Restart the App Service after configuration changes

---

## Next Steps

Once all three services are configured:

1. **Test the complete flow:**
   - User signs up as trainer
   - Creates Stripe Connect account
   - Uploads workout video (FFmpeg processes it)
   - Student finds gym nearby (Google Places)
   - Student purchases workout plan (Stripe payment)

2. **Monitor usage:**
   - Google Places API quota
   - Stripe transaction fees
   - Azure storage for video files
   - Container resource usage

3. **Production readiness:**
   - Switch to Stripe live keys
   - Add custom domain
   - Configure SSL certificate
   - Set up backups and monitoring
