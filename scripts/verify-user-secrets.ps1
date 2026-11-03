# Lists User Secrets KEY NAMES only (values never printed).
# Usage (from repo root):
#   powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify-user-secrets.ps1

$ErrorActionPreference = "Stop"
$project = Join-Path $PSScriptRoot "..\src\IgloReservation.Api\IgloReservation.Api.csproj"

$raw = dotnet user-secrets list --project $project 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet user-secrets list failed: $raw"
}

$keys = @()
foreach ($line in $raw) {
    $text = "$line".Trim()
    $idx = $text.IndexOf("=")
    if ($idx -gt 0) {
        $keys += $text.Substring(0, $idx).Trim()
    }
}

$required = @(
    "Jwt:SigningKey",
    "ConnectionStrings:DefaultConnection"
)

$optional = @(
    "AdminBootstrap:Email",
    "AdminBootstrap:Password"
)

Write-Host "User Secrets keys present:"
foreach ($k in ($keys | Sort-Object -Unique)) {
    Write-Host "  - $k=<set>"
}

Write-Host ""
foreach ($e in $required) {
    if ($keys -contains $e) {
        Write-Host "CHECK $e : OK"
    }
    else {
        Write-Host "CHECK $e : MISSING"
    }
}

Write-Host ""
$emailOk = $keys -contains "AdminBootstrap:Email"
$passwordOk = $keys -contains "AdminBootstrap:Password"
if ($emailOk -and $passwordOk) {
    Write-Host "CHECK AdminBootstrap (Email+Password) : OK"
}
elseif (-not $emailOk -and -not $passwordOk) {
    Write-Host "CHECK AdminBootstrap (Email+Password) : NOT SET (Admin user will not be created until both are set)"
}
else {
    Write-Host "CHECK AdminBootstrap (Email+Password) : PARTIAL — set both Email and Password or remove the lone value"
}

foreach ($e in $optional) {
    if ($keys -contains $e) {
        Write-Host "  optional key present: $e=<set>"
    }
}
