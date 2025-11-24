# Azure Fix Guide - Resolving Current Issues

## Issues Found

1. ❌ **404 Error on gym locations** - Frontend calling wrong URL
2. ❌ **CORS Error** - Backend not allowing frontend domain
3. ❌ **Port 8080 conflict** - App already running, can't restart

---

## Quick Fix (Azure Portal - Recommended)

### Step 1: Stop and Restart the Backend (Fix Port Conflict)

1. Go to: https://portal.azure.com
2. Navigate to: **taktiqRecursos** > **taktiq-api**
3. Click **Stop** at the top
4. Wait 30 seconds
5. Click **Start**

This will kill the zombie process occupying port 8080.

---

### Step 2: Configure CORS (Fix Login Error)

1. In **taktiq-api**, go to **CORS** in the left menu
2. Add these allowed origins:
   - `https://taktiq.app`
   - `https://www.taktiq.app`
   - `https://taktiq-web-frontend.azurestaticapps.net`
3. Check **Enable Access-Control-Allow-Credentials**
4. Click **Save**

---

### Step 3: Configure Google Places API (Already done locally)

✅ Already configured in code. After you redeploy, add in Azure Portal:

1. Go to **taktiq-api** > **Configuration**
2. Add application setting:
   - Name: `GooglePlaces__ApiKey`
   - Value: `AIzaSyDw18sH8dG1Hj-39KbtB-OJbMNBq3Ajfkk`
3. Click **Save**

---

### Step 4: Deploy Updated Code

#### Option A: Using Azure Portal Deployment Center

1. Go to **taktiq-api** > **Deployment Center**
2. Check your deployment source (GitHub, Local Git, etc.)
3. Trigger a new deployment or sync from your repository

#### Option B: Using Command Line (if Azure CLI installed)

```powershell
# Build the backend
cd C:\Users\cwbcordeti\source\gymhero2\src\GymHero.Api
dotnet publish -c Release -o ../../publish

# Create deployment package
cd ../../publish
tar -czf ../backend-deploy.zip *

# Deploy (requires Azure CLI)
cd ..
az webapp deployment source config-zip `
  --resource-group taktiqRecursos `
  --name taktiq-api `
  --src backend-deploy.zip
```

#### Option C: Manual Deployment via FTP

1. Go to **taktiq-api** > **Deployment Center** > **FTPS credentials**
2. Copy FTP endpoint, username, and password
3. Connect via FTP client (FileZilla, WinSCP)
4. Upload the published files to `/site/wwwroot`

---

### Step 5: Configure Frontend Environment Variable

1. Go to: **taktiq-web-frontend** (Static Web App or App Service)
2. Go to **Configuration** > **Application settings**
3. Add or update:
   - Name: `NEXT_PUBLIC_API_BASE_URL`
   - Value: `https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api`
4. Click **Save**

---

### Step 6: Rebuild and Deploy Frontend

Since the frontend code has been updated, you need to redeploy it:

#### If using GitHub Actions / Azure Static Web Apps:

1. Commit and push the changes:
   ```bash
   git add .
   git commit -m "fix: Update API base URL and gyms endpoint"
   git push
   ```

2. Wait for GitHub Actions to complete the deployment

#### If deploying manually:

```powershell
cd C:\Users\cwbcordeti\source\gymhero2\frontend
pnpm install
pnpm --filter @gymhero/web run build

# Upload the .next folder or standalone output to Azure
```

---

## Testing After Deployment

### Test 1: Backend API (Google Places)

Open in browser or use curl:
```
https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api/places?lat=-23.5505&lng=-46.6333&type=gym&radius=5000
```

**Expected**: JSON response with gym data

**If 404**: Backend hasn't restarted yet, wait 2-3 minutes

**If 503**: Google Places API key not configured properly

---

### Test 2: CORS (Login)

1. Go to: https://taktiq.app/login
2. Open browser console (F12)
3. Try to login
4. Check for CORS errors

**If still getting CORS error**:
- Verify CORS settings in Azure Portal include `https://taktiq.app`
- Check that backend has restarted after CORS configuration

---

### Test 3: Gyms Page

1. Go to: https://taktiq.app/gyms (or wherever the gyms page is)
2. Allow location access
3. Check if gyms load

**If 404**: Frontend environment variable not set or not redeployed

**If CORS error**: See Test 2

---

## Alternative: Quick Restart Script

If you have Azure CLI installed, run this PowerShell script:

```powershell
# Quick fix script
$rg = "taktiqRecursos"
$app = "taktiq-api"

# Stop the app
az webapp stop --name $app --resource-group $rg
Start-Sleep -Seconds 10

# Configure CORS
az webapp cors add --name $app --resource-group $rg --allowed-origins "https://taktiq.app" "https://www.taktiq.app"

# Configure Google Places
az webapp config appsettings set --name $app --resource-group $rg --settings GooglePlaces__ApiKey="AIzaSyDw18sH8dG1Hj-39KbtB-OJbMNBq3Ajfkk"

# Start the app
az webapp start --name $app --resource-group $rg

Write-Host "Backend restarted! Wait 2-3 minutes for the app to fully start."
```

---

## Monitoring

### Check Logs in Real-Time

**Option 1: Azure Portal**
1. Go to **taktiq-api**
2. Click **Log stream** in left menu
3. Watch for startup messages

**Option 2: Azure CLI**
```powershell
az webapp log tail --name taktiq-api --resource-group taktiqRecursos
```

---

## What Changed in the Code

1. ✅ **Fixed gyms page** (`frontend/apps/web/src/app/(app)/gyms/page.tsx`)
   - Now uses `NEXT_PUBLIC_API_BASE_URL` environment variable
   - Correctly points to backend API

2. ✅ **Updated production environment** (`frontend/apps/web/.env.production`)
   - Set `NEXT_PUBLIC_API_BASE_URL` to Azure backend URL

3. ✅ **Updated CORS configuration** (`src/GymHero.Api/appsettings.Production.json`)
   - Added production frontend domains

4. ✅ **Google Places API key** (`src/GymHero.Api/appsettings.json`)
   - Already configured locally

---

## Summary of Required Azure Portal Changes

| App Service | Setting | Value |
|-------------|---------|-------|
| **taktiq-api** | CORS Origins | `https://taktiq.app`, `https://www.taktiq.app` |
| **taktiq-api** | GooglePlaces__ApiKey | `AIzaSyDw18sH8dG1Hj-39KbtB-OJbMNBq3Ajfkk` |
| **taktiq-web-frontend** | NEXT_PUBLIC_API_BASE_URL | `https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api` |

---

## If Still Having Issues

### Backend won't start:
- Check Application Insights for errors
- Verify database connection string is correct
- Check if all required environment variables are set

### CORS still failing:
- Make sure both domains (`https://taktiq.app` AND `https://www.taktiq.app`) are in CORS
- Restart the backend after changing CORS
- Clear browser cache

### Gyms page still 404:
- Verify frontend environment variable is set
- Check that frontend was redeployed after code changes
- Test the backend API directly (see Test 1 above)

---

## Need Help?

1. Check Azure logs: **taktiq-api** > **Log stream**
2. Check Application Insights for detailed errors
3. Verify all environment variables are set correctly
4. Make sure both backend and frontend are redeployed

---

## Deployment Checklist

- [ ] Stop backend app in Azure Portal
- [ ] Configure CORS for `https://taktiq.app`
- [ ] Add Google Places API key to backend settings
- [ ] Start backend app
- [ ] Wait 2-3 minutes for app to fully start
- [ ] Test backend API directly
- [ ] Configure frontend environment variable
- [ ] Commit and push frontend changes (triggers deployment)
- [ ] Test frontend gyms page
- [ ] Test login (CORS)
- [ ] Monitor logs for any errors
