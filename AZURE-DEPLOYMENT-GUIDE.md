# TaktIQ - Azure Deployment Guide

## Overview
This guide covers deploying the TaktIQ fitness application to Microsoft Azure. The application consists of:
- **Frontend**: Next.js 14.2.5 web application
- **Backend**: .NET 8.0 API
- **Database**: PostgreSQL

## Pre-Deployment Checklist

### 1. Branding Updates ✅
- [x] App name changed from GymHero to TaktIQ
- [x] Slogan updated to "Seu ritmo, seus resultados."
- [x] Frontend metadata and layouts updated
- [x] Backend API configuration updated
- [ ] Logo files added (see `frontend/apps/web/public/LOGO-SETUP.md`)

### 2. Required Azure Resources

#### Backend (API)
- **Azure App Service** (Linux, .NET 8.0)
  - Recommended: B1 or higher
  - Enable HTTPS only
  - Configure custom domain (optional)

#### Frontend (Web)
- **Azure Static Web Apps** OR **Azure App Service**
  - Recommended: Azure Static Web Apps for Next.js
  - Alternative: App Service (Linux, Node.js 18+)

#### Database
- **Azure Database for PostgreSQL**
  - Recommended: Flexible Server
  - Minimum: B1ms (1 vCore, 2 GiB RAM)
  - Enable SSL/TLS connections

#### Optional
- **Azure Container Registry** (if using Docker)
- **Azure CDN** (for static assets)
- **Application Insights** (monitoring)

## Environment Variables

### Backend (API) - App Service Configuration

Set these in Azure Portal → App Service → Configuration → Application settings:

```bash
# Database
DB_HOST=<your-postgres-server>.postgres.database.azure.com
DB_PORT=5432
DB_NAME=taktiq_db
DB_USER=<your-db-username>
DB_PASSWORD=<your-db-password>

# JWT Settings (IMPORTANT: Update issuer/audience to TaktIQ)
JWT_SECRET=<generate-a-secure-secret-key>
JWT_ISSUER=TaktIQ
JWT_AUDIENCE=TaktIQ

# CORS
CORS_ALLOWED_ORIGINS=https://your-frontend-domain.azurestaticapps.net,https://your-custom-domain.com

# AI Services (Optional)
Gemini__ApiKey=<your-gemini-api-key>
OpenAI__ApiKey=<your-openai-api-key>

# Logging
ASPNETCORE_ENVIRONMENT=Production
```

### Frontend (Web) - Static Web App Configuration

Set these in Azure Portal → Static Web App → Configuration:

```bash
NEXT_PUBLIC_API_BASE_URL=https://your-api.azurewebsites.net
```

## Deployment Steps

### Step 1: Database Setup

1. Create Azure Database for PostgreSQL:
```bash
az postgres flexible-server create \
  --name taktiq-db-server \
  --resource-group taktiq-rg \
  --location eastus \
  --admin-user taktiqadmin \
  --admin-password <secure-password> \
  --sku-name Standard_B1ms \
  --tier Burstable \
  --version 14
```

2. Create database:
```bash
az postgres flexible-server db create \
  --resource-group taktiq-rg \
  --server-name taktiq-db-server \
  --database-name taktiq_db
```

3. Configure firewall to allow Azure services:
```bash
az postgres flexible-server firewall-rule create \
  --resource-group taktiq-rg \
  --name taktiq-db-server \
  --rule-name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

4. Run migrations:
```bash
cd src/GymHero.Api
dotnet ef database update --connection "<your-azure-connection-string>"
```

### Step 2: Backend API Deployment

#### Option A: Direct Deployment from Visual Studio
1. Right-click on `GymHero.Api` project
2. Select "Publish"
3. Choose "Azure App Service (Linux)"
4. Configure settings
5. Publish

#### Option B: Azure CLI
```bash
# Create App Service Plan
az appservice plan create \
  --name taktiq-api-plan \
  --resource-group taktiq-rg \
  --is-linux \
  --sku B1

# Create Web App
az webapp create \
  --name taktiq-api \
  --resource-group taktiq-rg \
  --plan taktiq-api-plan \
  --runtime "DOTNETCORE:8.0"

# Deploy
cd src/GymHero.Api
dotnet publish -c Release -o ./publish
cd publish
zip -r ../deploy.zip .
az webapp deployment source config-zip \
  --resource-group taktiq-rg \
  --name taktiq-api \
  --src ../deploy.zip
```

### Step 3: Frontend Deployment

#### Option A: Azure Static Web Apps (Recommended)

1. Install Azure Static Web Apps CLI:
```bash
npm install -g @azure/static-web-apps-cli
```

2. Create Static Web App in Azure Portal or CLI:
```bash
az staticwebapp create \
  --name taktiq-web \
  --resource-group taktiq-rg \
  --location eastus2
```

3. Build and deploy:
```bash
cd frontend/apps/web
pnpm build
# Deploy via GitHub Actions or Azure CLI
```

#### Option B: App Service
```bash
az webapp create \
  --name taktiq-web \
  --resource-group taktiq-rg \
  --plan taktiq-web-plan \
  --runtime "NODE:18-lts"
```

## Post-Deployment Configuration

### 1. Update CORS Settings
In the backend API App Service:
- Go to API → CORS
- Add your frontend domain(s)
- Enable "Access-Control-Allow-Credentials" if needed

### 2. Enable HTTPS
- Both App Services should have "HTTPS Only" enabled
- Configure custom domains and SSL certificates if needed

### 3. Database Connection
- Ensure SSL/TLS is required
- Update connection string in appsettings.Production.json or environment variables

### 4. Health Checks
- Configure health check endpoint in App Service
- Recommended: `/health` or `/api/health`

### 5. Monitoring
- Enable Application Insights
- Configure alerts for errors and performance

## Environment-Specific Notes

### JWT Configuration
⚠️ **IMPORTANT**: When deploying to Azure, ensure:
- JWT_ISSUER is set to "TaktIQ" (not "GymHero")
- JWT_AUDIENCE is set to "TaktIQ" (not "GymHero")
- These must match the values in your local development settings

### Database
- Use SSL Mode=Require in production connection string
- Example: `Host=server.postgres.database.azure.com;Port=5432;Database=taktiq_db;Username=user;Password=pass;SSL Mode=Require;`

### Static Files
- Logo files should be in `frontend/apps/web/public/`
- Ensure they're included in the deployment build

## Security Checklist

- [ ] All API keys stored in Azure Key Vault or App Service Configuration
- [ ] Database passwords are strong and rotated regularly
- [ ] SSL/TLS enabled for all connections
- [ ] CORS properly configured (not using "*")
- [ ] JWT secret is cryptographically secure
- [ ] Admin registration is disabled in production
- [ ] API has rate limiting enabled
- [ ] Application Insights configured for security monitoring

## Monitoring and Maintenance

### Application Insights Queries

**API Response Times**:
```kusto
requests
| where timestamp > ago(24h)
| summarize avg(duration), percentile(duration, 95) by name
```

**Error Rate**:
```kusto
exceptions
| where timestamp > ago(24h)
| summarize count() by type, outerMessage
```

### Scaling Considerations
- **Database**: Consider scaling up for > 100 concurrent users
- **API**: Enable auto-scaling based on CPU/memory
- **Frontend**: Static Web Apps auto-scales

## Rollback Procedure

If deployment fails:

1. **API**: Use deployment slots or redeploy previous version
```bash
az webapp deployment source config-zip --src previous-version.zip
```

2. **Database**: Restore from automated backup
```bash
az postgres flexible-server restore --source-server <server> --restore-point-in-time <timestamp>
```

3. **Frontend**: Revert to previous Static Web App deployment in Azure Portal

## Cost Optimization

**Estimated Monthly Costs** (as of 2024):
- PostgreSQL Flexible Server (B1ms): ~$13
- App Service (B1): ~$13 × 2 = $26
- Azure Static Web Apps: Free tier available
- **Total**: ~$40-60/month

**Tips**:
- Use Free tier Static Web Apps for frontend
- Scale down App Services during low-traffic periods
- Use reserved instances for 30-40% savings

## Support and Resources

- [Azure App Service Documentation](https://docs.microsoft.com/azure/app-service/)
- [Azure Static Web Apps Documentation](https://docs.microsoft.com/azure/static-web-apps/)
- [Azure Database for PostgreSQL Documentation](https://docs.microsoft.com/azure/postgresql/)

## Troubleshooting

### API doesn't start
- Check Application Insights logs
- Verify all environment variables are set
- Ensure database connection string is correct

### Frontend can't connect to API
- Verify CORS settings
- Check NEXT_PUBLIC_API_BASE_URL is correct
- Ensure API is accessible via HTTPS

### Database connection fails
- Verify SSL Mode=Require in connection string
- Check firewall rules allow Azure services
- Confirm credentials are correct

---

**Last Updated**: 2024-10-24
**App Version**: 1.0.0
**Deployment Target**: Azure
