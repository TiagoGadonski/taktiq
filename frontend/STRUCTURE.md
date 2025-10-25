# GymHero Frontend - Detailed Folder Structure

This document provides a comprehensive breakdown of the monorepo structure with explanations for each major file and directory.

## Root Level

```
frontend/
├── apps/                   # Application workspaces
├── packages/               # Shared packages
├── .github/               # GitHub Actions workflows
├── node_modules/          # Root dependencies (auto-generated)
├── .turbo/                # Turborepo cache (auto-generated)
├── .env.example           # Environment variables template
├── .gitignore            # Git ignore rules
├── .prettierrc           # Prettier configuration
├── docker-compose.yml     # Production Docker setup
├── docker-compose.dev.yml # Development Docker setup
├── package.json          # Root package.json with workspace scripts
├── pnpm-lock.yaml        # pnpm lockfile
├── pnpm-workspace.yaml   # pnpm workspace configuration
├── turbo.json            # Turborepo pipeline configuration
├── README.md             # Main documentation
└── STRUCTURE.md          # This file
```

---

## Apps Directory

### Web App (apps/web/)

```
apps/web/
├── src/
│   ├── app/                           # Next.js 14 App Router
│   │   ├── (auth)/                   # Route group for auth pages
│   │   │   ├── login/
│   │   │   │   └── page.tsx          # Login page
│   │   │   └── signup/
│   │   │       └── page.tsx          # Signup page
│   │   │
│   │   ├── (app)/                    # Route group for authenticated pages
│   │   │   ├── dashboard/
│   │   │   │   └── page.tsx          # Dashboard with stats and workout summary
│   │   │   ├── workout/
│   │   │   │   └── page.tsx          # Today's workout execution
│   │   │   ├── history/
│   │   │   │   └── page.tsx          # Workout history with filters
│   │   │   ├── plans/
│   │   │   │   └── page.tsx          # Workout plan management
│   │   │   ├── challenges/
│   │   │   │   └── page.tsx          # Challenge creation and tracking
│   │   │   ├── progress/
│   │   │   │   └── page.tsx          # Progress charts and analytics
│   │   │   ├── profile/
│   │   │   │   └── page.tsx          # User profile and settings
│   │   │   └── layout.tsx            # Authenticated layout with sidebar
│   │   │
│   │   ├── layout.tsx                # Root layout with providers
│   │   ├── page.tsx                  # Home page (redirects to dashboard)
│   │   ├── providers.tsx             # React Query and Theme providers
│   │   └── globals.css               # Global styles and Tailwind imports
│   │
│   ├── components/
│   │   ├── ui/                       # shadcn/ui components
│   │   │   ├── button.tsx
│   │   │   ├── card.tsx
│   │   │   ├── input.tsx
│   │   │   ├── label.tsx
│   │   │   ├── toast.tsx
│   │   │   ├── toaster.tsx
│   │   │   └── use-toast.ts         # Toast hook
│   │   │
│   │   ├── workout/                 # Workout-specific components
│   │   │   ├── exercise-card.tsx
│   │   │   ├── set-row.tsx
│   │   │   └── workout-timer.tsx
│   │   │
│   │   ├── progress/                # Progress/chart components
│   │   │   ├── volume-chart.tsx
│   │   │   └── pr-list.tsx
│   │   │
│   │   └── layout/                  # Layout components
│   │       ├── sidebar.tsx
│   │       └── header.tsx
│   │
│   ├── hooks/                       # Custom React hooks
│   │   ├── use-auth.ts             # Authentication hook
│   │   ├── use-workout.ts          # Workout management
│   │   ├── use-challenges.ts       # Challenge management
│   │   └── use-progress.ts         # Progress data
│   │
│   └── lib/                        # Utilities
│       ├── api.ts                  # API client initialization
│       └── utils.ts                # Helper functions (cn, etc.)
│
├── public/                         # Static assets
│   ├── favicon.ico
│   └── images/
│
├── .dockerignore
├── .eslintrc.json                 # ESLint configuration
├── Dockerfile                     # Production Docker image
├── next.config.js                 # Next.js configuration
├── package.json                   # Web app dependencies
├── postcss.config.js              # PostCSS configuration
├── tailwind.config.ts             # Tailwind CSS configuration
├── tsconfig.json                  # TypeScript configuration
└── vitest.config.ts               # Vitest test configuration
```

### Mobile App (apps/mobile/)

```
apps/mobile/
├── app/                           # Expo Router pages
│   ├── (auth)/                   # Auth screens
│   │   ├── login.tsx             # Login screen
│   │   ├── signup.tsx            # Signup screen
│   │   └── _layout.tsx           # Auth layout
│   │
│   ├── (tabs)/                   # Tab navigation
│   │   ├── index.tsx             # Home/Dashboard tab
│   │   ├── session.tsx           # Active workout session
│   │   ├── challenges.tsx        # Challenges tab
│   │   ├── progress.tsx          # Progress/stats tab
│   │   ├── profile.tsx           # Profile tab
│   │   └── _layout.tsx           # Tab layout with bottom navigation
│   │
│   └── _layout.tsx               # Root layout with providers
│
├── src/
│   ├── components/               # React Native components
│   │   ├── workout/
│   │   │   ├── ExerciseCard.tsx
│   │   │   ├── SetRow.tsx
│   │   │   └── WorkoutTimer.tsx
│   │   │
│   │   ├── progress/
│   │   │   └── VolumeChart.tsx
│   │   │
│   │   └── common/
│   │       ├── Button.tsx
│   │       ├── Card.tsx
│   │       └── Input.tsx
│   │
│   ├── hooks/                    # Custom hooks
│   │   ├── use-auth.ts
│   │   ├── use-workout.ts
│   │   └── use-haptics.ts       # Haptic feedback hook
│   │
│   └── lib/                     # Utilities
│       └── api.ts               # API client with Expo SecureStore
│
├── assets/                      # Images, fonts, etc.
│   ├── icon.png
│   ├── splash.png
│   └── adaptive-icon.png
│
├── .gitignore
├── app.json                     # Expo configuration
├── babel.config.js              # Babel configuration
├── eas.json                     # EAS Build configuration (optional)
├── global.css                   # Global styles for NativeWind
├── metro.config.js              # Metro bundler configuration
├── package.json                 # Mobile app dependencies
├── tailwind.config.js           # Tailwind configuration for NativeWind
└── tsconfig.json                # TypeScript configuration
```

---

## Packages Directory

### Shared Package (packages/shared/)

```
packages/shared/
├── src/
│   ├── types/                   # TypeScript type definitions
│   │   └── index.ts            # All type exports
│   │       - User, AuthTokens
│   │       - Exercise, WorkoutPlan, Workout
│   │       - WorkoutSession, WorkoutSet
│   │       - ProgressDashboard, Challenge, Badge
│   │       - API response types
│   │
│   ├── validation/              # Zod schemas
│   │   ├── auth.ts             # Login, signup, password schemas
│   │   ├── workout.ts          # Exercise, workout, set schemas
│   │   └── challenge.ts        # Challenge schemas
│   │
│   ├── api/                    # API client
│   │   ├── client.ts           # Axios client with interceptors
│   │   │   - Token refresh logic
│   │   │   - Error handling
│   │   │   - Request/response interceptors
│   │   │
│   │   └── endpoints.ts        # API endpoint classes
│   │       - AuthApi
│   │       - ExerciseApi
│   │       - WorkoutPlanApi
│   │       - SessionApi
│   │       - SetApi
│   │       - ProgressApi
│   │       - ChallengeApi
│   │       - BadgeApi
│   │
│   ├── utils/                  # Utility functions
│   │   ├── storage.ts         # Abstract storage interface
│   │   │   - WebStorage (localStorage)
│   │   │   - TokenStorage
│   │   │
│   │   ├── format.ts          # Formatting utilities
│   │   │   - Date/time formatting
│   │   │   - Weight/distance conversion
│   │   │   - Duration formatting
│   │   │
│   │   └── logger.ts          # Logging utility
│   │
│   └── index.ts               # Main exports
│
├── package.json
├── tsconfig.json
└── vitest.config.ts
```

---

## Configuration Files Explained

### Root Configuration

**package.json**
```json
{
  "workspaces": ["apps/*", "packages/*"],
  "scripts": {
    "dev": "turbo run dev",
    "build": "turbo run build",
    "test": "turbo run test"
  }
}
```
- Defines workspace structure
- Provides monorepo-wide scripts using Turborepo

**pnpm-workspace.yaml**
```yaml
packages:
  - 'apps/*'
  - 'packages/*'
```
- Configures pnpm workspaces
- Tells pnpm where to find packages

**turbo.json**
```json
{
  "pipeline": {
    "build": { "dependsOn": ["^build"] },
    "dev": { "cache": false },
    "test": { "dependsOn": ["^build"] }
  }
}
```
- Defines task dependencies
- Configures caching strategy
- Optimizes build performance

### Web App Configuration

**next.config.js**
- Transpiles shared package
- Configures environment variables
- Sets up image domains

**tailwind.config.ts**
- Dark mode class strategy
- Custom color palette
- shadcn/ui theming variables

**tsconfig.json**
- Path aliases (@/*)
- Strict mode enabled
- Next.js plugin integration

### Mobile App Configuration

**app.json**
- Expo configuration
- App name, slug, version
- Platform-specific settings
- Plugin configuration

**metro.config.js**
- Workspace package resolution
- Monorepo support
- Symlink handling

**babel.config.js**
- NativeWind plugin
- Reanimated plugin
- Expo preset

---

## Key Files and Their Purpose

| File | Purpose |
|------|---------|
| `frontend/package.json` | Root package with monorepo scripts |
| `frontend/turbo.json` | Turborepo pipeline configuration |
| `apps/web/src/app/layout.tsx` | Root layout with providers |
| `apps/web/src/lib/api.ts` | API client initialization for web |
| `apps/mobile/app/_layout.tsx` | Root layout for mobile |
| `apps/mobile/src/lib/api.ts` | API client with SecureStore for mobile |
| `packages/shared/src/api/client.ts` | Universal API client with JWT refresh |
| `packages/shared/src/types/index.ts` | All TypeScript interfaces |
| `packages/shared/src/validation/` | Zod schemas for form validation |

---

## Data Flow

### Authentication Flow

1. User submits login form
2. Form validated with Zod schema
3. API request sent via shared API client
4. Tokens stored (localStorage for web, SecureStore for mobile)
5. React Query invalidates and refetches user data
6. App redirects to dashboard

### Workout Session Flow

1. User starts session via `api.sessions.start()`
2. Session stored in React Query cache
3. User adds sets via `api.sets.create()`
4. Optimistic updates with React Query
5. Real-time sync with backend
6. Session completed with `api.sessions.complete()`

### Offline Support

- React Query caches all data
- Failed requests queued for retry
- Syncs when connection restored
- Works on both web and mobile

---

## Development Workflow

1. **Install dependencies**: `pnpm install`
2. **Start backend**: Backend must be running at configured URL
3. **Start frontend**: `pnpm dev` (starts both apps)
4. **Make changes**: Edit files in `apps/` or `packages/`
5. **Turborepo auto-rebuilds**: Changed packages rebuild automatically
6. **Test**: `pnpm test`
7. **Build**: `pnpm build`

---

## Important Notes

- All shared code lives in `packages/shared`
- Web and mobile import from `@gymhero/shared`
- API client automatically handles token refresh
- Forms use react-hook-form + Zod for validation
- Dark mode is default on both platforms
- i18n ready (currently pt-BR and en-US)

---

**Last Updated**: 2025-01-16
