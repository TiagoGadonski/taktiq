# Script de Deploy Manual para Azure

Write-Host "Building aplicacao..." -ForegroundColor Yellow
dotnet publish src/GymHero.Api/GymHero.Api.csproj -c Release -o ./publish

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro no build!" -ForegroundColor Red
    exit 1
}

Write-Host "Build concluido!" -ForegroundColor Green

Write-Host "Criando arquivo ZIP..." -ForegroundColor Yellow
$zipPath = ".\deploy.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath
}

Compress-Archive -Path .\publish\* -DestinationPath $zipPath

Write-Host "ZIP criado com sucesso!" -ForegroundColor Green
Write-Host "Arquivo: deploy.zip" -ForegroundColor Cyan
Write-Host ""
Write-Host "Agora va para Azure Portal e faca upload do ZIP!" -ForegroundColor Yellow
