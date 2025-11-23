# Azure Configuration Steps

This document outlines the remaining configuration steps needed to complete the Week 3 & 4 features.

## 1. Google Places API Configuration

The nearby gyms feature requires a Google Places API key.

### Get Your Google Places API Key

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the **Places API**:
   - Go to "APIs & Services" > "Library"
   - Search for "Places API"
   - Click "Enable"
4. Create credentials:
   - Go to "APIs & Services" > "Credentials"
   - Click "Create Credentials" > "API Key"
   - Copy the API key
5. (Optional but recommended) Restrict the API key:
   - Click on the API key you just created
   - Under "API restrictions", select "Restrict key"
   - Choose "Places API"
   - Under "Application restrictions", you can restrict by HTTP referrer or IP address

### Add to Azure App Settings

#### Option 1: Using Azure Portal
1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to your App Service: `gymhero`
3. Go to **Configuration** > **Application settings**
4. Click **+ New application setting**
5. Add the following setting:
   - **Name**: `GooglePlaces__ApiKey`
   - **Value**: `your-google-places-api-key-here`
6. Click **OK** then **Save**
7. Restart the app service

#### Option 2: Using Azure CLI
```bash
az webapp config appsettings set \
  --name gymhero \
  --resource-group GymHero \
  --settings GooglePlaces__ApiKey="your-google-places-api-key-here"
```

## 2. Stripe Connect Configuration

The trainer withdrawal system requires Stripe Connect to be configured.

### Set Up Stripe Connect

1. Go to [Stripe Dashboard](https://dashboard.stripe.com/)
2. Navigate to **Connect** > **Settings**
3. Fill in your platform profile:
   - Business name
   - Support email
   - Brand icon/logo
4. Configure your Connect settings:
   - Choose "Express" or "Standard" accounts (Express recommended for simpler setup)
   - Set up OAuth settings if needed

### Get Your Stripe Keys

1. Go to **Developers** > **API keys**
2. Copy your:
   - **Publishable key** (starts with `pk_`)
   - **Secret key** (starts with `sk_`)
3. For Connect-specific settings:
   - **Client ID** (found in Connect settings)

### Add to Azure App Settings

Add these settings to your Azure App Service:

```bash
# Using Azure CLI
az webapp config appsettings set \
  --name gymhero \
  --resource-group GymHero \
  --settings \
    Stripe__SecretKey="your-stripe-secret-key" \
    Stripe__PublishableKey="your-stripe-publishable-key" \
    Stripe__ConnectClientId="your-stripe-connect-client-id"
```

Or via Azure Portal:
- `Stripe__SecretKey` = your secret key
- `Stripe__PublishableKey` = your publishable key
- `Stripe__ConnectClientId` = your Connect client ID

### Implement Stripe Connect Onboarding (Frontend)

You'll need to add a Stripe Connect onboarding flow for trainers. Here's a basic implementation:

**Create a new page: `frontend/apps/web/src/app/(app)/stripe-connect/page.tsx`**

```typescript
"use client";

import { useState } from "react";
import { api } from "@/lib/api";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { useToast } from "@/components/ui/use-toast";

export default function StripeConnectPage() {
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);

  const handleConnectStripe = async () => {
    setLoading(true);
    try {
      // This endpoint should create a Stripe Connect account and return an onboarding link
      const { data } = await api.post("/api/stripe/connect/create-account");

      // Redirect to Stripe onboarding
      window.location.href = data.url;
    } catch (error: any) {
      toast({
        title: "Error",
        description: error.response?.data?.message || "Failed to connect Stripe account",
        variant: "destructive",
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container mx-auto py-8">
      <Card>
        <CardHeader>
          <CardTitle>Connect Your Stripe Account</CardTitle>
          <CardDescription>
            Connect your Stripe account to receive payments from workout plan sales
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Button onClick={handleConnectStripe} disabled={loading}>
            {loading ? "Connecting..." : "Connect with Stripe"}
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
```

**Add the backend endpoint in `src/GymHero.Api/Endpoints/StripeConnectEndpoints.cs`** (you'll need to create this):

```csharp
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Security.Claims;

namespace GymHero.Api.Endpoints;

public static class StripeConnectEndpoints
{
    public static void MapStripeConnectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stripe/connect")
            .RequireAuthorization()
            .WithTags("Stripe Connect");

        group.MapPost("/create-account", CreateConnectAccount);
        group.MapGet("/refresh-url", RefreshOnboardingUrl);
    }

    private static async Task<IResult> CreateConnectAccount(
        ClaimsPrincipal user,
        IApplicationDbContext context,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var dbUser = await context.Users.FindAsync(new object[] { userId }, cancellationToken);

        if (dbUser == null) return Results.NotFound();

        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];

        // Create or retrieve Stripe Connect account
        string accountId;

        if (!string.IsNullOrEmpty(dbUser.StripeAccountId))
        {
            accountId = dbUser.StripeAccountId;
        }
        else
        {
            var accountOptions = new AccountCreateOptions
            {
                Type = "express",
                Email = dbUser.Email,
                Capabilities = new AccountCapabilitiesOptions
                {
                    Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
                },
            };

            var accountService = new AccountService();
            var account = await accountService.CreateAsync(accountOptions);

            dbUser.StripeAccountId = account.Id;
            await context.SaveChangesAsync(cancellationToken);

            accountId = account.Id;
        }

        // Create account link for onboarding
        var accountLinkOptions = new AccountLinkCreateOptions
        {
            Account = accountId,
            RefreshUrl = $"{configuration["AppUrl"]}/stripe-connect?refresh=true",
            ReturnUrl = $"{configuration["AppUrl"]}/earnings",
            Type = "account_onboarding",
        };

        var accountLinkService = new AccountLinkService();
        var accountLink = await accountLinkService.CreateAsync(accountLinkOptions);

        return Results.Ok(new { url = accountLink.Url });
    }

    private static async Task<IResult> RefreshOnboardingUrl(
        ClaimsPrincipal user,
        IApplicationDbContext context,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var dbUser = await context.Users.FindAsync(new object[] { userId }, cancellationToken);

        if (dbUser == null || string.IsNullOrEmpty(dbUser.StripeAccountId))
            return Results.BadRequest("No Stripe account connected");

        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];

        var accountLinkOptions = new AccountLinkCreateOptions
        {
            Account = dbUser.StripeAccountId,
            RefreshUrl = $"{configuration["AppUrl"]}/stripe-connect?refresh=true",
            ReturnUrl = $"{configuration["AppUrl"]}/earnings",
            Type = "account_onboarding",
        };

        var accountLinkService = new AccountLinkService();
        var accountLink = await accountLinkService.CreateAsync(accountLinkOptions);

        return Results.Ok(new { url = accountLink.Url });
    }
}
```

Don't forget to register the endpoints in `Program.cs`:
```csharp
app.MapStripeConnectEndpoints();
```

## 3. FFmpeg Installation

FFmpeg is required for generating video thumbnails from workout videos.

### Option 1: Docker (Recommended)

If you're using Docker, add FFmpeg to your Dockerfile:

**Update `src/GymHero.Api/Dockerfile`:**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install FFmpeg
RUN apt-get update && \
    apt-get install -y ffmpeg && \
    rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# ... rest of your Dockerfile
```

### Option 2: Azure App Service (Linux)

If running on Azure App Service with Linux:

1. Go to Azure Portal
2. Open your App Service
3. Go to **SSH** or **Advanced Tools (Kudu)** > **SSH**
4. Run:
```bash
apt-get update
apt-get install -y ffmpeg
```

**Note**: This installation is temporary and will be lost on restart. For persistent installation, use a startup script:

1. Create a file `startup.sh`:
```bash
#!/bin/bash
apt-get update
apt-get install -y ffmpeg
```

2. Add to Azure App Service Configuration:
   - **Name**: `STARTUP_COMMAND`
   - **Value**: `/home/startup.sh`

### Option 3: Azure App Service (Windows)

If running on Windows App Service:

1. Download FFmpeg from https://www.gyan.dev/ffmpeg/builds/
2. Extract to `D:\home\site\tools\ffmpeg`
3. Add to App Settings:
   - **Name**: `FFmpegPath`
   - **Value**: `D:\home\site\tools\ffmpeg\bin\ffmpeg.exe`

Or install via Chocolatey in a startup script.

### Verify Installation

SSH into your container/server and run:
```bash
ffmpeg -version
```

You should see FFmpeg version information.

## 4. Add AppUrl Configuration

Add your app's public URL to Azure App Settings (needed for Stripe Connect redirects):

```bash
az webapp config appsettings set \
  --name gymhero \
  --resource-group GymHero \
  --settings AppUrl="https://gymhero.azurewebsites.net"
```

Or via Azure Portal:
- **Name**: `AppUrl`
- **Value**: `https://gymhero.azurewebsites.net`

## Testing Checklist

After completing the configuration:

- [ ] Google Places API key added to Azure
- [ ] Nearby gyms feature works (search for gyms)
- [ ] Stripe Connect configured
- [ ] Trainers can connect their Stripe account
- [ ] Trainers can request withdrawals
- [ ] Admins can approve/reject withdrawals
- [ ] FFmpeg installed on server
- [ ] Video thumbnail generation works
- [ ] Navigation updated with new links
- [ ] "Discover Plans" page accessible
- [ ] All three plan tabs work (Free, Premium, Friends)

## Troubleshooting

### Google Places API Not Working
- Check that the API key is correctly added in Azure App Settings
- Verify the Places API is enabled in Google Cloud Console
- Check API key restrictions aren't blocking your requests
- Look at Application Insights logs for error messages

### Stripe Connect Issues
- Ensure Stripe keys are correct (test keys start with `pk_test_` and `sk_test_`)
- Check that Connect is enabled in your Stripe account
- Verify redirect URLs match your AppUrl setting
- Check Stripe Dashboard logs for webhook/API errors

### FFmpeg Not Found
- SSH into the container and verify FFmpeg is installed: `which ffmpeg`
- Check the FFmpeg path in your code matches the installation location
- Look at application logs for FFmpeg-related errors

## Need Help?

If you encounter issues:
1. Check Application Insights logs in Azure Portal
2. Review Stripe Dashboard for payment/connect errors
3. Check Google Cloud Console for API usage/errors
4. SSH into your App Service to debug environment issues
