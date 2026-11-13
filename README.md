# TurboRent — Milestone 1

React + TypeScript + Vite, ASP.NET Core .NET 10, Modular Monolith və PostgreSQL/EF Core təməli.

## Tələblər

.NET SDK 10.0.401, Node.js 24, npm, Docker Desktop (Linux containers).
Layihə kökü: bu repozitoriyanın klonlandığı qovluq.
PowerShell npm.ps1 bloklayırsa npm.cmd istifadə edin.
Aşağıdakı ExecutionPolicy Bypass yalnız göstərilən lokal skript prosesinə aiddir; sistem siyasətini dəyişmir.

## İlk hazırlıq

Yalnız yeni clone üçün (hazırkı lokal .env artıq yaradılıb):

    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Initialize-Local.ps1

Skript mövcud .env faylını əvəz etmir. .env Git tərəfindən nəzərə alınmır.
.env.example həqiqi parol deyil. Parolu mənbə koduna daxil etməyin.

    npm.cmd --prefix apps/web ci
    dotnet restore TurboRent.slnx --locked-mode
    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Initialize-LocalHttps.ps1
    docker compose --env-file infra/docker/.env -f infra/docker/compose.yaml up -d --wait

PostgreSQL: 127.0.0.1:55432, database/user: turborent.
Garage S3 API: 127.0.0.1:3900.
Sayt: https://localhost:5174 (lokal sertifikat browser tərəfindən etibar edilməlidir).
Compose layihəsi turborent; PostgreSQL, Garage və Identity açar/mailbox məlumatları
named volume-larda saxlanır. API yalnız compose şəbəkəsində əlçatandır; UI `/api`
sorğularını API-yə ötürür. Bütün xidmətlər `turborent-network` şəbəkəsindədir.
Başqa layihələrin container-lərinə toxunulmur.

Əgər `.env` əvvəldən mövcud idisə, Garage üçün açarları və lokal config-i hazırlayın:

    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Initialize-Garage.ps1

Catalog modulunun `catalog` schema-sını lokal bazaya əlavə etmək üçün:

    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Apply-CatalogMigration.ps1

## Docker Compose ilə işə salma

Stack-i qurub başladın:

    docker compose --env-file infra/docker/.env -f infra/docker/compose.yaml up --build -d --wait

Yeni lokal bazada Identity və Catalog migration-larını bir dəfə tətbiq edin:

    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Apply-InitialIdentity.ps1
    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Apply-CatalogMigration.ps1

Brauzer: https://localhost:5174
API health: https://localhost:5174/api/v1/health/live
Database readiness: https://localhost:5174/api/v1/health/ready

Compose healthcheck PostgreSQL, Garage, API (API + DB readiness) və UI xidmətlərini yoxlayır.
Brauzer HTTPS developer sertifikatına etibar etmirsə, `dotnet dev-certs https --trust`
əmrini developer terminalında bir dəfə işlədin. Lokal email preview və Data Protection
açarları Compose `identity_data` volume-unda saxlanır.

Əvvəlki terminal-based API/Vite axını aşağıdakı ayrıca skriptlərlə hələ də mümkündür:

    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Start-Api.ps1
    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Start-Web.ps1

## Build və test

    dotnet build TurboRent.slnx
    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Backend.ps1
    npm.cmd --prefix apps/web run build
    npm.cmd --prefix apps/web run lint
    npm.cmd --prefix apps/web exec -- playwright install chromium
    npm.cmd --prefix apps/web run test:e2e

Backend testləri üçün PostgreSQL işləməlidir. E2E üçün PostgreSQL və API işləməlidir;
Playwright Vite serverini özü başladır və iş bitdikdə dayandırır.
Testlər real DB bağlantısını, DB xətasında 503 cavabını, OpenAPI-ni,
brauzerdən API bağlantısını və frontend xəta vəziyyətini yoxlayır.
Hazırda biznes məlumatını dəyişən test yoxdur.

## Dayandırma

    docker compose --env-file infra/docker/.env -f infra/docker/compose.yaml down

Bu əmr TurboRent container və şəbəkəsini dayandırıb silir; named volume-lardakı
PostgreSQL, Garage və Identity məlumatlarını saxlayır. Volume silən əmrlər işlətməyin.

## Sərhədlər

Foundation, əsas Identity qeydiyyat/giriş və ictimai Vehicle Catalog dilimləri mövcuddur.
Owner/company üçün ilkin elan formu və şəkillərin lokal Garage-a yüklənməsi mövcuddur.
Admin panelində sahib/şirkət hesablarının, həmçinin Pending elanların baxış və təsdiq
prosesləri mövcuddur; booking, availability, ödənişlər, Hangfire, OpenTelemetry və
deployment hələ implementasiya edilməyib.

## Saytın cari funksiyaları

- Ana səhifə və AZ/EN seçimləri.
- İctimai avtomobil marka/model kataloqu, təsdiqlənmiş elan axtarışı və elan detalları:
  `/`, `/cars`, `/cars/{id}`.
- Azərbaycan bazarı üçün 75 marka və 651 minik avtomobili/SUV model xətti olan başlanğıc kataloqu.
- HTTPS qeydiyyatı, email təsdiqi və təsdiqlənmiş hesabla giriş.
- Owner/Company hesabları Admin tərəfindən təsdiqlənənədək elan yerləşdirə bilməz.

Yeni lokal bazada hələ təsdiqlənmiş elan yoxdur; sayt bunu açıq göstərir.
Elanlar Admin təsdiqinə qədər gizlidir; Admin panelinə `/admin` ünvanından daxil olunur.
Kataloqun texniki təsviri: [catalog.md](docs/catalog/catalog.md).
Arxitektura izahı: docs/architecture.md.

## Identity migration yoxlaması

Backend test runner Identity və Catalog migration-larını yalnız ayrıca lokal test bazasında yoxlayır;
əsas `turborent` bazasına fallback etmir.
Bütün backend testləri üçün izolə runner istifadə edin:

    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-IdentityMigration.ps1

Test-Backend.ps1 də həmin runner-ə yönləndirir; işləyən Debug API varsa -Configuration Release verin.
Runner artifacts/identity-migration/test.env tələb edir və əsas bazaya fallback etmir.
Bu lokal fayl commit edilmir; yeni clone-da ayrıca test mühiti hazırlanmalıdır.
Nəticələr: docs/migrations/InitialIdentity-test-results.md.
Əsas bazanın təsdiq gözləyən backup planı: docs/migrations/InitialIdentity-main-backup-plan.md.
## Identity migration — cari lokal status

InitialIdentity backup və izolə restore yoxlamalarından sonra lokal turborent bazasına tətbiq edilib.
12 Identity cədvəli və 4 rol mövcuddur; Admin hesabı yalnız ilk bootstrap addımında yaradılır.
Əvvəlki migration bölməsi mərhələ tarixçəsidir. Yekun hesabat:
docs/migrations/InitialIdentity-main-results.md.
Qeydiyyat, email təsdiqi və əsas email/parol girişi hazırdır.

## Registration & Email Verification — cari mərhələ

Üç hesab növü üçün qeydiyyat, Pending profil, lokal mailbox, email verification/resend və əsas giriş hazırdır.
Başlatma, endpoint-lər və dəyişən fayllar: [registration.md](docs/identity/registration.md).
Test nəticələri: [registration-validation.md](docs/identity/registration-validation.md).
Giriş HTTPS üzərindən session cookie yaradır; Admin girişi TOTP MFA tələb edir.

## Admin hesab və elan yoxlaması

İlk Admin hesabı yalnız lokal development compose API-sinin loopback ünvanı ilə yaradılır.
`scripts/Initialize-Local.ps1` шинэ `.env` үүсгэхэд random bootstrap secret хадгална;
əvvəl `.env` varsa `scripts/Initialize-Garage.ps1` işlədərək secret-i əlavə edin.
Database migration-ları və compose stack işlək olduqda:

    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Apply-CatalogMigration.ps1
    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Bootstrap-Admin.ps1

Bootstrap script email, ad və parol soruşur, ilk Admin üçün authenticator TOTP
setup URI və manual secret-i bir dəfə göstərir. Onu authenticator tətbiqində qeyd edin
və parol kimi qoruyun. Bootstrap endpoint secret və API container loopback tələb edir,
Admin yarandıqdan sonra yenidən işləməyəcək. Public Admin qeydiyyatı yoxdur.
`/login` səhifəsində Admin email/parolundan sonra authenticator-un 6 rəqəmli kodunu
daxil edin. Eyni TOTP kodu təkrar qəbul olunmur; Admin session 15 dəqiqə hərəkətsizlik,
cəmi 8 saat müddətlidir. Panel: `https://localhost:5174/admin`.
Uğursuz login/MFA cəhdlərindən sonra hesab kilidi 2 dəqiqədir; email üzrə login limiti
3 sorğu/2 dəqiqədir. Admin sessiyasının 15 dəqiqəlik inactivity müddəti dəyişməyib.

Authenticator-a çıxış itirilibsə, API və lokal Compose stack işlək olduqda,
repo qovluğunda bu recovery skriptini başladın:

    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Reset-AdminMfa.ps1

Cari Admin emailini daxil edin. Yeni TOTP URI/secret yalnız həmin PowerShell pəncərəsində
göstərilir; dərhal authenticator-a əlavə edib təhlükəsiz saxlayın. Reset köhnə MFA
quruluşunu ləğv edir və Admin sessiyalarını bağlayır; Admin hesabı və parolu saxlanır.
Bu əməliyyat local-only bootstrap secret və API container loopback tələb edir.

`/admin/accounts` səhifəsində emaili təsdiqlənmiş Owner/Company profillərini yoxlayıb
təsdiqləmək və seçilmiş səbəblə rədd etmək mümkündür. Təsdiqədək hesab daxil ola bilər,
amma elan yerləşdirmə səlahiyyəti verilmir. Qərarlar Identity audit qeydlərində
saxlanılır; rədd səbəbi profilin cari vəziyyətində görünür.

`/admin` səhifəsi Pending vehicle listing-ləri göstərir; şəkillərə baxmaq, approve etmək
və ya seçilmiş səbəblə reject etmək mümkündür. Qərarlar UTC vaxtı, Admin user ID və
allowlist səbəbi ilə audit tarixçəsinə yazılır. Ümumi istifadəçi idarəetməsi və
kataloq redaktəsi bu versiyaya daxil deyil.

## Milestone 2 — marketplace funksiyaları

- **Rezervlər:** icarəçi tarix seçib sorğu göndərir, sahib `/owner/bookings` səhifəsində təsdiqləyir və ya rədd edir.
  Təsdiq eyni günlərə düşən digər sorğuları avtomatik rədd edir. Ödəniş sahiblə birbaşa razılaşdırılır.
- **Sahib paneli:** `/owner/listings` — elanları redaktə etmək (yenidən yoxlamaya gedir), arxivləşdirmək, bərpa və silmək.
- **Parol:** `/forgot-password` və `/reset-password` (link 2 saat etibarlıdır), `/account` səhifəsində parolu dəyişmək.
- **Şəkillər:** maksimum 10 MB/şəkil, cəmi 50 MB. Brauzer şəkli kiçildir; server EXIF/GPS metadata-nı silir və şəkli
  stream edir (keşləmə başlıqları ilə).
- Yeni catalog migration-ı (`AddBookingsAndOwnerManagement`) tətbiq edin: `scripts/Apply-CatalogMigration.ps1`.

### Konfiqurasiya

| Açar | Təsvir |
| --- | --- |
| `Email__Smtp__Host`, `Port`, `Username`, `Password`, `Email__FromAddress` | Real email (SMTP relay). Boşdursa Development-də lokal mailbox istifadə olunur. Production-da SMTP olmadan hesab endpoint-ləri açılmır. |
| `ReverseProxy__TrustedNetworks` | `X-Forwarded-For` göndərə bilən proxy şəbəkələri (CIDR). Compose-da private şəbəkələr (API portu xaricə açıq deyil). Olmasa rate limit bütün istifadəçilərə ortaq tətbiq olunur. |
| `Identity__DataProtectionKeysPath` | Sessiya/token açarlarının qovluğu; bütün API instansiyaları üçün ortaq və davamlı olmalıdır. |
| `RateLimits__WritesPerMinute`, `DashboardPerMinute`, `ContactPerMinute` | IP üzrə limitlər (default 30 / 300 / 10). |

Qeyd: Windows Hyper-V bəzən 55432 portunu rezerv edir (`netsh interface ipv4 show excludedportrange protocol=tcp`).
Bu halda `.env`-də `POSTGRES_PORT`-u rezerv olunmamış porta dəyişin.
