# Azure Blob Storage Setup for Profile Pictures

This guide explains how to configure Azure Blob Storage for storing profile pictures and other media files.

## Why Azure Blob Storage?

The application now uses Azure Blob Storage instead of local file storage because:

- **Persistent Storage**: Files survive deployments, restarts, and scaling operations
- **High Availability**: 99.9%+ uptime SLA with built-in redundancy
- **Scalability**: Handles growing storage needs automatically
- **Performance**: Global CDN integration for fast image loading
- **Cost-Effective**: Pay only for what you use (~$0.018 per GB/month)

## Setup Instructions

### Step 1: Create Azure Storage Account

1. **Login to Azure Portal**: https://portal.azure.com
2. **Create Storage Account**:
   - Click "Create a resource" → "Storage account"
   - **Resource Group**: Use your existing resource group (e.g., `taktiq-rg`)
   - **Storage account name**: Choose a unique name (e.g., `taktiqstorage`)
   - **Region**: Same as your App Service (for better performance)
   - **Performance**: Standard
   - **Redundancy**: LRS (Locally-redundant storage) for dev, GRS for production
   - Click "Review + Create" → "Create"

### Step 2: Get Connection String

1. Navigate to your Storage Account
2. Click **"Access keys"** in the left menu
3. Copy the **"Connection string"** from key1 or key2
4. It should look like:
   ```
   DefaultEndpointsProtocol=https;AccountName=taktiqstorage;AccountKey=abc123...;EndpointSuffix=core.windows.net
   ```

### Step 3: Configure Azure App Service

#### Option A: Using Azure Portal

1. Go to your App Service (e.g., `taktiqapi`)
2. Click **"Configuration"** → **"Application settings"**
3. Click **"+ New application setting"**
4. Add the following:
   - **Name**: `AzureStorage__ConnectionString`
   - **Value**: Paste your connection string from Step 2
5. Click **"OK"** → **"Save"**
6. Restart your App Service

#### Option B: Using Azure CLI

```bash
az webapp config appsettings set \
  --resource-group taktiq-rg \
  --name taktiqapi \
  --settings AzureStorage__ConnectionString="YOUR_CONNECTION_STRING_HERE"
```

### Step 4: Local Development Setup

For local development, update `appsettings.Development.json`:

```json
{
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=taktiqstorage;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net"
  }
}
```

**Alternative**: Use Azure Storage Emulator (Azurite) for local development:
```json
{
  "AzureStorage": {
    "ConnectionString": "UseDevelopmentStorage=true"
  }
}
```

## How It Works

### Upload Flow

1. User uploads profile picture via `/api/me/profile-picture`
2. Backend validates file (size, type, security checks)
3. File is uploaded to Azure Blob Storage container `profile-pictures`
4. Azure returns a public URL (e.g., `https://taktiqstorage.blob.core.windows.net/profile-pictures/user_123.jpg`)
5. URL is saved to database
6. Frontend displays image using the blob URL

### Container Structure

```
profile-pictures/           # Container for user profile photos
  ├── {userId}_{guid}.jpg   # Individual profile pictures
  └── {userId}_{guid}.png

post-images/               # Container for blog post images (future)
workout-videos/            # Container for workout videos (future)
```

### Public Access

The `profile-pictures` container is configured with **Blob-level public read access**, meaning:
- ✅ Anyone can view images via the URL
- ❌ Only authenticated API requests can upload/delete
- ✅ No Azure credentials needed to display images

## Security Considerations

### Implemented Security Measures

1. **File Validation**:
   - Size limit: 5MB
   - Allowed extensions: .jpg, .jpeg, .png, .gif
   - MIME type validation
   - Magic number (file signature) validation

2. **Access Control**:
   - Only authenticated users can upload
   - Users can only modify their own profile picture
   - Old pictures are automatically deleted

3. **Secure Storage**:
   - Files stored in Azure's SOC 2 Type II certified datacenters
   - Encrypted at rest and in transit (HTTPS)
   - Private connection string stored in App Service config

## Cost Estimation

Assuming:
- 1,000 users
- Average profile picture size: 500KB
- Total storage: 500MB

**Monthly Costs**:
- Storage (LRS): 500MB × $0.018/GB = ~$0.01/month
- Transactions: Negligible (<$0.01)
- Data Transfer: First 100GB free

**Total**: < $0.50/month for 1,000 users

## Troubleshooting

### Error: "Azure Storage connection string not found"

**Solution**: Ensure `AzureStorage__ConnectionString` is configured in App Service settings.

### Error: 404 on image URLs

**Possible Causes**:
1. Container doesn't exist → Will be auto-created on first upload
2. Blob was deleted → Check Azure Portal → Storage Browser
3. Connection string is wrong → Verify in App Service config

### Images not loading after deployment

**Solution**:
1. Verify connection string is in App Service settings (not appsettings.json)
2. Check blob URLs in database start with `https://` (not `/uploads/`)
3. Restart App Service after config changes

## Migration from Local Storage

### For Existing Users

Old profile pictures stored in `/wwwroot/uploads/profiles/` will not be migrated automatically. Users will need to:
1. Re-upload their profile picture
2. New uploads will go to Azure Blob Storage
3. Old local files can be deleted (they don't exist in production anyway)

### Database Update (Optional)

To clear old invalid URLs from the database:

```sql
-- Clear old local file URLs (they don't work in production anyway)
UPDATE "Users"
SET "ProfilePictureUrl" = NULL
WHERE "ProfilePictureUrl" LIKE '/uploads/%';
```

## Advanced: CDN Integration (Optional)

For even faster global image delivery:

1. Create Azure CDN endpoint
2. Point CDN to your storage account
3. Update frontend to use CDN URLs
4. Expected improvement: 50-200ms faster load times globally

## Support

For issues or questions:
- Check Azure Storage logs in Azure Portal
- Review Application Insights for API errors
- Contact: [GitHub Issues](https://github.com/TiagoGadonski/taktiq/issues)
