# Test Exercise Fix Endpoints
# Run this script after starting the API with: dotnet run --project src/GymHero.Api

$baseUrl = "https://localhost:7001"
$adminEmail = "admin@gymhero.com"
$adminPassword = "Admin123!"

# Ignore SSL certificate errors for local development
add-type @"
using System.Net;
using System.Security.Cryptography.X509Certificates;
public class TrustAllCertsPolicy : ICertificatePolicy {
    public bool CheckValidationResult(
        ServicePoint srvPoint, X509Certificate certificate,
        WebRequest request, int certificateProblem) {
        return true;
    }
}
"@
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing Exercise Fix Endpoints" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 0: Test API connectivity first
Write-Host "0. Testing API Connectivity" -ForegroundColor Yellow
try {
    $pingResponse = Invoke-RestMethod -Uri "$baseUrl/api/ping" -Method Get -TimeoutSec 5 -ErrorAction Stop
    Write-Host "API is running!" -ForegroundColor Green
    Write-Host "Response: $pingResponse" -ForegroundColor Gray
} catch {
    Write-Host "FAILED to connect to API: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Make sure the API is running with: dotnet run --project src/GymHero.Api" -ForegroundColor Yellow
    exit 1
}
Write-Host ""

# Step 1: Seed Admin User (dev only)
Write-Host "1. Seeding Admin User (Dev Only)" -ForegroundColor Yellow
try {
    $seedResponse = Invoke-RestMethod -Uri "$baseUrl/api/admin/dev/seed-admin" -Method Post -ContentType "application/json" -TimeoutSec 10 -ErrorAction Stop
    Write-Host "Admin seed result: $($seedResponse.message)" -ForegroundColor Green
} catch {
    $statusCode = $null
    if ($_.Exception.Response) {
        $statusCode = [int]$_.Exception.Response.StatusCode
    }
    if ($statusCode -eq 403) {
        Write-Host "Cannot seed admin in production environment" -ForegroundColor Yellow
    } elseif ($statusCode -eq 500) {
        Write-Host "Server error on seed - admin may already exist or DB issue" -ForegroundColor Yellow
    } else {
        Write-Host "Seed response: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}
Write-Host ""

# Step 2: Login
Write-Host "2. Logging in as Admin" -ForegroundColor Yellow
$token = $null
try {
    $loginBody = @{
        email = $adminEmail
        password = $adminPassword
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json" -TimeoutSec 10 -ErrorAction Stop
    $token = $loginResponse.token
    Write-Host "SUCCESS! Logged in as admin" -ForegroundColor Green
    Write-Host "Token received (first 50 chars): $($token.Substring(0, [Math]::Min(50, $token.Length)))..." -ForegroundColor Gray
} catch {
    Write-Host "FAILED to login: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Trying to create admin user with different method..." -ForegroundColor Yellow

    # Try creating custom admin
    try {
        $createAdminBody = @{
            name = "Admin"
            email = $adminEmail
            password = $adminPassword
        } | ConvertTo-Json

        $createResponse = Invoke-RestMethod -Uri "$baseUrl/api/admin/dev/create-admin" -Method Post -Body $createAdminBody -ContentType "application/json" -TimeoutSec 10 -ErrorAction Stop
        Write-Host "Created admin: $($createResponse.message)" -ForegroundColor Green

        # Try login again
        $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json" -TimeoutSec 10 -ErrorAction Stop
        $token = $loginResponse.token
        Write-Host "SUCCESS! Logged in as admin" -ForegroundColor Green
    } catch {
        Write-Host "FAILED to create/login admin: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Make sure the API is running in Development mode" -ForegroundColor Yellow
        exit 1
    }
}
Write-Host ""

if (-not $token) {
    Write-Host "No token available, cannot continue" -ForegroundColor Red
    exit 1
}

# Setup headers with auth token
$headers = @{
    "Authorization" = "Bearer $token"
}

# Test 3: Get Exercise Stats
Write-Host "3. Testing GET /api/admin/exercise-stats" -ForegroundColor Yellow
try {
    $statsResponse = Invoke-RestMethod -Uri "$baseUrl/api/admin/exercise-stats" -Method Get -Headers $headers -TimeoutSec 30
    Write-Host "SUCCESS!" -ForegroundColor Green
    Write-Host "Exercise Statistics:" -ForegroundColor White
    Write-Host "  - Total Exercises: $($statsResponse.total)" -ForegroundColor Gray
    Write-Host "  - Sem Traducao: $($statsResponse.semTraducao)" -ForegroundColor Gray
    Write-Host "  - Sem Descricao: $($statsResponse.semDescricao)" -ForegroundColor Gray
    Write-Host "  - Sem Video: $($statsResponse.semVideo)" -ForegroundColor Gray
    Write-Host "  - Sem Imagem: $($statsResponse.semImagem)" -ForegroundColor Gray
    Write-Host "  - Sem Instrucoes: $($statsResponse.semInstrucoes)" -ForegroundColor Gray
    Write-Host "  - Completos: $($statsResponse.completos)" -ForegroundColor Gray

    if ($statsResponse.porGrupoMuscular) {
        Write-Host "  Por Grupo Muscular:" -ForegroundColor White
        $statsResponse.porGrupoMuscular | Select-Object -First 5 | ForEach-Object {
            Write-Host "    - $($_.muscleGroup): $($_.count)" -ForegroundColor Gray
        }
    }

    if ($statsResponse.exerciciosSemTraducao -and $statsResponse.exerciciosSemTraducao.Count -gt 0) {
        Write-Host "  Sample exercises needing translation:" -ForegroundColor White
        $statsResponse.exerciciosSemTraducao | Select-Object -First 3 | ForEach-Object {
            Write-Host "    - $($_.Name) ($($_.MuscleGroup))" -ForegroundColor Gray
        }
    }
} catch {
    Write-Host "FAILED: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 4: Get Exercises Needing Translation
Write-Host "4. Testing GET /api/admin/exercises-needing-translation" -ForegroundColor Yellow
try {
    $translationResponse = Invoke-RestMethod -Uri "$baseUrl/api/admin/exercises-needing-translation?limit=20" -Method Get -Headers $headers -TimeoutSec 30
    Write-Host "SUCCESS! Found $($translationResponse.Count) exercises needing translation" -ForegroundColor Green
    if ($translationResponse.Count -gt 0) {
        Write-Host "First 5 exercises:" -ForegroundColor White
        $translationResponse | Select-Object -First 5 | ForEach-Object {
            Write-Host "  - $($_.originalName) -> $($_.suggestedTranslation)" -ForegroundColor Gray
        }
    }
} catch {
    Write-Host "FAILED: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 5: Dry Run Fix Exercises
Write-Host "5. Testing POST /api/admin/fix-exercises?dryRun=true" -ForegroundColor Yellow
try {
    $fixResponse = Invoke-RestMethod -Uri "$baseUrl/api/admin/fix-exercises?dryRun=true" -Method Post -Headers $headers -ContentType "application/json" -TimeoutSec 60
    Write-Host "SUCCESS! (Dry Run Mode)" -ForegroundColor Green
    Write-Host "Fix Preview:" -ForegroundColor White
    Write-Host "  - Total Processed: $($fixResponse.totalProcessed)" -ForegroundColor Gray
    Write-Host "  - Translated: $($fixResponse.translated)" -ForegroundColor Gray
    Write-Host "  - Descriptions Added: $($fixResponse.descriptionsAdded)" -ForegroundColor Gray
    Write-Host "  - Videos Added: $($fixResponse.videosAdded)" -ForegroundColor Gray
    Write-Host "  - Images Added: $($fixResponse.imagesAdded)" -ForegroundColor Gray
    Write-Host "  - Is Dry Run: $($fixResponse.dryRun)" -ForegroundColor Gray
    if ($fixResponse.sampleChanges -and $fixResponse.sampleChanges.Count -gt 0) {
        Write-Host "Sample Changes (first 5):" -ForegroundColor White
        $fixResponse.sampleChanges | Select-Object -First 5 | ForEach-Object {
            Write-Host "  - [$($_.exerciseId)] $($_.originalName)" -ForegroundColor Gray
            if ($_.newName -and $_.newName -ne $_.originalName) {
                Write-Host "    -> Name: $($_.newName)" -ForegroundColor Cyan
            }
            if ($_.newDescription) {
                $descPreview = $_.newDescription
                if ($descPreview.Length -gt 50) {
                    $descPreview = $descPreview.Substring(0, 50) + "..."
                }
                Write-Host "    -> Desc: $descPreview" -ForegroundColor Cyan
            }
        }
    }
} catch {
    Write-Host "FAILED: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Tests Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "To apply the fixes for real, run:" -ForegroundColor Yellow
Write-Host "  POST $baseUrl/api/admin/fix-exercises?dryRun=false" -ForegroundColor White
Write-Host ""
Write-Host "Or use Swagger UI at:" -ForegroundColor Yellow
Write-Host "  $baseUrl/swagger" -ForegroundColor White
