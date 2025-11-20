# Google Places API Setup Guide

This guide explains how to set up and configure the Google Places API for the GymHero "Nearby Gyms" feature.

## Features Implemented

The nearby gyms feature provides:
- **Real-time gym search** based on user's location
- **Geolocation support** using browser GPS
- **Address search** with geocoding
- **Detailed gym information** (ratings, reviews, hours, photos)
- **Open now status** indicating if gym is currently open
- **Direct Google Maps integration** for directions

## Prerequisites

1. **Google Cloud Platform Account**
2. **Billing enabled** (Google provides $200/month free credit)
3. **API Keys** for Places API and Geocoding API

## Setup Instructions

### 1. Create a Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Click "Select a project" → "New Project"
3. Enter project name: "GymHero"
4. Click "Create"

### 2. Enable Required APIs

1. Go to "APIs & Services" → "Library"
2. Search and enable the following APIs:
   - **Places API** (for nearby search and place details)
   - **Geocoding API** (for address to coordinates conversion)
   - **Maps JavaScript API** (optional, for embedded maps)

### 3. Create API Key

1. Go to "APIs & Services" → "Credentials"
2. Click "Create Credentials" → "API Key"
3. Copy the API key (starts with `AIza...`)
4. Click "Restrict Key" for security

### 4. Restrict API Key (Recommended)

**Application Restrictions:**
- Select "HTTP referrers (web sites)"
- Add your domains:
  ```
  http://localhost:3000/*
  https://yourdomain.com/*
  https://www.yourdomain.com/*
  ```

**API Restrictions:**
- Select "Restrict key"
- Check:
  - Places API
  - Geocoding API
  - Maps JavaScript API (if using maps)

### 5. Configure in GymHero

Edit `src/GymHero.Api/appsettings.json`:

```json
{
  "GooglePlaces": {
    "ApiKey": "AIzaSyC_your_actual_api_key_here"
  }
}
```

For production, use environment variables or Azure Key Vault:

```json
{
  "GooglePlaces": {
    "ApiKey": "#{GooglePlacesApiKey}#"
  }
}
```

## API Endpoints

### 1. Search Nearby Places

**GET** `/api/places?lat={latitude}&lng={longitude}&type={type}&radius={radius}`

**Parameters:**
- `lat` (required): Latitude (-90 to 90)
- `lng` (required): Longitude (-180 to 180)
- `type` (required): Place type (e.g., "gym", "restaurant", "cafe")
- `radius` (required): Search radius in meters (1 to 50000)

**Example:**
```
GET /api/places?lat=-23.5505&lng=-46.6333&type=gym&radius=5000
```

**Response:**
```json
{
  "results": [
    {
      "place_id": "ChIJN1t_tDeuEmsRUsoyG83frY4",
      "name": "SmartFit Academia",
      "vicinity": "Av. Paulista, 1000 - São Paulo",
      "rating": 4.5,
      "user_ratings_total": 250,
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
  "next_page_token": "CpQC..."
}
```

### 2. Geocode Address

**GET** `/api/places/geocode?address={address}`

**Parameters:**
- `address` (required): Street address or location description

**Example:**
```
GET /api/places/geocode?address=Av.%20Paulista%201000,%20São%20Paulo
```

**Response:**
```json
{
  "location": {
    "lat": -23.5631,
    "lng": -46.6558
  },
  "formatted_address": "Av. Paulista, 1000 - Bela Vista, São Paulo - SP, Brazil",
  "status": "OK"
}
```

### 3. Get Place Details

**GET** `/api/places/details/{placeId}`

**Parameters:**
- `placeId` (required): Google Place ID

**Example:**
```
GET /api/places/details/ChIJN1t_tDeuEmsRUsoyG83frY4
```

**Response:**
```json
{
  "result": {
    "place_id": "ChIJN1t_tDeuEmsRUsoyG83frY4",
    "name": "SmartFit Academia",
    "formatted_address": "Av. Paulista, 1000 - Bela Vista, São Paulo - SP",
    "formatted_phone_number": "(11) 1234-5678",
    "website": "https://www.smartfit.com.br",
    "rating": 4.5,
    "user_ratings_total": 250,
    "opening_hours": {
      "open_now": true,
      "weekday_text": [
        "Monday: 6:00 AM – 11:00 PM",
        "Tuesday: 6:00 AM – 11:00 PM",
        ...
      ]
    }
  }
}
```

## Pricing

Google Places API uses pay-as-you-go pricing with **$200/month free credit**.

### Cost per Request:
- **Nearby Search**: $0.032 per request
- **Place Details**: $0.017 per request
- **Geocoding**: $0.005 per request

### Free Tier Estimate:
With $200/month free credit:
- ~6,250 nearby searches
- ~11,765 place details requests
- ~40,000 geocoding requests

### Cost Optimization Tips:
1. **Cache results** for frequently searched locations
2. **Limit radius** to 5km or less
3. **Batch requests** when possible
4. **Use search parameters** (type, keyword) to reduce unnecessary calls

## Frontend Integration

The frontend page (`/gyms`) is already fully integrated and will automatically use the real API once you configure the API key.

### Features:
- ✅ Geolocation button (uses browser GPS)
- ✅ Address search input with geocoding
- ✅ Gym cards with ratings and status
- ✅ "View on Map" button opens Google Maps
- ✅ Fallback to default location (São Paulo) if geolocation fails
- ✅ Loading states and error handling
- ✅ Responsive design (mobile + desktop)

## Testing

### 1. Test with Mock Data (No API Key)

If API key is not configured or invalid, the service will throw an exception and the frontend will show an error.

### 2. Test with Real API Key

1. Add your API key to `appsettings.json`
2. Start the backend: `dotnet run --project src/GymHero.Api`
3. Start the frontend: `cd frontend/apps/web && pnpm dev`
4. Navigate to `/gyms`
5. Click "Use My Location" or search an address
6. Verify real gyms appear

### 3. Test API Directly

```bash
# Replace YOUR_API_KEY with your actual key
curl "http://localhost:5000/api/places?lat=-23.5505&lng=-46.6333&type=gym&radius=5000"
```

## Security Best Practices

1. **Never commit API keys** to version control
2. **Use environment variables** in production
3. **Restrict API key** to specific domains/IPs
4. **Monitor usage** in Google Cloud Console
5. **Set spending limits** to prevent unexpected charges
6. **Rotate keys** periodically

## Troubleshooting

### "Google Places API key is not configured"
- Check `appsettings.json` has `GooglePlaces:ApiKey` set
- Verify the key is correct (starts with `AIza`)

### "REQUEST_DENIED" Status
- API key is invalid or restricted
- Check API is enabled in Google Cloud Console
- Verify domain restrictions match your site

### "OVER_QUERY_LIMIT" Status
- You've exceeded the free tier
- Check usage in Google Cloud Console
- Consider implementing caching

### No Results Found
- Try increasing the `radius` parameter
- Check the `type` parameter is correct (e.g., "gym" not "gymnasium")
- Verify coordinates are valid

## Additional Resources

- [Google Places API Documentation](https://developers.google.com/maps/documentation/places/web-service)
- [Pricing Calculator](https://mapsplatform.google.com/pricing/)
- [API Key Best Practices](https://developers.google.com/maps/api-security-best-practices)
- [Places API Supported Types](https://developers.google.com/maps/documentation/places/web-service/supported_types)

## Support

For issues or questions, contact the development team or refer to the Google Cloud Console documentation.
