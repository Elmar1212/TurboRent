$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$certDirectory = Join-Path $root 'artifacts/dev-certs'
[IO.Directory]::CreateDirectory($certDirectory) | Out-Null
if ($env:OS -eq 'Windows_NT') {
    $principal = [Security.Principal.WindowsIdentity]::GetCurrent().Name
    & icacls $certDirectory /inheritance:r /grant:r ("{0}:(OI)(CI)F" -f $principal) | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Could not protect certificate directory.' }
} else {
    [IO.File]::SetUnixFileMode($certDirectory, [IO.UnixFileMode]::UserRead -bor [IO.UnixFileMode]::UserWrite -bor [IO.UnixFileMode]::UserExecute)
}
$certificate = Join-Path $certDirectory 'turborent.pem'
$key = Join-Path $certDirectory 'turborent.key'
if (!(Test-Path $certificate) -and !(Test-Path $key)) {
    dotnet dev-certs https --export-path $certificate --format Pem --no-password
    if ($LASTEXITCODE -ne 0) { throw 'Development certificate export failed.' }
}
if (!(Test-Path $certificate) -or !(Test-Path $key)) { throw 'Incomplete local certificate pair; existing files were not replaced.' }
Write-Output 'Local HTTPS certificate ready. System trust settings were not changed.'
