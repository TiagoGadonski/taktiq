# 🛠️ Development Environment Setup Guide

This guide will help you set up a local development environment for TaktIQ/GymHero separate from your production environment.

## 📋 Prerequisites

- **Node.js** (v18 or higher) - for frontend
- **.NET 8 SDK** - for backend
- **PostgreSQL** (v14 or higher) - for database
- **Git** - for version control
- **Visual Studio Code** or **Visual Studio** (optional but recommended)

---

## 🗄️ Step 1: Set Up Local Database

### Option A: Install PostgreSQL Locally

1. **Download PostgreSQL**: https://www.postgresql.org/download/
2. **Install** with default settings
3. **Remember** the password you set for the `postgres` user

4. **Create Development Database**:
   ```bash
   # Connect to PostgreSQL
   psql -U postgres

   # Create database and user
   CREATE DATABASE gymhero_dev;
   CREATE USER gymuser WITH ENCRYPTED PASSWORD 'gympassword';
   GRANT ALL PRIVILEGES ON DATABASE gymhero_dev TO gymuser;
   \q
   ```

### Option B: Use Docker (Recommended)

A `docker-compose.yml` already exists in the project root. Simply run:

```bash
# Start the database
docker-compose up -d

# Verify it's running
docker ps
```

This will create a PostgreSQL database with:
- **Database**: `gymhero2_db`
- **User**: `gymuser`
- **Password**: `gympassword`
- **Port**: `5432`

---

## ⚙️ Step 2: Configure Backend (API)

1. **Copy the example configuration**:
   ```bash
   cd src/GymHero.Api
   copy appsettings.Development.json.example appsettings.Development.json
   ```

2. **Edit `appsettings.Development.json`** with your values:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=gymhero2_db;Username=gymuser;Password=gympassword"
     },
     "JwtSettings": {
       "Secret": "NmbH63pPrXJvJzNRn8RS8VCGzr3TA6DK8iBvz2P2r6lE93oXB9q6zuzmDX6w8kk7weDZo5tV36vQVia7oZR7Jw==",
       "ExpiryMinutes": 60,
       "Issuer": "TaktIQ",
       "Audience": "TaktIQ"
     },
     "Gemini": {
       "ApiKey": "YOUR_REAL_GEMINI_API_KEY"
     },
     "OpenAI": {
       "ApiKey": ""
     }
   }
   ```

3. **Run Database Migrations**:
   ```bash
   cd src/GymHero.Api
   dotnet ef database update
   ```

4. **Run the Backend**:
   ```bash
   dotnet run
   ```

   API should be running at: `http://localhost:5001`

---

## 🎨 Step 3: Configure Frontend

1. **Install Dependencies**:
   ```bash
   cd frontend
   pnpm install
   ```

2. **Configure Environment**:
   Your `.env.local` should already point to localhost:
   ```bash
   # frontend/apps/web/.env.local
   NEXT_PUBLIC_API_BASE_URL=http://localhost:5001/api
   ```

3. **Run the Frontend**:
   ```bash
   pnpm dev
   ```

   Web app should be running at: `http://localhost:3000`

---

## 🔐 Environment Variables Reference

### Backend (appsettings.Development.json)

| Setting | Description | Example |
|---------|-------------|---------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string | `Host=localhost;Port=5432;Database=gymhero2_db;...` |
| `JwtSettings:Secret` | JWT signing key (64+ chars) | Generate: `node -e "console.log(require('crypto').randomBytes(64).toString('base64'))"` |
| `JwtSettings:ExpiryMinutes` | Token expiration time | `60` (1 hour) |
| `Gemini:ApiKey` | Google Gemini AI API key | Get from: https://ai.google.dev/ |
| `OpenAI:ApiKey` | OpenAI API key (optional) | Get from: https://platform.openai.com/ |

### Frontend (.env.local)

| Variable | Description | Dev Value | Production Value |
|----------|-------------|-----------|------------------|
| `NEXT_PUBLIC_API_BASE_URL` | Backend API URL | `http://localhost:5001/api` | `https://your-api.azurewebsites.net/api` |

---

## 🚀 Daily Development Workflow

### Starting Development

```bash
# Terminal 1: Start Database (if using Docker)
docker-compose up -d

# Terminal 2: Start Backend
cd src/GymHero.Api
dotnet run

# Terminal 3: Start Frontend
cd frontend
pnpm dev
```

### Stopping Development

```bash
# Stop backend: Ctrl+C in Terminal 2
# Stop frontend: Ctrl+C in Terminal 3
# Stop database:
docker-compose down
```

---

## 🔄 Working with Migrations

### Create a New Migration

```bash
cd src/GymHero.Api
dotnet ef migrations add YourMigrationName --startup-project GymHero.Api.csproj
```

### Apply Migrations

```bash
# Development
dotnet ef database update

# Production (via Azure or manually)
# Migrations are applied automatically on deployment
```

### Remove Last Migration (if not applied)

```bash
dotnet ef migrations remove
```

---

## 🌍 Environment Separation

| Environment | Database | API URL | Frontend URL | Purpose |
|-------------|----------|---------|--------------|---------|
| **Development** | `gymhero2_db` (localhost) | `localhost:5001` | `localhost:3000` | Local development & testing |
| **Production** | Azure PostgreSQL | `taktiq-api-*.azurewebsites.net` | `taktiq-web-*.azurewebsites.net` | Live application |

---

## 🧪 Testing Features

### Email (Password Reset)

In development, password reset codes are **logged to console** instead of being emailed.

Look for logs like:
```
========================================
PASSWORD RESET EMAIL
To: user@example.com
Reset Token: 123456
========================================
```

### AI Features

- Uses Gemini API (free tier available)
- Falls back to OpenAI if configured
- Falls back to mock responses if no API keys

---

## 📝 Best Practices

### ✅ DO:
- ✅ Keep `.env.local` and `appsettings.Development.json` private
- ✅ Use different databases for dev and production
- ✅ Test migrations locally before deploying
- ✅ Use `git pull` before starting work
- ✅ Commit often with clear messages

### ❌ DON'T:
- ❌ **Never** commit API keys or secrets to Git
- ❌ **Never** test directly on production database
- ❌ **Never** use production API keys in development
- ❌ **Never** push directly to main without testing

---

## 🐛 Troubleshooting

### Database Connection Failed
```
Solution: Check PostgreSQL is running
docker ps  # Should show postgres container
```

### Port Already in Use (5001 or 3000)
```
Solution: Kill the process using the port
Windows: netstat -ano | findstr :5001
         taskkill /PID <PID> /F
Linux/Mac: lsof -i :5001
           kill -9 <PID>
```

### Migrations Not Applying
```bash
Solution 1: Drop and recreate database
dropdb gymhero_dev
createdb gymhero_dev
dotnet ef database update

Solution 2: Reset migrations
# Delete all migration files
# Delete database
# Create fresh migration
dotnet ef migrations add Initial
dotnet ef database update
```

### Frontend Can't Connect to API
```
Solution: Check CORS settings and API URL
- Verify backend is running on http://localhost:5001
- Check .env.local has correct API URL
- Check browser console for errors
```

---

## 📚 Useful Commands

### Backend
```bash
# Build
dotnet build

# Run in watch mode (auto-reload)
dotnet watch run

# Run tests
dotnet test

# Clean build artifacts
dotnet clean
```

### Frontend
```bash
# Install dependencies
pnpm install

# Development mode
pnpm dev

# Type check
pnpm type-check

# Build for production
pnpm build

# Start production build
pnpm start
```

### Database
```bash
# View PostgreSQL logs
docker logs gymhero_dev_db

# Connect to database
docker exec -it gymhero_dev_db psql -U gymuser -d gymhero_dev

# Backup database
docker exec gymhero_dev_db pg_dump -U gymuser gymhero_dev > backup.sql

# Restore database
docker exec -i gymhero_dev_db psql -U gymuser gymhero_dev < backup.sql
```

---

## 🎯 Next Steps

1. ✅ Follow this guide to set up your local environment
2. ✅ Test that both backend and frontend run successfully
3. ✅ Create a test user and try the main features
4. ✅ Make a small change and verify it works
5. ✅ Read the main README.md for architecture details

---

## 🆘 Need Help?

- Check existing issues on GitHub
- Review error logs in `logs/` directory
- Use browser DevTools console for frontend issues
- Check backend logs in terminal

---

**Happy coding! 🚀**
