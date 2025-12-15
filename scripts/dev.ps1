# TaktIQ - Local Development Environment Setup Script
# This script starts all local services and prepares the development environment

param(
    [switch]$Stop,
    [switch]$Clean,
    [switch]$Seed,
    [switch]$NoMigrations
)

$ErrorActionPreference = "Stop"

# Colors for output
function Write-Info { Write-Host $args -ForegroundColor Cyan }
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Error { Write-Host $args -ForegroundColor Red }
function Write-Warning { Write-Host $args -ForegroundColor Yellow }

# Project paths
$projectRoot = Split-Path -Parent $PSScriptRoot
$apiProject = Join-Path $projectRoot "src\GymHero.Api"

Write-Info "═══════════════════════════════════════════════════════"
Write-Info "   TaktIQ - Local Development Environment"
Write-Info "═══════════════════════════════════════════════════════"

# Stop containers
if ($Stop) {
    Write-Info "`n[1/1] Stopping Docker containers..."
    docker-compose -f docker-compose.dev.yml down
    Write-Success "✓ Containers stopped"
    exit 0
}

# Clean and stop
if ($Clean) {
    Write-Warning "`n[WARNING] This will remove all local data!"
    $confirm = Read-Host "Are you sure? (yes/no)"
    if ($confirm -eq "yes") {
        Write-Info "`n[1/2] Stopping containers..."
        docker-compose -f docker-compose.dev.yml down -v
        Write-Info "`n[2/2] Cleaning logs..."
        Remove-Item -Path "$apiProject\logs" -Recurse -Force -ErrorAction SilentlyContinue
        Write-Success "✓ Cleaned successfully"
    } else {
        Write-Info "Cancelled."
    }
    exit 0
}

# Start development environment
Write-Info "`n[1/5] Starting Docker containers..."
docker-compose -f docker-compose.dev.yml up -d

# Wait for services to be healthy
Write-Info "`n[2/5] Waiting for services to be ready..."
Start-Sleep -Seconds 5

Write-Info "  → Checking PostgreSQL..."
$maxRetries = 10
$retry = 0
while ($retry -lt $maxRetries) {
    $result = docker exec taktiq-postgres-dev pg_isready -U postgres 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Success "  ✓ PostgreSQL is ready"
        break
    }
    $retry++
    Write-Info "  ... waiting ($retry/$maxRetries)"
    Start-Sleep -Seconds 2
}

Write-Info "  → Checking Redis..."
$result = docker exec taktiq-redis-dev redis-cli ping 2>&1
if ($result -eq "PONG") {
    Write-Success "  ✓ Redis is ready"
}

Write-Info "  → Checking Azurite..."
$result = Test-NetConnection -ComputerName localhost -Port 10000 -WarningAction SilentlyContinue
if ($result.TcpTestSucceeded) {
    Write-Success "  ✓ Azurite is ready"
}

# Run migrations
if (-not $NoMigrations) {
    Write-Info "`n[3/5] Running database migrations..."
    Push-Location $apiProject
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:DOTNET_ENVIRONMENT = "Development"

    dotnet ef database update --no-build

    if ($LASTEXITCODE -eq 0) {
        Write-Success "✓ Migrations completed"
    } else {
        Write-Error "✗ Migration failed. You may need to run: dotnet build first"
    }
    Pop-Location
} else {
    Write-Warning "`n[3/5] Skipping migrations (--NoMigrations flag)"
}

# Seed data
if ($Seed) {
    Write-Info "`n[4/5] Seeding development data..."
    Write-Warning "  TODO: Implement seed command"
    Write-Info "  For now, seed data will be created on first API run"
} else {
    Write-Info "`n[4/5] Skipping seed data (use -Seed flag to seed)"
}

# Display connection info
Write-Info "`n[5/5] Development environment is ready!"
Write-Success "`n═══════════════════════════════════════════════════════"
Write-Success "   🚀 TaktIQ Development Environment Ready!"
Write-Success "═══════════════════════════════════════════════════════"

Write-Host "`nServices running:"
Write-Host "  → PostgreSQL:  " -NoNewline
Write-Host "localhost:5432" -ForegroundColor Yellow
Write-Host "     Database:   taktiq_dev"
Write-Host "     User:       postgres"
Write-Host "     Password:   postgres_dev_password"

Write-Host "`n  → Redis:       " -NoNewline
Write-Host "localhost:6379" -ForegroundColor Yellow

Write-Host "`n  → Azurite:     " -NoNewline
Write-Host "http://localhost:10000" -ForegroundColor Yellow

Write-Host "`nNext steps:"
Write-Host "  1. Start the API:"
Write-Host "     cd src\GymHero.Api"
Write-Host "     dotnet run --launch-profile Development.Local"

Write-Host "`n  2. Start the Frontend:"
Write-Host "     cd webapp"
Write-Host "     npm run dev"

Write-Host "`nUseful commands:"
Write-Host "  → Stop containers:   " -NoNewline
Write-Host ".\scripts\dev.ps1 -Stop" -ForegroundColor Yellow
Write-Host "  → Clean all data:    " -NoNewline
Write-Host ".\scripts\dev.ps1 -Clean" -ForegroundColor Yellow
Write-Host "  → View logs:         " -NoNewline
Write-Host "docker-compose -f docker-compose.dev.yml logs -f" -ForegroundColor Yellow

Write-Host ""
