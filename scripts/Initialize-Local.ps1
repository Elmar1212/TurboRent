$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$envPath = Join-Path $root 'infra/docker/.env'
if (Test-Path -LiteralPath $envPath) { throw 'Local .env already exists; no files were overwritten.' }
$bytes = New-Object byte[] 24
$rng = [Security.Cryptography.RandomNumberGenerator]::Create()
try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
$password = [BitConverter]::ToString($bytes).Replace("-", "")
[IO.File]::WriteAllText($envPath, "POSTGRES_PORT=55432`nPOSTGRES_PASSWORD=$password`n")
Write-Output 'Local database credentials created in ignored .env.'
