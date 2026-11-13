# Foundation arxitekturası

API vahid ASP.NET Core prosesidir. System modulu ayrıca assembly daxilində qeydiyyatdan keçir.
Feature qovluqları funksiyaların şaquli dilimləridir. Gələcək modullar başqa modulun
DbContext-inə birbaşa müraciət etməyəcək; müqavilələr tələb yarandıqda müəyyənləşdiriləcək.
BuildingBlocks hazırda qəsdən qısadır: əvvəlcədən ümumi repository və mediator abstraksiyaları yoxdur.

SystemDbContext EF Core/Npgsql health check-i təmin edir.
Identity və Catalog öz DbContext/schema/migration history-sini saxlayır.
Liveness prosesin cavab verdiyini, readiness isə həqiqi PostgreSQL bağlantısını yoxlayır.
Readiness xətaları HTTP 503 qaytarır; connection string cavabda göstərilmir.

Vite /api sorğularını lokal API-yə ötürür. Buna görə geniş CORS siyasəti lazım deyil.
HTTP yalnız lokal development üçündür. Identity əməliyyatları HTTPS tələb edir.
OpenAPI yalnız Development mühitində açıqdır. Loglar JSON formatındadır.
Vehicle image storage development üçün lokal, single-node Garage S3 API istifadə edir.
Production storage və secret management ayrıca təsdiqlənəcək.
OpenTelemetry, JWT, Hangfire və digər biznes modulları gələcək mərhələlərdədir.

Pul vahidi AZN, biznes saat qurşağı Asia/Baku-dur. Gələcək tarixlər UTC saxlanacaq,
istifadəçiyə göstərilərkən biznes saat qurşağına çevriləcək.
Identity və public vehicle catalog interfeyslərində AZ/EN lokallaşdırma var.
Catalog public endpoint-ləri yalnız `Approved` elanları göstərir. Owner/company
istifadəçisi təsdiqlənmiş hesabla elan göndərə və 1–5 şəkil yükləyə bilər;
yeni elanlar Admin yoxlamasınadək `Pending` qalır. Admin paneli hələ yoxdur.
