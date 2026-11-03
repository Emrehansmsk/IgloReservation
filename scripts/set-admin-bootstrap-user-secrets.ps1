# Requires: interactive local terminal. Does not echo the password.
# Usage (from repo root):
#   powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\set-admin-bootstrap-user-secrets.ps1

$ErrorActionPreference = "Stop"
$project = Join-Path $PSScriptRoot "..\src\IgloReservation.Api\IgloReservation.Api.csproj"

if (-not (Test-Path $project)) {
    Write-Error "Project not found: $project"
}

Write-Host "IgloReservation — set AdminBootstrap via User Secrets"
Write-Host "Values are not printed."
Write-Host ""

$email = Read-Host "Enter AdminBootstrap email"
if ([string]::IsNullOrWhiteSpace($email)) {
    Write-Error "Email was empty; nothing was saved."
}

$secure = Read-Host "Enter AdminBootstrap password" -AsSecureString
if ($secure.Length -eq 0) {
    Write-Error "Password was empty; nothing was saved."
}

$bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
try {
    $password = [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    dotnet user-secrets set "AdminBootstrap:Email" $email.Trim() --project $project | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to set AdminBootstrap:Email"
    }

    dotnet user-secrets set "AdminBootstrap:Password" $password --project $project | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to set AdminBootstrap:Password"
    }
}
finally {
    if ($bstr -ne [IntPtr]::Zero) {
        [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
    }
    $password = $null
    $secure.Dispose()
}

Write-Host "Saved AdminBootstrap:Email and AdminBootstrap:Password to User Secrets (values not shown)."
Write-Host "Verify with: powershell -NoProfile -File .\scripts\verify-user-secrets.ps1"
Write-Host "Then run the API once (Development) so bootstrap can create the Admin user if missing."
