# Avtomobil kataloqu — ilk vertical slice

Bu mərhələ açıq avtomobil markası/model kataloqunu, təsdiqlənmiş elanların
siyahısını, filtrləri və elan detallarını təqdim edir. Demo elanları yaradılmır;
yalnız `Approved` statuslu elanlar ictimai API-də görünür. Owner/company hesabı
email təsdiqindən və Admin hesab təsdiqindən sonra elan göndərə bilər; yeni elan Admin baxışınadək
`Pending` statusunda qalır. Admin üçün MFA ilə qorunan panel Pending elanları göstərir,
şəkilləri baxmağa, təsdiqləməyə və ya allowlisted səbəblə rədd etməyə imkan verir.
Rezervasiya və ödənişlər bu mərhələyə daxil deyil.

## Lokal database

Catalog ayrıca `catalog` PostgreSQL schema-sında saxlanır və öz EF migration history
cədvəlindən istifadə edir. Mövcud `identity` məlumatlarına və migration-larına toxunmur.

Compose PostgreSQL işlədikdən sonra migration-ı bir dəfə tətbiq edin:

    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Apply-CatalogMigration.ps1

Skript yalnız lokal `turborent` bazasına yeni catalog schema-sını əlavə edir.
Mövcud schema və məlumatlar silinmir. Test və E2E skriptləri migration-ı yalnız
adında `turborent_identity_test_` olan izolə test bazasında tətbiq edir.
API startup avtomatik migration işləmir.

Catalog seed-i 75 marka və 651 minik avtomobili/SUV model xəttini, komplektasiya və
nəsilləri ayırmadan əlavə edir. Bu, Azərbaycan bazarı üçün geniş, əl ilə seçilmiş
başlanğıc siyahıdır; bütün tarixi, nadir və ya yeni təqdim edilmiş modelləri əhatə
etdiyinə dair rəsmi reyestr zəmanəti yoxdur. Marka və model adları JSON seed
faylında saxlanır ki, çatışmayan modellər sonradan aydın şəkildə əlavə edilə bilsin.
Avtomobil elanları qəsdən seed edilmir; buna görə boş yeni bazada sayt dürüst
“təsdiqlənmiş elan yoxdur” vəziyyətini göstərir.

## Sahibin avtomobil elanı və fotoşəkillər

Owner/company istifadəçisi email təsdiqindən sonra daxil ola bilər, lakin hesabı Admin
tərəfindən təsdiqlənənədək elan yerləşdirə bilməz. Status `/owner/listings/new`
səhifəsində görünür. Təsdiqdən sonra həmin səhifədə marka/model, il, şəhər, günlük qiymət, ötürücü, yanacaq,
oturacaq sayı və təsvir tələb olunur. Ən azı bir, ən çox beş JPG, PNG və ya WebP
şəkli qəbul edilir; hər şəkil maksimum 50 MiB, bütün şəkillər isə cəmi 100 MiB ola bilər.
Fayl adı istifadə edilmir; API
format imzasını yoxlayır, təsadüfi object key yaradır və private bucket-də saxlayır.
Şəkil endpoint-i yalnız elan `Approved` olduqda faylı qaytarır. Yeni elan `Pending`-dir;
Admin approval UI/API Pending elanların baxışını təmin edir; elanlar təsdiq verilənədək görünmür.

Lokal şəkil saxlama Docker Compose-də Garage S3-compatible serveridir. Garage
single-node developer storage-dir, production redundancy və backup həlli deyil.
Əvvəlki `.env` faylında Garage konfiqurasiyası yoxdursa, aşağıdakı əmri bir dəfə işlədin:

    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Initialize-Garage.ps1

`artifacts/garage/garage.toml` və `.env` Git-ə commit edilməyən lokal config-dir.
Garage S3 API `127.0.0.1:3900` üzərində işləyir və private bucket-i startup-da yaradır.
Single-node local storage production deployment üçün nəzərdə tutulmayıb.

## API

- `GET /api/v1/catalog/makes?locale=az|en` — marka/model seçimləri.
- `GET /api/v1/catalog/listings` — yalnız təsdiqlənmiş elanlar; səhifələmə `page`
  (1–10000), `pageSize` (1–48).
- `GET /api/v1/catalog/listings/{id}?locale=az|en` — aktiv elan detalı.
- `GET /api/v1/catalog/listings/{id}/images/{imageId}` — yalnız təsdiqlənmiş elan şəkli.
- `POST /api/v1/owner/listings` — verified owner/company session və CSRF ilə multipart
  elan və şəkillərin göndərilməsi; cavab `201` və `Pending` statusu qaytarır.
- `GET /api/v1/admin/listings?page=1&pageSize=20` — MFA ilə açılmış Admin session üçün
  səhifələnən Pending növbəsi və şəkil URL-ləri.
- `GET /api/v1/admin/accounts?page=1&pageSize=20` — emaili təsdiqlənmiş, Admin baxışı
  gözləyən Owner/Company profilləri.
- `POST /api/v1/admin/accounts/{userId}/approve` — HTTPS, CSRF və Admin MFA session ilə
  Owner/Company hesabına elan yerləşdirmə səlahiyyəti verir.
- `POST /api/v1/admin/accounts/{userId}/reject` — eyni qoruma ilə hesabı rədd edir;
  `incomplete_information`, `invalid_contact_details`, `invalid_business_information`
  (yalnız şirkət) və ya `other` səbəblərindən birini tələb edir.
- `GET /api/v1/admin/listings/{id}/images/{imageId}` — yalnız MFA Admin üçün Pending
  elanın private şəkli.
- `POST /api/v1/admin/listings/{id}/approve` — HTTPS, CSRF və Admin MFA session ilə elan
  təsdiqləyir.
- `POST /api/v1/admin/listings/{id}/reject` — eyni qoruma ilə
  `missing_information`, `invalid_vehicle_details`, `image_quality` və ya `other`
  allowlist səbəblərindən birini tələb edir.

Review event-lər `vehicle_listing_review_events` cədvəlində Admin ID, qərar, optional
səbəb və UTC timestamp ilə saxlanılır. Status optimistic concurrency token olduğundan
iki Admin eyni vaxtda baxış keçirəndə yalnız ilk qərar qeydə alınır.
Owner/Company qərarları `owner_profiles` / `company_profiles` review sahələrinə və
Identity security audit qeydlərinə yazılır; təkrar və paralel baxışlar konflikt qaytarır.

Elan siyahısı `city`, `make`, `model`, `minYear`, `maxYear`, `minPrice`,
`maxPrice`, `transmission`, `fuelType`, `seats` filtrlərini dəstəkləyir.
Qiymətlər AZN/gün, il 1980–2035, oturacaq sayı 2–9 aralığındadır.
Filtr dəyərləri allowlist/range ilə yoxlanılır; uğursuz və təsdiqlənməmiş elanlar
public detal endpoint-dən də 404 alır.

## UI

- `/` — TurboRent ana səhifəsi və sürətli şəhər/qiymət axtarışı.
- `/cars` — Azərbaycan/ingilis dillərində filtrli elan siyahısı.
- `/cars/{id}` — təsdiqlənmiş elan detalları.

Marka/model mətni serverdən cari AZ/EN dilində qaytarılır. Təsdiqlənmiş elanın
şəkilləri mövcuddursa kart və detal səhifəsində göstərilir; stok şəkil uydurulmur.
