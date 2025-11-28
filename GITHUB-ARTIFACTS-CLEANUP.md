# Limpeza de Artifacts do GitHub Actions

## Problema Identificado
A cota de armazenamento de artifacts do GitHub Actions foi atingida, impedindo novos uploads.

## Solução Implementada

### 1. Redução do Tempo de Retenção ✅
- **Antes**: Artifacts mantidos por 90 dias (padrão)
- **Depois**: Artifacts mantidos por apenas **1 dia**

Arquivos alterados:
- `.github/workflows/main_taktiq-web-frontend.yml` - Adicionado `retention-days: 1`
- `.github/workflows/main_taktiqwinback.yml` - Adicionado `retention-days: 1`

**Por que 1 dia é suficiente?**
- Os artifacts são usados apenas para transferir o build entre os jobs `build` e `deploy`
- O job de deploy roda imediatamente após o build
- Não há necessidade de manter artifacts por mais tempo

---

## Como Limpar Artifacts Antigos Manualmente

### Opção 1: Via Interface Web do GitHub (Recomendado)

1. Acesse o repositório: https://github.com/TiagoGadonski/taktiq
2. Clique em **Actions** (no menu superior)
3. No painel esquerdo, clique em **Artifacts** ou veja os workflows recentes
4. Para cada workflow concluído:
   - Clique no workflow
   - Role até a seção "Artifacts"
   - Clique no ícone de **lixeira** ao lado de cada artifact para deletá-lo

### Opção 2: Via GitHub CLI (Mais Rápido)

Se você tiver o GitHub CLI instalado:

```bash
# Instalar GitHub CLI (se não tiver)
# Windows (via winget):
winget install GitHub.cli

# Ou via Chocolatey:
choco install gh

# Login no GitHub
gh auth login

# Listar todos os artifacts do repositório
gh api repos/TiagoGadonski/taktiq/actions/artifacts --paginate | jq '.artifacts[] | {id, name, size_in_bytes, created_at}'

# Deletar todos os artifacts de uma vez
gh api repos/TiagoGadonski/taktiq/actions/artifacts --paginate | jq -r '.artifacts[].id' | xargs -I {} gh api --method DELETE repos/TiagoGadonski/taktiq/actions/artifacts/{}
```

### Opção 3: Script PowerShell (Windows)

```powershell
# Requer GitHub CLI (gh) instalado

# Listar artifacts
gh api repos/TiagoGadonski/taktiq/actions/artifacts --paginate | ConvertFrom-Json | Select-Object -ExpandProperty artifacts | Format-Table id, name, size_in_bytes, created_at

# Deletar todos os artifacts
$artifacts = gh api repos/TiagoGadonski/taktiq/actions/artifacts --paginate | ConvertFrom-Json | Select-Object -ExpandProperty artifacts
foreach ($artifact in $artifacts) {
    Write-Host "Deletando artifact: $($artifact.name) (ID: $($artifact.id))"
    gh api --method DELETE "repos/TiagoGadonski/taktiq/actions/artifacts/$($artifact.id)"
}
Write-Host "Todos os artifacts foram deletados!"
```

---

## Verificar Uso de Armazenamento

1. Vá em **Settings** do repositório
2. Clique em **Billing and plans** (no menu esquerdo)
3. Veja a seção **Storage for Actions and Packages**

Ou acesse diretamente:
https://github.com/TiagoGadonski/taktiq/settings/billing

---

## Prevenção Futura

Com a mudança implementada (`retention-days: 1`), os novos artifacts serão automaticamente deletados após 1 dia, evitando acúmulo de armazenamento.

**Benefícios:**
- ✅ Reduz uso de armazenamento em ~97% (90 dias → 1 dia)
- ✅ Evita atingir a cota novamente
- ✅ Mantém funcionalidade de deploy intacta
- ✅ Sem impacto no workflow de CI/CD

---

## Resumo da Cota do GitHub Actions

### GitHub Free (Repositórios Públicos)
- Storage: **500 MB** grátis
- Minutos: **2000 min/mês** grátis

### GitHub Free (Repositórios Privados)
- Storage: **500 MB** grátis
- Minutos: **2000 min/mês** grátis

**Nota**: O armazenamento é recalculado a cada 6-12 horas após deletar artifacts.

---

## Checklist de Ação Imediata

- [ ] Deletar artifacts antigos (via web ou CLI)
- [ ] Aguardar 6-12 horas para recálculo de cota
- [ ] Tentar novo push/deploy
- [ ] Verificar que novos artifacts têm `retention-days: 1`

---

## Referências

- [GitHub Actions Billing](https://docs.github.com/en/billing/managing-billing-for-github-actions/about-billing-for-github-actions)
- [Managing Artifacts](https://docs.github.com/en/actions/managing-workflow-runs/removing-workflow-artifacts)
- [GitHub CLI](https://cli.github.com/)
