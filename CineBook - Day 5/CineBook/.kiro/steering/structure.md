# CineBook - Arsitektur & Struktur Proyek

Ikuti aturan berikut PERSIS. Jangan menyimpang tanpa diminta.

## Layered Architecture (4 Lapis)

```
Controller  →  IService  →  IRepository<T>  →  DbContext
```

1. **Controller** hanya boleh memanggil method dari `IService` yang di-inject via constructor. Controller **TIDAK PERNAH** mengakses `DbContext` secara langsung.
2. **Service** memegang seluruh business logic (validasi, aturan bisnis, orkestrasi). Service memanggil `IRepository<T>` untuk operasi data.
3. **Repository bersifat GENERIC** — satu class `Repository<T>` untuk semua entity, diakses melalui `IRepository<T>`. **JANGAN** membuat repository spesifik per entity (mis. `MovieRepository`, `BookingRepository`).
4. **Repository HANYA berisi operasi data generik** (`Get`, `GetAll`, `Add`, `Update`, `Delete`, `SaveChanges`). **JANGAN** menaruh business logic di Repository.
5. `DbContext` hanya diakses dari Repository.

## Aturan Persistensi
- `SaveChanges()` / `SaveChangesAsync()` **HANYA** boleh dipanggil dari dalam **Service**.
- **JANGAN** memanggil `SaveChanges` dari Controller.
- **JANGAN** memanggil `SaveChanges` otomatis di dalam method Repository lain (mis. `Add`, `Update`, `Delete` tidak boleh SaveChanges sendiri).

## Dependency Injection
- Semua service didaftarkan di `Program.cs` lewat **interface**-nya:
  `services.AddScoped<IXxxService, XxxService>();`
- Sama untuk repository: `services.AddScoped(typeof(IRepository<>), typeof(Repository<>));`
- **JANGAN** mendaftarkan class konkret langsung tanpa interface.

## DTO
- **Semua** request/response body di Controller memakai **DTO**.
- **JANGAN** mengekspos entity `CineBook.Models` langsung ke client.
- Property DTO memakai **camelCase otomatis** dari serializer default. **JANGAN** menambahkan `JsonNamingPolicy` custom, dan **JANGAN** memakai snake_case.

## Namespace & Folder
- `CineBook.Models` — entity di folder `Models/`
- `CineBook.Data` — `DbContext` di folder `Data/`
- `CineBook.Repositories` — di folder `Repositories/`
- `CineBook.Services` — di folder `Services/`
- `CineBook.DTOs` — di folder `DTOs/`
- `CineBook.Controllers` — di folder `Controllers/`
- `CineBook.Exceptions` — di folder `Exceptions/`
- `CineBook.Middlewares` — di folder `Middlewares/`

## Autentikasi (Diperbarui Day 5 — lihat juga aturan #14)
- Autentikasi memakai **JWT Bearer token**.
- Endpoint `POST /api/auth/login` **TIDAK** diberi `[Authorize]`.
- Endpoint yang butuh identitas user **WAJIB** `[Authorize]`, `UserId` diambil dari klaim token (`User.FindFirst("userId")`).
- **JANGAN** hardcode `UserId` lagi.

## Migration & Seeding
- **DILARANG** memakai `HasData` atau seeding otomatis apapun di `OnModelCreating` maupun di dalam file migration.
- `OnModelCreating` **HANYA** berisi konfigurasi relasi antar entity (Fluent API `HasOne/WithMany/HasForeignKey`), unique index, dan konversi tipe data (mis. enum → string).
- Database sengaja dibiarkan **kosong** setelah `database update`. Data uji dimasukkan manual via `seed-data-manual.sql`.

## Transaksi Eksplisit (Day 5)

12. `Repository<T>` / `IRepository<T>` sekarang punya 3 method tambahan untuk transaksi eksplisit: `BeginTransactionAsync()`, `CommitTransactionAsync()`, `RollbackTransactionAsync()`. Transaksi menempel ke `DbContext` (bukan ke Repository instance tertentu) — karena semua `Repository<T>` dan `DbContext` sama-sama Scoped, transaksi yang dimulai lewat satu Repository otomatis berlaku untuk operasi lewat Repository lain dalam request yang sama.

13. Operasi yang menyentuh **LEBIH DARI SATU tabel** sekaligus (seperti booking: Seat + Booking + BookingSeat) **WAJIB** dibungkus `BeginTransactionAsync` → operasi → `SaveChangesAsync` (SEKALI di akhir, bukan di tengah loop) → `CommitTransactionAsync`, dengan `RollbackTransactionAsync` di blok catch.

## Autentikasi JWT (Day 5 — menggantikan bagian "Di Luar Cakupan")

14. Autentikasi memakai **JWT Bearer token**. Endpoint `POST /api/auth/login` **TIDAK** diberi atribut `[Authorize]`. Semua endpoint lain yang butuh identitas user (seperti `POST /api/bookings`) **WAJIB** diberi `[Authorize]`, dan `UserId` diambil dari klaim token (`User.FindFirst("userId")`) — **JANGAN** hardcode `UserId` lagi.

## Custom Exceptions (Day 5)

15. Custom exception dibuat di folder `Exceptions/`, namespace `CineBook.Exceptions`, inherit dari `System.Exception`, dengan constructor yang membentuk message informatif otomatis. Service **TIDAK BOLEH** lagi return `null` untuk menandakan kegagalan bisnis (seperti kursi tidak tersedia) — **WAJIB** throw custom exception yang sesuai, supaya Controller bisa menangkap secara spesifik dan mengembalikan status code yang tepat.

16. Controller yang memanggil Service yang bisa throw custom exception **WAJIB** membungkusnya dengan try-catch: tangkap custom exception **SPESIFIK** dulu (paling atas), **JANGAN** tambahkan `catch (Exception ex)` generik di Controller — biarkan exception tak terduga lolos ke Global Exception Middleware.

## Global Exception Middleware (Day 5)

17. Tambahkan folder `Middlewares/`, namespace `CineBook.Middlewares`, untuk custom middleware. `GlobalExceptionMiddleware` **WAJIB** didaftarkan **PALING PERTAMA** di pipeline `Program.cs` (sebelum `UseCors`/`UseAuthentication`/`UseAuthorization`), supaya bisa menangkap error dari middleware lain juga.

18. Urutan middleware **WAJIB** persis: `UseMiddleware<GlobalExceptionMiddleware>` → `UseCors` → `UseAuthentication` → `UseAuthorization` → `MapControllers`.

## Logging (Day 5)

19. Semua Service yang butuh logging di-inject `ILogger<NamaService>` lewat constructor (sama seperti inject `IRepository<T>`) — **JANGAN** pakai `Console.WriteLine` untuk logging apapun.

20. Logging framework: **Serilog**, dikonfigurasi di `Program.cs` lewat `builder.Host.UseSerilog(...)`, output ke Console dan File (`logs/cinebook-.txt`, rolling per hari). Konfigurasi ini **TIDAK** mengubah cara pemanggilan `_logger.LogXxx()` di Service manapun.

## Cara Kerja Kolaborasi
Prompt akan diberikan bertahap per layer (Models → Data → Repositories → Services → DTOs → Controllers → Program.cs). Ikuti struktur dan **signature method** yang diberikan **PERSIS**:
- Jangan mengganti nama method.
- Jangan menambah/mengurangi parameter tanpa diminta.
- Jangan mengubah nama field, tipe data, atau atribut entity dari yang diberikan.
