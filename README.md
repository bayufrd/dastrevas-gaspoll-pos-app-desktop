🖥️ #Gaspoll POS Application

Aplikasi Point of Sale (POS) berbasis Windows (.NET Framework) yang dikembangkan secara khusus untuk memenuhi kebutuhan customisasi client PT. Gaspoll Management Center
👉 https://gaspollmanagementcenter.com

Dikembangkan oleh:
#Dastrevas Team
👉 https://dastrevas.com

✨ Fitur Utama

🔹 Support Bluetooth Printer & In The Hand Bluetooth Library

🔹 Integrasi dengan API Service (online & offline-first sync)

🔹 Dual Monitor Display (kasir + customer screen)

🔹 FontAwesome Icons untuk tampilan UI modern

🔹 Logger Service untuk debugging dan monitoring

🔹 Modular Architecture dengan customisasi sesuai kebutuhan klien

🛠️ Teknologi

Bahasa: C# (.NET Framework)

Database: Local + API Service

UI Toolkit: Windows Forms + FontAwesome

Dependencies:

Logger (custom)

InTheHand Bluetooth

API Service Handler

FontAwesome.Sharp

Dual Monitor Support

📌 Standar Commit Message

Proyek ini menggunakan custom commit convention untuk menjaga konsistensi riwayat perubahan.

📑 Format
(```)[TYPE] Deskripsi singkat perubahan

📂 Jenis Commit

(```)[UPDATE] → Penambahan atau perubahan fitur

(```)[UPDATE] Tambah fitur login menggunakan kartu member


(```)[FIX] → Perbaikan bug/error

(```)[FIX] Perbaiki error saat print struk di monitor 2


(```)[DELETE] → Penghapusan kode/file yang tidak dipakai

(```)[DELETE] Hapus modul laporan lama yang tidak relevan


(```)[REFACTOR] → Perubahan struktur kode tanpa mengubah fitur

(```)[REFACTOR] Optimasi query transaksi untuk performa

📦 Struktur Repo (contoh)
/POS-App
  /Helper
  /Model
  /Services
  /UI
  /Logger
  /Api
  README.md

👥 Kontribusi

Semua kontribusi internal dilakukan oleh tim pengembang Dastrevas sesuai arahan dan standar PT. Gaspoll Management Center.

📜 Lisensi

Proyek ini bersifat proprietary dan hanya digunakan oleh klien resmi PT. Gaspoll Management Center.
Segala bentuk distribusi tanpa izin dilarang.