$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Compare-DatabaseSchema.ps1')
$first = @('-- PostgreSQL database dump', '\restrict first', '', 'CREATE SCHEMA public;', 'CREATE TABLE public.example ("Id" integer);', '\unrestrict first')
$second = @('-- Different dump metadata', '\restrict second', 'CREATE SCHEMA public;', 'CREATE TABLE public.example ("Id" integer);', ' ', '\unrestrict second')
$expected = "CREATE SCHEMA public;`nCREATE TABLE public.example (`"Id`" integer);"
if ((ConvertTo-ComparablePgDump $first) -cne $expected) { throw 'Expected SQL was not preserved.' }
if ((ConvertTo-ComparablePgDump $first) -cne (ConvertTo-ComparablePgDump $second)) { throw 'Dump metadata was not normalized.' }
$changed = @('CREATE SCHEMA public;', 'CREATE TABLE public.example ("Id" bigint);')
if ((ConvertTo-ComparablePgDump $changed) -ceq $expected) { throw 'Real schema difference was hidden.' }
$literal = "SELECT 'restrict marker';"
if ((ConvertTo-ComparablePgDump @($literal)) -cne $literal) { throw 'A SQL statement was incorrectly filtered.' }
Write-Output 'Schema comparison tests: 4 passed.'
