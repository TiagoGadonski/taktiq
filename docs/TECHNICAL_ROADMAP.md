# TaktIQ - Roadmap Técnico de Desenvolvimento

**Documento:** Roadmap Técnico e Infraestrutura
**Foco:** Desenvolvimento seguro, ambientes, CI/CD, features técnicas
**Data:** 08/12/2025

---

## 🚨 PROBLEMA CRÍTICO ATUAL

**Status Atual:** Todas as mudanças vão **DIRETO PARA PRODUÇÃO** ❌

**Riscos:**
- Bug em produção afeta todos os usuários
- Impossível testar features antes do deploy
- Rollback difícil e arriscado
- Dados de produção expostos em desenvolvimento
- Violação de boas práticas de DevOps

**Solução:** Implementar ambientes separados + pipeline de CI/CD robusto

---

## 🎯 FASE 0: Setup de Ambientes (PRIORIDADE MÁXIMA)

**Duração:** 1 semana
**Quando:** Imediatamente, antes de qualquer feature nova

### Objetivo
Criar 3 ambientes isolados:
1. **Development** (local + dev server)
2. **Staging** (cópia exata de produção)
3. **Production** (usuários reais)

---

### 🏗️ Arquitetura de Ambientes

```
┌─────────────────────────────────────────────────────────────┐
│                         DEVELOPMENT                         │
├─────────────────────────────────────────────────────────────┤
│ • Rodando localmente (localhost)                           │
│ • Banco de dados local (Docker PostgreSQL)                 │
│ • Hot reload ativado                                        │
│ • Debug mode ON                                             │
│ • API Mock (sem custo de APIs externas)                    │
│ • Branch: feature/* ou dev                                  │
└─────────────────────────────────────────────────────────────┘
                            ↓
                    git push to dev
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                          STAGING                            │
├─────────────────────────────────────────────────────────────┤
│ • Azure App Service (separado de produção)                 │
│ • URL: https://staging.taktiq.app                          │
│ • Banco de dados staging (PostgreSQL)                      │
│ • Dados fake/anonimizados                                  │
│ • Mesma configuração que produção                          │
│ • Testes automatizados executados                          │
│ • Branch: staging                                           │
└─────────────────────────────────────────────────────────────┘
                            ↓
                    Aprovação manual
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                         PRODUCTION                          │
├─────────────────────────────────────────────────────────────┤
│ • Azure App Service (atual)                                │
│ • URL: https://taktiq.app                                  │
│ • Banco de dados produção                                  │
│ • Dados reais de usuários                                  │
│ • Monitoramento 24/7                                       │
│ • Rollback automático se falhar                           │
│ • Branch: main                                              │
└─────────────────────────────────────────────────────────────┘
```

---

### 📋 Checklist de Implementação

#### 1. Ambiente de Development Local (Dia 1-2)

**Backend (API)**
- [ ] Criar `appsettings.Development.Local.json` (não commitado)
  ```json
  {
    "ConnectionStrings": {
      "DefaultConnection": "Host=localhost;Port=5432;Database=taktiq_dev;Username=dev;Password=dev123"
    },
    "SendGrid": {
      "ApiKey": "fake-key-for-dev"
    },
    "Stripe": {
      "SecretKey": "sk_test_fake"
    },
    "OpenAI": {
      "ApiKey": "sk-fake-for-dev"
    }
  }
  ```

- [ ] Setup Docker Compose para banco local
  ```yaml
  # docker-compose.dev.yml
  version: '3.8'
  services:
    postgres:
      image: postgres:15
      environment:
        POSTGRES_DB: taktiq_dev
        POSTGRES_USER: dev
        POSTGRES_PASSWORD: dev123
      ports:
        - "5432:5432"
      volumes:
        - postgres_dev_data:/var/lib/postgresql/data

    redis:
      image: redis:7-alpine
      ports:
        - "6379:6379"

  volumes:
    postgres_dev_data:
  ```

- [ ] Comandos para rodar localmente:
  ```bash
  # Subir banco de dados
  docker-compose -f docker-compose.dev.yml up -d

  # Aplicar migrations
  cd src/GymHero.Api
  dotnet ef database update

  # Seed de dados fake
  dotnet run --seed-dev-data

  # Rodar API
  dotnet run --environment Development
  ```

**Frontend (Next.js)**
- [ ] Criar `.env.local` (não commitado)
  ```bash
  NEXT_PUBLIC_API_URL=http://localhost:5000
  NEXT_PUBLIC_ENVIRONMENT=development
  ```

- [ ] Comandos para rodar:
  ```bash
  cd frontend/apps/web
  pnpm install
  pnpm dev
  ```

**Resultado:** Dev pode rodar tudo localmente, sem afetar produção

---

#### 2. Ambiente Staging no Azure (Dia 3-4)

**Criar App Services Separados**

- [ ] **API Staging**
  - Nome: `taktiq-api-staging`
  - URL: `https://taktiq-api-staging.azurewebsites.net`
  - Resource Group: `TaktIQ-Staging` (novo)
  - Plan: Same as production (para ser idêntico)

- [ ] **Frontend Staging**
  - Nome: `taktiq-web-staging`
  - URL: `https://staging.taktiq.app` (custom domain)
  - Resource Group: `TaktIQ-Staging`

- [ ] **Banco de Dados Staging**
  - Azure PostgreSQL separado
  - Nome: `taktiq-db-staging`
  - Copiar schema de produção (mas dados fake)
  - Connection string diferente

**Configurar App Settings Staging**
```bash
# API Staging
az webapp config appsettings set \
  --name taktiq-api-staging \
  --resource-group TaktIQ-Staging \
  --settings \
    ASPNETCORE_ENVIRONMENT="Staging" \
    ConnectionStrings__DefaultConnection="<staging-db-connection>" \
    SendGrid__ApiKey="<staging-sendgrid-key>" \
    Stripe__SecretKey="sk_test_staging_key"
```

**Dados de Teste em Staging**
- [ ] Script para popular banco com dados fake:
  ```bash
  # seed-staging-data.sh
  - 10 Personal Trainers fake
  - 50 Alunos fake
  - 20 Planos de treino
  - 100 Exercícios
  - Histórico de treinos
  ```

---

#### 3. Pipeline de CI/CD (Dia 5-7)

**GitHub Actions - 3 Workflows**

**A. Development (Auto-deploy)**
```yaml
# .github/workflows/dev-deploy.yml
name: Deploy to Development

on:
  push:
    branches: [dev, feature/*]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      # Backend
      - name: Build API
        run: dotnet build src/GymHero.Api

      - name: Run Tests
        run: dotnet test

      # Deploy automático para dev (opcional)
      # Ou apenas rodar testes e validar
```

**B. Staging (Auto-deploy + Testes)**
```yaml
# .github/workflows/staging-deploy.yml
name: Deploy to Staging

on:
  push:
    branches: [staging]

jobs:
  test-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      # 1. Testes
      - name: Run Unit Tests
        run: dotnet test --filter Category=Unit

      - name: Run Integration Tests
        run: dotnet test --filter Category=Integration

      # 2. Build
      - name: Build Backend
        run: dotnet publish -c Release

      - name: Build Frontend
        run: |
          cd frontend/apps/web
          npm install
          npm run build

      # 3. Deploy to Staging
      - name: Deploy API to Staging
        uses: azure/webapps-deploy@v2
        with:
          app-name: taktiq-api-staging
          publish-profile: ${{ secrets.AZURE_STAGING_API_PUBLISH_PROFILE }}

      - name: Deploy Frontend to Staging
        uses: azure/webapps-deploy@v2
        with:
          app-name: taktiq-web-staging
          publish-profile: ${{ secrets.AZURE_STAGING_WEB_PUBLISH_PROFILE }}

      # 4. E2E Tests em Staging
      - name: Run E2E Tests
        run: |
          cd frontend/apps/web
          npx playwright test --config=playwright.staging.config.ts

      # 5. Notificação
      - name: Notify Team
        if: success()
        run: echo "Staging deployed successfully!"
```

**C. Production (Manual Approval)**
```yaml
# .github/workflows/production-deploy.yml
name: Deploy to Production

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    environment:
      name: production
      # Requer aprovação manual no GitHub

    steps:
      - uses: actions/checkout@v3

      # Mesmos steps do staging, mas com:
      # - app-name: taktiq-api (produção)
      # - Rollback automático se falhar

      - name: Deploy with Rollback
        run: |
          # Deploy
          # Se falhar health check, rollback
```

**Configurar Environments no GitHub**
- [ ] Settings > Environments > New environment
  - Nome: `production`
  - Required reviewers: [adicionar seu usuário]
  - Wait timer: 5 minutos (opcional)

**Resultado:**
- Push para `dev` → Roda testes
- Push para `staging` → Deploy automático + testes E2E
- Push para `main` → Aguarda aprovação → Deploy produção

---

#### 4. Estratégia de Branches (Git Flow Simplificado)

```
main (production)
  ↑
  └── staging (pre-production)
        ↑
        └── dev (integration)
              ↑
              └── feature/nova-feature
              └── feature/mobile-app
              └── bugfix/corrigir-login
```

**Regras:**
1. **Nunca commitar direto em `main`**
2. Features desenvolvidas em `feature/*`
3. Merge `feature` → `dev` (via PR)
4. Quando `dev` estável → Merge `dev` → `staging`
5. Testar em staging por 1-2 dias
6. Se OK → Merge `staging` → `main` (COM APROVAÇÃO)

**Comandos:**
```bash
# Criar nova feature
git checkout dev
git pull
git checkout -b feature/mobile-app

# Desenvolver...
git add .
git commit -m "feat: Add mobile app base"
git push origin feature/mobile-app

# Criar Pull Request no GitHub
# feature/mobile-app → dev

# Após aprovação e merge
git checkout staging
git pull
git merge dev
git push  # Auto-deploy para staging

# Testar staging por 1-2 dias
# Se tudo OK:
git checkout main
git pull
git merge staging
git push  # Aguarda aprovação, depois deploy prod
```

---

### 🔍 Monitoramento e Observabilidade

**Configurar para cada ambiente:**

- [ ] **Application Insights** (Azure)
  - Staging: Insight separado
  - Production: Insight existente
  - Métricas: response time, errors, dependencies

- [ ] **Sentry** (Error Tracking)
  ```bash
  # Diferentes DSN por ambiente
  SENTRY_DSN_DEV=https://dev@sentry.io/project
  SENTRY_DSN_STAGING=https://staging@sentry.io/project
  SENTRY_DSN_PROD=https://prod@sentry.io/project
  ```

- [ ] **Health Checks**
  ```csharp
  // Program.cs
  app.MapHealthChecks("/health", new HealthCheckOptions
  {
      ResponseWriter = async (context, report) =>
      {
          context.Response.ContentType = "application/json";
          var result = JsonSerializer.Serialize(new
          {
              status = report.Status.ToString(),
              checks = report.Entries.Select(e => new
              {
                  name = e.Key,
                  status = e.Value.Status.ToString(),
                  description = e.Value.Description
              })
          });
          await context.Response.WriteAsync(result);
      }
  });
  ```

- [ ] **Dashboards**
  - Grafana ou Azure Dashboard
  - CPU, Memory, Requests/s
  - Error rate por ambiente
  - Comparação staging vs production

---

## 📅 FASE 1: Features Técnicas Prioritárias (Após Setup de Ambientes)

**Duração:** 6-8 semanas (Janeiro-Fevereiro)

### Semana 1-2: Testes Automatizados

**Por que primeiro?**
Antes de desenvolver features, precisamos garantir que não vamos quebrar o que já funciona.

**Backend (C# + xUnit)**

- [ ] **Setup de Testes**
  ```bash
  dotnet new xunit -n GymHero.Tests
  dotnet add package Moq
  dotnet add package FluentAssertions
  dotnet add package Microsoft.AspNetCore.Mvc.Testing
  ```

- [ ] **Unit Tests (40 testes mínimo)**
  - Services: EmailService, PasswordHasher, JwtTokenGenerator
  - Handlers (CQRS): LoginHandler, RegisterHandler, CreatePlanHandler
  - Validators: AuthDtos validators
  - Utils: Data transformations

- [ ] **Integration Tests (20 testes mínimo)**
  - Auth endpoints: signup, login, forgot-password, reset-password
  - Workout plans: CRUD operations
  - Marketplace: purchase flow
  - Personal trainer: invite students

- [ ] **Coverage Report**
  ```bash
  dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
  dotnet tool install -g dotnet-reportgenerator-globaltool
  reportgenerator -reports:coverage.opencover.xml -targetdir:coverage-report
  ```
  - Meta: >70% coverage

**Frontend (Vitest + Testing Library)**

- [ ] **Setup**
  ```bash
  cd frontend/apps/web
  pnpm add -D vitest @testing-library/react @testing-library/user-event
  ```

- [ ] **Component Tests (30 componentes)**
  - Forms: LoginForm, SignupForm, CreatePlanForm
  - Cards: WorkoutCard, ExerciseCard, PlanCard
  - Modals: ExerciseModal, MarketplaceSettings
  - Dashboard: InstructorDashboard, StudentDashboard

- [ ] **E2E Tests (Playwright - 10 flows)**
  ```bash
  pnpm add -D @playwright/test
  npx playwright install
  ```
  - Critical paths:
    1. Signup → Login → Dashboard
    2. Create workout plan
    3. Start workout session → Complete
    4. Purchase plan from marketplace
    5. PT invite student → Student activates
    6. Create challenge → Student joins
    7. Upload profile picture
    8. Chat with PT/student
    9. Generate AI workout
    10. Filter exercises by location

**CI Integration**
- [ ] Todos os testes rodam em PRs
- [ ] Merge bloqueado se testes falharem
- [ ] Coverage report comentado no PR

---

### Semana 3: Testes de Carga e Performance

**Objetivo:** Garantir que suporta 1.000 usuários simultâneos

**Setup k6**
```bash
# Instalar k6
brew install k6  # macOS
choco install k6  # Windows
```

**Cenários de Teste**

- [ ] **Cenário 1: Login Storm**
  ```javascript
  // load-tests/login-storm.js
  import http from 'k6/http';
  import { check, sleep } from 'k6';

  export let options = {
    stages: [
      { duration: '1m', target: 100 },   // Ramp up
      { duration: '3m', target: 500 },   // Stress
      { duration: '1m', target: 1000 },  // Peak
      { duration: '2m', target: 0 },     // Ramp down
    ],
    thresholds: {
      http_req_duration: ['p(95)<500'], // 95% requests < 500ms
      http_req_failed: ['rate<0.01'],   // Error rate < 1%
    },
  };

  export default function () {
    const payload = JSON.stringify({
      email: `user${__VU}@test.com`,
      password: 'Test123!',
    });

    const res = http.post('https://staging.taktiq.app/api/auth/login', payload, {
      headers: { 'Content-Type': 'application/json' },
    });

    check(res, {
      'status is 200': (r) => r.status === 200,
      'has token': (r) => JSON.parse(r.body).token !== undefined,
    });

    sleep(1);
  }
  ```

- [ ] **Cenário 2: AI Workout Generation**
  - 50 concurrent users gerando treinos
  - Medida: tempo de resposta
  - Meta: <10s para 95% das requests

- [ ] **Cenário 3: Marketplace Browse**
  - 200 users navegando marketplace
  - Filtros, busca, pagination
  - Meta: <200ms response time

- [ ] **Cenário 4: Database Stress**
  - Complex queries (analytics, joins)
  - Identificar N+1 queries
  - Adicionar índices se necessário

**Otimizações**
- [ ] Implementar Redis cache
  ```csharp
  services.AddStackExchangeRedisCache(options =>
  {
      options.Configuration = Configuration["Redis:ConnectionString"];
  });
  ```

- [ ] Query optimization
  - Usar `.AsNoTracking()` em read-only queries
  - Eager loading com `.Include()` onde necessário
  - Pagination em listas grandes

- [ ] Response compression
  ```csharp
  app.UseResponseCompression();
  ```

**Resultado:** Relatório de performance + lista de otimizações implementadas

---

### Semana 4-5: Analytics e Métricas

**Backend - Endpoints de Analytics**

- [ ] **Admin Analytics**
  ```csharp
  // GET /api/admin/analytics
  public class AdminAnalytics
  {
      public int TotalUsers { get; set; }
      public int ActiveUsersToday { get; set; }
      public int ActiveUsersWeek { get; set; }
      public int ActiveUsersMonth { get; set; }
      public Dictionary<string, int> UsersByRole { get; set; }
      public Dictionary<string, int> SignupsByDay { get; set; }
      public RetentionMetrics Retention { get; set; }
      public RevenueMetrics Revenue { get; set; }
  }
  ```

- [ ] **Cálculo de Métricas**
  ```csharp
  // Retention cohorts
  var userCohorts = await context.Users
      .GroupBy(u => new {
          Year = u.CreatedAt.Year,
          Month = u.CreatedAt.Month
      })
      .Select(g => new CohortMetrics
      {
          CohortDate = new DateTime(g.Key.Year, g.Key.Month, 1),
          TotalUsers = g.Count(),
          ActiveAfter30Days = g.Count(u => /* logic */),
          RetentionRate = /* calculation */
      })
      .ToListAsync();
  ```

- [ ] **Caching**
  - Cache analytics por 1 hora
  - Invalidar cache quando dados mudam
  - Background job para pre-calcular métricas pesadas

**Frontend - Dashboards**

- [ ] **Admin Dashboard** (`/admin/analytics`)
  - Charts com Recharts
  - Cards de overview (DAU, MAU, MRR)
  - User growth chart (line)
  - Retention table (cohorts)
  - Top PTs, top plans

- [ ] **PT Dashboard** (melhorar existente)
  - Engagement por aluno (gráfico)
  - Revenue trends
  - Plan performance
  - Conversion funnel

**Integração Google Analytics 4**
```typescript
// lib/analytics.ts
import ReactGA from 'react-ga4';

export const initGA = () => {
  ReactGA.initialize('G-XXXXXXXXXX');
};

export const logPageView = () => {
  ReactGA.send({ hitType: 'pageview', page: window.location.pathname });
};

export const logEvent = (category: string, action: string, label?: string) => {
  ReactGA.event({ category, action, label });
};

// Eventos importantes
logEvent('Workout', 'completed', planId);
logEvent('Purchase', 'plan', planId);
logEvent('Challenge', 'joined', challengeId);
```

---

### Semana 6: Notificações Push

**Backend - Firebase Cloud Messaging**

- [ ] **Setup Firebase**
  1. Criar projeto no Firebase Console
  2. Baixar `firebase-adminsdk.json`
  3. Adicionar ao Azure App Settings: `FIREBASE_CREDENTIALS` (base64 do JSON)

- [ ] **Service de Push**
  ```csharp
  public interface IPushNotificationService
  {
      Task SendToUserAsync(Guid userId, string title, string body, Dictionary<string, string>? data = null);
      Task SendToTopicAsync(string topic, string title, string body);
      Task ScheduleNotificationAsync(Guid userId, DateTime sendAt, string title, string body);
  }

  public class FirebasePushService : IPushNotificationService
  {
      public async Task SendToUserAsync(Guid userId, string title, string body, Dictionary<string, string>? data = null)
      {
          var token = await GetUserTokenAsync(userId);

          var message = new Message
          {
              Token = token,
              Notification = new Notification
              {
                  Title = title,
                  Body = body
              },
              Data = data,
              Android = new AndroidConfig
              {
                  Priority = Priority.High,
                  Notification = new AndroidNotification
                  {
                      ClickAction = "FLUTTER_NOTIFICATION_CLICK"
                  }
              },
              Apns = new ApnsConfig
              {
                  Aps = new Aps
                  {
                      Alert = new ApsAlert { Title = title, Body = body },
                      Sound = "default"
                  }
              }
          };

          await FirebaseMessaging.DefaultInstance.SendAsync(message);
      }
  }
  ```

- [ ] **Tipos de Notificação**
  1. Lembrete de treino (1h antes)
  2. Novo desafio do PT
  3. Comentário no treino
  4. Streak em risco (não treinou há 2 dias)
  5. Novo post do PT
  6. Badge desbloqueado

- [ ] **Background Jobs (Hangfire)**
  ```csharp
  // Job diário: verificar streaks
  RecurringJob.AddOrUpdate(
      "check-streaks",
      () => CheckStreaksAndNotify(),
      Cron.Daily(9) // 9h da manhã
  );

  // Job: lembrete de treino
  public async Task ScheduleWorkoutReminder(Guid sessionId, DateTime workoutTime)
  {
      BackgroundJob.Schedule(
          () => SendWorkoutReminder(sessionId),
          workoutTime.AddHours(-1) // 1h antes
      );
  }
  ```

**Frontend - Web Push**

- [ ] **Service Worker**
  ```javascript
  // public/sw.js
  self.addEventListener('push', (event) => {
    const data = event.data.json();

    self.registration.showNotification(data.title, {
      body: data.body,
      icon: '/icon-192x192.png',
      badge: '/badge-72x72.png',
      data: data.data
    });
  });

  self.addEventListener('notificationclick', (event) => {
    event.notification.close();
    event.waitUntil(
      clients.openWindow(event.notification.data.url)
    );
  });
  ```

- [ ] **Solicitar Permissão**
  ```typescript
  // hooks/use-push-notifications.ts
  export function usePushNotifications() {
    const requestPermission = async () => {
      const permission = await Notification.requestPermission();

      if (permission === 'granted') {
        const registration = await navigator.serviceWorker.ready;
        const subscription = await registration.pushManager.subscribe({
          userVisibleOnly: true,
          applicationServerKey: VAPID_PUBLIC_KEY
        });

        // Salvar subscription no backend
        await api.post('/api/notifications/subscribe', {
          subscription: subscription.toJSON()
        });
      }
    };

    return { requestPermission };
  }
  ```

- [ ] **Preferências** (`/settings/notifications`)
  - Toggle por tipo de notificação
  - Quiet hours (não perturbe entre 22h-7h)
  - Frequência (imediato, diário, semanal)

---

### Semana 7-8: Mobile App (Fase Inicial)

**Decisão Técnica: React Native com Expo**

**Por quê?**
- Reuso de código React
- Expo facilita build e updates OTA
- Community grande
- Suporta iOS + Android simultaneamente

**Setup**

```bash
# Criar projeto Expo
npx create-expo-app taktiq-mobile --template blank-typescript

cd taktiq-mobile

# Instalar dependências essenciais
npx expo install expo-router react-native-safe-area-context react-native-screens
npx expo install @react-native-async-storage/async-storage
npx expo install expo-secure-store
npx expo install axios react-query
```

**Estrutura**
```
taktiq-mobile/
├── app/
│   ├── (auth)/
│   │   ├── login.tsx
│   │   └── signup.tsx
│   ├── (tabs)/
│   │   ├── index.tsx        # Dashboard
│   │   ├── workout.tsx
│   │   ├── plans.tsx
│   │   └── profile.tsx
│   └── _layout.tsx
├── components/
│   ├── WorkoutCard.tsx
│   ├── ExerciseList.tsx
│   └── Timer.tsx
├── services/
│   ├── api.ts
│   └── auth.ts
└── hooks/
    ├── useWorkout.ts
    └── useAuth.ts
```

**Features MVP Mobile (8 semanas)**

- [ ] **Semana 1: Setup + Auth**
  - Projeto configurado
  - Navegação (Expo Router)
  - Login/Signup screens
  - Token storage (SecureStore)

- [ ] **Semana 2: Dashboard**
  - Home screen
  - Treino do dia
  - Quick stats
  - Feed de conteúdo

- [ ] **Semana 3: Workout Execution**
  - Lista de exercícios
  - Timer integrado
  - Contador de séries/reps
  - Marcar como concluído

- [ ] **Semana 4: Plans & Marketplace**
  - Ver meus planos
  - Browse marketplace
  - Detalhes de plano
  - Compra (in-app purchase setup)

- [ ] **Semana 5: Social**
  - Lista de amigos
  - Chat básico
  - Comentários
  - Desafios

- [ ] **Semana 6: Profile & Settings**
  - Editar perfil
  - Upload de foto (expo-image-picker)
  - Configurações
  - Logout

- [ ] **Semana 7: Polish**
  - Animações (react-native-reanimated)
  - Gestures (react-native-gesture-handler)
  - Offline mode (react-query cache)
  - Loading states

- [ ] **Semana 8: Build & Deploy**
  - Build iOS (EAS Build)
  - Build Android (EAS Build)
  - TestFlight beta (iOS)
  - Internal testing (Android)
  - Submit to stores

**Configuração de Build**
```json
// eas.json
{
  "build": {
    "development": {
      "developmentClient": true,
      "distribution": "internal"
    },
    "staging": {
      "android": {
        "buildType": "apk"
      },
      "ios": {
        "simulator": false
      },
      "env": {
        "API_URL": "https://staging.taktiq.app/api"
      }
    },
    "production": {
      "env": {
        "API_URL": "https://taktiq.app/api"
      }
    }
  }
}
```

---

## 📅 FASE 2: Features Avançadas (Março-Junho)

### Gamificação (2-3 semanas)

**Database Schema**
```sql
-- XP and Levels
CREATE TABLE user_xp (
    user_id UUID PRIMARY KEY,
    total_xp INT DEFAULT 0,
    current_level INT DEFAULT 1,
    xp_to_next_level INT DEFAULT 100
);

-- Badges
CREATE TABLE badges (
    id UUID PRIMARY KEY,
    name VARCHAR(100),
    description TEXT,
    icon_url VARCHAR(500),
    xp_reward INT,
    type VARCHAR(50), -- 'workout', 'streak', 'social', etc
    criteria JSONB     -- {type: 'workout_count', value: 10}
);

CREATE TABLE user_badges (
    id UUID PRIMARY KEY,
    user_id UUID,
    badge_id UUID,
    earned_at TIMESTAMP DEFAULT NOW()
);

-- Leaderboards
CREATE TABLE leaderboard_scores (
    id UUID PRIMARY KEY,
    user_id UUID,
    leaderboard_type VARCHAR(50), -- 'global', 'gym', 'friends'
    score INT,
    period VARCHAR(20),            -- 'weekly', 'monthly', 'all-time'
    period_start DATE,
    updated_at TIMESTAMP DEFAULT NOW()
);
```

**Implementação**
- [ ] XP system (ganhar XP por ações)
- [ ] Level progression
- [ ] Badge unlock logic
- [ ] Leaderboards (global, friends, gym)
- [ ] UI para exibir badges
- [ ] Animations de "level up"

---

### Marketplace 2.0 (2 semanas)

**Features**
- [ ] **Recomendações com IA**
  ```python
  # Collaborative filtering simples
  from sklearn.metrics.pairwise import cosine_similarity

  # Matriz usuário x plano (ratings ou compras)
  user_plan_matrix = ...

  # Similaridade entre usuários
  user_similarity = cosine_similarity(user_plan_matrix)

  # Recomendar planos que usuários similares compraram
  ```

- [ ] **Sistema de Reviews**
  - Rating 1-5 estrelas
  - Review text (opcional)
  - Fotos de progresso
  - Only verified purchases

- [ ] **Filtros Avançados**
  - Multi-select filters
  - Range sliders (preço, duração)
  - Sort by: popular, newest, highest rated, price

- [ ] **Preview de Plano**
  - Primeira semana visível
  - Estatísticas (X pessoas completaram)
  - Reviews destacados
  - "Try 1 week free" (opcional)

---

### Wearables Integration (3-4 semanas)

**Plataformas:**
1. **Apple Health** (iOS) - HealthKit
2. **Google Fit** (Android)
3. **Fitbit** (API)
4. **Garmin** (API)

**Dados Sincronizados:**
- Passos
- Calorias
- Frequência cardíaca
- Sono (duração, qualidade)
- Exercícios registrados
- Distância percorrida

**Implementação iOS (HealthKit)**
```swift
// React Native Module
import HealthKit

@objc(HealthKitModule)
class HealthKitModule: NSObject {

  @objc
  func requestAuthorization(_ resolve: @escaping RCTPromiseResolveBlock, reject: @escaping RCTPromiseRejectBlock) {
    let healthStore = HKHealthStore()

    let typesToRead: Set<HKObjectType> = [
      HKObjectType.quantityType(forIdentifier: .stepCount)!,
      HKObjectType.quantityType(forIdentifier: .activeEnergyBurned)!,
      HKObjectType.quantityType(forIdentifier: .heartRate)!,
    ]

    healthStore.requestAuthorization(toShare: nil, read: typesToRead) { success, error in
      if success {
        resolve(["authorized": true])
      } else {
        reject("AUTH_ERROR", error?.localizedDescription, error)
      }
    }
  }

  @objc
  func getStepsToday(_ resolve: @escaping RCTPromiseResolveBlock, reject: @escaping RCTPromiseRejectBlock) {
    // Query steps for today
    // Return count
  }
}
```

**Backend - Webhook de Sincronização**
```csharp
// POST /api/wearables/sync
public async Task<IActionResult> SyncWearableData([FromBody] WearableDataDto data)
{
    // Salvar dados do wearable
    var entry = new WearableDataEntry
    {
        UserId = data.UserId,
        Source = data.Source, // "apple_health", "fitbit", etc
        DataType = data.DataType,
        Value = data.Value,
        Timestamp = data.Timestamp
    };

    await context.WearableData.AddAsync(entry);
    await context.SaveChangesAsync();

    // Processar insights
    await ProcessWearableInsights(data.UserId);

    return Ok();
}
```

---

## 🔧 Ferramentas e Infraestrutura

### Ferramentas de Desenvolvimento

| Ferramenta | Uso | Custo |
|------------|-----|-------|
| **GitHub** | Code hosting, CI/CD | Grátis (plan atual) |
| **Azure DevOps** | Alternativa (se preferir) | Grátis até 5 users |
| **Docker** | Containers locais | Grátis |
| **Postman** | API testing | Grátis (team: $12/user/mês) |
| **Sentry** | Error tracking | Grátis até 5k events/mês |
| **Mixpanel/Amplitude** | Analytics | Grátis até 20M events/mês |
| **k6** | Load testing | Grátis (open source) |
| **Expo** | Mobile dev & build | Grátis (EAS: $29/mês) |

### Infraestrutura Azure (Por Ambiente)

**Development**
- Local (grátis)

**Staging**
- App Service: ~R$ 500/mês
- PostgreSQL: ~R$ 300/mês
- Blob Storage: ~R$ 50/mês
- **Total: ~R$ 850/mês**

**Production** (atual)
- App Service: ~R$ 1.000/mês
- PostgreSQL: ~R$ 600/mês
- Blob Storage: ~R$ 100/mês
- Redis: ~R$ 300/mês
- **Total: ~R$ 2.000/mês**

**TOTAL INFRA: ~R$ 2.850/mês**

---

## ✅ Checklist de Implementação Completa

### Fase 0: Ambientes (Semana 1)
- [ ] Docker Compose para dev local
- [ ] Staging no Azure (API + Frontend + DB)
- [ ] GitHub Actions workflows (dev, staging, prod)
- [ ] Git flow configurado
- [ ] Monitoring por ambiente

### Fase 1: Qualidade (Semanas 2-8)
- [ ] 70%+ test coverage backend
- [ ] 30+ component tests frontend
- [ ] 10+ E2E tests
- [ ] Load testing (suporta 1K users)
- [ ] Analytics dashboard
- [ ] Push notifications
- [ ] Mobile app MVP

### Fase 2: Growth (Semanas 9-16)
- [ ] Gamificação completa
- [ ] Marketplace 2.0
- [ ] Wearables integration
- [ ] Mobile app nas stores

---

## 📊 Métricas de Sucesso Técnicas

| Métrica | Target |
|---------|--------|
| Test Coverage | >70% |
| API Response Time (p95) | <200ms |
| Frontend Load Time | <2s |
| Error Rate | <1% |
| Deployment Frequency | Diário (staging), Semanal (prod) |
| Mean Time to Recovery | <30min |
| Mobile App Rating | >4.0 |
| CI/CD Success Rate | >95% |

---

**Última atualização:** 08/12/2025
**Próxima revisão:** Após conclusão Fase 0
