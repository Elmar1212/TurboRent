$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
& (Join-Path $PSScriptRoot 'Initialize-LocalHttps.ps1')
$names = @('TURBORENT_HTTPS','NODE_EXTRA_CA_CERTS')
$previous = @{}
foreach ($name in $names) { $previous[$name] = [Environment]::GetEnvironmentVariable($name,'Process') }
$env:TURBORENT_HTTPS = 'true'
$env:NODE_EXTRA_CA_CERTS = Join-Path $root 'artifacts/dev-certs/turborent.pem'
try {
    if ($env:OS -eq 'Windows_NT') { npm.cmd --prefix (Join-Path $root 'apps/web') run dev }
    else { npm --prefix (Join-Path $root 'apps/web') run dev }
} finally { foreach ($name in $names) { [Environment]::SetEnvironmentVariable($name,$previous[$name],'Process') } }
