> Cari status (2026-10-24): InitialIdentity backup/restore yoxlamalarından sonra lokal turborent bazasına tətbiq edilib. Aşağıdakı mətn əvvəlki mərhələnin tarixçəsidir. Yekun hesabat: [InitialIdentity-main-results.md](InitialIdentity-main-results.md).

# Milestone 2 — Mərhələ 1: migration review

Status: SQL hazırlanıb, heç bir bazaya tətbiq edilməyib.
Foundation baseline commit: e905a35.
Fayl: docs/migrations/InitialIdentity.sql
Migration: 20261025034229_InitialIdentity

## Dəyişikliklər

Ayrıca Identity modulu, Contracts assembly-si və Identity unit test layihəsi əlavə edilib.
IdentityDbContext yalnız identity schema-sına yazacaq. SystemDbContext dəyişdirilməyib.
API-yə yalnız project reference əlavə edilib; auth middleware və endpoint-lər hələ aktiv deyil.
EF Core komponentləri və Mvc.Testing 10.0.12 ilə uyğunlaşdırılıb; Npgsql provider 10.0.0 saxlanıb.
Test-Backend.ps1 skriptinə optional -Configuration Release parametri əlavə edilib.

## SQL təsiri

11 tətbiq cədvəli və 1 EF migration history cədvəli yaradır.
4 rol əlavə edir: Customer, IndividualOwner, RentalCompany, Admin.
İstifadəçi, Admin hesabı, parol və user-role təyinatı əlavə etmir.
Unique email, approval/status, öz müraciətini review etməmə, VÖEN və MFA/session constraint-ləri yaradır.
DDL yalnız identity schema-sına aiddir.
DROP, TRUNCATE, DELETE, UPDATE və mövcud cədvələ ALTER əmri yoxdur.
Yeni standart Identity foreign key-lərində ON DELETE CASCADE referential qaydası var;
bu script istifadəçi və ya məlumat silmir. Gələcək silmə əməliyyatları ayrıca nəzarət tələb edir.
Tətbiq cədvəlləri transaction daxilindədir; schema/history bootstrap transaction-dan əvvəldir.
Idempotent yoxlama migration history-yə əsaslanır; əvvəlcədən konfliktli obyektləri düzəltmir.

## Təsdiqdən sonrakı tətbiq ardıcıllığı

1. Eyni PostgreSQL container-ində ayrıca boş Identity test bazası yaratmaq.
2. Bu SQL-ni əvvəl test bazasına tətbiq etmək və constraint/integration testlərini işlətmək.
3. Lokal turborent bazasının backup-ını Git-dən kənarda saxlamaq.
4. Eyni təsdiqlənmiş SQL-ni lokal turborent bazasına tətbiq etmək.
5. Role seed və schema vəziyyətini yoxlamaq; sonra auth feature-lərini davam etdirmək.

Bu ardıcıllıq ayrıca SQL təsdiqindən əvvəl icra edilməyəcək.
dotnet ef database update, EnsureCreated və startup avtomatik migration yoxdur.
EF-in yaratdığı Down rollback-i cədvəl silə bilər; ayrıca təsdiqsiz icra edilməməlidir.

## Yoxlamalar

Release solution build: 0 xəbərdarlıq, 0 xəta.
Identity unit/model testləri: 14/14 keçdi, DB istifadə etmir.
Foundation unit: 1/1 keçdi.
Foundation integration: 4/4 keçdi, yalnız mövcud health/readiness sorğuları.
Frontend build və lint keçdi; frontend mənbə kodu dəyişməyib.
EF model/migration uyğunluğu: pending model change yoxdur.
Read-only DB yoxlaması: tətbiqdən əvvəl və sonra 0 istifadəçi cədvəli.
Real email, production Admin, JWT, payment, booking və deployment yoxdur.

M2 bütövlükdə tamamlanmayıb: qeydiyyat, login, cookie/CSRF, email, MFA use-case-ləri
və React auth səhifələri növbəti mərhələlərdə hazırlanacaq.

SQL SHA-256: 12B167C30FFECE7B9410F244AB2AE0D8F088CA81BE7B7BFD02C364275C813BCD

SQL-də yaradılan cədvəllər:
- identity."__EFMigrationsHistory"
- identity.roles
- identity.users
- identity.role_claims
- identity.company_profiles
- identity.owner_profiles
- identity.security_audit_records
- identity.sessions
- identity.user_claims
- identity.user_logins
- identity.user_roles
- identity.user_tokens


Sonrakı yoxlama: SQL yalnız izolə test bazasına tətbiq edilib. Əsas bazaya tətbiq hələ təsdiq gözləyir. Tam nəticə: InitialIdentity-test-results.md.
