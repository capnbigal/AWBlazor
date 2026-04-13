#!/usr/bin/env pwsh
# ElementaryApp — Local development setup script

$ErrorActionPreference = "Stop"

Write-Host "=== ElementaryApp Setup ===" -ForegroundColor Cyan

# Check .NET SDK
$dotnetVersion = dotnet --version 2>$null
if (-not $dotnetVersion) {
    Write-Host "ERROR: .NET SDK not found. Install from https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}
Write-Host "  .NET SDK: $dotnetVersion" -ForegroundColor Green

# Check SQL Server connectivity
Write-Host "`nChecking SQL Server connection to ELITE..." -ForegroundColor Yellow
try {
    $conn = New-Object System.Data.SqlClient.SqlConnection "Server=ELITE;Database=master;Trusted_Connection=True;TrustServerCertificate=True"
    $conn.Open()
    $conn.Close()
    Write-Host "  SQL Server ELITE: reachable" -ForegroundColor Green
} catch {
    Write-Host "  WARNING: Cannot reach SQL Server ELITE. Update ConnectionStrings:DefaultConnection in appsettings.json" -ForegroundColor Yellow
}

# Restore packages
Write-Host "`nRestoring NuGet packages..." -ForegroundColor Yellow
dotnet restore ElementaryApp.slnx

# Build
Write-Host "`nBuilding solution..." -ForegroundColor Yellow
dotnet build ElementaryApp.slnx --no-restore

# Run tests
Write-Host "`nRunning tests..." -ForegroundColor Yellow
dotnet test ElementaryApp.slnx --no-build

Write-Host "`n=== Setup Complete ===" -ForegroundColor Cyan
Write-Host "Run 'dotnet run --project ElementaryApp' to start the app." -ForegroundColor Green
Write-Host "Open https://localhost:5001/" -ForegroundColor Green
