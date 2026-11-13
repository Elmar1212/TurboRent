# Registration & Email Verification

Bu mərhələ Customer/Owner/Company qeydiyyatı, lokal mailbox, email təsdiqi, resend və əsas giriş üçündür.
Real email, payments və deployment yoxdur. Admin elan yoxlaması və MFA axını README-də təsvir olunur.

## Lokal başlatma

Layihə kökü: repository-nin klonlandığı qovluq.
PostgreSQL container-i işləməlidir. InitialIdentity artıq lokal bazaya tətbiq olunub;
startup avtomatik migration işləmir.
Compose əmri PostgreSQL ilə yanaşı Garage obyekt saxlanmasını da başladır.
Əvvəlki `.env` faylında Garage parametrləri yoxdursa `scripts/Initialize-Garage.ps1` işlədin.

Terminal 1 (yalnız HTTPS portu əvvəlki HTTP API prosesi ilə konflikt yaratmır):

    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Start-Api.ps1 -Urls https://localhost:7080

Terminal 2:

    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Start-Web.ps1

Brauzer: https://localhost:5174/register
Foundation: https://localhost:5174/
API health: https://localhost:7080/api/v1/health/live
OpenAPI: https://localhost:7080/openapi/v1.json

Skriptlər development sertifikatını artifacts/dev-certs/ daxilində saxlayır.
Sistem trust ayarı avtomatik dəyişdirilmir. Brauzer lokal sertifikatı tanımırsa
developer öz terminalında dotnet dev-certs https --trust işlədə bilər.
Node yalnız bu development sertifikatına NODE_EXTRA_CA_CERTS ilə etibar edir;
TLS validation qlobal söndürülmür. Playwright yalnız lokal self-signed sertifikat
üçün ignoreHTTPSErrors istifadə edir.

Start-Api.ps1 parametrsiz işlədildikdə HTTPS 7080 və əvvəlki HTTP 5080 açılır.
Əvvəlki API həmin portda işləyirsə onu öz terminalında dayandırın və ya yuxarıdakı
HTTPS-only əmrdən istifadə edin. Qeydiyyat və giriş HTTP üzərindən 403 qaytarır.
Foundation HTTP health endpoint-ləri saxlanır.

## Əllə qeydiyyat və təsdiq

1. /register səhifəsində AZ/EN seçin.
2. Customer, Individual Owner və ya Rental Company seçin.
3. Formanı doldurun. Owner/company üçün +994 və 9 rəqəmli nömrə tələb olunur.
   Company üçün hüquqi ad və 10 rəqəmli VÖEN də tələb olunur.
4. Parol 15–128 simvoldur. Uzun ifadə və boşluqlar qəbul olunur.
5. Gözləmə ekranında yazdığınız email göstərilir; emaili yenidən yazmaq tələb olunmur.
   Lokal sınaqda həqiqi Gmail göndərişi yoxdur; ekranda bu açıq bildirilir.
6. VS Code-da repo daxilindəki `artifacts/identity-local/mailbox/` qovluğunda
   ən son `.txt` mesajını açın. Docker Compose bu qovluğu hostda əlçatan edir.
   Göndərən/ad/subject/body insan oxuya bilən email preview-sudur; linki brauzerdə açın.
   `.json` faylı lokal test adapterinin texniki məlumatıdır.
   Bu faylı, tokeni və linki chat, log və Git-ə köçürməyin.
7. `/verify-email` səhifəsində təsdiq düyməsini seçin. Link etibarlıdırsa email formu göstərilmir.
   Linkin GET ilə açılması
   database-i dəyişmir. Token avtomatik olaraq URL history-sindən təmizlənir.
8. Təsdiqdən sonra daxil olun. Owner/company hesabı Admin baxışına gözləmə statusu ilə
   avtomobil elan səhifəsinə keçir; Customer əsas səhifəyə keçir.
9. MFA ilə Admin `/admin/accounts` səhifəsində profil məlumatlarını yoxlayıb təsdiqləyir
   və ya səbəb göstərərək rədd edir. Təsdiqədək sahib hesabı elan göndərə bilməz.
10. Təsdiqdən sonra Owner/company üçün marka/model, avtomobil xüsusiyyətləri və 1–5
    şəkil əlavə edin. Elan Admin təsdiqinə qədər public kataloqda görünmür.
11. Yeni təsdiq linki istəmək üçün yalnız link etibarsız və ya olmadıqda
   görünən `/verify-email` səhifəsinin email formasından istifadə edin.

Email tokeninin müddəti 24 saatdır. Səhv, expired, başqa user və başqa purpose
tokenləri qəbul edilmir. Eyni düzgün verification təkrarı idempotentdir.
Resend əvvəlki linki dərhal ləğv etmir; linklər expiry/security stamp qaydalarına tabedir.
Səhifə yenilənəndə silinmiş tokeni geri almaq üçün lokal email linkini yenidən açın.

## Endpoint müqavilələri

Bütün POST sorğuları HTTPS, application/json və X-CSRF-TOKEN tələb edir.
Əvvəl GET /api/v1/auth/csrf edin; requestToken-i header-ə qoyun və həmin client-in
antiforgery cookie-sini saxlayın. Bu cookie authentication session cookie deyil.

### GET /api/v1/auth/csrf

200: { requestToken }. Cookie: __Host-TurboRent.Csrf, Secure, HttpOnly, SameSite=Strict,
Path=/, Domain yoxdur. HTTP üçün 403.

### POST /api/v1/auth/register

Request:
- email, password, displayName, locale: az/en
- accountType: customer/owner/company
- contactPhone: owner/company üçün
- legalName, voen: company üçün

roles/isAdmin/status kimi public giriş sahələri yoxdur; əlavə naməlum JSON sahələri rədd edilir.
Customer üçün də public DTO-da göstərilən əlavə profile sahələri səlahiyyət yaratmır.

202: { code: "verification_requested" }. Mövcud email üçün də eyni cavab.
User ID, verification tokeni və account existence cavabda verilmir.
Bütün hesablar yalnız Customer rolunu alır. Owner/company profili Pending-dir.
EmailConfirmed başlanğıcda false-dur.

### POST /api/v1/auth/email/verify

Request: { userId, token }.
200: { code: "email_verified" }.
400: invalid_verification.
Token email linkindəki Base64URL dəyəridir; Identity provider tokeni serverdə decode edilir.
Token yalnız request body-dədir; server URL query-sinə daxil edilmir.

### POST /api/v1/auth/email/resend

Request: { email }.
202: { code: "verification_requested" }.
Mövcud olmayan, artıq verified və uyğun olmayan hesab üçün də eyni cavab.
Yalnız Active və unverified hesaba lokal email yazılır.

### POST /api/v1/auth/login

Request: { email, password }.
200: { displayName }. Uğurlu cavabda `__Host-TurboRent.Session` adlı HttpOnly,
Secure, SameSite=Strict, host-only cookie yaradılır. Admin hesabları MFA mövcud
olmadığı üçün giriş edə bilmir. Yanlış parol, təsdiqlənməmiş və ya aktiv olmayan
hesab üçün 401 `invalid_credentials`; hesaba görə 15 dəqiqəlik rate limit 429 verir.
IP rate limit və Identity lockout da aktivdir.

### GET /api/v1/auth/session

Etibarlı, revokasiya edilməmiş, aktiv session üçün 200: { displayName }.
Session yoxdursa və ya müddəti bitibsə 401 `not_authenticated`.

### POST /api/v1/auth/logout

Uğurlu cavab: 204. Cari server-side session revokasiya edilir və session cookie silinir.

Ümumi statuslar: 400 validation/CSRF, 403 HTTPS tələb olunur, 413 16 KiB request həddi,
429 rate limit + Retry-After. ProblemDetails cavabında code və lazım olduqda errors var.
İstifadəçiyə göstərilən mətnlər frontend az/en lüğətindən seçilir.
Cavablar no-store-dur. Referrer-Policy no-referrer-dir.

## Texniki qərarlar

IdentityCore + EF store + default token provider istifadə olunur.
Parol ASP.NET Core Identity PasswordHasher vasitəsilə saxlanır; custom crypto yoxdur.
UserName email əsasında qurulur, NormalizedEmail unique DB indeksi race-condition müdafiəsidir.
Duplicate hesab üçün də password hash işi görülür ki, timing fərqi azalsın;
tam sabit-zamanlı şəbəkə davranışı iddia edilmir.

User, Customer rolu, Pending profil və registration audit eyni transaction-dadır.
UserManager ayrı SaveChanges çağırsa da eyni scoped DbContext/transaction istifadə olunur.
Profile/audit persistence xətası user və rol daxil olmaqla hamısını rollback edir.
Concurrent duplicate unique constraint ilə tutulur və ümumi 202 qaytarılır.
Verification user sətrini FOR UPDATE ilə kilidləyir və confirmation/audit-i atomik saxlayır.

Email faylı DB commit-dən sonra yazılır; ayrıca outbox cədvəli əlavə edilməyib.
Disk yazılması alınmasa hesab unverified qalır, resend ilə bərpa edilə bilər.
Bu lokal adapter real email deliverability və durable background delivery təmin etmir.

Mailbox yalnız Development və Testing üçün qeydiyyatdan keçir.
Production-da bu mərhələnin registration route-ları açılmır; real provider ayrıca təsdiq tələb edir.
Mailbox public web qovluğunda deyil və HTTP mailbox endpoint-i yoxdur.
Local adapter Gmail-ə real göndəriş etmir. Real email provider ayrıca təsdiq və konfiqurasiya tələb edir.
Fayl əvvəl .tmp yazılır, sonra atomik rename ilə .json edilir.
Token, parol, cookie və email linki tətbiq loglarına daxil edilmir.

Data Protection açarları artifacts/identity-local/keys/ daxilində saxlanır.
Windows-da DPAPI və yalnız cari istifadəçi ACL-i, Linux-da 0700 qovluq icazəsi istifadə olunur.
Lokal mailbox və sertifikatlar da məhdud disk qovluqlarındadır.
Bu lokal key storage production həlli deyil.

IP limiti default 20 POST/dəqiqədir. Login normallaşdırılmış email üzrə 3 sorğu/2 dəqiqə,
register və resend isə ayrıca 3 sorğu/15 dəqiqə ilə məhdudlaşdırılır; key-lər email hash-ləridir.
Limitlər in-memory-dir və bir prosesə aiddir. Redis əlavə edilməyib.
Testdə IP limiti qaldırılır; real limit ayrıca integration testində yoxlanır.
Beş uğursuz parol və ya Admin MFA cəhdindən sonra Identity hesab kilidi 2 dəqiqə davam edir.

React-də password/token localStorage və sessionStorage-a yazılmır.
Yalnız dil seçimi localStorage-a yazılır. Token URL fragment-indən bir dəfə oxunur,
history-dən silinir və təsdiqdən sonra yaddaşdakı token təmizlənir.
İki səhifə pathname əsasında seçilir; əlavə router və form kitabxanası quraşdırılmayıb.

## Testləri işə salmaq

    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-IdentityMigration.ps1
    npm.cmd --prefix apps/web run build
    npm.cmd --prefix apps/web run lint
    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-IdentityE2E.ps1

Backend runner artifacts/identity-migration/test.env tələb edir.
Testing API əlavə olaraq connection string-i Npgsql ilə parse edib yalnız
turborent_identity_test_* database adına icazə verir. Əsas bazaya fallback yoxdur.
E2E ayrıca HTTPS API 7081 və Vite 5174 proseslərini başladır və sonunda dayandırır.
5174 testdən əvvəl boş olmalıdır; mövcud serverə qoşulmur.
Manual development serveri testdən əvvəl öz terminalınızda Ctrl+C ilə dayandırın.
E2E mailbox-u artifacts/identity-e2e/<runId>/mailbox/ daxilindədir.
Bütün email-lər example.test test məlumatlarıdır.

CI eyni izolə PostgreSQL və HTTPS E2E axınına uyğunlaşdırılıb; remote-da icra edilməyib.
Model/migration müqayisəsi dəyişiklik göstərmir; yeni migration və SQL tətbiqi yoxdur.

## Bu mərhələnin faylları

Backend:
- Modules/Identity/TurboRent.Modules.Identity/IdentityModule.cs
- Modules/Identity/TurboRent.Modules.Identity/IdentityRegistrationEndpoints.cs
- Features/Register/RegisterRequest.cs və RegisterHandler.cs
- Features/VerifyEmail/VerifyEmailHandler.cs
- Validation/RegistrationValidation.cs
- Infrastructure/Email/LocalMailbox.cs və LocalIdentityFiles.cs
- Infrastructure/Security/IdentityLimits.cs
- TurboRent.Api/Program.cs

Frontend:
- apps/web/src/App.tsx
- apps/web/src/features/system-status/SystemStatus.tsx (mövcud Foundation ekranı)
- apps/web/src/features/identity/api.ts, AuthLayout.tsx, FormField.tsx
- apps/web/src/features/identity/RegisterPage.tsx, VerifyEmailPage.tsx, identity.css
- apps/web/src/shared/i18n/messages.ts
- apps/web/index.html, vite.config.ts, playwright.config.ts

Test/infrastruktur:
- tests/backend/TurboRent.Identity.UnitTests/RegistrationValidationTests.cs
- tests/backend/TurboRent.Identity.IntegrationTests/RegistrationTests.cs, csproj və lock
- tests/e2e/identity/registration.spec.ts
- scripts/Initialize-LocalHttps.ps1, Start-Api.ps1, Start-Web.ps1, Test-IdentityE2E.ps1
- .github/workflows/ci.yml
- README.md və bu sənəd