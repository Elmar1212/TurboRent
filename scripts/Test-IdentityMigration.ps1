param([ValidateSet('Debug', 'Release')][string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$testFile = Join-Path $root 'artifacts/identity-migration/test.env'
if (!(Test-Path -LiteralPath $testFile)) { throw 'Isolated test environment is not initialized.' }
$values = @{}
Get-Content -LiteralPath $testFile | ForEach-Object {
    if ($_ -match '^([A-Z_]+)=(.*)$') { $values[$matches[1]] = $matches[2] }
}
if ($values.TEST_DATABASE -notmatch '^turborent_identity_test_[a-f0-9]{12}$' -or
    $values.TEST_ROLE -ne $values.TEST_DATABASE) { throw 'Refusing a non-isolated test target.' }
$previousConnection = $env:TURBORENT_TEST_DATABASE
$env:TURBORENT_TEST_DATABASE = "Host=127.0.0.1;Port=55432;Database=$($values.TEST_DATABASE);Username=$($values.TEST_ROLE);Password=$($values.TEST_PASSWORD);Timeout=3;Include Error Detail=false"
try {
    dotnet build (Join-Path $root 'TurboRent.slnx') -c $Configuration
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
    $catalogProject = Join-Path $root 'src/backend/Modules/Catalog/TurboRent.Modules.Catalog/TurboRent.Modules.Catalog.csproj'
    $previousCatalogConnection = $env:ConnectionStrings__Database
    $env:ConnectionStrings__Database = $env:TURBORENT_TEST_DATABASE
    try {
        dotnet ef database update --configuration $Configuration --project $catalogProject --startup-project $catalogProject `
            --context TurboRent.Modules.Catalog.Infrastructure.Persistence.CatalogDbContext
        if ($LASTEXITCODE -ne 0) { throw 'Catalog migration failed on the isolated test database.' }
    } finally {
        if ($null -eq $previousCatalogConnection) { Remove-Item Env:ConnectionStrings__Database -ErrorAction SilentlyContinue }
        else { $env:ConnectionStrings__Database = $previousCatalogConnection }
    }
    $runId = [Guid]::NewGuid().ToString('N').Substring(0,12)
    foreach ($project in @('TurboRent.UnitTests','TurboRent.Identity.UnitTests','TurboRent.IntegrationTests','TurboRent.Identity.IntegrationTests')) {
        $projectPath = Join-Path $root "tests/backend/$project"
        $resultPath = Join-Path $root "artifacts/identity-migration/results/$runId/$project"
        dotnet test $projectPath -c $Configuration --no-build --logger trx --results-directory $resultPath
        if ($LASTEXITCODE -ne 0) { throw "Test suite failed: $project" }
    }
    Write-Output "Successful test run: $runId"
} finally {
    if ($null -eq $previousConnection) { Remove-Item Env:TURBORENT_TEST_DATABASE -ErrorAction SilentlyContinue }
    else { $env:TURBORENT_TEST_DATABASE = $previousConnection }
}
