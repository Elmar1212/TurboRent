param([ValidateSet('Debug', 'Release')][string]$Configuration = 'Debug')
$ErrorActionPreference = 'Stop'
# All backend tests now use the isolated database; never fall back to the application database.
& (Join-Path $PSScriptRoot 'Test-IdentityMigration.ps1') -Configuration $Configuration