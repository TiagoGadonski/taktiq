# GymHero - Quick Production Setup Guide

This is a streamlined guide to get GymHero ready for production. For detailed security information, see [DEPLOYMENT-SECURITY.md](./DEPLOYMENT-SECURITY.md).

## Quick Start Checklist

### 1. Generate Secrets (5 minutes)

```bash
# Generate JWT secret (run in terminal)
node -e "console.log(require('crypto').randomBytes(64).toString('base64'))"

# Generate strong database password (use a password manager or similar tool)
```

Save these values - you'll need them in step 3.

### 2. Update Configuration Files (10 minutes)

#### Backend Environment Variables

Create `src/GymHero.Api/.env` (copy from `.env.example`):

```bash
DB_HOST=your-production-db-host
DB_PORT=5432
DB_NAME=gymhero_production
DB_USER=gymhero_user
DB_PASSWORD=<paste-strong-password-here>

JWT_SECRET=<paste-generated-secret-here>
JWT_ISSUER=GymHero
JWT_AUDIENCE=GymHero

CORS_ALLOWED_ORIGINS=https://yourdomain.com,https://www.yourdomain.com

ASPNETCORE_ENVIRONMENT=Production
```

#### Frontend Environment Variables

Create `frontend/.env.production`:

```bash
NEXT_PUBLIC_API_BASE_URL=https://api.yourdomain.com/api/v1
EXPO_PUBLIC_API_BASE_URL=https://api.yourdomain.com/api/v1
NODE_ENV=production
```

### 3. Verify .gitignore (2 minutes)

Make sure these files are in `.gitignore`:

```
.env
.env.local
.env.production
*.env
!.env.example
appsettings.Production.json
```

✅ Already configured in your repository!

### 4. Build & Test (15 minutes)

#### Backend

```bash
cd src/GymHero.Api
dotnet build -c Release
dotnet test  # Run tests if you have them
dotnet ef database update  # Apply migrations
```

#### Frontend

```bash
cd frontend
pnpm install
pnpm build
```

### 5. Deploy (varies by platform)

Choose your hosting platform:

#### Option A: Azure (Recommended)

1. Create Azure App Service for API
2. Create Azure Database for PostgreSQL
3. Set environment variables in App Service Configuration
4. Deploy using:
   ```bash
   dotnet publish -c Release
   # Upload to Azure
   ```
5. Deploy frontend to Vercel or Azure Static Web Apps

#### Option B: Docker

1. Build containers:
   ```bash
   docker-compose -f docker-compose.yml build
   ```
2. Set environment variables in compose file
3. Deploy to your container host

#### Option C: Traditional VPS

1. Install .NET 8.0 runtime
2. Install PostgreSQL
3. Copy published files
4. Set up systemd service
5. Configure nginx reverse proxy

### 6. Post-Deployment Verification (10 minutes)

Test these endpoints:

- [ ] API Health: `https://api.yourdomain.com/health` (if you have one)
- [ ] User Registration: Create a test account
- [ ] User Login: Authenticate with test account
- [ ] Profile Update: Upload a profile picture
- [ ] Core Features: Test main app functionality

### 7. Enable Monitoring (15 minutes)

#### Set up logging monitoring
- Check that logs are being written to `logs/` folder
- Set up log rotation (30 days retention)

#### Optional: Set up Sentry (recommended)
```bash
# Install Sentry SDK
dotnet add package Sentry.AspNetCore
npm install --save @sentry/nextjs
```

Then configure with your Sentry DSN.

## What's Already Secured ✅

Your GymHero application already includes:

- ✅ **JWT Authentication** - Secure token-based auth
- ✅ **Rate Limiting** - Protection against brute force & DoS
- ✅ **CORS Configuration** - Environment-aware origin restrictions
- ✅ **Input Validation** - FluentValidation on all endpoints
- ✅ **Exception Handling** - Global error handler
- ✅ **Password Hashing** - Using ASP.NET Core Identity
- ✅ **SQL Injection Protection** - Entity Framework parameterized queries
- ✅ **Secure File Uploads** - Type validation and size limits

## Common Issues & Solutions

### Issue: CORS errors in production

**Solution**: Make sure `CORS_ALLOWED_ORIGINS` includes your exact frontend URL (with https://)

### Issue: JWT token validation fails

**Solution**: Verify JWT_SECRET is the same on the server and hasn't been truncated

### Issue: Database connection fails

**Solution**: Check:
1. Database server allows connections from your app server
2. Connection string has correct host/port
3. SSL mode is configured correctly

### Issue: 429 Too Many Requests

**Solution**: This is the rate limiter working. Limits are:
- Auth: 5/minute per IP
- API: 100/minute per user
- Global: 200/minute per IP

Adjust in `Program.cs` if needed for your use case.

## Getting Help

1. Check [DEPLOYMENT-SECURITY.md](./DEPLOYMENT-SECURITY.md) for detailed guides
2. Review logs in `logs/` folder
3. Enable detailed logging temporarily:
   ```json
   "Logging": {
     "LogLevel": {
       "Default": "Debug"
     }
   }
   ```

## Next Steps After Deployment

1. **Set up monitoring** - Track errors, performance, uptime
2. **Configure backups** - Automated daily database backups
3. **Create documentation** - User guides, API docs
4. **Plan updates** - Regular dependency updates
5. **Security audit** - Quarterly security reviews

---

**Production Ready!** 🚀

Your GymHero application is now secured and ready for users to test. Monitor the initial rollout closely and gather user feedback.
