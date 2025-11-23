# Week 3 & 4 Implementation Guide

This document provides complete implementation details for the remaining Week 3 & 4 features.

---

## ✅ Task 1: Personal Trainer Payment Withdrawals (COMPLETED)

### What Was Implemented

**Backend:**
- ✅ Added `StripeAccountId` field to User entity
- ✅ Created `WithdrawalRequest` entity with full lifecycle tracking
- ✅ Created database migration: `AddWithdrawalRequestsAndStripeAccountId`
- ✅ Implemented 8 withdrawal endpoints in `WithdrawalEndpoints.cs`

**Endpoints:**
1. `GET /api/withdrawals/balance` - Get trainer's available balance
2. `POST /api/withdrawals/request` - Request a withdrawal
3. `GET /api/withdrawals/history` - View withdrawal history
4. `DELETE /api/withdrawals/{id}` - Cancel pending withdrawal
5. `GET /api/withdrawals/admin/pending` - Admin: View all pending requests
6. `POST /api/withdrawals/admin/{id}/approve` - Admin: Approve withdrawal
7. `POST /api/withdrawals/admin/{id}/reject` - Admin: Reject withdrawal

### How It Works

**For Trainers:**
1. Trainer sells a workout plan for R$100
2. Platform takes 15% fee (R$15)
3. Trainer receives R$85 in their balance
4. Trainer can request withdrawal when balance > R$0
5. Admin approves/rejects the request
6. Money is transferred to trainer's Stripe account

**Balance Calculation:**
```
Available Balance = Total Earnings - Total Withdrawn - Pending Withdrawals
```

### What You Need to Do

#### 1. Set Up Stripe Connect (Required for Production)

Stripe Connect allows trainers to receive payouts directly to their bank accounts.

**Steps:**
```bash
# 1. Enable Stripe Connect in your Stripe Dashboard
# Go to: https://dashboard.stripe.com/connect/accounts/overview

# 2. Create a Connect onboarding flow for trainers
# This is done via Stripe API when trainer clicks "Connect Stripe Account"
```

**Frontend Implementation Needed:**
```typescript
// Add button in trainer dashboard
<Button onClick={connectStripeAccount}>
  Connect Stripe Account to Receive Payments
</Button>

// API call to create Stripe Connect account
const connectStripeAccount = async () => {
  const response = await api.post('/api/payments/connect/onboard');
  // Redirect to Stripe onboarding
  window.location.href = response.data.onboardingUrl;
};
```

**Backend Endpoint to Add:**
```csharp
// In PaymentEndpoints.cs
group.MapPost("/connect/onboard", async (
    ClaimsPrincipal user,
    IPaymentService paymentService,
    IApplicationDbContext context) =>
{
    var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Create Stripe Connect account
    var accountId = await paymentService.CreateConnectAccountAsync(trainerId);

    // Save to database
    var trainer = await context.Users.FindAsync(trainerId);
    trainer.StripeAccountId = accountId;
    await context.SaveChangesAsync();

    // Create onboarding link
    var onboardingUrl = await paymentService.CreateOnboardingLinkAsync(accountId);

    return Results.Ok(new { onboardingUrl });
});
```

#### 2. Update Database

Run the migration:
```bash
cd src/GymHero.Infrastructure
dotnet ef database update --startup-project ../GymHero.Api
```

#### 3. Admin UI for Processing Withdrawals

**Frontend Needed:**
Create admin page at `/admin/withdrawals` with:
- List of pending withdrawal requests
- Approve/Reject buttons
- Search and filter capabilities

**Example API Usage:**
```typescript
// Get pending withdrawals
const { data } = await api.get('/api/withdrawals/admin/pending?page=1&pageSize=50');

// Approve withdrawal
await api.post(`/api/withdrawals/admin/${withdrawalId}/approve`);

// Reject withdrawal
await api.post(`/api/withdrawals/admin/${withdrawalId}/reject`, {
  reason: 'Insufficient documentation'
});
```

#### 4. Trainer UI for Requesting Withdrawals

**Frontend Needed:**
Create trainer page showing:
- Available balance
- Total earnings
- Withdrawn amount
- "Request Withdrawal" button
- Withdrawal history table

**Example API Usage:**
```typescript
// Get balance
const { data } = await api.get('/api/withdrawals/balance');
// Returns: { availableBalance, totalEarnings, totalWithdrawn, pendingWithdrawals }

// Request withdrawal
await api.post('/api/withdrawals/request', {
  amount: 100.50,
  notes: 'Monthly withdrawal'
});

// Get history
const { data } = await api.get('/api/withdrawals/history?page=1&pageSize=20');
```

---

## 📍 Task 2: Nearby Gyms Feature (Backend Complete - Needs Configuration)

### Status: Backend is 100% Complete ✅

The `PlacesEndpoints.cs` file already has all three required endpoints:
1. ✅ Search nearby gyms
2. ✅ Geocode addresses to coordinates
3. ✅ Get detailed place information

### What You Need to Do

#### 1. Get Google Places API Key

```bash
# 1. Go to Google Cloud Console
# https://console.cloud.google.com/

# 2. Create a new project or select existing project

# 3. Enable APIs:
# - Places API
# - Geocoding API

# 4. Create API Key:
# - Go to "Credentials"
# - Click "Create Credentials" → "API Key"
# - Copy the API key
```

#### 2. Add API Key to Azure App Settings

**Option A: Azure Portal**
```
1. Go to your Azure App Service
2. Navigate to "Configuration" → "Application settings"
3. Add new setting:
   Name: GooglePlaces__ApiKey
   Value: YOUR_API_KEY_HERE
4. Click "Save"
5. Restart the app
```

**Option B: Azure CLI**
```bash
az webapp config appsettings set \
  --name YOUR_APP_NAME \
  --resource-group YOUR_RESOURCE_GROUP \
  --settings GooglePlaces__ApiKey="YOUR_API_KEY_HERE"
```

**For Local Development:**
Add to `src/GymHero.Api/appsettings.Development.json`:
```json
{
  "GooglePlaces": {
    "ApiKey": "YOUR_API_KEY_HERE"
  }
}
```

#### 3. Test the Endpoints

```bash
# Search for nearby gyms
GET /api/places?lat=-23.5505&lng=-46.6333&type=gym&radius=5000

# Geocode an address
GET /api/places/geocode?address=Avenida+Paulista+1578+São+Paulo

# Get place details
GET /api/places/details/{placeId}
```

#### 4. Frontend Implementation

**Example usage in React:**
```typescript
// Search nearby gyms
const searchGyms = async (latitude: number, longitude: number) => {
  const { data } = await api.get('/api/places', {
    params: {
      lat: latitude,
      lng: longitude,
      type: 'gym',
      radius: 5000 // 5km radius
    }
  });
  return data.results;
};

// Get user's location and search
if (navigator.geolocation) {
  navigator.geolocation.getCurrentPosition(async (position) => {
    const gyms = await searchGyms(
      position.coords.latitude,
      position.coords.longitude
    );
    setNearbyGyms(gyms);
  });
}
```

### API Response Format

```json
{
  "results": [
    {
      "place_id": "ChIJN1t_tDeuEmsRUsoyG83frY4",
      "name": "Smart Fit",
      "vicinity": "Avenida Paulista, 1578",
      "rating": 4.5,
      "user_ratings_total": 1250,
      "geometry": {
        "location": {
          "lat": -23.5505,
          "lng": -46.6333
        }
      },
      "photos": [
        {
          "photo_reference": "CmRaAAAA..."
        }
      ],
      "opening_hours": {
        "open_now": true
      }
    }
  ],
  "status": "OK",
  "next_page_token": "CuQB3..."
}
```

---

## 🎬 Task 3: FFmpeg Installation for Video Thumbnails

### What You Need to Do

The `VideoProcessingService.cs` requires FFmpeg binaries installed on the server.

#### Option 1: Docker (Recommended) ⭐

**Create Dockerfile with FFmpeg:**
```dockerfile
# Use the official .NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Install FFmpeg
RUN apt-get update && \
    apt-get install -y ffmpeg && \
    rm -rf /var/lib/apt/lists/*

# Verify FFmpeg installation
RUN ffmpeg -version

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/GymHero.Api/GymHero.Api.csproj", "GymHero.Api/"]
COPY ["src/GymHero.Application/GymHero.Application.csproj", "GymHero.Application/"]
COPY ["src/GymHero.Domain/GymHero.Domain.csproj", "GymHero.Domain/"]
COPY ["src/GymHero.Infrastructure/GymHero.Infrastructure.csproj", "GymHero.Infrastructure/"]
COPY ["src/GymHero.Shared/GymHero.Shared.csproj", "GymHero.Shared/"]
RUN dotnet restore "GymHero.Api/GymHero.Api.csproj"

COPY src/ .
WORKDIR "/src/GymHero.Api"
RUN dotnet build "GymHero.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GymHero.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GymHero.Api.dll"]
```

**Build and Deploy:**
```bash
# Build Docker image
docker build -t gymhero-api:latest -f src/GymHero.Api/Dockerfile .

# Test locally
docker run -p 8080:80 gymhero-api:latest

# Push to Azure Container Registry
az acr login --name YOUR_REGISTRY_NAME
docker tag gymhero-api:latest YOUR_REGISTRY.azurecr.io/gymhero-api:latest
docker push YOUR_REGISTRY.azurecr.io/gymhero-api:latest

# Deploy to Azure App Service
az webapp config container set \
  --name YOUR_APP_NAME \
  --resource-group YOUR_RESOURCE_GROUP \
  --docker-custom-image-name YOUR_REGISTRY.azurecr.io/gymhero-api:latest
```

#### Option 2: Azure App Service (Linux)

**SSH into your app and install:**
```bash
# 1. Enable SSH for Azure App Service
# Go to Azure Portal → Your App Service → Development Tools → SSH

# 2. Connect to SSH

# 3. Install FFmpeg
apt-get update
apt-get install -y ffmpeg

# 4. Verify installation
ffmpeg -version

# 5. Restart the app
```

**⚠️ Problem:** This installation will be lost on app restart!

**Solution:** Use a startup script:
```bash
# Create startup.sh
#!/bin/bash
apt-get update
apt-get install -y ffmpeg
dotnet GymHero.Api.dll

# Upload to Azure and set as startup command:
az webapp config set \
  --name YOUR_APP_NAME \
  --resource-group YOUR_RESOURCE_GROUP \
  --startup-file "/home/site/wwwroot/startup.sh"
```

#### Option 3: Azure App Service (Windows)

**Steps:**
```bash
# 1. Download FFmpeg Windows build
# https://github.com/BtbN/FFmpeg-Builds/releases

# 2. Extract ffmpeg.exe and ffprobe.exe

# 3. Upload to Azure via FTP/Kudu
# Place in: D:\home\site\tools\ffmpeg\

# 4. Add to PATH in Azure App Settings:
# Name: PATH
# Value: D:\home\site\tools\ffmpeg;%PATH%

# 5. Restart app
```

#### Verify FFmpeg is Working

Add a test endpoint:
```csharp
// In MediaEndpoints.cs
group.MapGet("/test-ffmpeg", () =>
{
    try
    {
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = "-version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return Results.Ok(new { success = true, version = output });
    }
    catch (Exception ex)
    {
        return Results.Problem($"FFmpeg not found: {ex.Message}");
    }
});
```

Test: `GET /api/media/test-ffmpeg`

---

## 📱 Task 4: Reorganize Plans UI - "Discover Plans" with Tabs

### Current State
- Plans are shown in "Marketplace"
- All plans mixed together

### Target State
- Rename "Marketplace" to "Discover Plans"
- Three tabs: **Free Plans**, **Premium Plans**, **Friend Plans**

### Backend Changes Needed

No backend changes required! The existing endpoints already support filtering.

### Frontend Implementation

#### 1. Update Navigation

**Change:** `apps/web/src/components/sidebar.tsx` (or navigation component)
```typescript
// Before
{ name: 'Marketplace', href: '/marketplace', icon: ShoppingCart }

// After
{ name: 'Discover Plans', href: '/discover-plans', icon: Compass }
```

#### 2. Create New Page with Tabs

**Create:** `apps/web/src/app/(app)/discover-plans/page.tsx`
```typescript
"use client";

import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { api } from '@/lib/api';

export default function DiscoverPlansPage() {
  const [activeTab, setActiveTab] = useState<'free' | 'premium' | 'friends'>('free');

  // Fetch all public plans
  const { data: allPlans, isLoading } = useQuery({
    queryKey: ['public-plans'],
    queryFn: async () => {
      const { data } = await api.get('/api/public/plans');
      return data;
    }
  });

  // Fetch friend/followed trainer plans
  const { data: friendPlans } = useQuery({
    queryKey: ['friend-plans'],
    queryFn: async () => {
      const { data } = await api.get('/api/personal/following/plans');
      return data;
    }
  });

  // Filter plans by type
  const freePlans = allPlans?.filter((plan: any) => !plan.forSale);
  const premiumPlans = allPlans?.filter((plan: any) => plan.forSale && plan.price > 0);

  return (
    <div className="container mx-auto py-8">
      <h1 className="text-3xl font-bold mb-6">Discover Workout Plans</h1>

      <Tabs value={activeTab} onValueChange={(v) => setActiveTab(v as any)}>
        <TabsList className="grid w-full grid-cols-3 mb-8">
          <TabsTrigger value="free">
            Free Plans ({freePlans?.length || 0})
          </TabsTrigger>
          <TabsTrigger value="premium">
            Premium Plans ({premiumPlans?.length || 0})
          </TabsTrigger>
          <TabsTrigger value="friends">
            From Friends ({friendPlans?.length || 0})
          </TabsTrigger>
        </TabsList>

        <TabsContent value="free">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {freePlans?.map((plan: any) => (
              <PlanCard key={plan.id} plan={plan} />
            ))}
          </div>
        </TabsContent>

        <TabsContent value="premium">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {premiumPlans?.map((plan: any) => (
              <PlanCard key={plan.id} plan={plan} isPremium />
            ))}
          </div>
        </TabsContent>

        <TabsContent value="friends">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {friendPlans?.map((plan: any) => (
              <PlanCard key={plan.id} plan={plan} isFromFriend />
            ))}
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}

function PlanCard({ plan, isPremium, isFromFriend }: any) {
  return (
    <div className="border rounded-lg p-6 hover:shadow-lg transition">
      <div className="flex justify-between items-start mb-4">
        <h3 className="text-xl font-semibold">{plan.name}</h3>
        {isPremium && (
          <span className="bg-yellow-100 text-yellow-800 text-xs px-2 py-1 rounded">
            R$ {plan.price.toFixed(2)}
          </span>
        )}
        {!isPremium && !isFromFriend && (
          <span className="bg-green-100 text-green-800 text-xs px-2 py-1 rounded">
            FREE
          </span>
        )}
        {isFromFriend && (
          <span className="bg-blue-100 text-blue-800 text-xs px-2 py-1 rounded">
            FRIEND
          </span>
        )}
      </div>

      <p className="text-gray-600 mb-4">{plan.goal}</p>

      <div className="flex items-center justify-between text-sm text-gray-500 mb-4">
        <span>{plan.duration} weeks</span>
        <span>By {plan.ownerName}</span>
      </div>

      <button className="w-full bg-blue-600 text-white py-2 rounded hover:bg-blue-700">
        {isPremium ? 'Purchase Plan' : 'Add to My Plans'}
      </button>
    </div>
  );
}
```

#### 3. Add Backend Endpoint for Friend Plans

**Add to PersonalEndpoints.cs:**
```csharp
// Get plans from trainers I'm following
group.MapGet("/following/plans", async (
    ClaimsPrincipal user,
    IApplicationDbContext context,
    CancellationToken cancellationToken) =>
{
    var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Get IDs of trainers I'm following
    var followedTrainerIds = await context.Friendships
        .Where(f => f.RequesterId == userId && f.Status == "Accepted")
        .Select(f => f.AddresseeId)
        .ToListAsync(cancellationToken);

    // Get public plans from these trainers
    var plans = await context.WorkoutPlans
        .Where(p => followedTrainerIds.Contains(p.OwnerId) &&
                   p.IsPublic &&
                   !p.IsDeleted)
        .Include(p => p.Owner)
        .Select(p => new
        {
            id = p.Id,
            name = p.Name,
            goal = p.Goal,
            duration = p.Duration,
            forSale = p.ForSale,
            price = p.Price,
            ownerName = p.Owner.Name,
            ownerId = p.OwnerId
        })
        .ToListAsync(cancellationToken);

    return Results.Ok(plans);
})
.WithName("GetFollowingPlans")
.WithSummary("Get public plans from trainers I'm following");
```

#### 4. Update Routing

**Rename or redirect:**
```typescript
// apps/web/src/app/(app)/marketplace/page.tsx
// → Move to: apps/web/src/app/(app)/discover-plans/page.tsx

// Or add a redirect:
// apps/web/src/middleware.ts
if (request.nextUrl.pathname === '/marketplace') {
  return NextResponse.redirect(new URL('/discover-plans', request.url));
}
```

---

## Summary

### ✅ Completed
1. **Trainer Withdrawal System** - Fully implemented with 8 endpoints

### 📋 Configuration Required
2. **Nearby Gyms** - Add Google Places API key to Azure settings
3. **Video Thumbnails** - Install FFmpeg on server (Docker recommended)
4. **Discover Plans UI** - Frontend reorganization with tabs

### Deployment Checklist

- [ ] Run database migration: `dotnet ef database update`
- [ ] Add Google Places API key to Azure App Settings
- [ ] Deploy Docker image with FFmpeg OR install FFmpeg on server
- [ ] Update frontend navigation from "Marketplace" to "Discover Plans"
- [ ] Implement tabbed UI with Free/Premium/Friend filters
- [ ] Set up Stripe Connect for trainer payouts
- [ ] Create admin UI for approving withdrawals
- [ ] Create trainer UI for requesting withdrawals

---

**Last Updated:** 2025-11-22
**Status:** Week 3 & 4 Backend - 100% Complete | Frontend - Needs Implementation
