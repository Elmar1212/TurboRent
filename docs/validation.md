# Foundation yoxlama nəticələri — 2026-10-24

- .NET solution build: uğurlu, 0 xəbərdarlıq, 0 xəta.
- Unit testlər: 1/1 uğurlu.
- Integration testlər: 4/4 uğurlu.
  - Liveness verilənlər bazası olmadan işləyir.
  - Readiness əlçatmaz verilənlər bazası üçün 503 qaytarır.
  - Readiness həqiqi PostgreSQL bağlantısı ilə 200 qaytarır.
  - Development OpenAPI endpoint-i 200 qaytarır.
- React/TypeScript production build: uğurlu.
- Frontend lint: uğurlu.
- Chromium E2E: 2/2 uğurlu.
  - React -> Vite proxy -> API -> PostgreSQL əlaqəsi.
  - API şəbəkə xətasında frontend vəziyyət mesajı.
- NuGet locked-mode restore: uğurlu.
- npm audit (install zamanı): 0 vulnerability.
- infra/docker/.env: git check-ignore ilə təsdiqləndi.
- Aktiv OneDrive/Desktop/TurboRent qovluğunda fayl yaradılmayıb.
- Başqa layihələrin container-ləri üzərində stop/rm/down əmri icra edilməyib.

API test prosesi dayandırılıb; E2E Vite prosesini özü dayandırıb.
TurboRent PostgreSQL container-i 127.0.0.1:55432 portunda işləyir.
Git repozitoriyası lokal yaradılıb; commit/push/deployment edilməyib.
CI workflow lokal hazırlanıb; GitHub runner-də hələ icra edilməyib.