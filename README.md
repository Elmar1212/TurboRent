# TurboRent — Milestone 1

React + TypeScript + Vite, ASP.NET Core .NET 10, Modular Monolith və PostgreSQL/EF Core təməli.

## Tələblər

.NET SDK 10.0.401, Node.js 24, npm, Docker Desktop (Linux containers).
Layihə kökü: C:\Users\hesen\TurboRent.
PowerShell npm.ps1 bloklayırsa npm.cmd istifadə edin.
Aşağıdakı ExecutionPolicy Bypass yalnız göstərilən lokal skript prosesinə aiddir; sistem siyasətini dəyişmir.

## İlk hazırlıq

Yalnız yeni clone üçün (hazırkı lokal .env artıq yaradılıb):

    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Initialize-Local.ps1

Skript mövcud .env faylını əvəz etmir. .env Git tərəfindən nəzərə alınmır.
.env.example həqiqi parol deyil. Parolu mənbə koduna daxil etməyin.

    npm.cmd --prefix apps/web ci
    dotnet restore TurboRent.slnx --locked-mode
    docker compose --env-file infra/docker/.env -f infra/docker/compose.yaml up -d --wait

PostgreSQL: 127.0.0.1:55432, database/user: turborent.
Compose layihəsi turborent; ayrıca named volume istifadə olunur.
Başqa layihələrin container-lərinə toxunulmur.

## İşə salma

Terminal 1:

    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Start-Api.ps1

Terminal 2:

    npm.cmd --prefix apps/web run dev

Brauzer: http://127.0.0.1:5173
API: http://127.0.0.1:5080/api/v1/health/live
DB readiness: http://127.0.0.1:5080/api/v1/health/ready
OpenAPI: http://127.0.0.1:5080/openapi/v1.json

Liveness DB olmadan da 200 qaytarır. Readiness DB əlçatmazdırsa 503 qaytarır.
Frontend hər iki vəziyyəti ayrıca göstərir.

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

API/frontend terminalında Ctrl+C.

    docker compose --env-file infra/docker/.env -f infra/docker/compose.yaml stop

Bu əmr yalnız TurboRent container-ini dayandırır və məlumatları saxlayır.
Volume silən əmrlər işlətməyin.

## Sərhədlər

Bu mərhələ yalnız Foundation-dır. Authentication, ödəniş, booking, verification,
MinIO, Hangfire, tam lokallaşdırma, OpenTelemetry və deployment implementasiya edilməyib.
GitHub Actions build/test workflow hazırlanıb; remote repozitoriyaya göndərilməyib.
Arxitektura izahı: docs/architecture.md.
