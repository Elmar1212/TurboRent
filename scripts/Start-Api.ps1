$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$envPath = Join-Path $root 'infra/docker/.env'
if (!(Test-Path -LiteralPath $envPath)) { throw 'Run scripts/Initialize-Local.ps1 first.' }
$values = @{}
Get-Content -LiteralPath $envPath | ForEach-Object {
    if ($_ -match '^([A-Z_]+)=(.*)$') { $values[$matches[1]] = $matches[2] }
}
$env:ConnectionStrings__Database = "Host=127.0.0.1;Port=$($values.POSTGRES_PORT);Database=turborent;Username=turborent;Password=$($values.POSTGRES_PASSWORD);Timeout=3"
try { dotnet run --project (Join-Path $root 'src/backend/TurboRent.Api') }
finally { Remove-Item Env:ConnectionStrings__Database -ErrorAction SilentlyContinue }
