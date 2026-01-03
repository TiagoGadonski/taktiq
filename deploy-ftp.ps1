# Deploy via FTP - Método alternativo quando outros métodos falham
Write-Host "🔧 Deploy via FTP (Método de Emergência)" -ForegroundColor Yellow

# Primeiro, criar o pacote de publish se não existir
if (-Not (Test-Path ".\publish-fresh")) {
    Write-Host "📦 Criando build fresh..." -ForegroundColor Cyan
    dotnet clean src/GymHero.Api/GymHero.Api.csproj
    dotnet publish src/GymHero.Api/GymHero.Api.csproj -c Release -o ./publish-fresh
}

Write-Host ""
Write-Host "⚠️  INSTRUÇÕES MANUAIS DE DEPLOY VIA FTP:" -ForegroundColor Red
Write-Host "=========================================" -ForegroundColor Red
Write-Host ""
Write-Host "1. Vá para o Azure Portal: https://portal.azure.com" -ForegroundColor White
Write-Host "2. Procure por 'taktiqwinback'" -ForegroundColor White
Write-Host "3. No menu lateral, clique em 'Deployment Center'" -ForegroundColor White
Write-Host "4. Copie o FTP hostname, username e password" -ForegroundColor White
Write-Host "5. Use um cliente FTP (FileZilla, WinSCP) para fazer upload" -ForegroundColor White
Write-Host ""
Write-Host "OU use este comando Azure CLI:" -ForegroundColor Cyan
Write-Host "az webapp deployment source config-zip --resource-group <RESOURCE_GROUP> --name taktiqwinback --src deploy-v2-fresh.zip" -ForegroundColor Green
Write-Host ""
Write-Host "📁 Arquivos prontos em: .\publish-fresh\" -ForegroundColor Yellow
Write-Host "📦 ZIP pronto em: .\deploy-v2-fresh.zip (33 MB)" -ForegroundColor Yellow
Write-Host ""
Write-Host "Após upload, REINICIE o App Service!" -ForegroundColor Red
