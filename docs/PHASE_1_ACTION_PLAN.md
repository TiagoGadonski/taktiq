# Fase 1: Beta Ready - Plano de Ação Detalhado

**Período:** Janeiro - Fevereiro 2025 (8 semanas)
**Objetivo:** Lançar beta com 100 usuários validando product-market fit
**Data:** 08/12/2025

---

## 🎯 Visão Geral da Fase 1

**Meta Final:** Plataforma estável, testada e pronta para onboarding de 100 beta users

**Success Criteria:**
- ✅ 70%+ test coverage (backend)
- ✅ Suporta 1.000 usuários simultâneos
- ✅ <200ms API response time (p95)
- ✅ Analytics dashboard funcional
- ✅ Push notifications operacionais
- ✅ 100 beta testers onboarded
- ✅ NPS >40

---

## 📅 Cronograma Detalhado (8 Semanas)

### SEMANA 1-2: Setup de Testes (09 - 22 Jan)

#### Objetivos
- Setup ambiente de testes
- Primeiros testes implementados
- CI/CD melhorado

#### Tasks Backend (Dev 1)

**Dia 1-2: Setup**
- [ ] Instalar pacotes de teste
  ```bash
  cd src/GymHero.Api
  dotnet add package xUnit
  dotnet add package Moq
  dotnet add package FluentAssertions
  dotnet add package Microsoft.AspNetCore.Mvc.Testing
  ```
- [ ] Criar projeto de testes: `GymHero.Tests`
- [ ] Configurar test runners no CI/CD

**Dia 3-5: Unit Tests - Serviços Críticos**
- [ ] `EmailService` tests
  - SendPasswordResetEmail
  - SendWelcomeEmail
  - SendStudentInvitation
- [ ] `PasswordHasher` tests
  - Hash generation
  - Verification
- [ ] `JwtTokenGenerator` tests
  - Token generation
  - Claims validation

**Dia 6-8: Integration Tests - Endpoints**
- [ ] Auth endpoints
  - POST /api/auth/signup
  - POST /api/auth/login
  - POST /api/auth/forgot-password
  - POST /api/auth/reset-password
- [ ] Workout plan endpoints
  - GET /api/workout-plans
  - POST /api/workout-plans
  - PUT /api/workout-plans/{id}

**Dia 9-10: Test Coverage Report**
- [ ] Integrar coverlet para coverage
- [ ] Gerar relatório HTML
- [ ] Badge de coverage no README

**Deliverable:** 40-50% test coverage, CI rodando testes automaticamente

---

#### Tasks Frontend (Dev 2)

**Dia 1-2: Setup Testing Framework**
- [ ] Configurar Vitest ou Jest
  ```bash
  cd frontend/apps/web
  pnpm add -D vitest @testing-library/react @testing-library/jest-dom
  ```
- [ ] Setup Testing Library
- [ ] Configurar coverage

**Dia 3-5: Component Tests**
- [ ] Testar componentes críticos:
  - Login/Signup forms
  - Workout card
  - Exercise modal
  - Plan creation wizard

**Dia 6-8: E2E Tests Setup**
- [ ] Instalar Playwright
  ```bash
  pnpm add -D @playwright/test
  npx playwright install
  ```
- [ ] Primeiro E2E test: signup flow
- [ ] CI integration

**Dia 9-10: E2E Critical Flows**
- [ ] Login → Dashboard
- [ ] Create workout plan
- [ ] Start workout session
- [ ] Purchase plan from marketplace

**Deliverable:** 5+ E2E tests, componentes críticos testados

---

### SEMANA 3: Testes de Carga (23 - 29 Jan)

#### Objetivos
- Identificar gargalos de performance
- Garantir escalabilidade para 1.000 usuários simultâneos
- Otimizar queries lentas

#### Tasks (Dev 1 + Dev 2)

**Dia 1-2: Setup k6**
- [ ] Instalar k6: https://k6.io/docs/getting-started/installation/
- [ ] Criar pasta `load-tests/`
- [ ] Script básico:
  ```javascript
  import http from 'k6/http';
  import { check, sleep } from 'k6';

  export let options = {
    stages: [
      { duration: '2m', target: 100 },
      { duration: '5m', target: 500 },
      { duration: '2m', target: 1000 },
      { duration: '5m', target: 0 },
    ],
  };

  export default function () {
    let res = http.get('https://api.taktiq.app/health');
    check(res, { 'status was 200': (r) => r.status == 200 });
    sleep(1);
  }
  ```

**Dia 3: Cenários de Teste**
- [ ] Cenário 1: Login simultâneo (100 users)
- [ ] Cenário 2: Geração de treinos IA (50 concurrent)
- [ ] Cenário 3: Marketplace browse (200 users)
- [ ] Cenário 4: Workout session (100 active sessions)

**Dia 4-5: Executar Testes e Coletar Métricas**
- [ ] Rodar todos os cenários
- [ ] Monitorar:
  - Response times (p50, p95, p99)
  - Error rate
  - Throughput (req/s)
  - Database connections
  - Memory usage
  - CPU usage

**Dia 6-7: Otimizações**
- [ ] Identificar queries N+1
- [ ] Adicionar índices no banco
- [ ] Implementar caching (Redis)
  ```csharp
  services.AddStackExchangeRedisCache(options => {
      options.Configuration = Configuration["Redis:ConnectionString"];
  });
  ```
- [ ] Otimizar queries pesadas

**Deliverable:** Relatório de performance, otimizações implementadas, suporta 1.000 users

---

### SEMANA 4-5: Analytics Dashboard (30 Jan - 12 Fev)

#### Objetivos
- Dashboard de analytics para admin
- Dashboard para PTs
- Integração com Google Analytics 4

#### Tasks (Dev 1 Backend)

**Dia 1-2: Endpoints de Analytics**
- [ ] GET /api/admin/analytics
  ```csharp
  {
    "totalUsers": 1523,
    "activeUsers": {
      "dau": 342,
      "wau": 891,
      "mau": 1234
    },
    "retention": {
      "d1": 0.65,
      "d7": 0.42,
      "d30": 0.28
    },
    "revenue": {
      "mrr": 15234.50,
      "arr": 182814.00
    }
  }
  ```
- [ ] GET /api/admin/analytics/funnel
- [ ] GET /api/admin/analytics/cohorts
- [ ] GET /api/personal/analytics (já existe, melhorar)

**Dia 3-4: Cálculos de Métricas**
- [ ] DAU/WAU/MAU calculation
- [ ] Retention cohorts
- [ ] Churn rate
- [ ] LTV estimation
- [ ] Conversion funnels

**Dia 5: Caching e Performance**
- [ ] Cache analytics queries (Redis, 1h TTL)
- [ ] Background jobs para cálculos pesados

---

#### Tasks (Dev 2 Frontend)

**Dia 1-2: Layout do Dashboard**
- [ ] Criar `/admin/analytics` page
- [ ] Layout com cards de métricas
- [ ] Usar recharts para gráficos

**Dia 3-4: Visualizações**
- [ ] Cards de overview (DAU, MAU, MRR)
- [ ] Gráfico de crescimento (line chart)
- [ ] Funil de conversão (funnel chart)
- [ ] Cohort retention table
- [ ] Top PTs table
- [ ] Top plans table

**Dia 5: Filtros e Interatividade**
- [ ] Date range picker
- [ ] Filters (por PT, por plano, etc)
- [ ] Export para CSV

**Dia 6-7: Google Analytics 4**
- [ ] Setup GA4 property
- [ ] Adicionar gtag ao Next.js
- [ ] Track eventos custom:
  - signup
  - login
  - workout_started
  - workout_completed
  - plan_purchased
  - challenge_joined

**Deliverable:** Dashboard funcional com métricas reais, GA4 tracking ativo

---

### SEMANA 6: Push Notifications (13 - 19 Fev)

#### Objetivos
- Sistema de notificações push web e mobile prep
- Templates de notificações
- Preferências de usuário

#### Tasks (Dev 1 Backend)

**Dia 1: Firebase Setup**
- [ ] Criar projeto Firebase
- [ ] Baixar service account key
- [ ] Adicionar ao Azure App Settings: `FIREBASE_CREDENTIALS`
- [ ] Instalar pacote:
  ```bash
  dotnet add package FirebaseAdmin
  ```

**Dia 2-3: Serviço de Notificações**
- [ ] Criar `PushNotificationService.cs`
  ```csharp
  public interface IPushNotificationService
  {
      Task SendToUserAsync(Guid userId, string title, string body, object? data = null);
      Task SendToTopicAsync(string topic, string title, string body);
      Task ScheduleNotificationAsync(Guid userId, DateTime sendAt, string title, string body);
  }
  ```
- [ ] Implementar envio FCM
- [ ] Sistema de templates

**Dia 4: Tipos de Notificação**
- [ ] Lembrete de treino (1h antes)
- [ ] Novo desafio
- [ ] Comentário no treino
- [ ] Novo post do PT
- [ ] Streak em risco

**Dia 5: Background Jobs**
- [ ] Usar Hangfire para scheduling
  ```bash
  dotnet add package Hangfire
  dotnet add package Hangfire.PostgreSql
  ```
- [ ] Job diário: verificar streaks
- [ ] Job: lembretes de treino

---

#### Tasks (Dev 2 Frontend)

**Dia 1-2: Web Push Setup**
- [ ] Configurar service worker
- [ ] Solicitar permissão do usuário
- [ ] Salvar FCM token no backend

**Dia 3: Preferências de Notificação**
- [ ] Página `/settings/notifications`
- [ ] Toggles por tipo de notificação
- [ ] Quiet hours selector
- [ ] Teste de notificação

**Dia 4-5: UI de Notificações**
- [ ] Bell icon com badge de count
- [ ] Dropdown de notificações
- [ ] Marcar como lida
- [ ] Ver todas

**Deliverable:** Push notifications funcionando, preferências configuráveis

---

### SEMANA 7: UX/UI Polish (20 - 26 Fev)

#### Objetivos
- Onboarding melhorado
- Micro-interactions
- Empty states
- Acessibilidade

#### Tasks (Designer + Dev 2)

**Dia 1-2: Onboarding**
- [ ] Tour guiado (first-time user)
  - Usar react-joyride ou similar
- [ ] Steps:
  1. Bem-vindo ao TaktIQ
  2. Aqui está seu dashboard
  3. Crie seu primeiro plano
  4. Explore o marketplace
  5. Configure seu perfil

**Dia 3: Empty States**
- [ ] Dashboard sem treinos
- [ ] Planos vazio
- [ ] Marketplace vazio
- [ ] Amigos vazio
- [ ] Cada um com CTA específico

**Dia 4: Micro-interactions**
- [ ] Buttons com hover states
- [ ] Loading states (skeletons)
- [ ] Success animations (checkmarks)
- [ ] Toasts animados

**Dia 5: Acessibilidade**
- [ ] Audit com Lighthouse
- [ ] aria-labels nos botões
- [ ] Navegação por teclado
- [ ] Contraste de cores (WCAG AA)
- [ ] Screen reader friendly

**Deliverable:** Experiência polida, acessível, delightful

---

### SEMANA 8: Beta Launch Prep (27 Fev - 05 Mar)

#### Objetivos
- Sistema de feedback
- Recrutamento beta testers
- Help center
- Lançamento controlado

#### Tasks (Todos)

**Dia 1-2: Feedback System**
- [ ] Botão "Dar Feedback" in-app
- [ ] Modal com form:
  - Rating (1-5 estrelas)
  - Categoria (bug, feature request, elogio)
  - Descrição
  - Screenshot (opcional)
- [ ] Salvar no banco + enviar email para equipe

**Dia 3: Bug Report**
- [ ] Botão "Reportar Bug"
- [ ] Auto-include:
  - Browser/OS
  - URL atual
  - Console logs
  - Screenshot
- [ ] Integrar com Sentry

**Dia 4: Help Center**
- [ ] FAQ page básico
- [ ] Artigos:
  - Como criar um plano de treino
  - Como usar o gerador de IA
  - Como vender no marketplace
  - Como convidar alunos
- [ ] Search bar

**Dia 5: Recrutamento Beta**
- [ ] Landing page "/beta"
- [ ] Formulário de inscrição:
  - Nome, Email, Telefone
  - Você é: [ ] Aluno [ ] Personal Trainer
  - Por que quer participar?
  - Expectativas
- [ ] Email de confirmação
- [ ] Grupo Telegram/Discord

**Dia 6-7: Final Checks**
- [ ] Smoke tests em produção
- [ ] Revisão de segurança
- [ ] Backup do banco de dados
- [ ] Plano de rollback
- [ ] Monitoring dashboards prontos

**Dia 8: LANÇAMENTO BETA 🚀**
- [ ] Enviar convites para primeiros 20 beta testers
- [ ] Monitorar de perto
- [ ] Responder feedback rapidamente
- [ ] Daily standups com a equipe

**Deliverable:** 100 beta testers onboarded na primeira semana

---

## 📊 KPIs a Monitorar Durante Fase 1

### Técnicos
- [ ] Test coverage >70%
- [ ] API response time <200ms (p95)
- [ ] Error rate <1%
- [ ] Uptime >99.5%

### Produto
- [ ] Time to first workout <10min
- [ ] Signup conversion >30%
- [ ] D1 retention >60%
- [ ] D7 retention >40%

### Negócio
- [ ] Beta applications >200
- [ ] Beta acceptance rate 50% (100/200)
- [ ] NPS >40
- [ ] Critical bugs <5

---

## 👥 Responsabilidades

### Dev 1 (Backend)
- Testes backend (unit + integration)
- Load testing
- Analytics endpoints
- Push notifications backend
- Performance optimization

### Dev 2 (Frontend)
- Testes frontend (component + E2E)
- Analytics dashboard
- Push notifications UI
- UX/UI polish
- Beta landing page

### QA Engineer
- Test planning
- Manual testing
- Regression testing
- Bug tracking
- Test automation support

### Product Manager
- Roadmap management
- Beta recruitment
- Feedback analysis
- Stakeholder communication
- Metrics tracking

---

## 💰 Budget Fase 1

| Item | Custo Estimado |
|------|----------------|
| **Equipe (2 meses)** | R$ 60.000 |
| Dev 1 (Senior Backend) | R$ 15.000/mês × 2 |
| Dev 2 (Senior Frontend) | R$ 15.000/mês × 2 |
| QA Engineer | R$ 10.000/mês × 2 |
| Product Manager (part-time) | R$ 5.000/mês × 2 |
| **Infraestrutura** | R$ 6.000 |
| Azure (staging + prod) | R$ 2.000/mês × 2 |
| Firebase (push notif) | R$ 500/mês × 2 |
| Ferramentas (Sentry, etc) | R$ 500/mês × 2 |
| **Marketing Beta** | R$ 5.000 |
| Landing page ads | R$ 2.000 |
| Influencer partnerships | R$ 3.000 |
| **Ferramentas** | R$ 3.000 |
| Mixpanel/Amplitude | R$ 1.000/mês × 2 |
| Hotjar | R$ 500/mês × 2 |
| **TOTAL** | **R$ 74.000** |

---

## 🚨 Riscos e Plano de Contingência

### Risco 1: Atraso no desenvolvimento
**Mitigação:**
- Buffer de 1 semana no cronograma
- Daily standups para identificar blockers cedo
- Priorizar ruthlessly (cortar features se necessário)

### Risco 2: Bugs críticos descobertos na semana 8
**Mitigação:**
- Testing rigoroso nas semanas 1-7
- Staging environment idêntico a produção
- Lançamento gradual (20 users, depois 50, depois 100)

### Risco 3: Baixa adesão ao beta
**Mitigação:**
- Marketing pré-lançamento 2 semanas antes
- Incentivos (lifetime discount)
- Parcerias com influencers fitness

### Risco 4: Problemas de performance inesperados
**Mitigação:**
- Load testing na semana 3
- Auto-scaling configurado no Azure
- Monitoring 24/7 (Sentry, DataDog)

---

## ✅ Checklist de Lançamento

### 1 Semana Antes
- [ ] Staging 100% funcional
- [ ] Todos os testes passando
- [ ] Analytics configurado e testado
- [ ] Push notifications testadas
- [ ] Help center publicado
- [ ] Landing page "/beta" live
- [ ] Convites preparados

### 1 Dia Antes
- [ ] Backup do banco de dados
- [ ] Monitoring dashboards abertos
- [ ] Plano de rollback documentado
- [ ] Equipe em alerta (on-call)
- [ ] Comunicação com beta testers agendada

### Dia do Lançamento
- [ ] 09:00 - Enviar convites para primeiros 20 users
- [ ] 12:00 - Check-in: quantos ativaram?
- [ ] 15:00 - Enviar mais 30 convites se tudo OK
- [ ] 18:00 - Daily recap com equipe
- [ ] Monitorar feedback e bugs em tempo real

### Primeira Semana
- [ ] Daily monitoring de KPIs
- [ ] Responder todos os feedbacks <24h
- [ ] Bug fixes prioritários
- [ ] Onboarding de 100 users até fim da semana

---

## 📞 Próximos Passos Imediatos

### Esta Semana (09-15 Dez)
- [ ] Aprovar este plano com stakeholders
- [ ] Confirmar budget de R$ 74k
- [ ] Contratar/alocar QA Engineer
- [ ] Escolher ferramenta de analytics (Mixpanel vs Amplitude)
- [ ] Criar backlog detalhado no Jira/Linear

### Próxima Semana (16-22 Dez)
- [ ] Kickoff Fase 1 com equipe
- [ ] Setup ambientes de teste
- [ ] Começar implementação (Semana 1)

---

**Documento vivo - atualizar semanalmente**
**Última atualização:** 08/12/2025
**Owner:** Product Team
