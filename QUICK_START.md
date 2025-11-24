# Quick Start: Azure Configuration

## What's Been Done ✅

1. ✅ **Google Places API key configured locally** in `appsettings.json`
2. ✅ **FFmpeg already in Docker container** - Dockerfile includes FFmpeg installation
3. ✅ **Created deployment scripts** for Azure configuration

---

## What You Need to Do Next

### Step 1: Configure Google Places API in Azure Portal (5 minutes)

Since Azure CLI is not installed, use the Azure Portal:

1. Go to: https://portal.azure.com
2. Navigate to: **taktiqRecursos** > **taktiq-api** > **Configuration**
3. Click **+ New application setting**
4. Add:
   - **Name**: `GooglePlaces__ApiKey`
   - **Value**: `AIzaSyDw18sH8dG1Hj-39KbtB-OJbMNBq3Ajfkk`
5. Click **Save**, then **Restart** the app

**Test it:**
```
https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/places/nearby?latitude=-23.5505&longitude=-46.6333&radius=5000&type=gym
```

---

### Step 2: Deploy FFmpeg Container to Azure (15 minutes)

The Dockerfile already includes FFmpeg. You need to deploy the container:

**Option A: Deploy via Azure Portal Deployment Center**

1. Go to: **taktiq-api** > **Deployment Center**
2. Check current deployment method
3. If using containers, rebuild and push:

```powershell
# In PowerShell, from project root:
cd C:\Users\cwbcordeti\source\gymhero2

# Build the image
docker build -f src/GymHero.Api/Dockerfile -t gymhero-api:latest .

# Verify FFmpeg is installed
docker run --rm gymhero-api:latest ffmpeg -version
```

**Option B: Use the automated script**

I've created `azure-deploy-with-ffmpeg.sh` but since you're on Windows, you can:
- Install WSL and run the bash script, OR
- Deploy manually via Azure Portal as shown above

---

### Step 3: Configure Stripe (10 minutes)

1. **Get Stripe Keys:**
   - Go to: https://dashboard.stripe.com/apikeys
   - Copy your **Secret Key** (starts with `sk_test_`)
   - Copy your **Publishable Key** (starts with `pk_test_`)

2. **Create Webhook:**
   - Go to: https://dashboard.stripe.com/webhooks
   - Click **+ Add endpoint**
   - URL: `https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/stripe/webhook`
   - Select events: `payment_intent.succeeded`, `payment_intent.payment_failed`, `account.updated`
   - Copy the **Webhook Secret** (starts with `whsec_`)

3. **Configure in Azure Portal:**

   **Backend (taktiq-api):**
   - Go to: **taktiq-api** > **Configuration** > **Application settings**
   - Add these settings:

   | Name | Value |
   |------|-------|
   | `Stripe__SecretKey` | Your secret key |
   | `Stripe__WebhookSecret` | Your webhook secret |
   | `Marketplace__PaymentsEnabled` | `true` |

   - Click **Save** and **Restart**

   **Frontend (taktiq-web-frontend):**
   - Go to: **taktiq-web-frontend** > **Configuration**
   - Add:
     - **Name**: `NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY`
     - **Value**: Your publishable key
   - Click **Save**

---

## Summary: Manual Steps in Azure Portal

Since CLI isn't installed, here's what to configure in the Azure Portal:

### Backend App (taktiq-api) - Configuration Settings

Add these Application Settings:

```
GooglePlaces__ApiKey = AIzaSyDw18sH8dG1Hj-39KbtB-OJbMNBq3Ajfkk
Stripe__SecretKey = sk_test_YOUR_KEY_HERE
Stripe__WebhookSecret = whsec_YOUR_SECRET_HERE
Marketplace__PaymentsEnabled = true
VideoCompression__Enabled = true
VideoCompression__DefaultQuality = Medium
VideoCompression__MaxResolution = 1920
VideoCompression__AutoCompress = true
```

### Frontend App (taktiq-web-frontend) - Configuration

Add these Application Settings:

```
NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY = pk_test_YOUR_KEY_HERE
NEXT_PUBLIC_API_BASE_URL = https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/v1
```

---

## Testing

After configuration:

1. **Google Places:**
   ```
   GET https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/places/nearby?latitude=-23.5505&longitude=-46.6333&radius=5000&type=gym
   ```

2. **Stripe Connect:**
   ```
   POST https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/stripe-connect/account
   (requires authentication)
   ```

3. **Video Upload (FFmpeg):**
   ```
   POST https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/media/upload
   (requires authentication + video file)
   ```

---

## Need Help?

- **Detailed Guide:** See `AZURE_CONFIGURATION_GUIDE.md`
- **Automated Scripts:** `azure-configure-settings.ps1` (requires Az PowerShell module)
- **Docker Deployment:** `azure-deploy-with-ffmpeg.sh` (requires Azure CLI)

All files are in your project root: `C:\Users\cwbcordeti\source\gymhero2\`
