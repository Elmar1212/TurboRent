$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'Compare-DatabaseSchema.ps1')
$runDir = [IO.File]::ReadAllText((Join-Path $root 'artifacts/backups/latest-identity-run.txt'))
$expectedBase = [IO.Path]::GetFullPath((Join-Path $root 'artifacts/backups')) + [IO.Path]::DirectorySeparatorChar
if (![IO.Path]::GetFullPath($runDir).StartsWith($expectedBase,[StringComparison]::OrdinalIgnoreCase)) { throw 'STOP: invalid artifact path.' }
$statePath = Join-Path $runDir 'run.json'
$state = Get-Content -Raw -LiteralPath $statePath | ConvertFrom-Json
if (!$state.BackupVerified -or !$state.RestoreVerified -or $state.MigrationApplied -or $state.MainDatabase -ne 'turborent' -or $state.MainOid -ne 16384 -or $state.Container -ne 'turborent-postgres-1') { throw 'STOP: prerequisite mismatch.' }
$container = $state.Container
$expectedHash = '12B167C30FFECE7B9410F244AB2AE0D8F088CA81BE7B7BFD02C364275C813BCD'
$sqlPath = Join-Path $root 'docs/migrations/InitialIdentity.sql'
if ($state.SqlSha256 -ne $expectedHash -or (Get-FileHash $sqlPath -Algorithm SHA256).Hash -ne $expectedHash) { throw 'STOP: SQL changed.' }
if ((Get-FileHash $state.BackupPath -Algorithm SHA256).Hash -ne $state.BackupSha256) { throw 'STOP: backup changed.' }
$identity = & docker exec $container psql -X -A -t -v ON_ERROR_STOP=1 -U turborent -d turborent -c "SELECT current_database() || '|' || current_user || '|' || oid || '|' || pg_is_in_recovery() FROM pg_database WHERE datname=current_database();"
if ($LASTEXITCODE -ne 0 -or $identity.Trim() -ne 'turborent|turborent|16384|false') { throw 'STOP: database identity mismatch.' }
$beforeSchema = [IO.File]::ReadAllText((Join-Path $runDir 'main-before-schema.sql'))
$currentDump = & docker exec $container pg_dump -U turborent -d turborent --schema-only --no-owner --no-privileges
if ($LASTEXITCODE -ne 0 -or (ConvertTo-ComparablePgDump $currentDump) -cne $beforeSchema) { throw 'STOP: schema changed since backup.' }
$containerSql = "/tmp/initial-identity-$($state.RunId).sql"
& docker cp $sqlPath ('{0}:{1}' -f $container,$containerSql)
if ($LASTEXITCODE -ne 0) { throw 'STOP: SQL copy failed.' }
$copiedHash = & docker exec $container sha256sum $containerSql
if ($LASTEXITCODE -ne 0 -or ($copiedHash -split '\s+')[0].ToUpperInvariant() -ne $expectedHash) { throw 'STOP: copied SQL checksum mismatch.' }
$applyOutput = & docker exec $container psql -X -v ON_ERROR_STOP=1 -U turborent -d turborent "--file=$containerSql" 2>&1
$applyExit = $LASTEXITCODE
[IO.File]::WriteAllText((Join-Path $runDir 'migration-apply.log'), ($applyOutput -join "`n"))
if ($applyExit -ne 0) { throw 'STOP: migration failed. No automatic retry permitted.' }
$state.MigrationApplied = $true
$state | Add-Member -NotePropertyName AppliedAtUtc -NotePropertyValue ([DateTime]::UtcNow.ToString('o'))
[IO.File]::WriteAllText($statePath, ($state | ConvertTo-Json))
$afterDump = & docker exec $container pg_dump -U turborent -d turborent --schema-only --no-owner --no-privileges
if ($LASTEXITCODE -ne 0) { throw 'STOP: post-migration snapshot failed.' }
[IO.File]::WriteAllText((Join-Path $runDir 'main-after-schema.sql'), (ConvertTo-ComparablePgDump $afterDump))
$originalDump = & docker exec $container pg_dump -U turborent -d turborent --schema-only --no-owner --no-privileges --exclude-schema=identity
if ($LASTEXITCODE -ne 0 -or (ConvertTo-ComparablePgDump $originalDump) -cne $beforeSchema) { throw 'STOP: pre-existing schema changed.' }
$verificationSql = @"
SELECT jsonb_build_object(
 'database',current_database(),
 'schemas',(SELECT jsonb_agg(nspname ORDER BY nspname) FROM pg_namespace WHERE nspname NOT LIKE 'pg_%' AND nspname <> 'information_schema'),
 'tables',(SELECT jsonb_agg(table_schema || '.' || table_name ORDER BY table_schema,table_name) FROM information_schema.tables WHERE table_schema NOT IN ('pg_catalog','information_schema')),
 'roles',(SELECT jsonb_agg("Name" ORDER BY "Name") FROM identity.roles),
 'users',(SELECT COUNT(*) FROM identity.users),
 'user_roles',(SELECT COUNT(*) FROM identity.user_roles),
 'history',(SELECT jsonb_agg("MigrationId") FROM identity."__EFMigrationsHistory"));
"@
$verification = $verificationSql | & docker exec -i $container psql -X -A -t -v ON_ERROR_STOP=1 -U turborent -d turborent
if ($LASTEXITCODE -ne 0) { throw 'STOP: seed verification failed.' }
$parsed = ($verification -join "`n") | ConvertFrom-Json
$expectedTables = @('identity.__EFMigrationsHistory','identity.company_profiles','identity.owner_profiles','identity.role_claims','identity.roles','identity.security_audit_records','identity.sessions','identity.user_claims','identity.user_logins','identity.user_roles','identity.user_tokens','identity.users')
if (($parsed.tables -join '|') -cne ($expectedTables -join '|') -or
    ($parsed.schemas -join '|') -ne 'identity|public' -or
    ($parsed.roles -join '|') -ne 'Admin|Customer|IndividualOwner|RentalCompany' -or
    $parsed.users -ne 0 -or $parsed.user_roles -ne 0 -or
    ($parsed.history -join '|') -ne '20261025034229_InitialIdentity') { throw 'STOP: unexpected schema/seed/history result.' }
[IO.File]::WriteAllText((Join-Path $runDir 'main-after-inventory.json'), ($verification -join "`n"))
$state | Add-Member -NotePropertyName ExistingSchemaPreserved -NotePropertyValue $true
$state | Add-Member -NotePropertyName MainVerificationPassed -NotePropertyValue $true
[IO.File]::WriteAllText($statePath, ($state | ConvertTo-Json))
Write-Output ($state | ConvertTo-Json)
Write-Output ($verification -join "`n")