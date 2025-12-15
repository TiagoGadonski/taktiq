#!/bin/bash

# TaktIQ - Local Development Environment Setup Script
# This script starts all local services and prepares the development environment

set -e

# Colors for output
CYAN='\033[0;36m'
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

info() {
    echo -e "${CYAN}$1${NC}"
}

success() {
    echo -e "${GREEN}$1${NC}"
}

error() {
    echo -e "${RED}$1${NC}"
}

warning() {
    echo -e "${YELLOW}$1${NC}"
}

# Project paths
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
API_PROJECT="$PROJECT_ROOT/src/GymHero.Api"

info "═══════════════════════════════════════════════════════"
info "   TaktIQ - Local Development Environment"
info "═══════════════════════════════════════════════════════"

# Parse arguments
STOP=false
CLEAN=false
SEED=false
NO_MIGRATIONS=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --stop)
            STOP=true
            shift
            ;;
        --clean)
            CLEAN=true
            shift
            ;;
        --seed)
            SEED=true
            shift
            ;;
        --no-migrations)
            NO_MIGRATIONS=true
            shift
            ;;
        *)
            echo "Unknown option: $1"
            echo "Usage: $0 [--stop] [--clean] [--seed] [--no-migrations]"
            exit 1
            ;;
    esac
done

# Stop containers
if [ "$STOP" = true ]; then
    info "\n[1/1] Stopping Docker containers..."
    docker-compose -f "$PROJECT_ROOT/docker-compose.dev.yml" down
    success "✓ Containers stopped"
    exit 0
fi

# Clean and stop
if [ "$CLEAN" = true ]; then
    warning "\n[WARNING] This will remove all local data!"
    read -p "Are you sure? (yes/no): " confirm
    if [ "$confirm" = "yes" ]; then
        info "\n[1/2] Stopping containers..."
        docker-compose -f "$PROJECT_ROOT/docker-compose.dev.yml" down -v
        info "\n[2/2] Cleaning logs..."
        rm -rf "$API_PROJECT/logs"
        success "✓ Cleaned successfully"
    else
        info "Cancelled."
    fi
    exit 0
fi

# Start development environment
info "\n[1/5] Starting Docker containers..."
cd "$PROJECT_ROOT"
docker-compose -f docker-compose.dev.yml up -d

# Wait for services to be healthy
info "\n[2/5] Waiting for services to be ready..."
sleep 5

info "  → Checking PostgreSQL..."
max_retries=10
retry=0
while [ $retry -lt $max_retries ]; do
    if docker exec taktiq-postgres-dev pg_isready -U postgres > /dev/null 2>&1; then
        success "  ✓ PostgreSQL is ready"
        break
    fi
    retry=$((retry + 1))
    info "  ... waiting ($retry/$max_retries)"
    sleep 2
done

info "  → Checking Redis..."
if docker exec taktiq-redis-dev redis-cli ping > /dev/null 2>&1; then
    success "  ✓ Redis is ready"
fi

info "  → Checking Azurite..."
if nc -z localhost 10000 > /dev/null 2>&1; then
    success "  ✓ Azurite is ready"
fi

# Run migrations
if [ "$NO_MIGRATIONS" = false ]; then
    info "\n[3/5] Running database migrations..."
    cd "$API_PROJECT"
    export ASPNETCORE_ENVIRONMENT=Development
    export DOTNET_ENVIRONMENT=Development

    if dotnet ef database update --no-build; then
        success "✓ Migrations completed"
    else
        error "✗ Migration failed. You may need to run: dotnet build first"
    fi
else
    warning "\n[3/5] Skipping migrations (--no-migrations flag)"
fi

# Seed data
if [ "$SEED" = true ]; then
    info "\n[4/5] Seeding development data..."
    warning "  TODO: Implement seed command"
    info "  For now, seed data will be created on first API run"
else
    info "\n[4/5] Skipping seed data (use --seed flag to seed)"
fi

# Display connection info
info "\n[5/5] Development environment is ready!"
success "\n═══════════════════════════════════════════════════════"
success "   🚀 TaktIQ Development Environment Ready!"
success "═══════════════════════════════════════════════════════"

echo ""
echo "Services running:"
echo -e "  → PostgreSQL:  ${YELLOW}localhost:5432${NC}"
echo "     Database:   taktiq_dev"
echo "     User:       postgres"
echo "     Password:   postgres_dev_password"

echo ""
echo -e "  → Redis:       ${YELLOW}localhost:6379${NC}"

echo ""
echo -e "  → Azurite:     ${YELLOW}http://localhost:10000${NC}"

echo ""
echo "Next steps:"
echo "  1. Start the API:"
echo "     cd src/GymHero.Api"
echo "     dotnet run --launch-profile Development.Local"

echo ""
echo "  2. Start the Frontend:"
echo "     cd webapp"
echo "     npm run dev"

echo ""
echo "Useful commands:"
echo -e "  → Stop containers:   ${YELLOW}./scripts/dev.sh --stop${NC}"
echo -e "  → Clean all data:    ${YELLOW}./scripts/dev.sh --clean${NC}"
echo -e "  → View logs:         ${YELLOW}docker-compose -f docker-compose.dev.yml logs -f${NC}"

echo ""
