# GymHero - Deployment & Security Guide

This guide covers all security precautions and deployment steps for publishing GymHero to production.

## Table of Contents
- [Pre-Deployment Security Checklist](#pre-deployment-security-checklist)
- [Environment Configuration](#environment-configuration)
- [Backend (API) Setup](#backend-api-setup)
- [Frontend Setup](#frontend-setup)
- [Database Security](#database-security)
- [Production Deployment](#production-deployment)
- [Post-Deployment Monitoring](#post-deployment-monitoring)

---

## Pre-Deployment Security Checklist

### 🔴 CRITICAL - Must Complete Before Publishing

- [ ] **Generate Production JWT Secret**
  - Use a cryptographically secure random string (min 64 characters)
  - NEVER use the development secret in production
  - Command: `node -e "console.log(require('crypto').randomBytes(64).toString('base64'))"`

- [ ] **Secure Database Credentials**
  - Change default PostgreSQL password
  - Use strong passwords (min 16 characters, mixed case, numbers, symbols)
  - Store credentials in environment variables or secrets manager

- [ ] **Configure Environment Variables**
  - Set up production environment variables (see below)
  - Never commit `.env` files to git
  - Use `.env.example` files as templates only

- [ ] **Update API URLs**
  - Replace all `localhost` references with production URLs
  - Configure CORS to only allow your production domains

### 🟡 IMPORTANT - Security Hardening

- [ ] **HTTPS Only**
  - Ensure SSL/TLS certificates are installed
  - Enable HSTS (HTTP Strict Transport Security)
  - Redirect all HTTP traffic to HTTPS

- [ ] **Rate Limiting** ✅ Already Implemented
  - Auth endpoints: 5 requests/minute
  - API endpoints: 100 requests/minute
  - Global limit: 200 requests/minute

- [ ] **Input Validation**
  - All endpoints use FluentValidation ✅
  - Review custom validation rules
  - Test for SQL injection, XSS vulnerabilities

- [ ] **Error Handling**
  - Don't expose stack traces in production
  - Log errors securely server-side
  - Return generic error messages to clients

### 🟢 RECOMMENDED - Best Practices

- [ ] **Monitoring & Logging**
  - Set up Application Insights or Sentry
  - Configure log retention policies
  - Set up alerts for suspicious activities

- [ ] **Dependency Security**
  - Run `npm audit` for frontend
  - Run `dotnet list package --vulnerable` for backend
  - Update vulnerable packages

- [ ] **Backup Strategy**
  - Set up automated database backups
  - Test backup restoration procedures
  - Store backups in secure, separate location

---

## Environment Configuration

### Backend (API) - Required Environment Variables

Create a `.env` file in `src/GymHero.Api/` (or use your hosting platform's environment variables):

```bash
# Database Configuration
DB_HOST=your-db-host.com
DB_PORT=5432
DB_NAME=gymhero_production
DB_USER=gymhero_user
DB_PASSWORD=YOUR_STRONG_DATABASE_PASSWORD_HERE

# JWT Configuration
JWT_SECRET=YOUR_GENERATED_JWT_SECRET_MIN_64_CHARS
JWT_ISSUER=GymHero
JWT_AUDIENCE=GymHero

# CORS Configuration (comma-separated)
CORS_ALLOWED_ORIGINS=https://yourdomain.com,https://www.yourdomain.com

# Environment
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:5001

# Optional: Azure/Cloud Configuration
# APPLICATIONINSIGHTS_CONNECTION_STRING=your-app-insights-connection-string
# AZURE_KEY_VAULT_ENDPOINT=https://your-keyvault.vault.azure.net/
```

### Frontend - Required Environment Variables

Create a `.env.production` file in `frontend/`:

```bash
# API Configuration
NEXT_PUBLIC_API_BASE_URL=https://api.yourdomain.com/api/v1
EXPO_PUBLIC_API_BASE_URL=https://api.yourdomain.com/api/v1

# Environment
NODE_ENV=production

# Optional: Analytics
# NEXT_PUBLIC_SENTRY_DSN=your-sentry-dsn
# NEXT_PUBLIC_GA_MEASUREMENT_ID=your-google-analytics-id
```

---

## Backend (API) Setup

### 1. Database Migration

```bash
cd src/GymHero.Api
dotnet ef database update
```

### 2. Build for Production

```bash
dotnet build -c Release
dotnet publish -c Release -o ./publish
```

### 3. Configuration Files

The API uses the following configuration hierarchy:
1. `appsettings.json` - Base configuration
2. `appsettings.Production.json` - Production overrides
3. Environment variables - Highest priority

**Important**: The `appsettings.Production.json` uses placeholders (`${VAR_NAME}`) that must be replaced with actual environment variables.

### 4. Security Features Implemented

✅ **JWT Authentication**
- Token expiration: 60 minutes
- Secure signing with strong secret
- Validates issuer and audience

✅ **Rate Limiting**
- Protects against brute force attacks
- Per-endpoint and global limits
- Returns 429 status on limit exceeded

✅ **CORS Configuration**
- Development: Allows localhost
- Production: Restricts to configured domains only

✅ **Exception Handling**
- Global exception handler
- Sanitized error responses
- Detailed logging server-side

---

## Frontend Setup

### Web App (Next.js)

```bash
cd frontend/apps/web
npm install
npm run build
npm start
```

### Mobile App (Expo)

```bash
cd frontend/apps/mobile
npm install
npx expo build:android  # or build:ios
```

### Environment Variables

The frontend uses `NEXT_PUBLIC_*` and `EXPO_PUBLIC_*` prefixes for environment variables that are exposed to the browser/app.

**Security Note**: Never put secrets in these variables - they are publicly visible in the built app.

---

## Database Security

### PostgreSQL Best Practices

1. **User Permissions**
   ```sql
   -- Create dedicated user for the application
   CREATE USER gymhero_user WITH PASSWORD 'strong_password_here';

   -- Grant only necessary permissions
   GRANT CONNECT ON DATABASE gymhero_production TO gymhero_user;
   GRANT USAGE ON SCHEMA public TO gymhero_user;
   GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO gymhero_user;
   ```

2. **Connection Security**
   - Enable SSL/TLS for database connections
   - Use connection string with `SSL Mode=Require`
   - Restrict database access by IP address

3. **Backup Strategy**
   ```bash
   # Automated backup script
   pg_dump -h localhost -U gymhero_user gymhero_production > backup_$(date +%Y%m%d).sql
   ```

---

## Production Deployment

### Recommended Hosting Options

#### Option 1: Azure (Recommended for .NET)

**API Deployment**:
- Azure App Service (Web App)
- Azure SQL Database / PostgreSQL
- Azure Key Vault for secrets

**Frontend Deployment**:
- Azure Static Web Apps (Next.js)
- Azure CDN for global performance

#### Option 2: AWS

**API Deployment**:
- AWS Elastic Beanstalk / ECS
- RDS for PostgreSQL
- AWS Secrets Manager

**Frontend Deployment**:
- Vercel (Next.js) - easiest option
- AWS Amplify
- CloudFront + S3

#### Option 3: Docker

Use the included `docker-compose.yml` as a starting point:

```bash
docker-compose up -d
```

**Important**: Update the compose file for production:
- Remove development tools
- Use production environment variables
- Configure reverse proxy (nginx)
- Set up SSL certificates

### Deployment Steps

1. **Set up hosting infrastructure**
   - Provision database server
   - Create application hosting service
   - Configure CDN (optional but recommended)

2. **Configure DNS**
   - Point API subdomain to backend server (e.g., `api.yourdomain.com`)
   - Point main domain to frontend (e.g., `yourdomain.com`)

3. **Install SSL Certificates**
   - Use Let's Encrypt (free)
   - Or purchase SSL certificate
   - Configure auto-renewal

4. **Deploy Backend**
   - Upload published files
   - Set environment variables
   - Run database migrations
   - Verify API is accessible

5. **Deploy Frontend**
   - Build production bundle
   - Upload to hosting service
   - Verify environment variables
   - Test all features

---

## Post-Deployment Monitoring

### Health Checks

Set up endpoint monitoring for:
- API health endpoint
- Database connectivity
- Authentication service
- File upload functionality

### Logging

Serilog is configured to write logs to:
- Console (for container environments)
- File (`logs/gymhero-log-{date}.txt`)

**Production Log Settings**:
- Minimum level: Warning
- Retain logs for 30 days
- Monitor for:
  - Authentication failures
  - Database errors
  - Rate limit violations
  - Unhandled exceptions

### Security Monitoring

Monitor for:
- **Failed login attempts** - possible brute force
- **Rate limit hits** - possible DoS attack
- **SQL injection attempts** - malicious input
- **Unusual traffic patterns** - possible bot activity

### Performance Monitoring

Track:
- API response times
- Database query performance
- Error rates
- User session durations

---

## Security Incident Response

### If You Detect a Security Issue

1. **Immediate Actions**
   - Assess the severity and scope
   - Document everything
   - DO NOT delete logs

2. **Contain the Breach**
   - Rotate compromised credentials immediately
   - Block suspicious IP addresses
   - Temporarily disable affected features if needed

3. **Investigate**
   - Review logs for attack vectors
   - Identify affected user accounts
   - Determine what data was accessed

4. **Remediate**
   - Fix the vulnerability
   - Force password resets for affected users
   - Deploy patches immediately

5. **Communicate**
   - Notify affected users
   - Document lessons learned
   - Update security procedures

---

## Regular Maintenance

### Weekly
- Review error logs
- Check backup integrity
- Monitor disk space

### Monthly
- Update dependencies
- Review access logs
- Test disaster recovery

### Quarterly
- Security audit
- Performance optimization
- Update documentation

---

## Support & Resources

### Security Resources
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [Next.js Security](https://nextjs.org/docs/advanced-features/security-headers)

### Getting Help
- Report security issues: security@yourdomain.com
- GitHub Issues: (for non-security bugs)
- Documentation: Check README.md files in each project

---

## Version History

- **v1.0.0** - Initial production release
  - Core features implemented
  - Security hardening completed
  - Ready for user testing

---

**Remember**: Security is an ongoing process, not a one-time task. Keep dependencies updated, monitor logs regularly, and always follow the principle of least privilege.
