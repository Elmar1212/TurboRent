# Foundation arxitekturası

API vahid ASP.NET Core prosesidir. System modulu ayrıca assembly daxilində qeydiyyatdan keçir.
Feature qovluqları funksiyaların şaquli dilimləridir. Gələcək modullar başqa modulun
DbContext-inə birbaşa müraciət etməyəcək; müqavilələr tələb yarandıqda müəyyənləşdiriləcək.
BuildingBlocks hazırda qəsdən boşdur: əvvəlcədən ümumi repository və mediator abstraksiyaları yoxdur.

SystemDbContext EF Core/Npgsql bağlantısını təmin edir. Biznes cədvəli və migration yoxdur.
Liveness prosesin cavab verdiyini, readiness isə həqiqi PostgreSQL bağlantısını yoxlayır.
Readiness xətaları HTTP 503 qaytarır; connection string cavabda göstərilmir.

Vite /api sorğularını lokal API-yə ötürür. Buna görə geniş CORS siyasəti lazım deyil.
HTTP yalnız lokal development üçündür. Production TLS və authentication bu mərhələyə daxil deyil.
OpenAPI yalnız Development mühitində açıqdır. Loglar JSON formatındadır.
OpenTelemetry, JWT, Hangfire, MinIO və biznes modulları gələcək təsdiqlənən mərhələlərdədir.

Pul vahidi AZN, biznes saat qurşağı Asia/Baku-dur. Gələcək tarixlər UTC saxlanacaq,
istifadəçiyə göstərilərkən biznes saat qurşağına çevriləcək.
Hazırkı interfeys Azərbaycan dilindədir; tam az/en lokallaşdırma hələ implementasiya edilməyib.
