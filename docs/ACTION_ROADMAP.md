# TaktIQ - Roadmap de Ação (Fazermos Juntos)

**Objetivo:** Lista prática de tarefas que podemos executar juntos, sessão por sessão
**Status:** 🟡 Em Progresso
**Última atualização:** 08/12/2025

---

## 📋 Como Usar Este Roadmap

Este é um roadmap **executável** - cada item é algo que podemos fazer **agora**, juntos.

**Legenda:**
- ⏳ **Tempo estimado** por tarefa
- 🔴 **Prioridade ALTA** - fazer primeiro
- 🟡 **Prioridade MÉDIA** - importante mas não urgente
- 🟢 **Prioridade BAIXA** - nice to have

**Como funciona:**
1. Você escolhe uma tarefa
2. Eu crio/edito os arquivos necessários
3. Você testa e valida
4. Marcamos como ✅ concluído
5. Próxima tarefa!

---

## 🎯 FASE 0: Setup de Ambientes (URGENTE!)

**Meta:** Parar de fazer deploy direto em produção
**Duração total:** ~1 semana (dividido em 5 sessões)

### Sessão 1: Development Local (2-3 horas) 🔴

- [ ] **Criar docker-compose.dev.yml** ⏳ 10 min
  - PostgreSQL local
  - Redis local
  - Azurite (storage emulator)

- [ ] **Criar appsettings.Development.Local.json** ⏳ 5 min
  - Connection strings locais
  - API keys fake para dev

- [ ] **Criar scripts de desenvolvimento** ⏳ 15 min
  - `scripts/dev.sh` (Linux/Mac)
  - `scripts/dev.ps1` (Windows)
  - Comandos para subir tudo de uma vez

- [ ] **Adicionar .gitignore para arquivos locais** ⏳ 5 min
  - `appsettings.Development.Local.json`
  - `.env.local`

- [ ] **Criar seed de dados fake** ⏳ 30 min
  - 5 PTs
  - 20 Alunos
  - 10 Planos
  - 50+ Exercícios
  - Histórico de treinos

- [ ] **Testar setup local** ⏳ 30 min
  - Você: rodar `./scripts/dev.sh`
  - Você: acessar http://localhost:3000
  - Você: fazer login com dados fake

**Resultado:** Ambiente de dev 100% local, sem tocar produção

---

### Sessão 2: Staging no Azure (2-3 horas) 🔴

- [ ] **Criar Resource Group no Azure** ⏳ 5 min
  - Você: rodar comando que eu passo
  - Nome: `TaktIQ-Staging`

- [ ] **Criar App Services para Staging** ⏳ 15 min
  - API staging
  - Frontend staging
  - Eu passo todos os comandos Azure CLI

- [ ] **Criar banco de dados staging** ⏳ 10 min
  - PostgreSQL separado
  - Configurar firewall

- [ ] **Configurar App Settings staging** ⏳ 20 min
  - Connection strings
  - Variáveis de ambiente
  - SendGrid (chave de teste)

- [ ] **Deploy manual inicial para staging** ⏳ 30 min
  - Você: fazer build local
  - Você: fazer deploy via Azure CLI
  - Teste: acessar staging.taktiq.app

- [ ] **Aplicar migrations em staging** ⏳ 15 min
  - Seed de dados fake em staging

**Resultado:** Ambiente staging funcionando, idêntico a prod mas com dados fake

---

### Sessão 3: CI/CD com GitHub Actions (2-3 horas) 🔴

- [ ] **Criar workflow de staging** ⏳ 20 min
  - `.github/workflows/deploy-staging.yml`
  - Auto-deploy quando push para branch `staging`

- [ ] **Criar workflow de produção** ⏳ 20 min
  - `.github/workflows/deploy-production.yml`
  - Deploy com aprovação manual

- [ ] **Criar workflow de PR checks** ⏳ 15 min
  - `.github/workflows/pr-checks.yml`
  - Rodar testes em PRs

- [ ] **Configurar secrets no GitHub** ⏳ 15 min
  - Publish profiles do Azure
  - API keys

- [ ] **Configurar environment "production"** ⏳ 10 min
  - Required reviewers: você
  - Protection rules

- [ ] **Testar CI/CD completo** ⏳ 60 min
  - Push para staging → ver deploy automático
  - Abrir PR → ver testes rodando
  - Push para main → aprovar → ver deploy prod

**Resultado:** CI/CD completo, deploy automatizado, aprovação em prod

---

### Sessão 4: Git Flow e Processos (1-2 horas) 🟡

- [ ] **Criar branches estratégicos** ⏳ 10 min
  - `dev` (integração)
  - `staging` (pre-prod)
  - `main` já existe (prod)

- [ ] **Configurar branch protection** ⏳ 15 min
  - `main`: require PR + approvals
  - `staging`: require PR
  - `dev`: livre

- [ ] **Criar DEVELOPMENT.md** ⏳ 20 min
  - Guia para novos devs
  - Como rodar local
  - Como fazer PR
  - Git flow explicado

- [ ] **Criar template de PR** ⏳ 10 min
  - `.github/pull_request_template.md`
  - Checklist padrão

**Resultado:** Processo de desenvolvimento documentado e automatizado

---

### Sessão 5: Validação e Documentação (1 hora) 🟡

- [ ] **Validar fluxo completo** ⏳ 30 min
  - Feature branch → dev → staging → main
  - Testar em cada ambiente

- [ ] **Criar guia de troubleshooting** ⏳ 20 min
  - Problemas comuns
  - Soluções

- [ ] **Treinar time (se tiver)** ⏳ variável
  - Apresentar novo fluxo
  - Q&A

**Resultado:** Setup completo validado e documentado

---

## 🧪 FASE 1: Testes Automatizados (2-3 semanas)

**Meta:** 70%+ test coverage, testes rodando em CI

### Sessão 6: Setup de Testes Backend (2-3 horas) 🔴

- [ ] **Criar projeto de testes** ⏳ 10 min
  - `src/GymHero.Tests/GymHero.Tests.csproj`
  - Adicionar pacotes (xUnit, Moq, FluentAssertions)

- [ ] **Configurar test runner no CI** ⏳ 15 min
  - Adicionar step no workflow

- [ ] **Criar primeiros unit tests** ⏳ 60 min
  - PasswordHasher (2 testes)
  - EmailService (3 testes)
  - JwtTokenGenerator (3 testes)

- [ ] **Criar helper para testes** ⏳ 30 min
  - TestFixture
  - Database in-memory
  - Mocks comuns

- [ ] **Rodar testes e verificar coverage** ⏳ 15 min
  - Gerar relatório HTML
  - Ver coverage inicial (~10-20%)

**Resultado:** Infraestrutura de testes pronta, primeiros testes funcionando

---

### Sessão 7: Unit Tests - Serviços (3-4 horas) 🔴

- [ ] **Tests para AuthService** ⏳ 45 min
  - Register
  - Login
  - ForgotPassword
  - ResetPassword

- [ ] **Tests para WorkoutPlanService** ⏳ 45 min
  - CreatePlan
  - UpdatePlan
  - DeletePlan
  - SharePlan

- [ ] **Tests para ExerciseService** ⏳ 30 min
  - GetExercises (com filtros)
  - CreateExercise

- [ ] **Tests para Validators** ⏳ 30 min
  - RegisterRequestValidator
  - CreatePlanValidator

- [ ] **Verificar coverage** ⏳ 15 min
  - Meta: chegar em ~40-50%

**Resultado:** Serviços principais testados

---

### Sessão 8: Integration Tests (2-3 horas) 🟡

- [ ] **Setup WebApplicationFactory** ⏳ 20 min
  - Configurar test server

- [ ] **Tests para Auth endpoints** ⏳ 60 min
  - POST /api/auth/signup
  - POST /api/auth/login
  - POST /api/auth/forgot-password
  - POST /api/auth/reset-password

- [ ] **Tests para Workout Plan endpoints** ⏳ 60 min
  - GET /api/workout-plans
  - POST /api/workout-plans
  - PUT /api/workout-plans/{id}

- [ ] **Tests para Marketplace** ⏳ 30 min
  - Purchase flow completo

**Resultado:** Endpoints críticos testados end-to-end

---

### Sessão 9: Frontend Tests (2-3 horas) 🟡

- [ ] **Setup Vitest** ⏳ 15 min
  - Configurar vitest.config.ts
  - Adicionar script de teste

- [ ] **Component tests** ⏳ 90 min
  - LoginForm (3 testes)
  - SignupForm (3 testes)
  - WorkoutCard (2 testes)
  - ExerciseModal (3 testes)
  - MarketplaceSettingsDialog (3 testes)

- [ ] **Hook tests** ⏳ 30 min
  - useAuth
  - useWorkout

**Resultado:** Componentes principais testados

---

### Sessão 10: E2E Tests (3-4 horas) 🟡

- [ ] **Setup Playwright** ⏳ 15 min
  - Instalar
  - Configurar playwright.config.ts

- [ ] **Criar E2E tests críticos** ⏳ 120 min
  - Signup → Login → Dashboard
  - Create workout plan
  - Start workout → Complete
  - Purchase plan from marketplace
  - PT invite student → Activate

- [ ] **Rodar E2E em staging** ⏳ 30 min
  - Configurar para rodar contra staging
  - Adicionar ao CI (opcional)

**Resultado:** Fluxos críticos testados automaticamente

---

## 📊 FASE 2: Analytics e Métricas (1-2 semanas)

### Sessão 11: Analytics Backend (2-3 horas) 🔴

- [ ] **Criar endpoint /api/admin/analytics** ⏳ 60 min
  - Cálculo de DAU, MAU, WAU
  - Retention (D1, D7, D30)
  - Revenue (MRR, ARR)
  - User growth

- [ ] **Criar endpoint para cohorts** ⏳ 45 min
  - Análise de coortes por mês
  - Taxa de retenção por coorte

- [ ] **Implementar caching** ⏳ 30 min
  - Redis cache (1 hora TTL)
  - Invalidação inteligente

**Resultado:** Backend de analytics pronto

---

### Sessão 12: Analytics Frontend (3-4 horas) 🔴

- [ ] **Criar página /admin/analytics** ⏳ 30 min
  - Layout básico
  - Cards de overview

- [ ] **Implementar gráficos** ⏳ 120 min
  - User growth (line chart)
  - Retention table
  - Revenue trend
  - Top PTs
  - Top Plans

- [ ] **Adicionar filtros** ⏳ 30 min
  - Date range picker
  - Export CSV

**Resultado:** Dashboard de analytics funcional

---

### Sessão 13: Google Analytics 4 (1-2 horas) 🟡

- [ ] **Setup GA4 property** ⏳ 15 min
  - Você: criar no console Google

- [ ] **Adicionar gtag ao Next.js** ⏳ 30 min
  - Componente GoogleAnalytics
  - Tracking de pageviews

- [ ] **Track eventos custom** ⏳ 45 min
  - signup, login
  - workout_started, workout_completed
  - plan_purchased
  - challenge_joined

**Resultado:** Tracking completo de eventos importantes

---

## 🔔 FASE 3: Push Notifications (1 semana)

### Sessão 14: Firebase Setup (1-2 horas) 🟡

- [ ] **Criar projeto Firebase** ⏳ 15 min
  - Você: criar no console

- [ ] **Baixar service account** ⏳ 5 min
  - Adicionar ao Azure App Settings

- [ ] **Instalar pacote FirebaseAdmin** ⏳ 5 min
  - Backend .NET

**Resultado:** Firebase configurado

---

### Sessão 15: Push Backend (2-3 horas) 🟡

- [ ] **Criar PushNotificationService** ⏳ 60 min
  - SendToUser
  - SendToTopic
  - ScheduleNotification

- [ ] **Criar tipos de notificação** ⏳ 45 min
  - Lembrete de treino
  - Novo desafio
  - Comentário
  - Streak em risco

- [ ] **Setup Hangfire para scheduling** ⏳ 45 min
  - Job diário: check streaks
  - Job: lembretes de treino

**Resultado:** Sistema de notificações backend pronto

---

### Sessão 16: Push Frontend (2-3 horas) 🟡

- [ ] **Criar service worker** ⏳ 30 min
  - public/sw.js
  - Push event handler

- [ ] **Solicitar permissão** ⏳ 30 min
  - Hook usePushNotifications
  - UI para pedir permissão

- [ ] **Página de preferências** ⏳ 60 min
  - /settings/notifications
  - Toggles por tipo
  - Quiet hours

- [ ] **UI de notificações** ⏳ 30 min
  - Bell icon com badge
  - Dropdown

**Resultado:** Push notifications completo (web)

---

## 📱 FASE 4: Mobile App MVP (6-8 semanas)

### Sessão 17: Mobile Setup (2-3 horas) 🟡

- [ ] **Criar projeto Expo** ⏳ 15 min
  - Você: rodar comando que eu passo

- [ ] **Instalar dependências** ⏳ 20 min
  - Navegação, auth, API client

- [ ] **Estrutura de pastas** ⏳ 30 min
  - app/, components/, services/, hooks/

- [ ] **Configurar API client** ⏳ 30 min
  - axios setup
  - Token storage

- [ ] **Primeiras telas** ⏳ 60 min
  - Login
  - Signup
  - Splash

**Resultado:** App mobile rodando em simulator

---

### Sessão 18-25: Features Mobile (várias sessões)

Podemos fazer juntos, feature por feature:
- Dashboard
- Workout execution
- Plans
- Marketplace
- Social
- Profile
- etc.

---

## 🎮 FASE 5: Gamificação (2-3 semanas)

### Sessão 26: Gamification Schema (2 horas) 🟢

- [ ] **Criar migrations** ⏳ 30 min
  - Tabelas: user_xp, badges, user_badges, leaderboard_scores

- [ ] **Criar entidades** ⏳ 30 min
  - UserXP, Badge, UserBadge, LeaderboardScore

- [ ] **Seed de badges** ⏳ 45 min
  - 40+ badges diferentes
  - Ícones e descrições

**Resultado:** Database pronto para gamificação

---

### Sessão 27: XP System (2-3 horas) 🟢

- [ ] **XP Service** ⏳ 90 min
  - AwardXP()
  - CheckLevelUp()
  - Regras de XP por ação

- [ ] **Integrar em ações** ⏳ 45 min
  - Workout completed → 50 XP
  - Challenge joined → 20 XP
  - Comment → 5 XP

**Resultado:** Sistema de XP funcionando

---

### Sessão 28: Badges (2-3 horas) 🟢

- [ ] **Badge unlock logic** ⏳ 90 min
  - Verificar critérios
  - Unlock automático
  - Notificação

- [ ] **UI de badges** ⏳ 60 min
  - Página /badges
  - Grid de badges
  - Progress bars

**Resultado:** Badges desbloqueando automaticamente

---

### Sessão 29: Leaderboards (2 horas) 🟢

- [ ] **Leaderboard calculations** ⏳ 60 min
  - Global
  - Friends
  - Gym

- [ ] **UI de leaderboards** ⏳ 60 min
  - Tabela rankeada
  - Highlight do usuário

**Resultado:** Leaderboards funcionando

---

## 🛍️ FASE 6: Marketplace 2.0 (1-2 semanas)

### Sessão 30: Recomendações IA (3 horas) 🟢

- [ ] **Collaborative filtering simples** ⏳ 120 min
  - Algoritmo de similaridade
  - "Usuários que compraram X também compraram Y"

- [ ] **Endpoint de recomendações** ⏳ 60 min
  - GET /api/marketplace/recommendations

**Resultado:** Recomendações personalizadas

---

### Sessão 31: Reviews System (2-3 horas) 🟢

- [ ] **Schema de reviews** ⏳ 30 min
  - Migration + entidades

- [ ] **CRUD de reviews** ⏳ 90 min
  - Create, Read, Update, Delete
  - Only verified purchases

- [ ] **UI de reviews** ⏳ 60 min
  - Star rating component
  - Review list
  - Write review modal

**Resultado:** Sistema de reviews completo

---

## 🔧 MELHORIAS CONTÍNUAS (Sempre)

### Performance 🟡

- [ ] **Implementar Redis cache** ⏳ 2h
- [ ] **Otimizar queries N+1** ⏳ variável
- [ ] **Adicionar índices no banco** ⏳ 1h
- [ ] **Lazy loading de componentes** ⏳ 1h

### Security 🔴

- [ ] **Rate limiting** ⏳ 2h
- [ ] **Input validation middleware** ⏳ 2h
- [ ] **CORS hardening** ⏳ 1h
- [ ] **SQL injection protection audit** ⏳ 2h

### UX/UI 🟢

- [ ] **Loading skeletons** ⏳ 3h
- [ ] **Empty states** ⏳ 2h
- [ ] **Error boundaries** ⏳ 2h
- [ ] **Onboarding tour** ⏳ 3h

### DevOps 🟡

- [ ] **Monitoring dashboard** ⏳ 3h
- [ ] **Automated backups** ⏳ 2h
- [ ] **Blue-green deployment** ⏳ 4h
- [ ] **Feature flags** ⏳ 3h

---

## 📊 Progresso Geral

### Fase 0: Ambientes
- Sessões: 0/5 concluídas
- Progresso: 0%

### Fase 1: Testes
- Sessões: 0/5 concluídas
- Progresso: 0%

### Fase 2: Analytics
- Sessões: 0/3 concluídas
- Progresso: 0%

### Fase 3: Push Notifications
- Sessões: 0/3 concluídas
- Progresso: 0%

### Fase 4: Mobile
- Sessões: 0/8+ concluídas
- Progresso: 0%

### Fase 5: Gamificação
- Sessões: 0/4 concluídas
- Progresso: 0%

### Fase 6: Marketplace 2.0
- Sessões: 0/2 concluídas
- Progresso: 0%

**TOTAL:** 0/30+ sessões concluídas (0%)

---

## ✅ Como Começar AGORA

### Opção 1: Mais Urgente (Recomendado)
**Começar por:** Sessão 1 - Development Local
**Por quê:** Precisamos parar de mexer direto em produção
**Tempo:** 2-3 horas
**Diga:** "Vamos começar pela Sessão 1"

### Opção 2: Quick Win
**Começar por:** Sessão 11 - Analytics Backend
**Por quê:** Rápido de fazer, resultado visível
**Tempo:** 2-3 horas
**Diga:** "Quero começar por Analytics"

### Opção 3: Você Escolhe
**Diga:** "Quero fazer [nome da sessão]"

---

## 🎯 Próxima Sessão

**Status:** Aguardando escolha
**Opções:**
1. Sessão 1: Development Local (RECOMENDADO)
2. Outra sessão (você escolhe)

**Para começar, basta dizer:**
- "Vamos fazer a Sessão 1"
- "Bora começar pelo ambiente local"
- "Quero fazer [outra sessão]"

---

**Última atualização:** 08/12/2025
**Próxima atualização:** Após cada sessão concluída
