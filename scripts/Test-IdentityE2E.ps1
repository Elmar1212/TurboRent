param([ValidateSet('Debug', 'Release')][string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
& (Join-Path $PSScriptRoot 'Initialize-LocalHttps.ps1')
$names = @('TURBORENT_TEST_DATABASE','TURBORENT_E2E_MAILBOX','NODE_EXTRA_CA_CERTS','TURBORENT_E2E_CONFIGURATION','ConnectionStrings__Database')
$previous = @{}
foreach ($name in $names) { $previous[$name] = [Environment]::GetEnvironmentVariable($name,'Process') }
if (!$env:TURBORENT_TEST_DATABASE) {
    $values = @{}
    Get-Content (Join-Path $root 'artifacts/identity-migration/test.env') | ForEach-Object {
        if ($_ -match '^([A-Z_]+)=(.*)$') { $values[$matches[1]] = $matches[2] }
    }
    if ($values.TEST_DATABASE -notmatch '^turborent_identity_test_[a-f0-9]{12}$') { throw 'Refusing non-isolated E2E database.' }
    $env:TURBORENT_TEST_DATABASE = "Host=127.0.0.1;Port=55432;Database=$($values.TEST_DATABASE);Username=$($values.TEST_ROLE);Password=$($values.TEST_PASSWORD);Timeout=3"
}
if ($env:TURBORENT_TEST_DATABASE -notmatch 'Database=turborent_identity_test_[a-z0-9_]+;') { throw 'Isolated E2E database required.' }
$runId = [Guid]::NewGuid().ToString('N').Substring(0,12)
$env:TURBORENT_E2E_MAILBOX = Join-Path $root "artifacts/identity-e2e/$runId/mailbox"
$env:NODE_EXTRA_CA_CERTS = Join-Path $root 'artifacts/dev-certs/turborent.pem'
try {
    $env:TURBORENT_E2E_CONFIGURATION = $Configuration
    dotnet build (Join-Path $root 'TurboRent.slnx') -c $Configuration
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
    $catalogProject = Join-Path $root 'src/backend/Modules/Catalog/TurboRent.Modules.Catalog/TurboRent.Modules.Catalog.csproj'
    $env:ConnectionStrings__Database = $env:TURBORENT_TEST_DATABASE
    dotnet ef database update --project $catalogProject --startup-project $catalogProject `
        --context TurboRent.Modules.Catalog.Infrastructure.Persistence.CatalogDbContext
    if ($LASTEXITCODE -ne 0) { throw 'Catalog migration failed on the isolated E2E database.' }
    if ($env:OS -eq 'Windows_NT') { npm.cmd --prefix (Join-Path $root 'apps/web') run test:e2e }
    else { npm --prefix (Join-Path $root 'apps/web') run test:e2e }
    if ($LASTEXITCODE -ne 0) { throw 'E2E failed.' }
    Write-Output "Successful E2E run: $runId"
} finally { foreach ($name in $names) { [Environment]::SetEnvironmentVariable($name,$previous[$name],'Process') } }
