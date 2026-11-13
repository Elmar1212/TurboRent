> Cari status (2026-10-24): InitialIdentity backup/restore yoxlamalarından sonra lokal turborent bazasına tətbiq edilib. Aşağıdakı mətn əvvəlki mərhələnin tarixçəsidir. Yekun hesabat: [InitialIdentity-main-results.md](InitialIdentity-main-results.md).

# Əsas turborent bazası — backup və tətbiq planı

Status: yalnız plandır. Bu sənəddəki əmrlər hələ icra edilməyib.
Tətbiq ayrıca istifadəçi təsdiqi tələb edir.
Test nəticələri: InitialIdentity-test-results.md.
Tətbiq ediləcək SQL: InitialIdentity.sql.
SHA-256: 12B167C30FFECE7B9410F244AB2AE0D8F088CA81BE7B7BFD02C364275C813BCD.

## Təsdiqdən sonrakı ardıcıllıq

1. Container, əsas database adı, SQL checksum-u və mövcud obyektləri yenidən yoxlamaq.
   identity schema-sı və ya history-də gözlənilməz obyekt varsa tətbiqi dayandırmaq.
2. Qısa dəyişiklik pəncərəsi seçmək; schema dəyişiklikləri zamanı başqa migration işlətməmək.
3. Əsas turborent bazasından full custom-format pg_dump backup yaratmaq.
   Parollar CLI argument-lərinə və loglara verilməyəcək.
4. Binary faylı PowerShell text pipeline ilə ötürmədən docker cp ilə
   artifacts/backups/ qovluğuna köçürmək; Git ignore və məhdud fayl icazələrini yoxlamaq.
5. pg_restore --list və SHA-256 ilə backup faylını yoxlamaq.
6. Backup-ı ayrıca yeni rehearsal database-ə restore edib sxem/sətir saylarını yoxlamaq.
   Əsas bazanın üzərinə restore, --clean və drop əmrləri yoxdur.
7. Backup və restore yoxlaması uğurlu olduqdan sonra təsdiqlənmiş SQL-ni yalnız
   turborent bazasına psql -X -v ON_ERROR_STOP=1 vasitəsilə tətbiq etmək.
8. 12 Identity cədvəli, 4 rol, 1 migration history qeydi və Admin hesabının
   yaradılmadığını yoxlamaq; Foundation health/readiness yoxlamalarını aparmaq.
9. Backup, checksum, tətbiq vaxtı və nəticə hesabatını saxlamaq.

## Backup əmrlərinin nümunəsi

Aşağıdakılar gələcəkdə konkret timestamp ilə işlədiləcək, indi icra edilmir:

    docker exec turborent-postgres-1 pg_dump -U turborent -d turborent --format=custom --file=/tmp/turborent-before-identity-TIMESTAMP.dump
    docker cp turborent-postgres-1:/tmp/turborent-before-identity-TIMESTAMP.dump artifacts/backups/
    docker exec turborent-postgres-1 pg_restore --list /tmp/turborent-before-identity-TIMESTAMP.dump

pg_dump bir database-in məlumat/schema backup-ıdır, cluster role və server
konfiqurasiyasının tam backup-ı deyil. Bu additive migration cluster role-larını dəyişmir.
Tətbiq cədvəlləri transaction daxilindədir; schema/history bootstrap həmin
transaction-dan əvvəldir. Xəta olarsa bootstrap obyektləri qala bilər, ona görə
avtomatik silmə və ya Down migration işlədilməyəcək.
Əsas bazaya restore və hər hansı destructive rollback ayrıca təsdiq tələb edir.

İndi əsas bazaya tətbiq edilməyib və backup yaradılmayıb.
