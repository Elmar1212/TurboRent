$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$envPath = Join-Path $root 'infra/docker/.env'
if (!(Test-Path -LiteralPath $envPath)) { throw 'Run scripts/Initialize-Local.ps1 first.' }
$bootstrapSecret = $null
Get-Content -LiteralPath $envPath | ForEach-Object {
    if ($_ -match '^ADMIN_BOOTSTRAP_SECRET=(.*)$') { $bootstrapSecret = $matches[1] }
}
if ($bootstrapSecret -notmatch '^[A-Fa-f0-9]{64}$') {
    throw 'Admin bootstrap secret is missing or invalid; run scripts/Initialize-Garage.ps1.'
}
$email = Read-Host 'Admin email'
$displayName = Read-Host 'Admin display name'
$locale = Read-Host 'Locale (az/en; default az)'
if ([string]::IsNullOrWhiteSpace($locale)) { $locale = 'az' }
$securePassword = Read-Host 'Admin password (15-128 characters)' -AsSecureString
$passwordPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
try {
    $password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($passwordPointer)
    if ($password.Length -lt 15 -or $password.Length -gt 128) {
        throw 'Admin password must contain 15-128 characters.'
    }

    $body = @{
        bootstrapSecret = $bootstrapSecret
        email = $email
        password = $password
        displayName = $displayName
        locale = $locale
    } | ConvertTo-Json -Compress

    $process = [Diagnostics.Process]::new()
    $process.StartInfo = [Diagnostics.ProcessStartInfo]::new()
    $process.StartInfo.FileName = 'docker'
    $process.StartInfo.Arguments = 'compose --env-file "infra/docker/.env" -f "infra/docker/compose.yaml" exec --no-TTY api curl --fail --silent --show-error --insecure https://127.0.0.1:8080/internal/admin/bootstrap -H "Content-Type: application/json" --data-binary @-'
    $process.StartInfo.WorkingDirectory = $root
    $process.StartInfo.UseShellExecute = $false
    $process.StartInfo.RedirectStandardInput = $true
    $process.StartInfo.RedirectStandardOutput = $true
    $process.StartInfo.RedirectStandardError = $true
    if (!$process.Start()) { throw 'Could not start the Docker bootstrap request.' }
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    $process.StandardInput.WriteLine($body)
    $process.StandardInput.Close()
    $process.WaitForExit()
    $response = $stdoutTask.GetAwaiter().GetResult()
    $errorOutput = $stderrTask.GetAwaiter().GetResult()
    if ($process.ExitCode -ne 0) {
        if ($response -match '"code":"admin_bootstrap_closed"') {
            throw 'Admin bootstrap is already closed; no account was changed.'
        }
        throw "Admin bootstrap failed. Check that the TurboRent API is running. $errorOutput"
    }

    $result = $response | ConvertFrom-Json
    Write-Output "Admin account created: $($result.email)"
    Write-Output 'Add this TOTP URI to an authenticator app. Treat it as a password:'
    Write-Output $result.authenticatorUri
    Write-Output 'Manual TOTP secret:'
    Write-Output $result.authenticatorSecret
    Write-Output 'Save the secret securely. It cannot be displayed again.'
} finally {
    if ($passwordPointer -ne [IntPtr]::Zero) {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($passwordPointer)
    }
    $password = $null
    $body = $null
    $bootstrapSecret = $null
}
