$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Compare-DatabaseSchema.ps1')
$root = Split-Path $PSScriptRoot -Parent
$container = 'turborent-postgres-1'
$mainDatabase = 'turborent'
$sqlPath = Join-Path $root 'docs/migrations/InitialIdentity.sql'
$expectedSqlHash = '12B167C30FFECE7B9410F244AB2AE0D8F088CA81BE7B7BFD02C364275C813BCD'
if ((Get-FileHash $sqlPath -Algorithm SHA256).Hash -ne $expectedSqlHash) { throw 'STOP: reviewed SQL changed.' }
$sql = [IO.File]::ReadAllText($sqlPath)
if ($sql -match '(?im)^\s*(DROP|TRUNCATE|DELETE|UPDATE|ALTER)\b') { throw 'STOP: unexpected SQL operation.' }
$inspection = (& docker inspect $container | ConvertFrom-Json)[0]
if ($LASTEXITCODE -ne 0 -or $inspection.Config.Labels.'com.docker.compose.project' -ne 'turborent') { throw 'STOP: wrong container.' }
if ($inspection.NetworkSettings.Ports.'5432/tcp'[0].HostIp -ne '127.0.0.1' -or $inspection.NetworkSettings.Ports.'5432/tcp'[0].HostPort -ne '55432') { throw 'STOP: unexpected port binding.' }
$identity = & docker exec $container psql -X -A -t -v ON_ERROR_STOP=1 -U turborent -d $mainDatabase -c "SELECT current_database() || '|' || current_user || '|' || oid || '|' || pg_is_in_recovery() FROM pg_database WHERE datname=current_database();"
if ($LASTEXITCODE -ne 0 -or $identity.Trim() -ne 'turborent|turborent|16384|false') { throw 'STOP: main database identity mismatch.' }
$existingIdentity = & docker exec $container psql -X -A -t -v ON_ERROR_STOP=1 -U turborent -d $mainDatabase -c "SELECT COUNT(*) FROM pg_namespace WHERE nspname='identity';"
if ($LASTEXITCODE -ne 0 -or $existingIdentity.Trim() -ne '0') { throw 'STOP: unexpected existing identity schema.' }
$tableCount = & docker exec $container psql -X -A -t -v ON_ERROR_STOP=1 -U turborent -d $mainDatabase -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema NOT IN ('pg_catalog','information_schema');"
if ($LASTEXITCODE -ne 0 -or $tableCount.Trim() -ne '0') { throw 'STOP: unexpected existing application tables.' }
$runId = [Guid]::NewGuid().ToString('N').Substring(0,12)
$runDir = Join-Path $root "artifacts/backups/identity-$runId"
[IO.Directory]::CreateDirectory($runDir) | Out-Null
$principal = [Security.Principal.WindowsIdentity]::GetCurrent().Name
& icacls $runDir /inheritance:r /grant:r ("{0}:(OI)(CI)F" -f $principal) | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'STOP: backup directory permission setup failed.' }
function Get-NormalizedSchema([string]$database) {
    $dump = & docker exec $container pg_dump -U turborent -d $database --schema-only --no-owner --no-privileges
    if ($LASTEXITCODE -ne 0) { throw 'STOP: schema snapshot failed.' }
    return (ConvertTo-ComparablePgDump $dump)
}
$beforeSchema = Get-NormalizedSchema $mainDatabase
[IO.File]::WriteAllText((Join-Path $runDir 'main-before-schema.sql'), $beforeSchema)
$schemaQuery = "SELECT jsonb_build_object('schemas',(SELECT jsonb_agg(nspname ORDER BY nspname) FROM pg_namespace WHERE nspname NOT LIKE 'pg_%' AND nspname <> 'information_schema'),'tables',(SELECT COALESCE(jsonb_agg(table_schema || '.' || table_name ORDER BY table_schema,table_name),'[]'::jsonb) FROM information_schema.tables WHERE table_schema NOT IN ('pg_catalog','information_schema')));"
$beforeInventory = & docker exec $container psql -X -A -t -v ON_ERROR_STOP=1 -U turborent -d $mainDatabase -c $schemaQuery
if ($LASTEXITCODE -ne 0) { throw 'STOP: before inventory failed.' }
[IO.File]::WriteAllText((Join-Path $runDir 'main-before-inventory.json'), ($beforeInventory -join "`n"))
$backupName = "turborent-before-identity-$runId.dump"
$containerBackup = "/tmp/$backupName"
$backupPath = Join-Path $runDir $backupName
& docker exec $container pg_dump -U turborent -d $mainDatabase --format=custom "--file=$containerBackup"
if ($LASTEXITCODE -ne 0) { throw 'STOP: pg_dump failed.' }
& docker cp ('{0}:{1}' -f $container, $containerBackup) $backupPath
if ($LASTEXITCODE -ne 0 -or !(Test-Path $backupPath) -or (Get-Item $backupPath).Length -eq 0) { throw 'STOP: backup copy failed.' }
$localHash = (Get-FileHash $backupPath -Algorithm SHA256).Hash
$containerHash = & docker exec $container sha256sum $containerBackup
if ($LASTEXITCODE -ne 0 -or ($containerHash -split '\s+')[0].ToUpperInvariant() -ne $localHash) { throw 'STOP: binary backup checksum mismatch.' }
$listing = & docker exec $container pg_restore --list $containerBackup
if ($LASTEXITCODE -ne 0) { throw 'STOP: pg_restore --list failed.' }
[IO.File]::WriteAllText((Join-Path $runDir 'pg-restore-list.txt'), ($listing -join "`n"))
$testValues = @{}
Get-Content (Join-Path $root 'artifacts/identity-migration/test.env') | ForEach-Object { if ($_ -match '^([A-Z_]+)=(.*)$') { $testValues[$matches[1]] = $matches[2] } }
$restoreOwner = $testValues.TEST_ROLE
if ($restoreOwner -notmatch '^turborent_identity_test_[a-f0-9]{12}$') { throw 'STOP: invalid restore owner.' }
$restoreDatabase = "turborent_identity_restore_$runId"
& docker exec $container createdb -U turborent "--owner=$restoreOwner" --template=template0 $restoreDatabase
if ($LASTEXITCODE -ne 0) { throw 'STOP: restore rehearsal database creation failed.' }
& docker exec $container pg_restore -U $restoreOwner "--dbname=$restoreDatabase" --no-owner --no-privileges --exit-on-error $containerBackup
if ($LASTEXITCODE -ne 0) { throw 'STOP: restore failed.' }
$restoreSchema = Get-NormalizedSchema $restoreDatabase
[IO.File]::WriteAllText((Join-Path $runDir 'restore-schema.sql'), $restoreSchema)
if ($beforeSchema -cne $restoreSchema) { throw 'STOP: restored schema differs from backup source.' }
$restoreInventory = & docker exec $container psql -X -A -t -v ON_ERROR_STOP=1 -U $restoreOwner -d $restoreDatabase -c $schemaQuery
if ($LASTEXITCODE -ne 0 -or ($restoreInventory -join "`n") -cne ($beforeInventory -join "`n")) { throw 'STOP: restored object inventory differs.' }
$state = [ordered]@{
    RunId=$runId; RunDirectory=$runDir; MainDatabase=$mainDatabase; MainOid=16384;
    Container=$container; BackupPath=$backupPath; ContainerBackup=$containerBackup;
    BackupSha256=$localHash; BackupBytes=(Get-Item $backupPath).Length;
    RestoreDatabase=$restoreDatabase; SqlSha256=$expectedSqlHash;
    BackupVerified=$true; RestoreVerified=$true; MigrationApplied=$false
}
[IO.File]::WriteAllText((Join-Path $runDir 'run.json'), ($state | ConvertTo-Json))
[IO.File]::WriteAllText((Join-Path $root 'artifacts/backups/latest-identity-run.txt'), $runDir)
& git -C $root check-ignore $backupPath
if ($LASTEXITCODE -ne 0) { throw 'STOP: backup is not ignored by Git.' }
Write-Output ($state | ConvertTo-Json)
Write-Output "Before/restore inventory: $beforeInventory"
