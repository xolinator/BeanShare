# BeanShare Docker Setup Script - Simple Version
param(
    [switch]$Verify = $false
)

Write-Host "BeanShare Docker Setup Script" -ForegroundColor Green

# Function to test Docker
function Test-Docker {
    Write-Host "Checking Docker..." -ForegroundColor Blue
    try {
        $version = docker --version 2>$null
        if ($version) {
            Write-Host "Docker is installed: $version" -ForegroundColor Green
            return $true
        }
    } catch {
        Write-Host "Docker not found" -ForegroundColor Red
        return $false
    }
    return $false
}

# Function to test Docker service
function Test-DockerService {
    Write-Host "Checking Docker service..." -ForegroundColor Blue
    try {
        docker info 2>$null | Out-Null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Docker service is running" -ForegroundColor Green
            return $true
        } else {
            Write-Host "Docker service is not running" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "Docker service problem" -ForegroundColor Red
        return $false
    }
}

# Function to test PostgreSQL
function Test-PostgreSQL {
    Write-Host "Testing PostgreSQL container..." -ForegroundColor Blue
    try {
        Write-Host "  Pulling PostgreSQL image..." -ForegroundColor Yellow
        docker pull postgres:15-alpine | Out-Null

        Write-Host "  Starting test container..." -ForegroundColor Yellow
        $containerId = docker run -d --rm -e POSTGRES_PASSWORD=test -e POSTGRES_DB=test postgres:15-alpine

        if ($containerId) {
            Start-Sleep -Seconds 8
            $result = docker exec $containerId psql -U postgres -d test -c "SELECT 1;" 2>$null
            docker stop $containerId | Out-Null

            if ($result) {
                Write-Host "PostgreSQL container works" -ForegroundColor Green
                return $true
            }
        }
        Write-Host "PostgreSQL container failed" -ForegroundColor Red
        return $false
    } catch {
        Write-Host "PostgreSQL test error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Function to test Infrastructure tests
function Test-InfrastructureTests {
    Write-Host "Testing Infrastructure tests..." -ForegroundColor Blue
    try {
        Set-Location -Path $PSScriptRoot
        dotnet test tests/BeanShare.Infrastructure.Tests/ --verbosity quiet
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Infrastructure tests PASSED" -ForegroundColor Green
            return $true
        } else {
            Write-Host "Infrastructure tests FAILED" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "Test error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Main logic
if ($Verify) {
    Write-Host "Verifying existing installation..." -ForegroundColor Cyan

    $dockerOk = Test-Docker
    $serviceOk = $false
    $postgresOk = $false
    $testsOk = $false

    if ($dockerOk) {
        $serviceOk = Test-DockerService
        if ($serviceOk) {
            $postgresOk = Test-PostgreSQL
            if ($postgresOk) {
                $testsOk = Test-InfrastructureTests
            }
        }
    }

    Write-Host "`nResults:" -ForegroundColor Cyan
    Write-Host "  Docker installed: $(if($dockerOk){'YES'}else{'NO'})" -ForegroundColor $(if($dockerOk){'Green'}else{'Red'})
    Write-Host "  Docker running: $(if($serviceOk){'YES'}else{'NO'})" -ForegroundColor $(if($serviceOk){'Green'}else{'Red'})
    Write-Host "  PostgreSQL works: $(if($postgresOk){'YES'}else{'NO'})" -ForegroundColor $(if($postgresOk){'Green'}else{'Red'})
    Write-Host "  Infrastructure tests: $(if($testsOk){'YES'}else{'NO'})" -ForegroundColor $(if($testsOk){'Green'}else{'Red'})

    if ($dockerOk -and $serviceOk -and $postgresOk -and $testsOk) {
        Write-Host "`nSUCCESS: Everything works!" -ForegroundColor Green
        exit 0
    } else {
        Write-Host "`nFAILED: Some components don't work" -ForegroundColor Red
        Write-Host "Install Docker Desktop and restart system if needed" -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "Run with -Verify to check existing installation" -ForegroundColor Yellow
    Write-Host "Example: .\setup-docker-simple.ps1 -Verify" -ForegroundColor Yellow
}