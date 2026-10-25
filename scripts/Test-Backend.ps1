$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$values = @{}
Get-Content -LiteralPath (Join-Path $root 'infra/docker/.env') | ForEach-Object {
    if ($_ -match '^([A-Z_]+)=(.*)$') { $values[$matches[1]] = $matches[2] }
}
$env:TURBORENT_TEST_DATABASE = "Host=127.0.0.1;Port=$($values.POSTGRES_PORT);Database=turborent;Username=turborent;Password=$($values.POSTGRES_PASSWORD);Timeout=3"
try {
    dotnet test (Join-Path $root 'TurboRent.slnx')
    if ($LASTEXITCODE -ne 0) { throw 'Backend tests failed.' }
} finally { Remove-Item Env:TURBORENT_TEST_DATABASE -ErrorAction SilentlyContinue }
