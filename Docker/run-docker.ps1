param(
    [switch]$Stop
)

# Docker folder
Set-Location $PSScriptRoot

# ---------------------------------------
# STOP
# ---------------------------------------
if ($Stop) {

    Write-Host "Stopping Docker services..." -ForegroundColor Yellow

    docker compose down

    Write-Host "Closing Docker Swagger windows..." -ForegroundColor Yellow

    $swaggerProfile = Join-Path $PSScriptRoot ".swagger-browser"

    # Find Chrome processes using our dedicated Swagger profile
    $chromeProcesses = Get-CimInstance Win32_Process |
        Where-Object {
            $_.Name -eq "chrome.exe" -and
            $_.CommandLine -like "*$swaggerProfile*"
        }

    foreach ($process in $chromeProcesses) {
        Write-Host "Closing Swagger browser process $($process.ProcessId)..."
        Stop-Process -Id $process.ProcessId -Force -ErrorAction SilentlyContinue
    }

    Write-Host ""
    Write-Host "Docker services stopped." -ForegroundColor Green
    Write-Host "Swagger windows closed." -ForegroundColor Green

    exit
}

# ---------------------------------------
# START
# ---------------------------------------

Write-Host "Starting Docker services..." -ForegroundColor Cyan

docker compose up -d

Write-Host "Waiting for services to start..." -ForegroundColor Yellow

Start-Sleep -Seconds 10

# ---------------------------------------
# Swagger browser
# ---------------------------------------

$swaggerProfile = Join-Path $PSScriptRoot ".swagger-browser"

# Create dedicated browser profile directory
if (!(Test-Path $swaggerProfile)) {
    New-Item -ItemType Directory -Path $swaggerProfile | Out-Null
}

$swaggerUrls = @(
    "http://localhost:5001/swagger",
    "http://localhost:5005/swagger",
    "http://localhost:5006/swagger",
    "http://localhost:5007/swagger",
    "http://localhost:5008/swagger",
    "http://localhost:5009/swagger"
)

Write-Host "Opening Swagger pages..." -ForegroundColor Cyan

foreach ($url in $swaggerUrls) {

    Start-Process "chrome.exe" `
        -ArgumentList "--user-data-dir=`"$swaggerProfile`"", "--app=$url"

    Start-Sleep -Milliseconds 500
}

Write-Host ""
Write-Host "Docker services started successfully." -ForegroundColor Green
Write-Host "Swagger pages opened." -ForegroundColor Green
Write-Host ""
Write-Host "To stop Docker and close Swagger:" -ForegroundColor Yellow
Write-Host ".\run-docker.ps1 -Stop" -ForegroundColor White