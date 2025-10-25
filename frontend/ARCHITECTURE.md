# GymHero Frontend - Architecture Overview

This document explains the architectural decisions, patterns, and best practices used in the GymHero monorepo.

## Table of Contents

1. [Monorepo Architecture](#monorepo-architecture)
2. [Shared Code Strategy](#shared-code-strategy)
3. [Authentication Flow](#authentication-flow)
4. [Data Fetching & State Management](#data-fetching--state-management)
5. [API Client Design](#api-client-design)
6. [Routing](#routing)
7. [Styling Approach](#styling-approach)
8. [Testing Strategy](#testing-strategy)
9. [Build & Deployment](#build--deployment)
10. [Performance Optimizations](#performance-optimizations)

---

## Monorepo Architecture

### Why Monorepo?

We chose a monorepo structure with Turborepo for:

1. **Code Sharing**: Web and mobile share 60%+ of business logic
2. **Atomic Changes**: Update API types once, both apps get it
3. **Consistent Versioning**: All packages stay in sync
4. **Developer Experience**: Single `pnpm install`, unified workflows
5. **Build Optimization**: Turborepo caches and parallelizes builds

### Workspace Structure

```
frontend/
├── apps/          # Deployable applications
│   ├── web/       # Next.js (user-facing web)
│   └── mobile/    # Expo (native iOS/Android)
└── packages/      # Shared libraries
    └── shared/    # Types, API client, utils
```

### Why This Structure?

- **apps/**: Platform-specific UI and routing
- **packages/shared**: Universal business logic
- Clear separation of concerns
- Easy to add new apps (admin panel, desktop, etc.)

---

## Shared Code Strategy

### What Goes in packages/shared?

✅ **Include:**
- TypeScript types and interfaces
- Zod validation schemas
- API client and endpoints
- Business logic utilities
- Data formatting functions
- Constants and enums

❌ **Exclude:**
- React components (platform-specific rendering)
- Platform-specific APIs (localStorage vs SecureStore)
- Routing logic
- UI styling

### Package Boundaries

```
apps/web  ──┐
            ├──> packages/shared ──> Backend API
apps/mobile ┘
```

- Apps depend on shared
- Shared is platform-agnostic
- Shared never imports from apps

### Type Safety

All shared code is fully typed:

```typescript
// packages/shared/src/types/index.ts
export interface User {
  id: string;
  email: string;
  name: string;
}

// Used in both apps
import type { User } from '@gymhero/shared';
```

---

## Authentication Flow

### JWT Token Management

```
┌─────────────┐
│   Client    │
│  (Web/App)  │
└──────┬──────┘
       │ 1. Login (email, password)
       ▼
┌─────────────┐
│  API Client │
│   (Shared)  │
└──────┬──────┘
       │ 2. POST /auth/login
       ▼
┌─────────────┐
│   Backend   │
│  (.NET API) │
└──────┬──────┘
       │ 3. Returns { accessToken, refreshToken }
       ▼
┌─────────────┐
│   Storage   │
│ (Web/Mobile)│
└──────┬──────┘
       │ 4. Store tokens
       │    - Web: localStorage + memory
       │    - Mobile: SecureStore + memory
       ▼
┌─────────────┐
│ React Query │
│   Cache     │
└──────┬──────┘
       │ 5. Fetch user data
       ▼
┌─────────────┐
│  Dashboard  │
└─────────────┘
```

### Token Refresh

When a 401 Unauthorized is received:

1. API client intercepts the error
2. Checks if refresh token exists
3. Calls `/auth/refresh` with refresh token
4. Gets new access + refresh tokens
5. Retries original request
6. If refresh fails, logs user out

**Implementation:**
- `packages/shared/src/api/client.ts` handles all refresh logic
- Automatic retry with exponential backoff
- Queue concurrent requests during refresh

---

## Data Fetching & State Management

### Why TanStack Query?

We use **TanStack Query** (formerly React Query) for:

1. **Automatic Caching**: Reduces API calls
2. **Background Refetch**: Keeps data fresh
3. **Optimistic Updates**: Instant UI feedback
4. **Request Deduplication**: Prevents duplicate fetches
5. **Offline Support**: Graceful degradation

### Query Structure

```typescript
// Example: Fetch dashboard data
const { data: progress, isLoading } = useQuery({
  queryKey: ['progress', 'dashboard'],
  queryFn: () => api.progress.getDashboard(),
  staleTime: 5 * 60 * 1000, // 5 minutes
});
```

### Mutation Pattern

```typescript
// Example: Complete a workout set
const mutation = useMutation({
  mutationFn: (set: CreateSetInput) => api.sets.create(set),
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ['sessions', 'current'] });
  },
  onError: (error) => {
    toast({ title: 'Error', description: error.message });
  },
});
```

### Cache Strategy

| Data Type | Stale Time | Refetch on Focus |
|-----------|------------|------------------|
| User profile | 5 min | Yes |
| Dashboard stats | 1 min | Yes |
| Exercise list | 15 min | No |
| Session history | 5 min | Yes |
| Current session | 30 sec | Yes |

---

## API Client Design

### Axios Interceptor Pattern

```typescript
// Request Interceptor
axios.interceptors.request.use((config) => {
  // Add Authorization header
  config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Response Interceptor
axios.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      // Trigger token refresh
      await refreshToken();
      // Retry original request
      return axios(error.config);
    }
    return Promise.reject(error);
  }
);
```

### API Endpoint Classes

Each resource has its own class:

```typescript
class WorkoutPlanApi {
  constructor(private client: ApiClient) {}

  async getAll(): Promise<WorkoutPlan[]> {
    return this.client.get<WorkoutPlan[]>('/workout-plans');
  }

  async create(data: Partial<WorkoutPlan>): Promise<WorkoutPlan> {
    return this.client.post<WorkoutPlan>('/workout-plans', data);
  }
}
```

Benefits:
- Type-safe API calls
- Centralized error handling
- Easy to mock for testing
- Auto-complete in IDE

---

## Routing

### Web (Next.js 14 App Router)

File-based routing with route groups:

```
app/
├── (auth)/        # Public routes
│   ├── login/
│   └── signup/
└── (app)/         # Protected routes
    ├── dashboard/
    ├── workout/
    └── layout.tsx # Sidebar + auth check
```

**Benefits:**
- Automatic code splitting
- Parallel loading
- Shared layouts
- Server components (opt-in)

### Mobile (Expo Router)

File-based routing with tabs:

```
app/
├── (auth)/        # Auth stack
│   └── login.tsx
└── (tabs)/        # Bottom tabs
    ├── index.tsx      # Home
    ├── session.tsx    # Workout
    └── _layout.tsx    # Tab bar
```

**Benefits:**
- Native navigation
- Type-safe routes
- Deep linking support
- Tab persistence

---

## Styling Approach

### Web: Tailwind CSS + shadcn/ui

**Why?**
- Utility-first CSS for rapid development
- No runtime JS overhead
- Consistent design system
- Pre-built accessible components

**Example:**
```tsx
<div className="flex flex-col gap-4 p-6 bg-card rounded-lg">
  <h1 className="text-2xl font-bold">Dashboard</h1>
  <Button variant="primary">Start Workout</Button>
</div>
```

### Mobile: NativeWind

**Why?**
- Same Tailwind syntax as web
- Compiles to React Native styles
- Shared design tokens
- No StyleSheet overhead

**Example:**
```tsx
<View className="flex-1 px-6 bg-background">
  <Text className="text-2xl font-bold text-foreground">Dashboard</Text>
  <TouchableOpacity className="bg-primary rounded-lg py-3">
    <Text className="text-primary-foreground">Start Workout</Text>
  </TouchableOpacity>
</View>
```

### Design Tokens

Shared color palette in both configs:

```javascript
colors: {
  primary: '#3b82f6',
  background: '#0f172a',
  foreground: '#f8fafc',
  // ... more colors
}
```

---

## Testing Strategy

### Unit Tests

**Shared Package:**
- Vitest for utilities and validation
- 80%+ coverage target

**Web:**
- Vitest for components and hooks
- React Testing Library

**Mobile:**
- Jest for components and hooks
- React Native Testing Library

### Integration Tests

**Web:**
- Playwright for E2E flows
- Login → Create Workout → Complete Session

**Mobile:**
- Detox or Maestro (optional)
- Critical user journeys

### API Mocking

```typescript
// Mock API client for tests
const mockApi = {
  auth: {
    login: vi.fn().mockResolvedValue(mockTokens),
  },
};
```

---

## Build & Deployment

### Web App

**Development:**
```bash
pnpm dev:web  # Next.js dev server with hot reload
```

**Production:**
```bash
pnpm build:web  # Optimized build
```

**Deployment Options:**
1. **Vercel** (recommended) - Zero config, automatic HTTPS
2. **Docker** - Containerized deployment
3. **Azure Static Web Apps** - Serverless hosting

### Mobile App

**Development:**
```bash
pnpm dev:mobile  # Expo dev server
```

**Production:**
```bash
eas build --platform ios
eas build --platform android
```

**Distribution:**
1. **Expo Go** - Development testing
2. **TestFlight** - iOS beta testing
3. **App Store / Play Store** - Production

---

## Performance Optimizations

### Web

1. **Code Splitting**: Automatic with Next.js App Router
2. **Image Optimization**: next/image with lazy loading
3. **Bundle Size**: Analyzed with Next.js bundle analyzer
4. **Caching**: TanStack Query + SWR pattern
5. **Server Components**: Opt-in for static content

### Mobile

1. **Lazy Loading**: React.lazy for heavy screens
2. **FlatList**: Virtualized lists for large datasets
3. **Reanimated**: 60 FPS animations
4. **Hermes**: JavaScript engine optimization
5. **Bundle Optimization**: Metro tree-shaking

### Shared

1. **Tree Shaking**: ESM exports for optimal bundles
2. **Type Stripping**: Runtime type checks removed in production
3. **API Batching**: Multiple requests combined when possible

---

## Security Considerations

1. **Token Storage**:
   - Web: localStorage (XSS protected)
   - Mobile: SecureStore (encrypted)
   - Memory fallback for added security

2. **API Communication**:
   - HTTPS only in production
   - CORS configured on backend
   - Rate limiting on sensitive endpoints

3. **Input Validation**:
   - Zod schemas on client
   - DTO validation on server
   - Prevents injection attacks

4. **Authentication**:
   - JWT with short expiration (15 min)
   - Refresh tokens (7 days)
   - Automatic logout on tampered tokens

---

## Future Enhancements

### Planned Features

1. **Offline-First**:
   - WatermelonDB or SQLite
   - Sync queue for offline actions
   - Conflict resolution

2. **Real-Time Updates**:
   - WebSocket support
   - SignalR integration
   - Live workout tracking

3. **PWA Support**:
   - Service workers
   - Install prompts
   - Offline mode

4. **Admin Panel**:
   - New app in monorepo
   - Shared admin API client
   - User management

5. **Analytics**:
   - Mixpanel or Amplitude
   - User behavior tracking
   - Performance monitoring

---

## Conclusion

This architecture provides:

- ✅ Scalable monorepo structure
- ✅ Maximum code reuse (60%+ shared)
- ✅ Type safety end-to-end
- ✅ Excellent developer experience
- ✅ Production-ready deployment
- ✅ Easy to maintain and extend

**Questions?** Open an issue or contact the team!

---

**Last Updated**: 2025-01-16
