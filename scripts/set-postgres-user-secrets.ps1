# Requires: interactive local terminal. Does not echo the password.
# Usage (from repo root):
#   powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\set-postgres-user-secrets.ps1

$ErrorActionPreference = "Stop"
$project = Join-Path $PSScriptRoot "..\src\IgloReservation.Api\IgloReservation.Api.csproj"

if (-not (Test-Path $project)) {
    Write-Error "Project not found: $project"
}

Write-Host "IgloReservation — set PostgreSQL connection via User Secrets"
Write-Host "Host=localhost Port=5432 Database=IgloReservationDb Username=iglo_app"
Write-Host "Password will not be printed."
Write-Host ""

$secure = Read-Host "Enter PostgreSQL password for iglo_app" -AsSecureString
if ($secure.Length -eq 0) {
    Write-Error "Password was empty; nothing was saved."
}

$bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
try {
    $password = [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    $connectionString = "Host=localhost;Port=5432;Database=IgloReservationDb;Username=iglo_app;Password=$password"
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" $connectionString --project $project | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet user-secrets set failed with exit code $LASTEXITCODE"
    }
}
finally {
    if ($bstr -ne [IntPtr]::Zero) {
        [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
    }
    $password = $null
    $connectionString = $null
    $secure.Dispose()
}

Write-Host "Saved ConnectionStrings:DefaultConnection to User Secrets (value not shown)."
Write-Host "Verify with: powershell -NoProfile -File .\scripts\verify-user-secrets.ps1"
