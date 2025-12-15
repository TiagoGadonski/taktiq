# TaktIQ - Local Development Environment

Este diretório contém scripts para facilitar o setup e uso do ambiente de desenvolvimento local.

## 📋 Pré-requisitos

Antes de começar, certifique-se de ter instalado:

- ✅ [Docker Desktop](https://www.docker.com/products/docker-desktop) (Windows/Mac) ou Docker Engine (Linux)
- ✅ [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- ✅ [Node.js 18+](https://nodejs.org/) (para o frontend)
- ✅ PowerShell (Windows) ou Bash (Linux/Mac)

## 🚀 Quick Start

### Windows

```powershell
# 1. Iniciar ambiente de desenvolvimento
.\scripts\dev.ps1

# 2. Aguardar todos os serviços subirem (cerca de 30 segundos)

# 3. Rodar migrations (se necessário)
cd src\GymHero.Api
dotnet ef database update

# 4. Iniciar a API
dotnet run --launch-profile Development.Local

# 5. (Em outro terminal) Popular com dados fake
# Acesse: POST http://localhost:5001/api/admin/seed-dev-data

# 6. (Em outro terminal) Iniciar frontend
cd webapp
npm run dev
```

### Linux/Mac

```bash
# 1. Iniciar ambiente de desenvolvimento
./scripts/dev.sh

# 2. Aguardar todos os serviços subirem (cerca de 30 segundos)

# 3. Rodar migrations (se necessário)
cd src/GymHero.Api
dotnet ef database update

# 4. Iniciar a API
dotnet run --launch-profile Development.Local

# 5. (Em outro terminal) Popular com dados fake
# Acesse: POST http://localhost:5001/api/admin/seed-dev-data

# 6. (Em outro terminal) Iniciar frontend
cd webapp
npm run dev
```

## 📦 O que o script faz?

O script `dev.ps1` (Windows) ou `dev.sh` (Linux/Mac) automatiza:

1. **Inicia containers Docker:**
   - PostgreSQL (localhost:5432)
   - Redis (localhost:6379)
   - Azurite - Azure Storage Emulator (localhost:10000)

2. **Aguarda serviços ficarem prontos:**
   - Verifica health checks
   - Aguarda PostgreSQL aceitar conexões
   - Valida Redis e Azurite

3. **Roda migrations (opcional):**
   - Atualiza esquema do banco automaticamente
   - Pode ser pulado com flag `--no-migrations`

## 🎯 Serviços Locais

Após rodar o script, você terá acesso a:

| Serviço | URL | Credenciais |
|---------|-----|-------------|
| **PostgreSQL** | `localhost:5432` | DB: `taktiq_dev`<br>User: `postgres`<br>Password: `postgres_dev_password` |
| **Redis** | `localhost:6379` | Sem autenticação |
| **Azurite (Blob)** | `http://localhost:10000` | Account: `devstoreaccount1`<br>(credenciais fixas do Azurite) |
| **API** | `https://localhost:7001` | Swagger: `/swagger` |
| **Frontend** | `http://localhost:3000` | Next.js dev server |

## 🗄️ Dados de Desenvolvimento (Seed)

Para popular o banco com dados fake para testes, use o endpoint:

```bash
# PowerShell (Windows)
Invoke-WebRequest -Uri "http://localhost:5001/api/admin/seed-dev-data" -Method POST

# cURL (Linux/Mac/Windows)
curl -X POST http://localhost:5001/api/admin/seed-dev-data
```

### O que será criado:

- ✅ **5 Personal Trainers** (com perfis completos, especializações, etc.)
- ✅ **20 Alunos** (distribuídos entre os PTs)
- ✅ **10 Planos de Treino** (alguns gratuitos, alguns pagos, públicos e privados)
- ✅ **15 Amizades** entre usuários
- ✅ Todos os usuários têm a senha: `Dev@123456`

### Personal Trainers de exemplo:

| Nome | Email | Especialização |
|------|-------|----------------|
| Tiago Cordeiro | tiago@taktiq.app | Musculação, Hipertrofia |
| Mariana Silva | mariana@taktiq.app | Funcional, Emagrecimento |
| Carlos Mendes | carlos@taktiq.app | CrossFit, Condicionamento |
| Fernanda Costa | fernanda@taktiq.app | Yoga, Pilates |
| Roberto Alves | roberto@taktiq.app | Reabilitação, Terceira Idade |

### Alunos de exemplo:

| Nome | Email |
|------|-------|
| Ana Santos | ana.santos@email.com |
| Bruno Oliveira | bruno.oliveira@email.com |
| Carla Pereira | carla.pereira@email.com |
| ... | ... |

**Senha para todos:** `Dev@123456`

## 🛠️ Comandos Úteis

### Parar ambiente

```powershell
# Windows
.\scripts\dev.ps1 -Stop

# Linux/Mac
./scripts/dev.sh --stop
```

### Limpar dados e recomeçar

⚠️ **ATENÇÃO:** Isto apagará TODOS os dados locais!

```powershell
# Windows
.\scripts\dev.ps1 -Clean

# Linux/Mac
./scripts/dev.sh --clean
```

### Ver logs dos containers

```bash
docker-compose -f docker-compose.dev.yml logs -f
```

### Rodar migrations manualmente

```bash
cd src\GymHero.Api
dotnet ef database update
```

### Popular exercícios da API externa (Wger)

```bash
# PowerShell
Invoke-WebRequest -Uri "http://localhost:5001/api/admin/seed-exercises" -Method POST

# cURL
curl -X POST http://localhost:5001/api/admin/seed-exercises
```

### Acessar banco PostgreSQL diretamente

```bash
docker exec -it taktiq-postgres-dev psql -U postgres -d taktiq_dev
```

## 📁 Arquivos de Configuração

| Arquivo | Descrição |
|---------|-----------|
| `docker-compose.dev.yml` | Define os serviços Docker (PostgreSQL, Redis, Azurite) |
| `src/GymHero.Api/appsettings.Development.Local.json` | Configurações para ambiente local (connection strings, API keys fake) |
| `scripts/dev.ps1` | Script PowerShell para Windows |
| `scripts/dev.sh` | Script Bash para Linux/Mac |

## 🔧 Troubleshooting

### Erro: "Port 5432 already in use"

Você já tem PostgreSQL rodando localmente. Opções:

1. Parar seu PostgreSQL local
2. Mudar a porta no `docker-compose.dev.yml` (ex: `"5433:5432"`)
3. Atualizar connection string no `appsettings.Development.Local.json`

### Erro: "Docker daemon not running"

1. Abra Docker Desktop
2. Aguarde inicializar completamente
3. Rode o script novamente

### Migrations não aplicam

```bash
# Rebuild e aplique migrations
cd src\GymHero.Api
dotnet build
dotnet ef database update --verbose
```

### Erro ao popular dados (seed)

```bash
# Limpe o banco e recomeçe
.\scripts\dev.ps1 -Clean
.\scripts\dev.ps1
# Depois popule novamente
```

## 🔐 Segurança

⚠️ **IMPORTANTE:**

- Estes arquivos são para **DESENVOLVIMENTO LOCAL APENAS**
- As credenciais aqui são **FAKE** e não devem ser usadas em produção
- O arquivo `appsettings.Development.Local.json` **NÃO é commitado** no Git (está no `.gitignore`)
- **NUNCA** use estas configurações em Staging ou Production

## 🎉 Pronto!

Seu ambiente local está configurado! Agora você pode:

1. ✅ Desenvolver sem afetar produção
2. ✅ Testar features com dados fake
3. ✅ Rodar testes de integração
4. ✅ Debugar com breakpoints
5. ✅ Experimentar mudanças sem medo

## 📚 Próximos Passos

Depois de configurar o ambiente local, veja:

- [SETUP_ENVIRONMENTS.md](../docs/SETUP_ENVIRONMENTS.md) - Configurar Staging e Production
- [TECHNICAL_ROADMAP.md](../docs/TECHNICAL_ROADMAP.md) - Roadmap técnico completo
- [ACTION_ROADMAP.md](../docs/ACTION_ROADMAP.md) - Tarefas práticas para implementar

---

**Última atualização:** 08/12/2025
**Autor:** Equipe TaktIQ
