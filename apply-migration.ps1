# PowerShell script to apply database migration to Azure PostgreSQL
# Run this script from the gymhero2 directory

$env:PGPASSWORD = "K3jop5%tmr"

Write-Host "Connecting to Azure PostgreSQL and applying migration..." -ForegroundColor Green

# Run the migration SQL script
psql -h taktiq-db.postgres.database.azure.com `
     -U tasktiqadmin `
     -d postgres `
     -p 5432 `
     -f "src/GymHero.Infrastructure/migration.sql" `
     --set=sslmode=require

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nMigration applied successfully!" -ForegroundColor Green
    Write-Host "You can now restart your Azure App Service." -ForegroundColor Yellow
} else {
    Write-Host "`nError applying migration. Exit code: $LASTEXITCODE" -ForegroundColor Red
}

# Clear password from environment
Remove-Item Env:PGPASSWORD
