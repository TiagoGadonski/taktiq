# GymHero Frontend Monorepo

> Unified Web + Mobile frontend for GymHero - Smart Workout Tracking and Gamified Fitness Challenges

This is a production-ready monorepo setup that shares code between a **Next.js 14 web app** and a **React Native Expo mobile app**, powered by **Turborepo** and **pnpm workspaces**.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Development](#development)
- [Testing](#testing)
- [Deployment](#deployment)
- [Environment Variables](#environment-variables)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)

---

## Features

### Web App (Next.js 14)
- Personal dashboard with workout summaries and stats
- Workout of the day with exercise tracking
- History with filters and search
- Workout plan management
- Challenge creation and tracking
- Progress visualization with charts (Recharts)
- Profile settings and preferences
- Dark mode by default
- Keyboard shortcuts
- Offline mode support

### Mobile App (React Native + Expo)
- Daily workout companion
- Swipe-based exercise navigation
- Quick set input with haptic feedback
- Challenge tracking
- Progress charts and session history
- AsyncStorage for offline support
- Push notifications (ready for implementation)

### Shared Capabilities
- JWT authentication with automatic token refresh
- TanStack Query for data fetching and caching
- React Hook Form + Zod for form validation
- Internationalization (pt-BR and en-US)
- Type-safe API client
- Unified business logic

---

## Tech Stack

### Monorepo Management
- **Turborepo** - Build system and task runner
- **pnpm** - Fast, disk space efficient package manager

### Web (apps/web)
- **Next.js 14** - React framework with App Router
- **Tailwind CSS** - Utility-first CSS
- **shadcn/ui** - High-quality React components
- **Recharts** - Charting library
- **Vitest** - Unit testing
- **Playwright** - E2E testing

### Mobile (apps/mobile)
- **React Native** - Native mobile framework
- **Expo SDK** - Managed workflow and tools
- **NativeWind** - Tailwind for React Native
- **Expo Router** - File-based routing
- **Jest** - Unit testing
- **Detox/Maestro** - E2E testing (optional)

### Shared (packages/shared)
- **TypeScript** - Type safety
- **Zod** - Schema validation
- **TanStack Query** - Data fetching and caching
- **Axios** - HTTP client

---

## Project Structure

```
frontend/
├── apps/
│   ├── web/                    # Next.js 14 web application
│   │   ├── src/
│   │   │   ├── app/           # App Router pages
│   │   │   │   ├── (auth)/   # Auth layout group
│   │   │   │   │   ├── login/
│   │   │   │   │   └── signup/
│   │   │   │   ├── (app)/    # Main app layout group
│   │   │   │   │   ├── dashboard/
│   │   │   │   │   ├── workout/
│   │   │   │   │   ├── history/
│   │   │   │   │   ├── plans/
│   │   │   │   │   ├── challenges/
│   │   │   │   │   ├── progress/
│   │   │   │   │   └── profile/
│   │   │   │   ├── layout.tsx
│   │   │   │   ├── page.tsx
│   │   │   │   └── providers.tsx
│   │   │   ├── components/    # React components
│   │   │   │   └── ui/       # shadcn/ui components
│   │   │   ├── hooks/        # Custom React hooks
│   │   │   └── lib/          # Utilities and API setup
│   │   ├── public/           # Static assets
│   │   ├── Dockerfile
│   │   ├── next.config.js
│   │   ├── tailwind.config.ts
│   │   └── package.json
│   │
│   └── mobile/                # React Native Expo application
│       ├── app/              # Expo Router pages
│       │   ├── (auth)/      # Auth screens
│       │   │   ├── login.tsx
│       │   │   └── signup.tsx
│       │   ├── (tabs)/      # Tab navigation
│       │   │   ├── index.tsx     # Home
│       │   │   ├── session.tsx   # Workout
│       │   │   ├── challenges.tsx
│       │   │   ├── progress.tsx
│       │   │   └── profile.tsx
│       │   └── _layout.tsx
│       ├── src/
│       │   ├── components/  # React Native components
│       │   ├── hooks/       # Custom hooks
│       │   └── lib/         # Utilities and API setup
│       ├── assets/          # Images, fonts
│       ├── app.json
│       ├── babel.config.js
│       ├── metro.config.js
│       ├── tailwind.config.js
│       └── package.json
│
├── packages/
│   └── shared/               # Shared code between apps
│       ├── src/
│       │   ├── types/       # TypeScript interfaces
│       │   ├── validation/  # Zod schemas
│       │   ├── api/         # API client and endpoints
│       │   └── utils/       # Utility functions
│       ├── package.json
│       └── tsconfig.json
│
├── .github/
│   └── workflows/
│       └── ci.yml           # GitHub Actions CI/CD
│
├── docker-compose.yml       # Production setup
├── docker-compose.dev.yml   # Development setup
├── turbo.json              # Turborepo configuration
├── pnpm-workspace.yaml     # pnpm workspace config
├── package.json            # Root package.json
└── README.md
```

---

## Prerequisites

Ensure you have the following installed:

- **Node.js** >= 20.0.0
- **pnpm** >= 9.0.0
- **Docker** (optional, for containerized development)
- **Git**

For mobile development:
- **Expo CLI**: `npm install -g expo-cli`
- **EAS CLI** (for builds): `npm install -g eas-cli`
- **iOS Simulator** (macOS only) or **Android Studio/Emulator**

---

## Getting Started

### 1. Clone the Repository

```bash
cd C:\Users\cwbcordeti\source\gymhero2\frontend
```

### 2. Install Dependencies

```bash
pnpm install
```

This will install dependencies for all workspaces (web, mobile, shared).

### 3. Set Up Environment Variables

Copy the example environment file:

```bash
cp .env.example .env
```

Edit `.env` and configure your API base URL:

```env
NEXT_PUBLIC_API_BASE_URL=https://localhost:5001
EXPO_PUBLIC_API_BASE_URL=https://localhost:5001
```

### 4. Start the Backend

Make sure your .NET 8 backend is running:

```bash
cd ../backend
dotnet run
```

The backend should be accessible at `https://localhost:5001`.

---

## Development

### Run All Apps

```bash
pnpm dev
```

This starts both web and mobile apps in development mode.

### Run Web App Only

```bash
pnpm dev:web
```

Visit http://localhost:3000

### Run Mobile App Only

```bash
pnpm dev:mobile
```

Then:
- Press `i` for iOS simulator
- Press `a` for Android emulator
- Scan QR code with Expo Go app on your phone

### Build All Apps

```bash
pnpm build
```

### Build Web App Only

```bash
pnpm build:web
```

### Type Checking

```bash
pnpm type-check
```

### Linting

```bash
pnpm lint
```

---

## Testing

### Unit Tests

**Web:**
```bash
pnpm test:web
```

**Mobile:**
```bash
pnpm test:mobile
```

### E2E Tests (Web)

```bash
cd apps/web
pnpm test:e2e
```

---

## Deployment

### Web App

#### Option 1: Vercel (Recommended)

1. Install Vercel CLI:
```bash
npm i -g vercel
```

2. Deploy:
```bash
cd apps/web
vercel
```

3. Set environment variables in Vercel dashboard

#### Option 2: Docker + Azure/AWS

1. Build the Docker image:
```bash
docker build -f apps/web/Dockerfile -t gymhero-web .
```

2. Run the container:
```bash
docker run -p 3000:3000 -e NEXT_PUBLIC_API_BASE_URL=https://your-api.com gymhero-web
```

3. Deploy to Azure Container Instances, AWS ECS, or any container platform

#### Option 3: Azure Static Web Apps

1. Build the app:
```bash
pnpm build:web
```

2. Deploy using Azure CLI:
```bash
az staticwebapp create --name gymhero-web --resource-group <your-rg> --location eastus2
```

### Mobile App

#### Expo Application Services (EAS)

1. Login to EAS:
```bash
eas login
```

2. Configure EAS:
```bash
cd apps/mobile
eas build:configure
```

3. Create a development build:
```bash
eas build --profile development --platform android
eas build --profile development --platform ios
```

4. Create a production build:
```bash
eas build --profile production --platform android
eas build --profile production --platform ios
```

5. Submit to app stores:
```bash
eas submit --platform android
eas submit --platform ios
```

---

## Environment Variables

### Web App (.env)

| Variable | Description | Default |
|----------|-------------|---------|
| `NEXT_PUBLIC_API_BASE_URL` | Backend API base URL | `https://localhost:5001` |
| `NODE_ENV` | Environment mode | `development` |

### Mobile App

| Variable | Description | Default |
|----------|-------------|---------|
| `EXPO_PUBLIC_API_BASE_URL` | Backend API base URL | `https://localhost:5001` |

**Note:** Expo environment variables must be prefixed with `EXPO_PUBLIC_` to be accessible in the app.

---

## Troubleshooting

### Port Conflicts

If port 3000 is already in use:

**Web:**
```bash
PORT=3001 pnpm dev:web
```

**Mobile:**
Change the port in `metro.config.js` or use Expo's automatic port selection.

### pnpm Installation Issues

If pnpm fails to install:

```bash
# Clear pnpm cache
pnpm store prune

# Delete node_modules
rm -rf node_modules apps/*/node_modules packages/*/node_modules

# Reinstall
pnpm install
```

### Turborepo Cache Issues

Clear Turborepo cache:

```bash
rm -rf .turbo
pnpm build
```

### Mobile App Not Connecting to Backend

1. Make sure your backend is accessible from your mobile device
2. For local development, use your computer's IP address instead of `localhost`:
   ```env
   EXPO_PUBLIC_API_BASE_URL=https://192.168.1.100:5001
   ```
3. Trust the development certificate on your mobile device

### TypeScript Errors

```bash
# Rebuild shared package
pnpm --filter @gymhero/shared build

# Restart TypeScript server in your IDE
```

---

## Docker Development

### Development Mode

```bash
docker-compose -f docker-compose.dev.yml up
```

### Production Mode

```bash
docker-compose up --build
```

---

## GitHub Actions CI/CD

The project includes a GitHub Actions workflow (`.github/workflows/ci.yml`) that:

1. Runs on every push to `main` and PRs
2. Installs dependencies with pnpm
3. Type checks all packages
4. Runs linting
5. Runs tests
6. Builds all apps

To customize, edit `.github/workflows/ci.yml`.

---

## Contributing

1. Create a feature branch: `git checkout -b feature/my-feature`
2. Make your changes
3. Run tests: `pnpm test`
4. Type check: `pnpm type-check`
5. Lint: `pnpm lint`
6. Commit: `git commit -am 'Add my feature'`
7. Push: `git push origin feature/my-feature`
8. Open a Pull Request

---

## Additional Documentation

- [Next.js Documentation](https://nextjs.org/docs)
- [Expo Documentation](https://docs.expo.dev/)
- [Turborepo Documentation](https://turbo.build/repo/docs)
- [TanStack Query Documentation](https://tanstack.com/query/latest)
- [shadcn/ui Documentation](https://ui.shadcn.com/)

---

## License

Proprietary - GymHero 2025

---

## Support

For issues or questions:
- Open an issue on GitHub
- Contact the development team

**Happy coding and stay fit!**
