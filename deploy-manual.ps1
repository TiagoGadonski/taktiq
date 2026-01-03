# Script de Deploy Manual para Azure
Write-Host "🚀 Iniciando deploy manual para Azure..." -ForegroundColor Cyan

# 1. Build
Write-Host "`n📦 Compilando projeto..." -ForegroundColor Yellow
dotnet build src/GymHero.Api/GymHero.Api.csproj --configuration Release

# 2. Publish
Write-Host "`n📦 Publicando..." -ForegroundColor Yellow
$publishPath = ".\publish-temp"
Remove-Item -Path $publishPath -Recurse -Force -ErrorAction SilentlyContinue
dotnet publish src/GymHero.Api/GymHero.Api.csproj -c Release -o $publishPath

# 3. Criar ZIP
Write-Host "`n📦 Criando ZIP..." -ForegroundColor Yellow
$zipPath = ".\deploy-package.zip"
Remove-Item -Path $zipPath -Force -ErrorAction SilentlyContinue
Compress-Archive -Path "$publishPath\*" -DestinationPath $zipPath -Force

Write-Host "`n✅ Pacote criado: $zipPath" -ForegroundColor Green
Write-Host "`nPRÓXIMO: Use Azure CLI ou Portal para fazer upload" -ForegroundColor Cyan
