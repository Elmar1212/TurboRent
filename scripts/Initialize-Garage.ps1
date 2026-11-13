$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$envPath = Join-Path $root 'infra/docker/.env'
if (!(Test-Path -LiteralPath $envPath)) { throw 'Run scripts/Initialize-Local.ps1 first.' }

$values = @{}
Get-Content -LiteralPath $envPath | ForEach-Object {
    if ($_ -match '^([A-Z_]+)=(.*)$') { $values[$matches[1]] = $matches[2] }
}
$additions = [Collections.Generic.List[string]]::new()
function New-HexSecret([int]$size) {
    $bytes = New-Object byte[] $size
    $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
    return [BitConverter]::ToString($bytes).Replace('-', '')
}
function New-Base64Secret([int]$size) {
    $bytes = New-Object byte[] $size
    $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
    return [Convert]::ToBase64String($bytes)
}

if (!$values.GARAGE_DEFAULT_ACCESS_KEY) {
    $values.GARAGE_DEFAULT_ACCESS_KEY = 'GK' + (New-HexSecret 16)
    $additions.Add("GARAGE_DEFAULT_ACCESS_KEY=$($values.GARAGE_DEFAULT_ACCESS_KEY)")
}
if (!$values.GARAGE_DEFAULT_SECRET_KEY) {
    $values.GARAGE_DEFAULT_SECRET_KEY = New-HexSecret 32
    $additions.Add("GARAGE_DEFAULT_SECRET_KEY=$($values.GARAGE_DEFAULT_SECRET_KEY)")
}
if (!$values.GARAGE_DEFAULT_BUCKET) {
    $values.GARAGE_DEFAULT_BUCKET = 'turborent-vehicle-images'
    $additions.Add("GARAGE_DEFAULT_BUCKET=$($values.GARAGE_DEFAULT_BUCKET)")
}
if (!$values.ADMIN_BOOTSTRAP_SECRET) {
    $values.ADMIN_BOOTSTRAP_SECRET = New-HexSecret 32
    $additions.Add("ADMIN_BOOTSTRAP_SECRET=$($values.ADMIN_BOOTSTRAP_SECRET)")
}
if ($values.GARAGE_DEFAULT_ACCESS_KEY -notmatch '^GK[A-Fa-f0-9]{32}$' -or
    $values.GARAGE_DEFAULT_SECRET_KEY -notmatch '^[A-Fa-f0-9]{64}$' -or
    $values.GARAGE_DEFAULT_BUCKET -ne 'turborent-vehicle-images' -or
    $values.ADMIN_BOOTSTRAP_SECRET -notmatch '^[A-Fa-f0-9]{64}$') {
    throw 'Garage credentials or bucket settings are invalid; existing values were preserved.'
}
if ($additions.Count -gt 0) {
    [IO.File]::AppendAllText($envPath, [Environment]::NewLine + ($additions -join [Environment]::NewLine) + [Environment]::NewLine)
}

$garageDirectory = Join-Path $root 'artifacts/garage'
[IO.Directory]::CreateDirectory($garageDirectory) | Out-Null
if ($env:OS -eq 'Windows_NT') {
    $principal = [Security.Principal.WindowsIdentity]::GetCurrent().Name
    & icacls $garageDirectory /inheritance:r /grant:r ("{0}:(OI)(CI)F" -f $principal) | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Could not protect Garage credentials.' }
} else {
    [IO.File]::SetUnixFileMode($garageDirectory,
        [IO.UnixFileMode]::UserRead -bor [IO.UnixFileMode]::UserWrite -bor [IO.UnixFileMode]::UserExecute)
}

$configPath = Join-Path $garageDirectory 'garage.toml'
if (!(Test-Path -LiteralPath $configPath)) {
    $rpcSecret = New-HexSecret 32
    $adminToken = New-Base64Secret 32
    $metricsToken = New-Base64Secret 32
    $config = @"
metadata_dir = "/var/lib/garage/meta"
data_dir = "/var/lib/garage/data"
db_engine = "sqlite"
replication_factor = 1
rpc_bind_addr = "[::]:3901"
rpc_public_addr = "127.0.0.1:3901"
rpc_secret = "$rpcSecret"

[s3_api]
s3_region = "garage"
api_bind_addr = "[::]:3900"
root_domain = ".s3.garage.localhost"
public_endpoint = "http://127.0.0.1:3900"

[admin]
api_bind_addr = "[::]:3903"
admin_token = "$adminToken"
metrics_token = "$metricsToken"
"@
    [IO.File]::WriteAllText($configPath, $config + [Environment]::NewLine)
    if ($env:OS -ne 'Windows_NT') {
        [IO.File]::SetUnixFileMode($configPath,
            [IO.UnixFileMode]::UserRead -bor [IO.UnixFileMode]::UserWrite)
    }
}
Write-Output 'Local Garage settings are present in ignored files. Existing credentials/config were preserved.'
