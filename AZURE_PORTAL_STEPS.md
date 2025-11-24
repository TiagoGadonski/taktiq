# Azure Portal Configuration Checklist

Use this checklist to configure everything directly in the Azure Portal.

---

## Prerequisites

- [ ] Access to Azure Portal: https://portal.azure.com
- [ ] Google Places API Key: `AIzaSyDw18sH8dG1Hj-39KbtB-OJbMNBq3Ajfkk` ✅ Ready
- [ ] Stripe account created

---

## Part 1: Google Places API (5 minutes)

### Configure Backend

- [ ] 1. Open Azure Portal and login
- [ ] 2. Navigate to **Resource Groups** > **taktiqRecursos**
- [ ] 3. Click on **taktiq-api**
- [ ] 4. In left menu, click **Configuration**
- [ ] 5. Click **+ New application setting**
- [ ] 6. Enter:
  - Name: `GooglePlaces__ApiKey`
  - Value: `AIzaSyDw18sH8dG1Hj-39KbtB-OJbMNBq3Ajfkk`
- [ ] 7. Click **OK**
- [ ] 8. Click **Save** at the top
- [ ] 9. Click **Continue** to confirm
- [ ] 10. Go to **Overview** tab
- [ ] 11. Click **Restart**
- [ ] 12. Click **Yes** to confirm restart

### Test Google Places API

- [ ] 13. Open browser or Postman
- [ ] 14. Visit: `https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/places/nearby?latitude=-23.5505&longitude=-46.6333&radius=5000&type=gym`
- [ ] 15. Verify you get a JSON response with gym data

✅ **Google Places API Configured!**

---

## Part 2: FFmpeg Container (15 minutes)

### Check Current Deployment

- [ ] 1. In **taktiq-api**, click **Deployment Center** in left menu
- [ ] 2. Check if you're using **Container Registry** or **Code**
- [ ] 3. Note the deployment source

### If Using Containers (Recommended):

#### Build Docker Image Locally

- [ ] 4. Open PowerShell
- [ ] 5. Navigate to project:
  ```powershell
  cd C:\Users\cwbcordeti\source\gymhero2
  ```
- [ ] 6. Build Docker image:
  ```powershell
  docker build -f src/GymHero.Api/Dockerfile -t gymhero-api:latest .
  ```
- [ ] 7. Verify FFmpeg is installed:
  ```powershell
  docker run --rm gymhero-api:latest ffmpeg -version
  ```
  - You should see FFmpeg version info

#### Deploy to Azure

**Option A: Using Azure Container Registry (Recommended)**

- [ ] 8. In Azure Portal, search for **Container registries**
- [ ] 9. Click **+ Create**
- [ ] 10. Select:
  - Resource group: **taktiqRecursos**
  - Registry name: Choose unique name (e.g., `taktiqacr`)
  - Location: Same as your app (Brazil South)
  - SKU: **Basic**
- [ ] 11. Click **Review + create**, then **Create**
- [ ] 12. Once created, go to the registry
- [ ] 13. Click **Access keys** in left menu
- [ ] 14. Enable **Admin user**
- [ ] 15. Copy the **Login server**, **Username**, and **Password**

- [ ] 16. In PowerShell, login to ACR:
  ```powershell
  docker login [YOUR_LOGIN_SERVER] -u [USERNAME] -p [PASSWORD]
  ```
- [ ] 17. Tag your image:
  ```powershell
  docker tag gymhero-api:latest [YOUR_LOGIN_SERVER]/gymhero-api:latest
  ```
- [ ] 18. Push to ACR:
  ```powershell
  docker push [YOUR_LOGIN_SERVER]/gymhero-api:latest
  ```

- [ ] 19. Back in Azure Portal, go to **taktiq-api**
- [ ] 20. Click **Deployment Center**
- [ ] 21. Click **Settings** tab
- [ ] 22. Configure:
  - Registry source: **Azure Container Registry**
  - Registry: Select your registry
  - Image: **gymhero-api**
  - Tag: **latest**
- [ ] 23. Click **Save**
- [ ] 24. Go to **Overview** and click **Restart**

**Option B: Using Docker Hub (Alternative)**

- [ ] 8. Create account at https://hub.docker.com
- [ ] 9. In PowerShell:
  ```powershell
  docker login
  docker tag gymhero-api:latest YOUR_DOCKERHUB_USERNAME/gymhero-api:latest
  docker push YOUR_DOCKERHUB_USERNAME/gymhero-api:latest
  ```
- [ ] 10. In Azure Portal, go to **taktiq-api** > **Deployment Center**
- [ ] 11. Configure Docker Hub as source
- [ ] 12. Enter your image: `YOUR_DOCKERHUB_USERNAME/gymhero-api:latest`
- [ ] 13. Click **Save** and **Restart**

### If Using Code Deployment (Less Ideal):

Note: FFmpeg won't be available with standard code deployment. Container deployment is strongly recommended.

✅ **FFmpeg Container Deployed!**

---

## Part 3: Stripe Configuration (10 minutes)

### Get Stripe Keys

- [ ] 1. Go to https://dashboard.stripe.com
- [ ] 2. Sign up or log in
- [ ] 3. Click **Developers** in top menu
- [ ] 4. Click **API keys**
- [ ] 5. Copy the following keys:
  - **Publishable key** (starts with `pk_test_`)
  - **Secret key** (starts with `sk_test_`)
- [ ] 6. Keep these safe - you'll need them next

### Create Webhook

- [ ] 7. In Stripe Dashboard, click **Webhooks**
- [ ] 8. Click **+ Add endpoint**
- [ ] 9. Enter:
  - Endpoint URL: `https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/stripe/webhook`
- [ ] 10. Click **Select events**
- [ ] 11. Select these events:
  - [ ] `payment_intent.succeeded`
  - [ ] `payment_intent.payment_failed`
  - [ ] `account.updated`
- [ ] 12. Click **Add events**
- [ ] 13. Click **Add endpoint**
- [ ] 14. Click on the newly created endpoint
- [ ] 15. Click **Reveal** next to **Signing secret**
- [ ] 16. Copy the webhook secret (starts with `whsec_`)

### Configure Backend in Azure

- [ ] 17. Go to Azure Portal
- [ ] 18. Navigate to **taktiq-api** > **Configuration**
- [ ] 19. Add these application settings (click **+ New application setting** for each):

**Setting 1:**
- [ ] Name: `Stripe__SecretKey`
- [ ] Value: `[Your sk_test_... key]`
- [ ] Click **OK**

**Setting 2:**
- [ ] Name: `Stripe__WebhookSecret`
- [ ] Value: `[Your whsec_... key]`
- [ ] Click **OK**

**Setting 3:**
- [ ] Name: `Marketplace__PaymentsEnabled`
- [ ] Value: `true`
- [ ] Click **OK**

- [ ] 20. Click **Save** at the top
- [ ] 21. Click **Continue** to confirm
- [ ] 22. Go to **Overview** and click **Restart**

### Configure Frontend in Azure

- [ ] 23. Navigate to **taktiq-web-frontend**
- [ ] 24. Click **Configuration** in left menu

**Note:** If it's a Static Web App, look for "Application settings" or "Environment variables"

- [ ] 25. Add application setting:
  - Name: `NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY`
  - Value: `[Your pk_test_... key]`
- [ ] 26. Add another setting:
  - Name: `NEXT_PUBLIC_API_BASE_URL`
  - Value: `https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/v1`
- [ ] 27. Click **Save**

### Update Local Frontend (Optional - for local development)

- [ ] 28. Open: `C:\Users\cwbcordeti\source\gymhero2\frontend\apps\web\.env.local`
- [ ] 29. Add or update:
  ```
  NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=pk_test_YOUR_KEY
  NEXT_PUBLIC_API_BASE_URL=https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/v1
  ```

✅ **Stripe Configured!**

---

## Part 4: Testing Everything (5 minutes)

### Test Google Places

- [ ] 1. Open Postman or browser
- [ ] 2. Test nearby gyms:
  ```
  GET https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/places/nearby?latitude=-23.5505&longitude=-46.6333&radius=5000&type=gym
  ```
- [ ] 3. Verify you get a list of gyms with names, addresses, ratings

### Test Stripe (Requires Auth Token)

- [ ] 4. First, create a test user or login to get auth token
- [ ] 5. Test Stripe Connect account creation:
  ```
  POST https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/stripe-connect/account
  Headers: Authorization: Bearer YOUR_TOKEN
  ```
- [ ] 6. Verify you get a Stripe account ID and onboarding URL

### Test FFmpeg (Requires Auth Token)

- [ ] 7. Test video upload:
  ```
  POST https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/media/upload
  Headers: Authorization: Bearer YOUR_TOKEN
  Body: multipart/form-data with video file
  ```
- [ ] 8. Verify video is uploaded and compressed

### Check Logs

- [ ] 9. In Azure Portal, go to **taktiq-api**
- [ ] 10. Click **Log stream** in left menu
- [ ] 11. Watch for any errors
- [ ] 12. Look for:
  - Google Places API calls
  - Stripe webhook events
  - FFmpeg processing logs

✅ **Everything Tested!**

---

## Final Configuration Summary

After completing all steps, your Azure configuration should have:

### Backend (taktiq-api) Application Settings:

```
GooglePlaces__ApiKey = AIzaSyDw18sH8dG1Hj-39KbtB-OJbMNBq3Ajfkk
Stripe__SecretKey = sk_test_[YOUR_KEY]
Stripe__WebhookSecret = whsec_[YOUR_SECRET]
Marketplace__PaymentsEnabled = true
VideoCompression__Enabled = true
VideoCompression__DefaultQuality = Medium
VideoCompression__MaxResolution = 1920
VideoCompression__AutoCompress = true
```

### Frontend (taktiq-web-frontend) Application Settings:

```
NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY = pk_test_[YOUR_KEY]
NEXT_PUBLIC_API_BASE_URL = https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/v1
```

### Container Deployment:

- [ ] Docker image includes FFmpeg
- [ ] Deployed to Azure Container Registry or Docker Hub
- [ ] App Service configured to use container image

---

## Troubleshooting

### Google Places not working?

- [ ] Check the API key in Configuration > Application settings
- [ ] Verify Places API is enabled in Google Cloud Console
- [ ] Check Log stream for authentication errors
- [ ] Restart the App Service

### FFmpeg errors?

- [ ] Verify you're using container deployment (not code deployment)
- [ ] Check Dockerfile includes FFmpeg installation (it does!)
- [ ] Verify container image was built and pushed correctly
- [ ] Check Log stream for FFmpeg command errors

### Stripe not working?

- [ ] Verify all three Stripe settings are added
- [ ] Check keys are correct (sk_test_ and whsec_)
- [ ] Verify Marketplace__PaymentsEnabled is set to "true" (not "True")
- [ ] Check webhook endpoint is accessible
- [ ] View webhook delivery logs in Stripe Dashboard

### Can't access configuration?

- [ ] Verify you have Contributor or Owner role on the resource group
- [ ] Try refreshing the Azure Portal
- [ ] Check if you're in the correct subscription

---

## Need More Help?

- **Detailed guide:** `AZURE_CONFIGURATION_GUIDE.md`
- **Quick start:** `QUICK_START.md`
- **Automated scripts:** `azure-configure-settings.ps1`

All configuration is done! 🎉
