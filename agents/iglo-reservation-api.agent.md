# Agent: Iglo Reservation API Developer

## Görev

SGK Üsküdar İglo rezervasyon yönetim sistemi için üretime uygun temelleri olan bir ASP.NET Core Web API backend'i oluştur. Katmanlı mimari kullan; PostgreSQL, EF Core Code First, AutoMapper, JWT authentication, role-based authorization, validation, rate limiting, logging ve güvenli hata yanıtlarını uygula.

## Proje hedefleri

- Backend: ASP.NET Core Web API (C#)
- Database: PostgreSQL
- ORM: Entity Framework Core
- DB yaklaşımı: Code First + EF Core migrations
- Mapping: AutoMapper
- Mimari: Controller → Service → Repository → DbContext
- DTO'lar: request/response modelleri entity'lerden ayrı
- Authentication: JWT Bearer
- Authorization: `Admin`, `User` rolleri
- API davranışı:
  - Admin: rezervasyon ve gerekli yönetim kaynaklarında CRUD
  - User: yalnızca READ
- Frontend daha sonra ayrı bir istemci olarak API'yi kullanacak. API CORS ayarlarını yalnızca gerekli origin'lere izin verecek şekilde yapılandır.

## Uygulama sırası

1. Mevcut repository/proje dosyalarını incele. Var olan kodu veya kullanıcı değişikliklerini izinsiz silme.
2. Uygulamadan önce kısa bir plan ve önerilen entity/endpoint listesini sun.
3. .NET SDK ve proje durumunu doğrula. Proje yoksa Web API oluştur.
4. Katmanları ve klasörleri oluştur:
   - `Controllers`
   - `Services` ve `Services/Interfaces`
   - `Repositories` ve `Repositories/Interfaces`
   - `DTOs` (Request/Response veya feature bazlı alt klasörler)
   - `Models/Entities`
   - `Data`
   - `Mapping`
   - `Middleware` (gerekiyorsa)
   - `Configurations`
5. NuGet paketlerini projeye ekle. Paket adlarını doğrula; uyumsuz veya eski paket sürümlerini körlemesine kullanma.
6. PostgreSQL bağlantısını `appsettings.json` üzerinden yapılandır. Gerçek parola veya JWT secret değerini dosyaya yazma; development secrets/environment variables kullan.
7. Entity, EF Core configuration ve DbContext tasarımını oluştur.
8. DTO'ları ve AutoMapper mapping profillerini oluştur. AutoMapper paketini yükle, profil sınıfını oluştur ve DI'a kaydet.
9. Repository interface/implementation ve service interface/implementation oluştur. İş kuralları service katmanında kalmalı.
10. Controller'ları oluştur; controller içinde doğrudan DbContext veya repository kullanma.
11. JWT authentication, role authorization ve güvenli admin bootstrap mekanizmasını uygula.
12. Validation, global exception handling, ProblemDetails, logging ve rate limiting ekle.
13. Migration oluşturma ve çalıştırma komutlarını README'ye ekle. Uygulama başlangıcında production'da otomatik migration çalıştırma; migration'lar kontrollü deploy adımı olmalı.
14. Build/test çalıştır. Hata varsa düzelt. Çalıştırmadığın komutları çalıştırılmış gibi bildirme.
15. Sonuçta oluşturulan mimariyi, endpoint listesini, paketleri, migration komutlarını ve eksik yapılandırmaları özetle.

## Beklenen başlangıç domain modeli

İlk sürüm için aşağıdaki modelleri değerlendir:

- `ApplicationUser`: kullanıcı, parola hash'i, aktiflik, rol ilişkisi
- `Reservation`: rezervasyon
- `IgloUnit`: rezervasyon yapılabilen iglo/birim
- `ReservationStatus` veya status enum: gerekiyorsa
- `AuditLog`: admin işlemlerini izlemek için; kişisel verileri gereksiz yere loglama

Kullanıcı ve rol yönetimi için ASP.NET Core Identity tercih et. Identity tabloları `Data` altında kullanılan `ApplicationDbContext` ile PostgreSQL'e Code First migration üzerinden oluşturulmalı. Identity kullanılıyorsa parola hash'ini kendin yazma.

Rezervasyon için alanları işletme kuralları doğrultusunda belirle. En azından rezervasyon sahibi, birim, başlangıç/bitiş tarih-saatleri, oluşturulma zamanı ve durumunu değerlendir. Aynı birimde çakışan aktif rezervasyonları engelle. Çakışma kontrolü sadece frontend veya controller'da kalmamalı; service katmanında yapılmalı ve mümkünse DB seviyesinde de güvence sağlanmalı.

## Endpoint kapsamı

En az şu endpoint gruplarını tasarla:

- Auth: login, gerekirse register (public kayıt açık olacaksa yalnızca `User` rolü atansın), current user
- Reservations: list/detail READ; create/update/delete yalnızca Admin
- Iglo units: list/detail READ; create/update/delete yalnızca Admin
- Users: admin tarafından listeleme/rol atama/aktiflik yönetimi; parola gösterme veya döndürme yok
- Health: health check

Endpoint'lerin kesin path ve request/response DTO'larını implementasyondan önce listele. Auth endpoint'i dışında her endpoint'in role/authorization davranışını açıkça tanımla. `User` rolüne hiçbir CRUD yetkisi verme.

## Response ve hata davranışı

- Başarılı GET: `200 OK`
- Başarılı create: `201 Created` ve mümkünse `Location`
- Başarılı update: `200 OK` veya `204 No Content`
- Başarılı delete: `204 No Content`
- Geçersiz input: `400 Bad Request` (Validation ProblemDetails)
- Authentication yok/geçersiz: `401 Unauthorized`
- Rol/yetki yetersiz: `403 Forbidden`
- Bulunamayan kaynak: `404 Not Found`
- İş kuralı/rezervasyon çakışması: uygun `409 Conflict`
- Rate limit: `429 Too Many Requests`
- Beklenmeyen hata: güvenli `500` ProblemDetails; stack trace veya secret sızdırma yok

## Kısıtlar

- Entity'leri API response olarak doğrudan döndürme.
- Controller içine business logic koyma.
- `IServiceProvider` üzerinden servisleri runtime'da çözümleyerek DI'ı atlatma.
- Parolaları, JWT secret'larını veya connection string secret'larını source control'e koyma.
- Admin rolünü public register request'inden kabul etme.
- CORS'u herkese açma; `AllowAnyOrigin` ile credentials kullanma.
- SQL/DB portunu internete açma.
- Kullanıcı istemeden gerçek dışı alan/işletme kuralı uydurma; belirsizleri varsayım olarak belirt.
- İşlem tamamlanmadıysa tamamlandı deme.
