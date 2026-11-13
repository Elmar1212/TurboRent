# InitialIdentity — yekun lokal migration hesabatı

Tarix: 2026-10-24, America/Tijuana.
Status: backup/restore yoxlamalarından sonra yalnız lokal turborent bazasına tətbiq edildi.
Tətbiq vaxtı: 2026-10-24 21:00:57 America/Tijuana (2026-10-25T04:00:57Z).

## Hədəf və schema comparison

Container: turborent-postgres-1; Compose layihəsi: turborent.
Port: 127.0.0.1:55432 -> 5432; database/user: turborent; database OID: 16384.
Əvvəl: public schema-sı, 0 tətbiq cədvəli.
Tətbiqdən əvvəl schema dump backup-dan əvvəlki snapshot ilə yenidən müqayisə edildi: fərq yoxdur.
Sonra: public + identity schema-ları, 12 Identity cədvəli.
identity çıxarılaraq qalan schema dump əvvəlki vəziyyətlə müqayisə edildi: fərq yoxdur.
Mövcud cədvəl, database və Docker volume silinməyib. Staging/production-a qoşulunmayıb.

Regex metadata filtri dəqiq StartsWith/Ordinal prefiks yoxlaması ilə əvəz edildi.
4 test SQL-in saxlandığını, dump metadata-sının normallaşdığını və həqiqi column
tipi fərqinin gizlədilmədiyini təsdiqlədi. Əvvəlki inventory nəticəsi də eynidir:
public schema-sı və 0 tətbiq cədvəli.

## Backup və restore

Backup: artifacts/backups/identity-c553f6a5b49c/turborent-before-identity-c553f6a5b49c.dump
Format: pg_dump custom; ölçü: 844 byte.
SHA-256: 862B0A10F94F8D91ABE4479808E3B66B04F8BBAAA24C755005C8E1D32ED4A398
Container və lokal binary faylın checksum-ları eynidir; docker cp ilə köçürülüb.
pg_restore --list uğurludur; çıxış backup qovluğunda saxlanılıb.

Ayrıca restore bazası: turborent_identity_restore_c553f6a5b49c.
template0-dan yaradılıb və pg_restore --exit-on-error --no-owner --no-privileges ilə bərpa edilib.
Normallaşdırılmış schema dump və schema/table inventory mənbə ilə eynidir.
Əvvəl biznes cədvəli olmadığı üçün bu yoxlama boş tətbiq bazasının bərpasını təsdiqləyir;
biznes sətirlərinin bərpası barədə iddia edilmir.
Owner/grant-lər sxem müqayisəsindən çıxarılıb; restore bazasının sahibi məhdud lokal test roludur.

Backup qovluğuna yalnız cari Windows istifadəçisi üçün ACL verilib.
Backup, test credential-ləri və .env Git ignore ilə qorunur, git ls-files-də yoxdur.
Backup database səviyyəsindədir; cluster rollarının tam backup-ı deyil.

## Migration və seed nəticəsi

SQL: docs/migrations/InitialIdentity.sql
SHA-256: 12B167C30FFECE7B9410F244AB2AE0D8F088CA81BE7B7BFD02C364275C813BCD
Təsdiqlənmiş SQL və container-ə köçürülmüş nüsxəsi həmin checksum ilə yoxlanılıb.
psql -X -v ON_ERROR_STOP=1 ilə tətbiq edilib.
DROP, ALTER, TRUNCATE, DELETE və UPDATE statement-i yoxdur.
Yeni Identity FK-lərində ON DELETE CASCADE silmə əmri deyil, referential qaydadır.

Yaradılmış cədvəllər:
- identity.users
- identity.roles
- identity.user_roles
- identity.user_claims
- identity.role_claims
- identity.user_logins
- identity.user_tokens
- identity.owner_profiles
- identity.company_profiles
- identity.sessions
- identity.security_audit_records
- identity.__EFMigrationsHistory

Rollar: Admin, Customer, IndividualOwner, RentalCompany — 4 sətir.
History: 20261025034229_InitialIdentity, EF Core 10.0.12 — 1 sətir.
Users: 0. User-role assignments: 0. Admin hesabı yaradılmayıb.
SQL təkrar tətbiqi izolə test bazasında sxem və bütün mövcud sətirlər
əvvəl/sonra müqayisə edilərək yenidən uğurla yoxlanılıb.

## Test nəticələri

Backend run ID: f65d793b7144.
Release build: 0 xəta, 0 xəbərdarlıq.
Backend testləri: 35/35; yazan testlər yalnız turborent_identity_test_* bazasında işləyir.
Sxem filtri: 4/4.
Chromium Foundation E2E: 2/2.
Frontend production build və lint: uğurlu.
Lokal API liveness və PostgreSQL readiness: healthy.
| Backend suite | Nəticə |
|---|---:|
| TurboRent.Identity.IntegrationTests | 16/16 |
| TurboRent.Identity.UnitTests | 14/14 |
| TurboRent.IntegrationTests | 4/4 |
| TurboRent.UnitTests | 1/1 |

## Artefaktlar və sərhədlər

Backup qovluğunda run.json, pg-restore-list.txt, migration-apply.log,
main-before-schema.sql, main-before-inventory.json, restore-schema.sql,
main-after-schema.sql və main-after-inventory.json saxlanılıb.
TRX-lər: artifacts/identity-migration/results/f65d793b7144/.

İlk regex xətası backup başlamadan düzəldilib və test edilib.
Bir helper importunu execution policy blokladı; migration başlamamışdı.
Sonra ayrıca prosesdə ExecutionPolicy Bypass istifadə edildi; sistem siyasəti dəyişmədi.

Backup-InitialIdentity.ps1 və Apply-InitialIdentity.ps1 ilkin vəziyyət üçün guard-lıdır;
schema artıq mövcud olduğu üçün onları kor-koranə təkrar işlətmək olmaz.
Avtomatik Down migration, destructive rollback və restore-over-main yoxdur.
Növbəti auth funksiyası implementasiya edilməyib. Milestone 2 bütövlükdə tamamlanmayıb.
Real email, production Admin, remote push və deployment yoxdur.