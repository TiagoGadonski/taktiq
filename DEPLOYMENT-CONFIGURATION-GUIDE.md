# GymHero Deployment & Configuration Guide

## 📋 Overview

This guide covers all API keys, secrets, and Azure configurations needed for GymHero to function properly in production.

---

## ✅ Database Status

**Migrations:** ✅ All migrations applied successfully to Azure PostgreSQL
- Last migration: `AddPaymentProviderToTransaction`
- Database is up to date

---

## 🔑 Required API Keys & Secrets

### 1. **Stripe Payment Integration** (REQUIRED for paid plans)

#### Get Stripe Keys:
1. Go to https://dashboard.stripe.com/
2. Create a Stripe account or login
3. Navigate to **Developers** → **API Keys**
4. Copy your keys:
   - **Secret Key** (starts with `sk_test_` for test mode)
   - **Publishable Key** (starts with `pk_test_` for test mode)

#### Setup Stripe Webhook:
1. Go to **Developers** → **Webhooks**
2. Click **Add endpoint**
3. Endpoint URL: `https://your-api-domain.azurewebsites.net/api/payments/stripe/webhook`
4. Events to send:
   - `checkout.session.completed`
   - `payment_intent.succeeded`
   - `payment_intent.payment_failed`
5. Copy the **Webhook signing secret** (starts with `whsec_`)

**Backend Configuration** (`src/GymHero.Api/appsettings.json`):
```json
{
  "Stripe": {
    "SecretKey": "sk_test_YOUR_STRIPE_SECRET_KEY_HERE",
    "WebhookSecret": "whsec_YOUR_WEBHOOK_SECRET_HERE"
  }
}
```

**Frontend Configuration** (`frontend/apps/web/.env.local`):
```bash
NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=pk_test_YOUR_PUBLISHABLE_KEY_HERE
```

**Cost:** Free tier available, charges only when processing real payments
**Documentation:** See `STRIPE-WEBHOOK-SETUP.md` for detailed setup

---

### 2. **PayPal Payment Integration** (OPTIONAL alternative to Stripe)

#### Get PayPal Keys:
1. Go to https://developer.paypal.com/
2. Login and go to **Dashboard**
3. Navigate to **Apps & Credentials**
4. Create a new app or use **Default Application**
5. Copy:
   - **Client ID**
   - **Secret**

**Configuration** (`appsettings.json`):
```json
{
  "PayPal": {
    "ClientId": "YOUR_PAYPAL_CLIENT_ID_HERE",
    "ClientSecret": "YOUR_PAYPAL_CLIENT_SECRET_HERE",
    "Mode": "sandbox"  // Change to "live" for production
  }
}
```

**Cost:** Free tier available
**Note:** PayPal is optional if you're only using Stripe

---

### 3. **Google Places API** (REQUIRED for Nearby Gyms feature)

#### Get Google Places API Key:
1. Go to https://console.cloud.google.com/
2. Create a new project: **"GymHero"**
3. Enable APIs:
   - **Places API**
   - **Geocoding API**
   - **Maps JavaScript API** (optional, for embedded maps)
4. Go to **Credentials** → **Create Credentials** → **API Key**
5. Copy the API key (starts with `AIza`)
6. Click **Restrict Key**:
   - **Application restrictions:** HTTP referrers
   - Add domains:
     ```
     http://localhost:3000/*
     https://yourdomain.com/*
     ```
   - **API restrictions:** Select only the APIs listed above

**Configuration** (`appsettings.json`):
```json
{
  "GooglePlaces": {
    "ApiKey": "AIzaSyC_YOUR_GOOGLE_PLACES_API_KEY_HERE"
  }
}
```

**Cost:** $200/month free credit
**Documentation:** See `GOOGLE-PLACES-SETUP.md` for detailed setup

---

### 4. **Azure Blob Storage** (REQUIRED for image/video uploads)

#### Create Storage Account:
1. Go to Azure Portal: https://portal.azure.com/
2. Create **Storage Account**:
   - **Name:** `gymherostorage` (or your preferred name)
   - **Performance:** Standard
   - **Redundancy:** LRS (or your preference)
3. Once created, go to **Access keys**
4. Copy **Connection string**

#### Create Containers:
1. Go to **Containers** in your storage account
2. Create the following containers:
   - `images` (Public access: Blob)
   - `videos` (Public access: Blob)
   - `thumbnails` (Public access: Blob)

**Configuration** (`appsettings.json`):
```json
{
  "AzureBlobStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=gymherostorage;AccountKey=YOUR_ACCOUNT_KEY;EndpointSuffix=core.windows.net",
    "ImagesContainer": "images",
    "VideosContainer": "videos",
    "ThumbnailsContainer": "thumbnails"
  }
}
```

**Cost:** Pay-as-you-go (very low cost for small apps)

---

### 5. **AI APIs (Gemini & OpenAI)** (OPTIONAL for AI workout generation)

#### Option A: Google Gemini API (Recommended)
1. Go to https://makersuite.google.com/app/apikey
2. Create API key
3. Copy the key

**Configuration** (`appsettings.json`):
```json
{
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE"
  }
}
```

#### Option B: OpenAI API (Alternative)
1. Go to https://platform.openai.com/api-keys
2. Create API key
3. Copy the key

**Configuration** (`appsettings.json`):
```json
{
  "OpenAI": {
    "ApiKey": "sk-YOUR_OPENAI_API_KEY_HERE"
  }
}
```

**Cost:** Both have free tiers
**Note:** If both are missing, the system falls back to mock AI generation (still functional)

---

## 🔧 Azure App Service Configuration

### Environment Variables (Application Settings)

In Azure Portal → Your App Service → **Configuration** → **Application settings**, add:

```
Stripe__SecretKey = sk_test_YOUR_STRIPE_SECRET_KEY
Stripe__WebhookSecret = whsec_YOUR_WEBHOOK_SECRET
PayPal__ClientId = YOUR_PAYPAL_CLIENT_ID
PayPal__ClientSecret = YOUR_PAYPAL_CLIENT_SECRET
PayPal__Mode = live
GooglePlaces__ApiKey = AIzaSyC_YOUR_GOOGLE_API_KEY
AzureBlobStorage__ConnectionString = DefaultEndpointsProtocol=https;AccountName=...
Gemini__ApiKey = YOUR_GEMINI_API_KEY
OpenAI__ApiKey = sk-YOUR_OPENAI_API_KEY
Marketplace__PlatformFeePercentage = 10.0
Marketplace__MinimumPlatformFee = 0.50
VideoCompression__Enabled = true
VideoCompression__DefaultQuality = Medium
VideoCompression__MaxResolution = 1920
VideoCompression__AutoCompress = true
```

### Frontend Environment Variables

In **Vercel/Netlify/Your hosting** → Environment Variables:

```
NEXT_PUBLIC_API_URL=https://your-api.azurewebsites.net
NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=pk_test_YOUR_PUBLISHABLE_KEY
```

---

## 📦 Azure Services Needed

### 1. **Azure App Service** (Backend API)
- Runtime: .NET 8
- Plan: B1 or higher recommended

### 2. **Azure Database for PostgreSQL**
- Already configured: `taktiq-db.postgres.database.azure.com`
- ✅ Migrations applied

### 3. **Azure Blob Storage**
- For images, videos, and thumbnails
- Public access enabled for blob containers

---

## 🚀 Deployment Steps

### Backend (Azure App Service)

1. **Build the backend:**
   ```bash
   dotnet publish src/GymHero.Api/GymHero.Api.csproj -c Release -o ./publish
   ```

2. **Create deployment zip:**
   ```bash
   cd publish
   tar -czf ../deploy.tar.gz *
   ```

3. **Deploy to Azure:**
   ```bash
   az webapp deployment source config-zip --resource-group YOUR_RG --name YOUR_APP_NAME --src deploy.tar.gz
   ```

### Frontend (Vercel/Netlify)

1. **Connect repository to Vercel/Netlify**
2. **Set build settings:**
   - Build command: `pnpm build`
   - Output directory: `frontend/apps/web/.next`
   - Install command: `pnpm install`
3. **Add environment variables** (see above)
4. **Deploy**

---

## ✅ Testing Checklist

After configuration, test these features:

### Payment System
- [ ] Create a paid workout plan
- [ ] Purchase plan with Stripe test card (`4242 4242 4242 4242`)
- [ ] Verify webhook receives payment confirmation
- [ ] Check transaction appears in `/transactions`

### Media Uploads
- [ ] Upload profile picture
- [ ] Upload post image
- [ ] Upload video (if enabled)
- [ ] Verify files appear in Azure Blob Storage

### Google Places
- [ ] Visit `/gyms` page
- [ ] Click "Use My Location"
- [ ] Verify nearby gyms appear (or remove "Coming Soon" banner)

### AI Workouts
- [ ] Generate AI workout
- [ ] Test with/without cardio restriction
- [ ] Verify duration is saved correctly

### PT Features
- [ ] Login as Personal Trainer
- [ ] Create and publish post
- [ ] Create workout plan and publish to marketplace
- [ ] Create challenge for client
- [ ] Verify instructor dashboard shows metrics

---

## 🔒 Security Best Practices

1. **API Keys:**
   - ✅ Never commit secrets to git
   - ✅ Use environment variables in production
   - ✅ Rotate keys periodically
   - ✅ Restrict API keys to specific domains

2. **Stripe:**
   - ✅ Use webhook secrets to verify events
   - ✅ Validate amounts on the backend
   - ✅ Never trust client-side data

3. **Azure:**
   - ✅ Enable HTTPS only
   - ✅ Set up Application Insights for monitoring
   - ✅ Configure CORS properly
   - ✅ Use managed identities where possible

---

## 💰 Cost Estimates

**Monthly costs for small/medium app:**

| Service | Free Tier | Estimated Cost |
|---------|-----------|----------------|
| Azure App Service (B1) | - | $13/month |
| Azure PostgreSQL | - | $5-30/month |
| Azure Blob Storage | 5GB free | ~$1-5/month |
| Stripe | Free | 2.9% + $0.30 per transaction |
| Google Places API | $200/month free | Usually $0-50/month |
| Gemini API | Free tier | Usually free |
| Vercel/Netlify | Free for hobby | $0/month |
| **TOTAL** | - | **~$20-100/month** |

---

## 📞 Support & Troubleshooting

### Common Issues:

**"Google Places API key is not configured"**
- Check `appsettings.json` has `GooglePlaces:ApiKey`
- Verify API is enabled in Google Cloud Console

**"Stripe payment failed"**
- Check webhook is configured with correct URL
- Verify webhook secret matches
- Check Application Insights logs

**"File upload failed"**
- Verify Azure Blob Storage connection string
- Check containers exist and have correct permissions
- Verify container names in configuration

**"Database connection failed"**
- Check connection string format
- Verify firewall allows Azure services
- Check credentials

---

## 🎯 What's Working vs What Needs Setup

### ✅ Already Working (No configuration needed):
- User authentication & registration
- Workout plans creation
- Exercise database
- Challenges system
- Friends system
- Progress tracking
- Admin dashboard

### ⚠️ Needs API Keys:
- **Stripe** - For paid workout plans
- **Google Places** - For nearby gyms
- **Azure Blob Storage** - For media uploads

### 🔧 Optional (Will use fallback):
- **Gemini/OpenAI** - Falls back to mock AI generation
- **PayPal** - Optional if using Stripe only

---

## 📝 Quick Start (Minimum Configuration)

**To get the app working quickly:**

1. **Backend (`appsettings.json`):**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=taktiq-db.postgres.database.azure.com;..."
     },
     "Stripe": {
       "SecretKey": "sk_test_...",
       "WebhookSecret": "whsec_..."
     },
     "AzureBlobStorage": {
       "ConnectionString": "DefaultEndpointsProtocol=https;..."
     }
   }
   ```

2. **Frontend (`.env.local`):**
   ```
   NEXT_PUBLIC_API_URL=https://your-api.azurewebsites.net
   NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=pk_test_...
   ```

3. **Deploy and test!**

---

**Last Updated:** 2025-11-20
**Status:** ✅ Ready for Production Deployment
