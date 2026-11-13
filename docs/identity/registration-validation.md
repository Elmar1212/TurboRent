# Registration & Email Verification — yekun yoxlama

Tarixi status: bu hesabat ilkin qeydiyyat/email mərhələsinin nəticələrini saxlayır.
Giriş və sessiya üçün 2026-10-26 yenilənməsi bu sənədin sonunda verilib.

## Nəticələr

Release solution build: 0 xəta, 0 xəbərdarlıq.
Backend run: 47fcc833655b, cəmi 62/62.
E2E run: 1c8ba06c6778, 7/7 (2 Foundation + 5 Identity).
Frontend TypeScript/Vite production build və oxlint: uğurlu.
EF has-pending-model-changes: dəyişiklik yoxdur.
InitialIdentity SQL checksum-u dəyişməyib:
12B167C30FFECE7B9410F244AB2AE0D8F088CA81BE7B7BFD02C364275C813BCD.
| Backend suite | Nəticə |
|---|---:|
| TurboRent.Identity.IntegrationTests | 36/36 |
| TurboRent.Identity.UnitTests | 21/21 |
| TurboRent.IntegrationTests | 4/4 |
| TurboRent.UnitTests | 1/1 |

## Sübut edilən davranışlar

- Customer, Owner və Company qeydiyyatı: hash-lənmiş parol, yalnız Customer rolu.
- Owner/company Pending profili; session yaradılmır.
- User/rol/profil/audit transaction-u; profile persistence fault-da hamısı rollback olur.
- Duplicate və parallel duplicate üçün eyni 202 cavabı, yalnız 1 user.
- Invalid input, 15–128 password həddi, public Admin/role injection rədd edilir.
- CSRF yoxdursa və ya başqa client-dən gəlirsə 400; cookie Secure/HttpOnly/Strict/host-only.
- Email verification işləyir və təkrar düzgün təsdiq idempotentdir, audit təkrarlanmır.
- Səhv, expired, wrong-user və password-reset purpose tokeni rədd edilir.
- Resend mövcud olmayan hesabı açıqlamır; account/IP limitləri 429 və Retry-After qaytarır.
- Plain HTTP auth sorğusu rədd edilir; Production-da lokal registration route-u yoxdur.
- Oversize request 413; həmin test run zamanı login endpoint-i mövcud deyildi.
- Testing mühiti səhvən turborent bazasına yönlənsə bağlantı açmadan bloklanır.
- API cavabında token yoxdur; capture edilmiş loglarda test parolu/email linki yoxdur.
- Üç hesab növü üçün HTTPS brauzer axını: register -> lokal mailbox -> explicit verify.
- Azərbaycan mobil validation, focus, az/en, invalid link recovery və network failure.
- 390px mobil görünüşdə üfüqi overflow yoxdur.

## Vizual review

Desktop və mobil screenshot-lar açılıb vizual yoxlanılıb:
artifacts/identity-e2e/7b6241e83456/register-desktop.png
artifacts/identity-e2e/7b6241e83456/register-mobile.png
Son E2E run-da eyni screenshot-lar artifacts/identity-e2e/1c8ba06c6778/ daxilindədir.
Bunlarda yalnız sintetik test məlumatları var; password input-ları maskalanıb.

## İzolyasiya və sərhədlər

Yazan testlər yalnız turborent_identity_test_ff70ee3852b8 bazasında işləyib.
Son read-only əsas baza yoxlaması: users=0, sessions=0.
Yeni migration yaradılmayıb və heç bir schema dəyişdirilməyib.
Mailbox, key-lər, cert private key, .env və backup Git-də tracked deyil.
Local mailbox yalnız development/test üçündür; production email yoxdur.
CI faylı uyğunlaşdırılıb, amma GitHub runner-də icra edilməyib və push edilməyib.
Windows-local nəticələrdir; Linux CI nəticəsi kimi təqdim edilmir.

## Düzəldilən məsələlər

İlkin compile mərhələsində çatışmayan extension using-ləri düzəldildi.
HTTP request-size testi TestServer-də chunked JsonContent göndərdiyi üçün native
validation 400 qaytarırdı. Məlum Content-Length ilə real ölçü-limit yolunu yoxlayan
fixture düzəldildi; son suite 413 nəticəsini təsdiqləyir.
Lokal scriptlər yalnız ayrıca prosesin ExecutionPolicy Bypass parametri ilə işlədilir;
sistem siyasəti və OS certificate trust avtomatik dəyişdirilmir.

## Giriş mərhələsi — 2026-10-26

HTTPS email/parol girişi, verifikasiya yoxlaması, DB-backed session cookie,
session statusu və çıxış əlavə edildi. Schema və migration dəyişməyib.
Debug backend run: identity integration 41/41, identity unit 23/23,
core integration 4/4, core unit 1/1. Frontend build, oxlint və unit tests uğurlu.
Isolated Debug browser E2E run 952121c5936f: 12/12, registration -> verification ->
login -> homepage session -> logout daxil olmaqla.

İstifadə qaydası və bu mərhələnin tam fayl siyahısı: registration.md.