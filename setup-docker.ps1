# BeanShare Docker Setup Script
# Installs Docker Desktop and verifies functionality for Infrastructure tests

param(
    [switch]$SkipInstall = $false,
    [switch]$Verify = $false
)

Write-Host "🚀 BeanShare Docker Setup Script" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

# Check admin privileges
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "❌ This script must be run as Administrator!"
    Write-Host "💡 Right-click on PowerShell and select 'Run as administrator'" -ForegroundColor Yellow
    exit 1
}

# Function to verify Docker installation
function Test-DockerInstallation {
    Write-Host "🔍 Checking Docker installation..." -ForegroundColor Blue

    try {
        $dockerVersion = docker --version 2>$null
        if ($dockerVersion) {
            Write-Host "✅ Docker is installed: $dockerVersion" -ForegroundColor Green
            return $true
        }
    } catch {
        Write-Host "❌ Docker is not installed or not in PATH" -ForegroundColor Red
        return $false
    }
    return $false
}

# Function to verify Docker service
function Test-DockerService {
    Write-Host "🔍 Checking Docker service..." -ForegroundColor Blue

    try {
        $dockerInfo = docker info 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Docker service is running correctly" -ForegroundColor Green
            return $true
        } else {
            Write-Host "❌ Docker service is not running" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "❌ Problem with Docker service" -ForegroundColor Red
        return $false
    }
}

# Function to test PostgreSQL container
function Test-PostgreSQLContainer {
    Write-Host "🔍 Testing PostgreSQL container..." -ForegroundColor Blue

    try {
        Write-Host "   Downloading PostgreSQL image..." -ForegroundColor Yellow
        docker pull postgres:15-alpine | Out-Null

        Write-Host "   Starting test container..." -ForegroundColor Yellow
        $containerId = docker run -d --rm -e POSTGRES_PASSWORD=testpass -e POSTGRES_DB=testdb postgres:15-alpine

        if ($containerId) {
            Start-Sleep -Seconds 10

            # Test connection
            $testResult = docker exec $containerId psql -U postgres -d testdb -c "SELECT 1 as test;" 2>$null

            # Stop container
            docker stop $containerId | Out-Null

            if ($testResult.Contains("test")) {
                Write-Host "✅ PostgreSQL container works correctly" -ForegroundColor Green
                return $true
            } else {
                Write-Host "❌ PostgreSQL container doesn't work" -ForegroundColor Red
                return $false
            }
        } else {
            Write-Host "❌ Failed to start PostgreSQL container" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "❌ Error testing PostgreSQL container: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Function to install Docker Desktop
function Install-DockerDesktop {
    Write-Host "📦 Installing Docker Desktop..." -ForegroundColor Blue

    $downloadUrl = "https://desktop.docker.com/win/main/amd64/Docker%20Desktop%20Installer.exe"
    $installerPath = "$env:TEMP\DockerDesktopInstaller.exe"

    try {
        Write-Host "   Downloading Docker Desktop Installer..." -ForegroundColor Yellow
        Invoke-WebRequest -Uri $downloadUrl -OutFile $installerPath -UseBasicParsing

        Write-Host "   Running installation..." -ForegroundColor Yellow
        Start-Process -FilePath $installerPath -ArgumentList "install", "--quiet" -Wait

        Write-Host "✅ Docker Desktop installation completed" -ForegroundColor Green
        Write-Host "⚠️  SYSTEM RESTART MAY BE REQUIRED!" -ForegroundColor Yellow

        return $true
    } catch {
        Write-Host "❌ Error installing Docker Desktop: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    } finally {
        if (Test-Path $installerPath) {
            Remove-Item $installerPath -Force
        }
    }
}

# Function to start Docker Desktop
function Start-DockerDesktop {
    Write-Host "🚀 Starting Docker Desktop..." -ForegroundColor Blue

    $dockerDesktopPath = "${env:ProgramFiles}\Docker\Docker\Docker Desktop.exe"

    if (Test-Path $dockerDesktopPath) {
        try {
            Start-Process -FilePath $dockerDesktopPath
            Write-Host "✅ Docker Desktop started" -ForegroundColor Green
            Write-Host "⏱️  Waiting 30 seconds for initialization..." -ForegroundColor Yellow
            Start-Sleep -Seconds 30
            return $true
        } catch {
            Write-Host "❌ Error starting Docker Desktop: $($_.Exception.Message)" -ForegroundColor Red
            return $false
        }
    } else {
        Write-Host "❌ Docker Desktop not found at standard location" -ForegroundColor Red
        return $false
    }
}

# Function to test Infrastructure tests
function Test-InfrastructureTests {
    Write-Host "🧪 Testing Infrastructure tests..." -ForegroundColor Blue

    try {
        Set-Location -Path $PSScriptRoot
        $testResult = dotnet test tests/BeanShare.Infrastructure.Tests/ --verbosity normal 2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Infrastructure tests passed successfully!" -ForegroundColor Green
            return $true
        } else {
            Write-Host "❌ Infrastructure tests failed:" -ForegroundColor Red
            Write-Host $testResult -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "❌ Error running Infrastructure tests: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# ===== MAIN LOGIC =====

if ($Verify) {
    Write-Host "🔍 Only verifying existing installation..." -ForegroundColor Cyan

    $dockerInstalled = Test-DockerInstallation
    $dockerRunning = $false
    $postgresWorking = $false
    $testsWorking = $false

    if ($dockerInstalled) {
        $dockerRunning = Test-DockerService
        if ($dockerRunning) {
            $postgresWorking = Test-PostgreSQLContainer
            if ($postgresWorking) {
                $testsWorking = Test-InfrastructureTests
            }
        }
    }

    Write-Host "`n📊 VERIFICATION RESULTS:" -ForegroundColor Cyan
    Write-Host "  Docker installed: $(if($dockerInstalled){'✅'}else{'❌'})" -ForegroundColor $(if($dockerInstalled){'Green'}else{'Red'})
    Write-Host "  Docker running: $(if($dockerRunning){'✅'}else{'❌'})" -ForegroundColor $(if($dockerRunning){'Green'}else{'Red'})
    Write-Host "  PostgreSQL works: $(if($postgresWorking){'✅'}else{'❌'})" -ForegroundColor $(if($postgresWorking){'Green'}else{'Red'})
    Write-Host "  Infrastructure tests: $(if($testsWorking){'✅'}else{'❌'})" -ForegroundColor $(if($testsWorking){'Green'}else{'Red'})

    if ($dockerInstalled -and $dockerRunning -and $postgresWorking -and $testsWorking) {
        Write-Host "`n🎉 EVERYTHING WORKS CORRECTLY!" -ForegroundColor Green
        exit 0
    } else {
        Write-Host "`n❌ SOME COMPONENTS DON'T WORK" -ForegroundColor Red
        exit 1
    }
}

if (-not $SkipInstall) {
    # Check if Docker is already installed
    if (Test-DockerInstallation) {
        Write-Host "✅ Docker is already installed" -ForegroundColor Green
    } else {
        Write-Host "📦 Docker is not installed, installing..." -ForegroundColor Yellow
        $installResult = Install-DockerDesktop
        if (-not $installResult) {
            Write-Host "❌ Docker Desktop installation failed!" -ForegroundColor Red
            exit 1
        }
    }

    # Start Docker Desktop
    if (-not (Test-DockerService)) {
        Write-Host "🚀 Docker not running, starting Docker Desktop..." -ForegroundColor Yellow
        $startResult = Start-DockerDesktop
        if (-not $startResult) {
            Write-Host "❌ Starting Docker Desktop failed!" -ForegroundColor Red
            exit 1
        }
    }
}

# Final verification
Write-Host "`n🔍 Final verification..." -ForegroundColor Cyan

$allGood = $true

if (-not (Test-DockerInstallation)) {
    Write-Host "❌ Docker is still not available" -ForegroundColor Red
    $allGood = $false
}

if (-not (Test-DockerService)) {
    Write-Host "❌ Docker service is still not running" -ForegroundColor Red
    Write-Host "💡 System restart or manual Docker Desktop startup may be needed" -ForegroundColor Yellow
    $allGood = $false
}

if ($allGood -and (Test-PostgreSQLContainer)) {
    Write-Host "✅ PostgreSQL container works" -ForegroundColor Green

    if (Test-InfrastructureTests) {
        Write-Host "✅ Infrastructure tests work!" -ForegroundColor Green
    } else {
        Write-Host "❌ Infrastructure tests still don't work" -ForegroundColor Red
        $allGood = $false
    }
} else {
    $allGood = $false
}

Write-Host "`n" -NoNewline
if ($allGood) {
    Write-Host "🎉 SUCCESS! Everything is set up and working." -ForegroundColor Green
    Write-Host "   You can now run: dotnet test tests/BeanShare.Infrastructure.Tests/" -ForegroundColor Green
} else {
    Write-Host "⚠️  WARNING! Some steps failed." -ForegroundColor Yellow
    Write-Host "   Try:" -ForegroundColor Yellow
    Write-Host "   1. System restart" -ForegroundColor Yellow
    Write-Host "   2. Manual Docker Desktop startup" -ForegroundColor Yellow
    Write-Host "   3. Run: .\setup-docker.ps1 -Verify" -ForegroundColor Yellow
}

Write-Host "`n📖 SCRIPT USAGE:" -ForegroundColor Cyan
Write-Host "   .\setup-docker.ps1                    # Full installation and setup" -ForegroundColor White
Write-Host "   .\setup-docker.ps1 -SkipInstall       # Only start and test (no installation)" -ForegroundColor White
Write-Host "   .\setup-docker.ps1 -Verify            # Only verify existing installation" -ForegroundColor White