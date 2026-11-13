$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$envPath = Join-Path $root 'infra/docker/.env'
if (!(Test-Path -LiteralPath $envPath)) { throw 'Run scripts/Initialize-Local.ps1 first.' }
$bootstrapSecret = $null
Get-Content -LiteralPath $envPath | ForEach-Object {
    if ($_ -match '^ADMIN_BOOTSTRAP_SECRET=(.*)$') { $bootstrapSecret = $matches[1] }
}
if ($bootstrapSecret -notmatch '^[A-Fa-f0-9]{64}$') {
    throw 'Admin recovery secret is missing or invalid; run scripts/Initialize-Garage.ps1.'
}
$email = Read-Host 'Current Admin email'

$body = @{
    bootstrapSecret = $bootstrapSecret
    email = $email
} | ConvertTo-Json -Compress

try {
    $process = [Diagnostics.Process]::new()
    $process.StartInfo = [Diagnostics.ProcessStartInfo]::new()
    $process.StartInfo.FileName = 'docker'
    $process.StartInfo.Arguments = 'compose --env-file "infra/docker/.env" -f "infra/docker/compose.yaml" exec --no-TTY api curl --fail --silent --show-error --insecure https://127.0.0.1:8080/internal/admin/mfa/reset -H "Content-Type: application/json" --data-binary @-'
    $process.StartInfo.WorkingDirectory = $root
    $process.StartInfo.UseShellExecute = $false
    $process.StartInfo.RedirectStandardInput = $true
    $process.StartInfo.RedirectStandardOutput = $true
    $process.StartInfo.RedirectStandardError = $true
    if (!$process.Start()) { throw 'Could not start the local MFA reset request.' }
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    $process.StandardInput.WriteLine($body)
    $process.StandardInput.Close()
    $process.WaitForExit()
    $response = $stdoutTask.GetAwaiter().GetResult()
    $errorOutput = $stderrTask.GetAwaiter().GetResult()
    if ($process.ExitCode -ne 0) {
        throw "Admin MFA reset failed. Check that the TurboRent API is running and the Admin email is correct. $errorOutput"
    }

    $result = $response | ConvertFrom-Json
    Write-Output "Authenticator reset for Admin: $($result.email)"
    Write-Output 'Add this TOTP URI to an authenticator app. Treat it as a password:'
    Write-Output $result.authenticatorUri
    Write-Output 'Manual TOTP secret:'
    Write-Output $result.authenticatorSecret
    Write-Output 'Save the new secret securely. The old Authenticator entry no longer works.'
} finally {
    $body = $null
    $response = $null
    $bootstrapSecret = $null
}
