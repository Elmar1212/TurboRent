param(
    [ValidateSet('Debug','Release')][string]$Configuration = 'Release',
    [string]$Urls = 'https://localhost:7080;http://127.0.0.1:5080'
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
& (Join-Path $PSScriptRoot 'Initialize-LocalHttps.ps1')
$envPath = Join-Path $root 'infra/docker/.env'
if (!(Test-Path -LiteralPath $envPath)) { throw 'Run scripts/Initialize-Local.ps1 first.' }
$values = @{}
Get-Content -LiteralPath $envPath | ForEach-Object {
    if ($_ -match '^([A-Z_]+)=(.*)$') { $values[$matches[1]] = $matches[2] }
}
if (!$values.GARAGE_DEFAULT_ACCESS_KEY -or !$values.GARAGE_DEFAULT_SECRET_KEY -or !$values.GARAGE_DEFAULT_BUCKET) {
    throw 'Run scripts/Initialize-Garage.ps1 to add local image storage settings.'
}
$settings = @{
    'ConnectionStrings__Database' = "Host=127.0.0.1;Port=$($values.POSTGRES_PORT);Database=turborent;Username=turborent;Password=$($values.POSTGRES_PASSWORD);Timeout=3"
    'ASPNETCORE_ENVIRONMENT' = 'Development'
    'ASPNETCORE_Kestrel__Certificates__Default__Path' = (Join-Path $root 'artifacts/dev-certs/turborent.pem')
    'ASPNETCORE_Kestrel__Certificates__Default__KeyPath' = (Join-Path $root 'artifacts/dev-certs/turborent.key')
    'Identity__FrontendOrigin' = 'https://localhost:5174'
    'AdminBootstrap__Secret' = $values.ADMIN_BOOTSTRAP_SECRET
    'Storage__S3Endpoint' = 'http://127.0.0.1:3900'
    'Storage__AccessKey' = $values.GARAGE_DEFAULT_ACCESS_KEY
    'Storage__SecretKey' = $values.GARAGE_DEFAULT_SECRET_KEY
    'Storage__Bucket' = $values.GARAGE_DEFAULT_BUCKET
    'Storage__S3Region' = 'garage'
}
$previous = @{}
foreach ($name in $settings.Keys) {
    $previous[$name] = [Environment]::GetEnvironmentVariable($name,'Process')
    [Environment]::SetEnvironmentVariable($name,$settings[$name],'Process')
}
try {
    dotnet run -c $Configuration --no-launch-profile --project (Join-Path $root 'src/backend/TurboRent.Api') --urls $Urls
} finally { foreach ($name in $settings.Keys) { [Environment]::SetEnvironmentVariable($name,$previous[$name],'Process') } }
