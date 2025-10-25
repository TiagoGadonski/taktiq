# GymHero Frontend - Quick Start Guide

Get up and running with the GymHero monorepo in 5 minutes.

## Prerequisites Check

Before starting, ensure you have:

```bash
# Check Node.js version (should be >= 20.0.0)
node --version

# Check if pnpm is installed
pnpm --version

# If pnpm is not installed
npm install -g pnpm
```

## Step 1: Install Dependencies (2 min)

```bash
cd C:\Users\cwbcordeti\source\gymhero2\frontend

# Install all workspace dependencies
pnpm install
```

This installs dependencies for:
- Web app
- Mobile app
- Shared package

## Step 2: Configure Environment (1 min)

```bash
# Copy the example environment file
cp .env.example .env
```

Edit `.env` if your backend is not at `https://localhost:5001`:

```env
NEXT_PUBLIC_API_BASE_URL=https://your-backend-url
EXPO_PUBLIC_API_BASE_URL=https://your-backend-url
```

## Step 3: Start the Backend

Make sure your .NET backend is running:

```bash
cd ../backend
dotnet run
```

Backend should be accessible at `https://localhost:5001`.

## Step 4: Choose Your Platform

### Option A: Web Development

```bash
pnpm dev:web
```

Open http://localhost:3000

Default credentials (if seeded):
- Email: `admin@gymhero.com`
- Password: `Admin123!`

### Option B: Mobile Development

```bash
pnpm dev:mobile
```

Then:
- Press `i` for iOS Simulator (macOS only)
- Press `a` for Android Emulator
- Scan QR code with Expo Go app

### Option C: Both (Recommended)

```bash
pnpm dev
```

Starts both web and mobile in parallel.

## Step 5: Verify Everything Works

### Web App:
1. Navigate to http://localhost:3000
2. You should see the login page
3. Click "Criar conta" to sign up
4. Fill in the form and submit
5. You should be redirected to the dashboard

### Mobile App:
1. Open the Expo app on your device
2. Scan the QR code
3. App should load and show the login screen
4. Sign up or log in
5. Navigate through tabs

## Common First-Time Issues

### Issue: Port 3000 already in use

**Solution:**
```bash
PORT=3001 pnpm dev:web
```

### Issue: Backend connection refused

**Solution:**
1. Ensure backend is running
2. Check backend URL in `.env`
3. For mobile, use your computer's IP instead of localhost:
   ```env
   EXPO_PUBLIC_API_BASE_URL=https://192.168.1.100:5001
   ```

### Issue: pnpm install fails

**Solution:**
```bash
# Clear cache
pnpm store prune

# Remove all node_modules
rm -rf node_modules apps/*/node_modules packages/*/node_modules

# Reinstall
pnpm install
```

### Issue: TypeScript errors

**Solution:**
```bash
# Rebuild shared package
pnpm --filter @gymhero/shared build

# Restart your IDE's TypeScript server
```

### Issue: Mobile app can't connect to backend

**Solution:**
1. Trust the dev certificate on your mobile device
2. Use your local IP address instead of localhost
3. Ensure your device is on the same network

## Next Steps

### Explore the Code

**Web App:**
- `apps/web/src/app/(app)/dashboard/page.tsx` - Dashboard
- `apps/web/src/hooks/use-auth.ts` - Auth hook
- `apps/web/src/lib/api.ts` - API setup

**Mobile App:**
- `apps/mobile/app/(tabs)/index.tsx` - Home screen
- `apps/mobile/src/hooks/use-auth.ts` - Auth hook
- `apps/mobile/src/lib/api.ts` - API setup

**Shared:**
- `packages/shared/src/types/index.ts` - Types
- `packages/shared/src/api/endpoints.ts` - API endpoints
- `packages/shared/src/validation/` - Zod schemas

### Make Your First Change

1. Open `apps/web/src/app/(app)/dashboard/page.tsx`
2. Change the welcome message
3. Save the file
4. See the change hot-reload instantly

### Run Tests

```bash
pnpm test
```

### Build for Production

```bash
pnpm build
```

## Development Tips

1. **Turborepo caching**: Turborepo caches build outputs. Clear cache with `rm -rf .turbo`

2. **Shared package changes**: When you modify `packages/shared`, dependent apps auto-rebuild

3. **API client**: All API calls go through the shared API client with automatic token refresh

4. **Forms**: Use react-hook-form + Zod for type-safe form validation

5. **Styling**:
   - Web: Tailwind + shadcn/ui
   - Mobile: NativeWind (Tailwind for RN)

## Helpful Commands

```bash
# Development
pnpm dev              # Start all apps
pnpm dev:web          # Web only
pnpm dev:mobile       # Mobile only

# Building
pnpm build            # Build all
pnpm build:web        # Web only
pnpm build:mobile     # Mobile only

# Testing
pnpm test             # Run all tests
pnpm test:web         # Web tests only
pnpm test:mobile      # Mobile tests only

# Type checking
pnpm type-check       # Check all packages

# Linting
pnpm lint             # Lint all packages

# Clean
pnpm clean            # Remove node_modules and build artifacts
```

## Resources

- [Full Documentation](./README.md)
- [Folder Structure](./STRUCTURE.md)
- [Next.js Docs](https://nextjs.org/docs)
- [Expo Docs](https://docs.expo.dev/)
- [TanStack Query](https://tanstack.com/query/latest)

## Getting Help

If you encounter issues:

1. Check the [Troubleshooting](./README.md#troubleshooting) section
2. Review the [folder structure](./STRUCTURE.md)
3. Open an issue on GitHub
4. Contact the development team

**Happy coding!**
