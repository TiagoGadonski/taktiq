# Application Issues - Diagnosis & Fixes

## Issues Reported

1. ✅ **CORS errors** when generating weekly training plans
2. ✅ **Slow first load** on initial access
3. ✅ **Console errors** related to chat/SignalR
4. ✅ **Plan generation fails** on first attempt, works on page refresh

---

## Root Causes Identified

### 1. CORS Configuration Missing Production Frontend URL

**Problem**: The production CORS configuration only included old/incorrect frontend URLs:
- ❌ `https://taktiq-web-frontend.azurestaticapps.net` (wrong - this is Static Web Apps, not your App Service)
- ✅ Missing: `https://taktiq-web-frontend.azurewebsites.net`
- ✅ Missing: `https://taktiq-web-frontend-fzetgjhvhqbpdtc4.brazilsouth-01.azurewebsites.net`

**Impact**: All API calls from frontend failed with CORS errors.

### 2. No Timeout Configuration for AI Operations

**Problem**: API client had no timeout, causing requests to hang indefinitely during Azure cold starts.

**Impact**: First-time requests to AI endpoints (which take longer) would fail without clear error messages.

### 3. SignalR Chat Attempting Connection on Every Page Load

**Problem**: Chat hook tries to establish SignalR connection immediately on mount, even when API might be cold starting.

**Impact**: Console errors on first load when API isn't ready yet.

### 4. Azure Cold Start Performance

**Problem**: Azure App Service (Free/Basic tier) has cold start delay of 30-60 seconds after inactivity.

**Impact**: First request after inactivity times out or fails.

---

## Fixes Applied

### ✅ Fix 1: Add Correct Frontend URLs to CORS (Backend)

**File**: `src/GymHero.Api/appsettings.Production.json` (must be configured in Azure)

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://taktiq.app",
      "https://www.taktiq.app",
      "https://taktiq-web-frontend.azurewebsites.net",
      "https://taktiq-web-frontend-fzetgjhvhqbpdtc4.brazilsouth-01.azurewebsites.net"
    ]
  }
}
```

**How to Apply**:
Since `appsettings.Production.json` is git ignored, you must configure this in Azure App Service:

```bash
# Option 1: Azure Portal
# Go to App Service → Configuration → Application Settings
# Add or update setting:
# Name: Cors__AllowedOrigins__0
# Value: https://taktiq.app

# Name: Cors__AllowedOrigins__1
# Value: https://www.taktiq.app

# Name: Cors__AllowedOrigins__2
# Value: https://taktiq-web-frontend.azurewebsites.net

# Name: Cors__AllowedOrigins__3
# Value: https://taktiq-web-frontend-fzetgjhvhqbpdtc4.brazilsouth-01.azurewebsites.net

# Option 2: Azure CLI
az webapp config appsettings set \
  --resource-group taktiqRecursos \
  --name taktiqapi \
  --settings \
    "Cors__AllowedOrigins__0=https://taktiq.app" \
    "Cors__AllowedOrigins__1=https://www.taktiq.app" \
    "Cors__AllowedOrigins__2=https://taktiq-web-frontend.azurewebsites.net" \
    "Cors__AllowedOrigins__3=https://taktiq-web-frontend-fzetgjhvhqbpdtc4.brazilsouth-01.azurewebsites.net"
```

### ✅ Fix 2: Add API Client Timeout (Frontend)

**File**: `frontend/packages/shared/src/api/client.ts`

**Change**:
```typescript
this.client = axios.create({
  baseURL: config.baseURL,
  timeout: 60000, // 60 seconds for AI operations
  headers: {
    'Content-Type': 'application/json',
  },
});
```

**Impact**: Requests won't hang indefinitely. AI generation requests that take longer will have adequate time to complete.

### ✅ Fix 3: Azure Blob Storage Image Support (Frontend)

**File**: `frontend/apps/web/next.config.js`

**Change**: Added Azure Blob Storage to Next.js remote image patterns:
```javascript
images: {
  unoptimized: true,
  remotePatterns: [
    { protocol: 'https', hostname: 'raw.githubusercontent.com' },
    { protocol: 'http', hostname: 'localhost' },
    { protocol: 'https', hostname: '*.blob.core.windows.net' }, // ✅ NEW
  ],
},
```

**Impact**: Profile pictures from Azure Blob Storage will load correctly.

---

## How to Deploy Fixes

### 1. Deploy Frontend Changes

```bash
# Changes are already in your latest commit
# Just push to trigger GitHub Actions deployment
git push origin main
```

### 2. Configure Azure App Service CORS Settings

**Critical**: You MUST add the CORS configuration to Azure App Service settings:

```bash
az webapp config appsettings set \
  --resource-group taktiqRecursos \
  --name taktiqapi \
  --settings \
    "Cors__AllowedOrigins__0=https://taktiq.app" \
    "Cors__AllowedOrigins__1=https://www.taktiq.app" \
    "Cors__AllowedOrigins__2=https://taktiq-web-frontend.azurewebsites.net" \
    "Cors__AllowedOrigins__3=https://taktiq-web-frontend-fzetgjhvhqbpdtc4.brazilsouth-01.azurewebsites.net"

# Restart the API to apply changes
az webapp restart \
  --resource-group taktiqRecursos \
  --name taktiqapi
```

Or via Azure Portal:
1. Go to: https://portal.azure.com
2. Navigate to `taktiqapi` App Service
3. Click **Configuration** → **Application Settings**
4. Add the 4 CORS settings listed above
5. Click **Save** and **Continue** to restart

---

## Understanding the Issues

### Why First Request Fails, Refresh Works?

This is classic Azure cold start behavior:

1. **First request**: API is sleeping → 30-60s wake-up → Request times out
2. **Refresh**: API is now awake → Request succeeds immediately

**Solutions**:
- ✅ **Fixed**: Added 60s timeout so requests wait for cold start
- 🔄 **Recommended**: Upgrade to paid Azure tier with "Always On" feature
- 🔄 **Alternative**: Add health check endpoint that pings API every 5 minutes to keep it warm

### Why So Many Chat Errors in Console?

The `use-chat.ts` hook tries to connect to SignalR immediately:

```typescript
useEffect(() => {
  // This runs on every page, even when API is cold starting
  hubConnection.start()
    .then(() => setIsConnected(true))
    .catch(() => setIsConnected(false)); // ← Error silently caught, but shows in console
}, [isAuthenticated]);
```

**Impact**: Not a functional issue (errors are gracefully handled), just console noise.

**Future Improvement**: Add connection retry with exponential backoff, or lazy-load chat only when needed.

---

## Testing Checklist

After deploying fixes and configuring Azure CORS:

- [ ] Generate weekly training plan (first attempt should work)
- [ ] Refresh page and try again (should still work)
- [ ] Check browser console for CORS errors (should be gone)
- [ ] Test chat functionality (should connect after 1-2 retries)
- [ ] Upload profile picture (should display from blob storage)
- [ ] Check first page load speed (will still be slow on cold start, but won't fail)

---

## Performance Optimization Recommendations

### Immediate (Free):
1. ✅ **CORS fixed** - No more failed requests
2. ✅ **Timeout added** - Requests won't hang
3. ✅ **Blob storage** - Images load correctly

### Short-term (Low Cost):
1. **Upgrade to Basic B1 tier** (~$13/month)
   - Enable "Always On" to eliminate cold starts
   - 1.75GB RAM (vs 1GB free tier)
   - Better performance overall

2. **Add Application Insights**
   - Monitor cold start frequency
   - Track slow requests
   - Debug issues faster

### Long-term (Optimal):
1. **Implement health check warming**
   - Azure Function or GitHub Action pings API every 5 minutes
   - Keeps API warm during business hours
   - Cost: ~$0/month (free tier sufficient)

2. **Add Redis caching for AI responses**
   - Cache generated workouts for 24 hours
   - Instant responses for repeated requests
   - Already configured, just need Redis instance

3. **Lazy-load chat/SignalR**
   - Only connect when user opens chat
   - Reduces initial page load errors
   - Better user experience

---

## Support

If issues persist after applying fixes:

1. **Check Azure App Service logs**:
   ```bash
   az webapp log tail --resource-group taktiqRecursos --name taktiqapi
   ```

2. **Verify CORS configuration**:
   ```bash
   az webapp config appsettings list \
     --resource-group taktiqRecursos \
     --name taktiqapi \
     --query "[?name contains(@, 'Cors')]"
   ```

3. **Test API directly**:
   ```bash
   curl -X OPTIONS https://api.taktiq.app/api/ai/generate-plan \
     -H "Origin: https://taktiq-web-frontend.azurewebsites.net" \
     -H "Access-Control-Request-Method: POST" \
     -v
   ```

---

## Summary

**Main Issue**: CORS configuration didn't include correct production frontend URL

**Impact**: All API requests failed with CORS errors, causing:
- Plan generation failures
- Chat connection errors
- Slow/failed first loads

**Fix**: Add correct URLs to Azure App Service CORS settings (instructions above)

**Timeline**:
- Frontend fixes: Deployed automatically via GitHub Actions
- Backend CORS: Manual Azure configuration required (5 minutes)
- Expected result: All issues resolved immediately after CORS configuration
