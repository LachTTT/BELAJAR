# CineBook - Product

## Ringkasan
CineBook adalah REST API untuk sistem pemesanan tiket bioskop. Klien dapat menjelajah daftar film, jadwal tayang di theater, memilih kursi yang tersedia, dan membuat booking dengan status PENDING/SUCCESS/CANCELLED.

## Domain Utama
- **Movie** — informasi film (judul, deskripsi, durasi).
- **Theater** — bioskop/studio dengan nama dan lokasi.
- **Schedule** — jadwal tayang film di theater tertentu, dengan waktu mulai dan harga.
- **Seat** — kursi per Schedule, memiliki nomor kursi dan flag ketersediaan.
- **Booking** — transaksi pemesanan oleh user untuk satu Schedule; berisi total tiket, total harga, dan status.
- **BookingSeat** — pivot antara Booking dan Seat (banyak kursi per booking).
- **User** — akun pemesan (Email unik, Nama, Password).

## Cakupan & Batasan Fungsional
- **Autentikasi memakai JWT Bearer token** (diperbarui Day 5).
- Endpoint `POST /api/auth/login` dibuat tetapi **TIDAK** diberi `[Authorize]`.
- Endpoint yang butuh identitas user **WAJIB** `[Authorize]`; `UserId` diambil dari klaim token (`User.FindFirst("userId")`).
- Password disimpan sebagai string biasa di kolom (placeholder). Hashing bukan bagian dari cakupan saat ini.

## Data Uji
Setelah migration dijalankan, database sengaja **kosong**. Data uji untuk testing diisi manual lewat file SQL terpisah (`seed-data-manual.sql`) yang dijalankan via pgAdmin/psql. **Tidak** ada seeding otomatis di migration atau `OnModelCreating`.
