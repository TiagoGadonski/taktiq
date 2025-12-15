# Setup de Ambientes - Guia Prático

**Objetivo:** Sair de "tudo em produção" para ambientes isolados (Dev, Staging, Prod)
**Tempo estimado:** 1 semana
**Prioridade:** CRÍTICA ⚠️

---

## 🚨 Por Que Isso É Urgente?

**Situação Atual:**
```
Developer → git push → PRODUÇÃO ❌
```

**Problemas:**
- ❌ Bug afeta usuários reais imediatamente
- ❌ Impossível testar antes de deploy
- ❌ Dados de produção expostos
- ❌ Rollback difícil
- ❌ Múltiplos devs conflitando

**Solução:**
```
Developer → Dev → Staging → ✅ Aprovação → Produção
```

---

## 📋 Checklist Rápido

- [ ] **Dia 1:** Development local setup
- [ ] **Dia 2:** Staging no Azure
- [ ] **Dia 3:** CI/CD workflows
- [ ] **Dia 4:** Testes e validação
- [ ] **Dia 5:** Documentação e treinamento

---

## DIA 1: Development Local

### Setup Backend (API .NET)

**1. Criar arquivo de configuração local**

```bash
cd src/GymHero.Api
```

Criar `appsettings.Development.Local.json` (não versionar!):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=taktiq_dev;Username=postgres;Password=postgres"
  },
  "JwtSettings": {
    "Secret": "dev-secret-key-at-least-32-characters-long",
    "ExpiryMinutes": 60,
    "Issuer": "TaktIQ-Dev",
    "Audience": "TaktIQ-Users-Dev"
  },
  "SendGrid": {
    "ApiKey": "fake-key-for-development"
  },
  "Stripe": {
    "SecretKey": "sk_test_fake",
    "WebhookSecret": "whsec_fake"
  },
  "OpenAI": {
    "ApiKey": "sk-fake-for-dev"
  },
  "AzureStorage": {
    "ConnectionString": "UseDevelopmentStorage=true"
  },
  "GooglePlaces": {
    "ApiKey": "fake-key"
  }
}
```

Adicionar ao `.gitignore`:
```bash
echo "appsettings.Development.Local.json" >> .gitignore
```

**2. Setup Docker para banco de dados**

Criar `docker-compose.dev.yml` na raiz do projeto:
```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    container_name: taktiq-postgres-dev
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: taktiq_dev
    ports:
      - "5432:5432"
    volumes:
      - postgres_dev_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    container_name: taktiq-redis-dev
    ports:
      - "6379:6379"
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  azurite:
    image: mcr.microsoft.com/azure-storage/azurite
    container_name: taktiq-storage-dev
    ports:
      - "10000:10000" # Blob
      - "10001:10001" # Queue
      - "10002:10002" # Table
    volumes:
      - azurite_data:/data

volumes:
  postgres_dev_data:
  azurite_data:
```

**3. Comandos de desenvolvimento**

Criar script `scripts/dev.sh`:
```bash
#!/bin/bash

echo "🚀 Iniciando ambiente de desenvolvimento..."

# 1. Subir containers
docker-compose -f docker-compose.dev.yml up -d

# 2. Aguardar containers ficarem healthy
echo "⏳ Aguardando banco de dados..."
sleep 5

# 3. Aplicar migrations
echo "📊 Aplicando migrations..."
cd src/GymHero.Api
dotnet ef database update

# 4. Seed de dados (opcional)
echo "🌱 Populando banco com dados fake..."
dotnet run --seed-dev

# 5. Rodar API
echo "✅ Ambiente pronto! Iniciando API..."
dotnet run --environment Development
```

Tornar executável:
```bash
chmod +x scripts/dev.sh
```

**No Windows (PowerShell):**
Criar `scripts/dev.ps1`:
```powershell
Write-Host "🚀 Iniciando ambiente de desenvolvimento..." -ForegroundColor Green

# 1. Subir containers
docker-compose -f docker-compose.dev.yml up -d

# 2. Aguardar
Start-Sleep -Seconds 5

# 3. Migrations
Write-Host "📊 Aplicando migrations..." -ForegroundColor Yellow
Set-Location src\GymHero.Api
dotnet ef database update

# 4. Seed
Write-Host "🌱 Populando banco..." -ForegroundColor Yellow
dotnet run --seed-dev

# 5. Run
Write-Host "✅ Ambiente pronto! Iniciando API..." -ForegroundColor Green
dotnet run --environment Development
```

**4. Seed de dados para desenvolvimento**

Criar `src/GymHero.Api/SeedDevelopmentData.cs`:
```csharp
public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(IApplicationDbContext context)
    {
        if (await context.Users.AnyAsync()) return; // Já tem dados

        // 5 Personal Trainers
        var pts = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Carlos Silva",
                Email = "carlos@pt.com",
                PasswordHash = /* hash de "Dev123!" */,
                Role = "PersonalTrainer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            },
            // ... mais 4 PTs
        };

        await context.Users.AddRangeAsync(pts);

        // 20 Alunos
        var students = new List<User>();
        for (int i = 1; i <= 20; i++)
        {
            students.Add(new User
            {
                Id = Guid.NewGuid(),
                Name = $"Aluno {i}",
                Email = $"aluno{i}@test.com",
                PasswordHash = /* hash de "Dev123!" */,
                Role = "Aluno",
                PersonalTrainerId = pts[i % 5].Id, // Distribuir entre PTs
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 180))
            });
        }

        await context.Users.AddRangeAsync(students);

        // 10 Planos de treino
        // 50 Treinos
        // 100+ Exercícios

        await context.SaveChangesAsync();
    }
}
```

Modificar `Program.cs`:
```csharp
var builder = WebApplication.CreateBuilder(args);

// ... configurações

var app = builder.Build();

// Seed de dados em desenvolvimento
if (app.Environment.IsDevelopment() && args.Contains("--seed-dev"))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
    await DevelopmentDataSeeder.SeedAsync(context);
    Console.WriteLine("✅ Dados de desenvolvimento inseridos!");
    return; // Não rodar o app, só fazer seed
}

app.Run();
```

---

### Setup Frontend (Next.js)

**1. Variáveis de ambiente**

Criar `.env.local` (não versionar):
```bash
# API
NEXT_PUBLIC_API_URL=http://localhost:5000

# Environment
NEXT_PUBLIC_ENVIRONMENT=development

# Analytics (desabilitado em dev)
NEXT_PUBLIC_GA_ID=

# Sentry (desabilitado em dev)
NEXT_PUBLIC_SENTRY_DSN=
```

Adicionar ao `.gitignore`:
```
.env.local
```

**2. Scripts de desenvolvimento**

Editar `package.json`:
```json
{
  "scripts": {
    "dev": "next dev",
    "dev:https": "next dev --experimental-https",
    "build": "next build",
    "build:staging": "NEXT_PUBLIC_API_URL=https://staging.taktiq.app/api next build",
    "build:prod": "NEXT_PUBLIC_API_URL=https://taktiq.app/api next build",
    "start": "next start",
    "lint": "next lint",
    "type-check": "tsc --noEmit"
  }
}
```

**3. Rodar frontend**

```bash
cd frontend/apps/web
pnpm install
pnpm dev
```

Frontend estará em: http://localhost:3000

---

### Testar Local

**1. Verificar tudo funcionando:**
```bash
# Terminal 1: Backend
cd src/GymHero.Api
dotnet run

# Terminal 2: Frontend
cd frontend/apps/web
pnpm dev

# Terminal 3: Verificar containers
docker ps
```

**2. Acessar:**
- Frontend: http://localhost:3000
- API: http://localhost:5000
- API Docs: http://localhost:5000/swagger

**3. Login de teste:**
- Email: `carlos@pt.com`
- Senha: `Dev123!`

---

## DIA 2: Staging no Azure

### Criar Recursos no Azure

**1. Resource Group**
```bash
az group create \
  --name TaktIQ-Staging \
  --location brazilsouth
```

**2. App Service Plan**
```bash
az appservice plan create \
  --name taktiq-plan-staging \
  --resource-group TaktIQ-Staging \
  --sku B1 \
  --is-linux
```

**3. API App Service**
```bash
az webapp create \
  --name taktiq-api-staging \
  --resource-group TaktIQ-Staging \
  --plan taktiq-plan-staging \
  --runtime "DOTNETCORE:8.0"
```

**4. Frontend App Service**
```bash
az webapp create \
  --name taktiq-web-staging \
  --resource-group TaktIQ-Staging \
  --plan taktiq-plan-staging \
  --runtime "NODE:20-lts"
```

**5. PostgreSQL Database**
```bash
az postgres flexible-server create \
  --name taktiq-db-staging \
  --resource-group TaktIQ-Staging \
  --location brazilsouth \
  --admin-user taktiqadmin \
  --admin-password "Staging@Pass2024!" \
  --sku-name Standard_B1ms \
  --tier Burstable \
  --storage-size 32 \
  --version 15

# Criar database
az postgres flexible-server db create \
  --resource-group TaktIQ-Staging \
  --server-name taktiq-db-staging \
  --database-name taktiqdb
```

**6. Configurar Firewall (permitir Azure services)**
```bash
az postgres flexible-server firewall-rule create \
  --resource-group TaktIQ-Staging \
  --name taktiq-db-staging \
  --rule-name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

---

### Configurar App Settings Staging

**API Staging:**
```bash
# Connection string
az webapp config connection-string set \
  --name taktiq-api-staging \
  --resource-group TaktIQ-Staging \
  --connection-string-type PostgreSQL \
  --settings DefaultConnection="Host=taktiq-db-staging.postgres.database.azure.com;Database=taktiqdb;Username=taktiqadmin;Password=Staging@Pass2024!;SSL Mode=Require"

# App Settings
az webapp config appsettings set \
  --name taktiq-api-staging \
  --resource-group TaktIQ-Staging \
  --settings \
    ASPNETCORE_ENVIRONMENT="Staging" \
    SendGrid__ApiKey="${SENDGRID_STAGING_KEY}" \
    Stripe__SecretKey="sk_test_staging" \
    JwtSettings__Secret="${JWT_SECRET_STAGING}" \
    JwtSettings__Issuer="TaktIQ-Staging" \
    JwtSettings__Audience="TaktIQ-Users-Staging"
```

**Frontend Staging:**
```bash
az webapp config appsettings set \
  --name taktiq-web-staging \
  --resource-group TaktIQ-Staging \
  --settings \
    NEXT_PUBLIC_API_URL="https://taktiq-api-staging.azurewebsites.net" \
    NEXT_PUBLIC_ENVIRONMENT="staging"
```

---

### Aplicar Migrations em Staging

```bash
# Connection string de staging
export ConnectionStrings__DefaultConnection="Host=taktiq-db-staging.postgres.database.azure.com;Database=taktiqdb;Username=taktiqadmin;Password=Staging@Pass2024!;SSL Mode=Require"

# Aplicar migrations
cd src/GymHero.Api
dotnet ef database update --connection "$ConnectionStrings__DefaultConnection"

# Seed de dados staging (dados fake)
dotnet run --seed-staging --connection "$ConnectionStrings__DefaultConnection"
```

---

## DIA 3: CI/CD Workflows

### GitHub Actions - 3 Workflows

**1. Staging Deployment**

Criar `.github/workflows/deploy-staging.yml`:
```yaml
name: Deploy to Staging

on:
  push:
    branches: [staging]

jobs:
  deploy-api:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Run Tests
        run: |
          cd src/GymHero.Tests
          dotnet test --configuration Release

      - name: Publish API
        run: |
          cd src/GymHero.Api
          dotnet publish -c Release -o ./publish

      - name: Deploy to Azure
        uses: azure/webapps-deploy@v2
        with:
          app-name: taktiq-api-staging
          publish-profile: ${{ secrets.AZURE_STAGING_API_PUBLISH_PROFILE }}
          package: src/GymHero.Api/publish

  deploy-frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup Node
        uses: actions/setup-node@v3
        with:
          node-version: '20'

      - uses: pnpm/action-setup@v2
        with:
          version: 8

      - name: Install dependencies
        run: |
          cd frontend/apps/web
          pnpm install

      - name: Build
        run: |
          cd frontend/apps/web
          pnpm build:staging

      - name: Deploy to Azure
        uses: azure/webapps-deploy@v2
        with:
          app-name: taktiq-web-staging
          publish-profile: ${{ secrets.AZURE_STAGING_WEB_PUBLISH_PROFILE }}
          package: frontend/apps/web/.next
```

**2. Production Deployment (com aprovação)**

Criar `.github/workflows/deploy-production.yml`:
```yaml
name: Deploy to Production

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    environment:
      name: production
      url: https://taktiq.app

    steps:
      # Mesmos steps do staging
      # mas com:
      # - app-name: taktiq-api (produção)
      # - app-name: taktiq-web (produção)
```

**3. Pull Request Checks**

Criar `.github/workflows/pr-checks.yml`:
```yaml
name: PR Checks

on:
  pull_request:
    branches: [dev, staging, main]

jobs:
  test-backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Run Tests
        run: dotnet test

      - name: Check Coverage
        run: |
          dotnet test /p:CollectCoverage=true /p:Threshold=70
          # Falha se coverage < 70%

  test-frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup Node
        uses: actions/setup-node@v3
        with:
          node-version: '20'

      - uses: pnpm/action-setup@v2

      - name: Install & Test
        run: |
          cd frontend/apps/web
          pnpm install
          pnpm test
          pnpm type-check

  lint:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Lint Backend
        run: dotnet format --verify-no-changes

      - name: Lint Frontend
        run: |
          cd frontend/apps/web
          pnpm lint
```

---

### Configurar Secrets no GitHub

1. Ir em: **Settings** → **Secrets and variables** → **Actions**

2. Adicionar secrets:
   - `AZURE_STAGING_API_PUBLISH_PROFILE`
   - `AZURE_STAGING_WEB_PUBLISH_PROFILE`
   - `AZURE_PROD_API_PUBLISH_PROFILE`
   - `AZURE_PROD_WEB_PUBLISH_PROFILE`

3. Obter publish profiles:
```bash
# No Azure Portal:
# App Service → taktiq-api-staging → Get publish profile
# Copiar conteúdo e colar no secret
```

---

### Configurar Environments no GitHub

1. **Settings** → **Environments** → **New environment**

2. Criar `production`:
   - Required reviewers: [seu usuário]
   - Wait timer: 0 (ou 5 min para pensar)
   - Deployment branches: `main` only

---

## DIA 4: Git Flow e Processos

### Branch Strategy

```
main (production)
  ↑
  └── staging (pre-production, estável)
        ↑
        └── dev (integration, pode ter bugs)
              ↑
              ├── feature/mobile-app
              ├── feature/analytics
              └── bugfix/login-error
```

### Workflow Diário

**1. Começar nova feature:**
```bash
git checkout dev
git pull origin dev
git checkout -b feature/nome-da-feature
```

**2. Desenvolver:**
```bash
# Fazer alterações
git add .
git commit -m "feat: adicionar funcionalidade X"

# Push
git push origin feature/nome-da-feature
```

**3. Criar Pull Request:**
- No GitHub: `feature/nome-da-feature` → `dev`
- Preencher descrição
- Aguardar CI passar
- Pedir review
- Merge após aprovação

**4. Deploy para Staging:**
```bash
git checkout staging
git pull origin staging
git merge dev
git push origin staging
# Auto-deploy para staging
```

**5. Testar Staging:**
- Acessar https://staging.taktiq.app
- Testar feature nova
- Verificar se não quebrou nada
- Se bugs: corrigir em `bugfix/*` → merge `dev` → merge `staging`

**6. Deploy para Production (quando estável):**
```bash
git checkout main
git pull origin main
git merge staging
git push origin main
# Aguarda aprovação no GitHub
# Depois de aprovado, auto-deploy para produção
```

---

## DIA 5: Documentação e Treinamento

### README para Desenvolvedores

Criar `DEVELOPMENT.md`:
```markdown
# Guia de Desenvolvimento

## Setup Local

### Pré-requisitos
- .NET 8 SDK
- Node.js 20+
- Docker
- Git

### Primeira vez

1. Clone o repositório
2. Rode `scripts/dev.sh` (ou `dev.ps1` no Windows)
3. Acesse http://localhost:3000

### Comandos úteis

# Rodar tudo
./scripts/dev.sh

# Apenas banco
docker-compose -f docker-compose.dev.yml up -d

# Apenas API
cd src/GymHero.Api && dotnet run

# Apenas frontend
cd frontend/apps/web && pnpm dev

# Testes
dotnet test
pnpm test

### URLs

- Frontend: http://localhost:3000
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- Staging: https://staging.taktiq.app

### Credentials de Teste

- Email: carlos@pt.com
- Senha: Dev123!
```

### Treinamento do Time

**Session de 1 hora:**

1. **Introdução (10 min)**
   - Por que mudamos?
   - Fluxo antigo vs novo

2. **Demo: Setup Local (15 min)**
   - Rodar `dev.sh`
   - Mostrar containers
   - Login no app local

3. **Demo: Git Flow (15 min)**
   - Criar branch
   - Fazer commit
   - Abrir PR
   - Merge

4. **Demo: Deploy Staging (10 min)**
   - Push para staging
   - Verificar CI
   - Testar staging

5. **Q&A (10 min)**

---

## ✅ Checklist Final

### Setup Completo
- [ ] Dev local rodando (API + Frontend + DB)
- [ ] Staging no Azure funcionando
- [ ] Production mantido como está
- [ ] CI/CD workflows configurados
- [ ] Git flow definido
- [ ] Documentação criada
- [ ] Time treinado

### Validação
- [ ] Push para `dev` → CI roda testes ✅
- [ ] Push para `staging` → Deploy staging automático ✅
- [ ] Push para `main` → Aguarda aprovação → Deploy prod ✅
- [ ] Rollback funciona
- [ ] Environments isolados (não há cross-talk)

---

## 🆘 Troubleshooting

### "Connection to database failed" (local)
```bash
# Verificar se containers estão rodando
docker ps

# Se não estiverem:
docker-compose -f docker-compose.dev.yml up -d

# Aguardar 10 segundos
sleep 10

# Tentar novamente
```

### "Migrations failed" (staging)
```bash
# Verificar firewall do PostgreSQL
az postgres flexible-server firewall-rule list \
  --resource-group TaktIQ-Staging \
  --name taktiq-db-staging

# Adicionar seu IP se necessário
az postgres flexible-server firewall-rule create \
  --resource-group TaktIQ-Staging \
  --name taktiq-db-staging \
  --rule-name MyIP \
  --start-ip-address <SEU-IP> \
  --end-ip-address <SEU-IP>
```

### "CI failed on PR"
- Ver logs no GitHub Actions
- Rodar testes localmente: `dotnet test`
- Corrigir e fazer novo commit

---

## 📞 Contato

Dúvidas? Fale com o time de DevOps ou abra issue no repo.

---

**Última atualização:** 08/12/2025
**Próxima revisão:** Após 1 mês de uso
