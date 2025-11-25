# Azure API Port Conflict Fix

## Problem
The API is crashing with error: `Failed to bind to address http://[::]:8080: address already in use`

This happens because Azure is starting the API twice, causing a port conflict.

## Root Cause
Duplicate or misconfigured startup commands are causing multiple instances to start simultaneously.

## Solution

### Option 1: Azure Portal (RECOMMENDED)

1. **Go to Azure Portal:**
   - Visit: https://portal.azure.com
   - Navigate to: **App Services** → **taktiq-api**

2. **Check Configuration:**
   - In left menu: Click **Configuration**
   - Go to **General settings** tab
   - Look at **Startup Command** field

3. **Fix Startup Command:**
   - **If it shows:** `dotnet GymHero.Api.dll` or `sh -c 'dotnet GymHero.Api.dll'`
   - **Change to:** (leave completely EMPTY)
   - **Reason:** Azure will auto-detect the DLL file
   - Click **Save** at top

4. **Restart API:**
   - In left menu: Click **Overview**
   - Click **Restart** button at top
   - Wait 2-3 minutes for restart

5. **Verify Fix:**
   - Check logs at: https://taktiq-api.scm.azurewebsites.net/api/logs/docker
   - You should see ONLY ONE instance starting:
     ```
     [INF] Now listening on: http://[::]:8080
     [INF] Application started
     ```
   - NO MORE "address already in use" errors

### Option 2: PowerShell Script

Run the provided script:
```powershell
.\fix-api-startup.ps1
```

**Note:** Requires Azure CLI installed. If not installed, follow Option 1 (Azure Portal).

### Option 3: Alternative Startup Command (If Option 1 doesn't work)

Sometimes Azure requires an explicit startup command. If the API still fails after Option 1:

1. Go back to: **Configuration** → **General settings**
2. Set **Startup Command** to:
   ```bash
   sh -c 'dotnet GymHero.Api.dll'
   ```
3. Save and Restart

This wraps the command in a shell, preventing Azure from calling it multiple times.

## How to Verify Success

1. **Check Logs:**
   - Go to: https://taktiq-api.scm.azurewebsites.net/api/logs/docker
   - Look for single startup message (not duplicate)
   - No "address already in use" errors

2. **Test API Health:**
   - Visit: https://api.taktiq.app/health
   - Should return: `{"status":"healthy","timestamp":"..."}`

3. **Test Frontend:**
   - Visit: https://taktiq.app
   - Try logging in
   - Should work on first try (no timeout errors)

## Why This Happens

Azure App Service on Linux uses **Oryx** build system which:
1. Detects .NET apps automatically
2. Creates startup scripts
3. Sometimes conflicts with custom startup commands

When you set a startup command like `dotnet GymHero.Api.dll`, Azure:
- Runs its auto-detected startup (first instance)
- Also runs your custom startup command (second instance)
- Both try to bind to port 8080
- Second one fails → crash loop

**Solution:** Let Azure handle it automatically (empty startup command).

## Additional Troubleshooting

### If API still crashes after fix:

1. **Check App Service Plan:**
   - Ensure you're not on Free tier (has limitations)
   - Recommended: B1 or higher

2. **Check Environment Variables:**
   - Configuration → Application settings
   - Verify database connection string is set
   - Verify JWT settings are configured

3. **Check Application Insights:**
   - Look for exceptions in the logs
   - Check for database connection errors

4. **Force Clean Restart:**
   ```powershell
   # Stop the app
   az webapp stop --name taktiq-api --resource-group taktiq-group

   # Wait 30 seconds
   Start-Sleep -Seconds 30

   # Start the app
   az webapp start --name taktiq-api --resource-group taktiq-group
   ```

## Prevention

To avoid this in the future:
1. Don't set custom startup commands unless absolutely necessary
2. Let Azure auto-detect .NET applications
3. Use web.config for advanced IIS/Kestrel configuration if needed

## Related Issues

This fix also resolves:
- Frontend timeout errors (`timeout of 60000ms exceeded`)
- CORS errors (API unavailable)
- "Works on second try" behavior (API restarting)
- Slow application performance (crash loops)
