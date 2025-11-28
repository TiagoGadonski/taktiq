# Script para limpar artifacts do GitHub Actions
# Execute como Administrador!

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "GitHub Artifacts Cleanup Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as admin
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "AVISO: Execute este script como Administrador!" -ForegroundColor Yellow
    Write-Host ""
}

# Check if gh CLI is installed
$ghPath = Get-Command gh -ErrorAction SilentlyContinue

if (-not $ghPath) {
    Write-Host "GitHub CLI não encontrado. Instalando..." -ForegroundColor Yellow

    # Install via winget
    try {
        winget install --id GitHub.cli --silent --accept-package-agreements --accept-source-agreements
        Write-Host "GitHub CLI instalado com sucesso!" -ForegroundColor Green

        # Refresh PATH
        $env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User")

    } catch {
        Write-Host "Erro ao instalar GitHub CLI via winget." -ForegroundColor Red
        Write-Host "Por favor, instale manualmente: https://cli.github.com/" -ForegroundColor Yellow
        Write-Host "Ou use a opção web (veja GITHUB-ARTIFACTS-CLEANUP.md)" -ForegroundColor Yellow
        exit 1
    }
}

Write-Host "GitHub CLI encontrado!" -ForegroundColor Green
Write-Host ""

# Check if logged in
$authStatus = gh auth status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Você não está logado no GitHub CLI." -ForegroundColor Yellow
    Write-Host "Executando login..." -ForegroundColor Yellow
    gh auth login
}

Write-Host ""
Write-Host "Buscando artifacts do repositório TiagoGadonski/taktiq..." -ForegroundColor Cyan
Write-Host ""

# Get all artifacts
try {
    $artifactsJson = gh api repos/TiagoGadonski/taktiq/actions/artifacts --paginate
    $artifacts = $artifactsJson | ConvertFrom-Json | Select-Object -ExpandProperty artifacts

    if ($artifacts.Count -eq 0) {
        Write-Host "Nenhum artifact encontrado! Sua quota já está limpa." -ForegroundColor Green
        exit 0
    }

    # Calculate total size
    $totalSize = ($artifacts | Measure-Object -Property size_in_bytes -Sum).Sum
    $totalSizeMB = [math]::Round($totalSize / 1MB, 2)

    Write-Host "Encontrados $($artifacts.Count) artifacts" -ForegroundColor Yellow
    Write-Host "Tamanho total: $totalSizeMB MB" -ForegroundColor Yellow
    Write-Host ""

    # Show artifacts
    Write-Host "Lista de artifacts:" -ForegroundColor Cyan
    $artifacts | Format-Table -Property @{
        Label = "ID"; Expression = { $_.id }
    }, @{
        Label = "Nome"; Expression = { $_.name }
    }, @{
        Label = "Tamanho (MB)"; Expression = { [math]::Round($_.size_in_bytes / 1MB, 2) }
    }, @{
        Label = "Criado em"; Expression = { $_.created_at }
    }

    Write-Host ""
    $confirm = Read-Host "Deseja deletar TODOS os artifacts? (S/N)"

    if ($confirm -eq 'S' -or $confirm -eq 's') {
        Write-Host ""
        Write-Host "Deletando artifacts..." -ForegroundColor Yellow

        $deletedCount = 0
        $errorCount = 0

        foreach ($artifact in $artifacts) {
            try {
                gh api --method DELETE "repos/TiagoGadonski/taktiq/actions/artifacts/$($artifact.id)" | Out-Null
                Write-Host "✓ Deletado: $($artifact.name) (ID: $($artifact.id))" -ForegroundColor Green
                $deletedCount++
            } catch {
                Write-Host "✗ Erro ao deletar: $($artifact.name)" -ForegroundColor Red
                $errorCount++
            }
        }

        Write-Host ""
        Write-Host "========================================" -ForegroundColor Cyan
        Write-Host "Resumo:" -ForegroundColor Cyan
        Write-Host "  Deletados: $deletedCount" -ForegroundColor Green
        Write-Host "  Erros: $errorCount" -ForegroundColor $(if ($errorCount -gt 0) { "Red" } else { "Green" })
        Write-Host "  Espaço liberado: ~$totalSizeMB MB" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "IMPORTANTE:" -ForegroundColor Yellow
        Write-Host "A quota do GitHub é recalculada a cada 6-12 horas." -ForegroundColor Yellow
        Write-Host "Aguarde este período antes de tentar fazer deploy novamente." -ForegroundColor Yellow
        Write-Host ""

    } else {
        Write-Host "Operação cancelada." -ForegroundColor Yellow
    }

} catch {
    Write-Host "Erro ao buscar artifacts:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Tente usar a opção web (veja GITHUB-ARTIFACTS-CLEANUP.md)" -ForegroundColor Yellow
    exit 1
}
