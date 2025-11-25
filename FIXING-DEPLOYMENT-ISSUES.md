# Fixing Azure Deployment Issues - Complete Guide

## Current Status

### ✅ What's Working
- Student account CAN log in on first try (when timing is right)
- Nearby gym feature is working properly
- App does start successfully (briefly)

### ❌ What's Broken
- **Double-startup problem STILL HAPPENING** - Oryx is creating a second startup, causing crashes
- **Latest code NOT deployed** - My fixes from commits d5d89ea and b38a53f are not running
- Personal Trainer login fails on first try (timing issue, not a code bug)
- Profile images showing 404 errors
- Notifications section errors for trainers

---

## Root Cause Analysis

### The Double-Startup Problem

Looking at your Azure logs:

1. **21:50:39** - App starts successfully ✓
   ```
   [INF] Now listening on: http://[::]:8080
   [INF] Application started
   ```

2. **21:51:42** - Oryx runs and creates startup script ✗
   ```
   Running oryx create-script... -userStartupCommand 'exec dotnet GymHero.Api.dll'
   ```

3. **21:53:25** - Second instance tries to start and CRASHES ✗
   ```
   [ERR] Failed to bind to address http://[::]:8080: address already in use
   ```

**Why Student works but Trainer doesn't:**
- Student logged in during the "stable window" (21:50-21:53) ✓
- Trainer tried during crash/restart cycle (after 21:53) ✗
- **NOT a role-specific bug** - just bad timing

### Why Latest Code Isn't Deployed

Evidence from logs:
- Shows: `Storing keys in '/root/ASP.NET/DataProtection-Keys'`
- Should show: `Storing keys in '/home/site/DataProtection-Keys'`
- Missing diagnostic logs: `=== Application Starting ===`
- Missing warmup logs: `Warming up database connection...`

**The GitHub Actions workflow is configuring Azure, but Azure might be resetting the config during deployment.**

---

## Step-by-Step Fix

### Option 1: Manual Configuration Fix (RECOMMENDED FIRST)

1. **Run the configuration fix script:**
   ```powershell
   .\fix-azure-double-startup.ps1
   ```

2. **Monitor Azure logs** (script will open browser):
   - Look for ONE startup only
   - Should NOT see Oryx running
   - Should NOT see "address already in use"

3. **Test both accounts:**
   - Try Student login
   - Try Personal Trainer login
   - Both should work on first attempt

### Option 2: Force New Deployment (If Option 1 Fails)

If Oryx still runs after manual configuration:

1. **Run the deployment script:**
   ```powershell
   .\force-new-deployment.ps1
   ```

2. **Wait 5-10 minutes** for GitHub Actions to complete

3. **Verify deployment** in Azure logs:
   ```
   ✓ === Application Starting ===
   ✓ Warming up database connection...
   ✓ Database connection successful
   ✓ DataProtection keys will be persisted to: /home/site/DataProtection-Keys
   ✓ Now listening on: http://[::]:8080  (ONLY ONCE!)
   ```

4. **Test login** with both account types

---

## What the Fixes Include

### Backend Fixes (Already Coded, Need to Deploy)

1. **Double-Startup Prevention** (`src/GymHero.Api/Program.cs` line 1)
   - Version comment tracking
   - Diagnostic logging added

2. **Database Connection Warmup** (`src/GymHero.Api/Program.cs` lines 213-233)
   - Warms up DB connection on startup
   - Prevents first-request 60-second timeout
   - Added connection logging

3. **DataProtection Keys Persistence** (`src/GymHero.Api/Program.cs` lines 46-66)
   - Changed from `/app` (ephemeral) to `/home/site` (persistent)
   - Prevents authentication issues across restarts

4. **Database Timeout Increases** (`src/GymHero.Infrastructure/DependencyInjection.cs` lines 60-65)
   - Command timeout: 60s → 120s
   - Retry count: 3 → 5
   - Handles Azure cold starts better

5. **Oryx Prevention** (`.deployment` + `.github/workflows/main_taktiq-api.yml`)
   - Disables automatic Oryx builds
   - Clears startup command
   - Configures `SCM_DO_BUILD_DURING_DEPLOYMENT=false`

### Frontend Fixes (Already Applied)

1. **Removed "Coming Soon" Banner** (`frontend/apps/web/src/app/(app)/gyms/page.tsx`)
   - Cleaned up unused imports
   - Feature is now production-ready

---

## Verification Checklist

After running the fixes, verify:

### Azure Portal
- [ ] Configuration → General Settings → Startup Command = **(empty)**
- [ ] Configuration → Application Settings → `SCM_DO_BUILD_DURING_DEPLOYMENT` = `false`
- [ ] Configuration → Application Settings → `WEBSITE_RUN_FROM_PACKAGE` = `0`

### Azure Logs (https://taktiq-api.scm.azurewebsites.net/api/logs/docker)
- [ ] See: `=== Application Starting ===`
- [ ] See: `Warming up database connection...`
- [ ] See: `Database connection successful`
- [ ] See: `DataProtection keys will be persisted to: /home/site/DataProtection-Keys`
- [ ] See: `Now listening on: http://[::]:8080` **ONLY ONCE**
- [ ] Do NOT see: `address already in use`
- [ ] Do NOT see: Oryx running `create-script`

### Application Testing
- [ ] Student account logs in on FIRST try
- [ ] Personal Trainer account logs in on FIRST try
- [ ] Nearby gym feature shows gyms (no "coming soon" banner)
- [ ] Health endpoint responds: https://api.taktiq.app/health
- [ ] No console errors for CORS (indicates API is stable)

---

## Still Having Issues?

### If Double-Startup Persists

**Nuclear Option** - Complete Azure Reset:

1. Stop the API in Azure Portal
2. Configuration → General Settings:
   - Change Platform from 64-bit → 32-bit
   - Save and wait 2 minutes
   - Change back to 64-bit
   - Save
3. This forces complete container recreation
4. Start the API

### If Login Still Timeouts

Check if it's the same issue or a new one:

```powershell
# Check current Azure logs
Start-Process "https://taktiq-api.scm.azurewebsites.net/api/logs/docker"
```

Look for:
- Is Oryx still running? → Configuration issue
- Is DB timing out? → Check connection string
- Are there new errors? → Share new logs for analysis

---

## Profile Photo 404 Errors

**TODO**: This needs separate investigation.

Likely causes:
1. Files not uploaded to blob storage
2. Incorrect URL generation
3. Missing files in deployment

Will address after fixing the critical double-startup issue.

---

## Summary

**The core issue is that Azure's configuration is not being applied correctly, causing Oryx to interfere with our deployment. The manual fix script should resolve this. If not, the forced deployment will ensure the latest code (with all fixes) actually runs in production.**

**The Personal Trainer timeout is NOT a code bug - it's the same double-startup crash affecting all users, just with worse timing.**
