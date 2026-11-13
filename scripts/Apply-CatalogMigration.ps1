$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$envPath = Join-Path $root 'infra/docker/.env'
if (!(Test-Path -LiteralPath $envPath)) { throw 'Run scripts/Initialize-Local.ps1 first.' }
$values = @{}
Get-Content -LiteralPath $envPath | ForEach-Object {
    if ($_ -match '^([A-Z_]+)=(.*)$') { $values[$matches[1]] = $matches[2] }
}
if ($values.POSTGRES_PORT -notmatch '^[0-9]{1,5}$' -or
    [int]$values.POSTGRES_PORT -notin 1..65535 -or
    $values.POSTGRES_PASSWORD -notmatch '^[A-Fa-f0-9]{48}$') {
    throw 'Local database configuration is invalid; no migration was applied.'
}
$previous = [Environment]::GetEnvironmentVariable('ConnectionStrings__Database', 'Process')
$env:ConnectionStrings__Database = "Host=127.0.0.1;Port=$($values.POSTGRES_PORT);Database=turborent;Username=turborent;Password=$($values.POSTGRES_PASSWORD);Timeout=3"
try {
    $project = Join-Path $root 'src/backend/Modules/Catalog/TurboRent.Modules.Catalog/TurboRent.Modules.Catalog.csproj'
    dotnet ef database update --configuration Release --project $project --startup-project $project `
        --context TurboRent.Modules.Catalog.Infrastructure.Persistence.CatalogDbContext
    if ($LASTEXITCODE -ne 0) { throw 'Catalog migration failed.' }
    Write-Output 'Catalog schema is up to date. Existing schemas and data were not modified.'
} finally {
    if ($null -eq $previous) { Remove-Item Env:ConnectionStrings__Database -ErrorAction SilentlyContinue }
    else { $env:ConnectionStrings__Database = $previous }
}
