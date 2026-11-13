> Cari status (2026-10-24): InitialIdentity backup/restore yoxlamalarından sonra lokal turborent bazasına tətbiq edilib. Aşağıdakı mətn əvvəlki mərhələnin tarixçəsidir. Yekun hesabat: [InitialIdentity-main-results.md](InitialIdentity-main-results.md).

# InitialIdentity — izolə test hesabatı

Tarix: 2026-10-24 (America/Tijuana).
Status: yalnız izolə test bazasına tətbiq edildi; əsas turborent bazasına tətbiq edilməyib.

## İzolyasiya

Container: turborent-postgres-1; host portu 55432.
Test database: turborent_identity_test_ff70ee3852b8 (OID 16390).
Əsas database: turborent (OID 16384).
Bunlar eyni PostgreSQL instance daxilində ayrı database-lərdir; ayrıca container deyil.
Test bazası template0 əsasında boş yaradılıb, əvvəl 0 tətbiq cədvəli var idi.
Test connection string-i həm database, həm username üçün turborent_identity_test_ prefix-i tələb edir.
Server current_database/current_user cavabları ilə hədəf yenidən yoxlanır.

Ayrıca test login-i superuser, CREATEDB, CREATEROLE və replication səlahiyyəti olmadan yaradılıb.
Bu login-in əsas turborent bazasında CREATE səlahiyyəti yoxdur.
Əsas database-in permission-ları dəyişdirilməyib. PostgreSQL-in default PUBLIC CONNECT
qaydasını dəyişməmişik; izolyasiya ayrı database, məhdud rol və test target guard ilə təmin edilir.
Əsas bazaya və başqa layihələrin bazalarına bağlantı açılmayıb və migration göndərilməyib.
Heç bir container, database, mövcud cədvəl və volume silinməyib.

## SQL və təkrar tətbiq

Migration: 20261025034229_InitialIdentity, EF Core 10.0.12.
SHA-256: 12B167C30FFECE7B9410F244AB2AE0D8F088CA81BE7B7BFD02C364275C813BCD.
Təsdiqlənmiş SQL faylı dəyişməyib.
DROP, TRUNCATE, DELETE, UPDATE və ALTER statement-i yoxdur.
CREATE əmrləri identity schema-sına aiddir; 4 rol və history qeydi əlavə edilir.
ON DELETE CASCADE yeni standart Identity foreign key qaydasıdır; silmə əmri deyil.

İlk tətbiq uğurlu oldu. Integration testində həmin SQL yenidən tətbiq edildi.
Təkrar tətbiqdən əvvəl/sonra bütün identity cədvəllərinin sətirləri, column metadata,
constraint-lər və indekslər müqayisə edildi: fərq yoxdur.
Migration history-də 1 qeyd, roles cədvəlində 4 rol qaldı.
Bu idempotentlik history qeydinə əsaslanır; əl ilə dəyişdirilmiş sxemi avtomatik düzəltmir.

## Yaradılan cədvəllər

identity.users
identity.roles
identity.user_roles
identity.user_claims
identity.role_claims
identity.user_logins
identity.user_tokens
identity.owner_profiles
identity.company_profiles
identity.sessions
identity.security_audit_records
identity.__EFMigrationsHistory

## Testlər

Release solution build: 0 xəbərdarlıq, 0 xəta.
Son uğurlu test run ID: e6c1be90b272.

| Suite | Keçdi | Uğursuz | Buraxıldı |
|---|---:|---:|---:|
| Foundation unit | 1 | 0 | 0 |
| Foundation integration | 4 | 0 | 0 |
| Identity unit/model | 14 | 0 | 0 |
| Identity integration | 16 | 0 | 0 |
| Cəmi | 35 | 0 | 0 |

Identity integration əhatəsi:
- gözlənilən 12 cədvəl, applied/pending migration vəziyyəti;
- 4 rol və Admin user-role təyinatının olmaması;
- duplicate normalized email;
- invalid locale/account status;
- profil foreign key və hər user üçün unique owner profile;
- SQL səviyyəsində self-review-in rədd edilməsi;
- VÖEN formatı və approved-only unikallığı;
- MFA-sız Admin sessiyası və səhv expiry-nin DB-də rədd edilməsi;
- stale session update üçün optimistic concurrency;
- pending profillərin öz-özünə privileged rol verməməsi;
- audit foreign key;
- SQL təkrar tətbiqində sxem və məlumatın saxlanması.

Fixture-lərin çoxu transaction rollback ilə geri qaytarılır.
Təkrar tətbiqin məlumat qoruduğunu yoxlamaq üçün test bazasında 2 sintetik
example.test istifadəçisi saxlanılıb (ilk uğursuz cəhd və son uğurlu cəhd).
Onların parolu və Admin təyinatı yoxdur; real istifadəçi və production credential deyil.
İlk cəhddə SQL oxuyucusu UTF-8 BOM-u SQL mətninə daxil etmişdi. ReadAllText ilə
düzəldildi; SQL faylı dəyişdirilmədi. Son tam test run-da bütün testlər keçdi.

TRX-lər: artifacts/identity-migration/results/e6c1be90b272/.
Lokal test credential faylı: artifacts/identity-migration/test.env (Git tərəfindən ignore edilir).
Təkrar yoxlama:
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-IdentityMigration.ps1

Test-Backend.ps1 artıq əsas bazaya fallback etmir və eyni izolə runner-i istifadə edir.
CI workflow da ayrıca turborent_identity_test_ci bazasına uyğunlaşdırılıb.
Workflow yalnız lokal dəyişdirilib; GitHub-da icra edilməyib və remote push yoxdur.
Auth endpoint-ləri, login, email, CSRF və MFA use-case-ləri bu mərhələdə implementasiya edilməyib.
