# 🖥️ Gaspoll POS Application

Aplikasi Point of Sale (POS) berbasis Windows (.NET 6.0 WinForms) dengan dukungan offline mode, dual monitor, serta integrasi API untuk kebutuhan bisnis retail.  
Dikembangkan khusus untuk customisasi client **PT. Gaspoll Management Center**  
👉 https://gaspollmanagementcenter.com

Dikembangkan oleh:  
**Dastrevas Team**  
👉 https://dastrevas.com

---

## ✨ Fitur Utama

- 🔹 **Offline Mode**: transaksi tetap berjalan tanpa internet, data akan sinkron otomatis saat online.  
- 🔹 **Bluetooth Printer Support** (32feet.NET + ESCPOS_NET).  
- 🔹 **Dual Monitor Display** (kasir + pelanggan).  
- 🔹 **Entity Framework Core (SQLite)** untuk database lokal.  
- 🔹 **API Service Integration** dengan retry & resiliency (Polly).  
- 🔹 **FontAwesome.Sharp** untuk ikon modern dan konsisten.  
- 🔹 **Logging dengan Serilog** (Console & File Sink) untuk debugging.  
- 🔹 **Modular Forms & Komponen** untuk kemudahan customisasi.  
- 🔹 **Kompresi Data** (SharpCompress) untuk efisiensi penyimpanan.  

---

## 🛠️ Teknologi & Dependensi

### Bahasa & Framework
- C#  
- .NET 6.0 (Windows)  
- Windows Forms (WinForms)  

### Paket NuGet
- 32feet.NET → Bluetooth library  
- ESCPOS_NET → ESC/POS printer support  
- FontAwesome.Sharp → ikon modern  
- Microsoft.EntityFrameworkCore.Sqlite → database SQLite lokal  
- Microsoft.EntityFrameworkCore.Tools → tooling EF Core  
- Microsoft.Extensions.Configuration → konfigurasi (JSON, dll)  
- Newtonsoft.Json → serialisasi JSON  
- Polly → retry & resiliency untuk API call  
- Serilog + Serilog.Sinks.Console + Serilog.Sinks.File → logging terstruktur  
- SharpCompress → kompresi file/data  
- System.Management → informasi sistem Windows  
- System.Net.Http → komunikasi HTTP  

---

## 📂 Struktur Aplikasi

### Offline Mode Forms
- Offline_MemberCustom  
- Offline_MemberData  
- Offline_notifikasiPengeluaran  
- Offline_Complaint  
- Offline_listBill  
- Offline_saveBill  
- Offline_HistoryShift  
- Offline_settingsForm  
- Offline_splitBill  
- Offline_refund  
- Offline_inputPin  
- Offline_successTransaction  
- Offline_updatePerItemForm  
- Offline_deletePerItemForm  
- Offline_dataDiskon  
- Offline_updateCartForm  
- Offline_deleteForm  
- Offline_payForm  
- Offline_addCartForm  
- Offline_masterPos  
- Offline_shiftReport  

### Komponen & Utilities
- SettingsConfig  
- SettingsDual  
- shiftReport  
- Logger (Serilog)  
- API Service Handler  

### Struktur Repo (contoh)
```
/POS-App
  /Helper
  /Model
  /Services
  /UI
  /Logger
  /Api
  /OffineMode
  /Komponen
  /Properties
  README.md
```

---

## 📌 Standar Commit Message

Proyek ini menggunakan **custom commit convention** untuk menjaga konsistensi riwayat perubahan.

### 📑 Format
```
[TYPE] Deskripsi singkat perubahan
```

### 📂 Jenis Commit
- **[UPDATE]** → Penambahan atau perubahan fitur  
  - `[UPDATE] Tambah fitur login menggunakan kartu member`  
- **[FIX]** → Perbaikan bug/error  
  - `[FIX] Perbaiki error saat print struk di monitor 2`  
- **[DELETE]** → Penghapusan kode/file yang tidak dipakai  
  - `[DELETE] Hapus modul laporan lama yang tidak relevan`  
- **[REFACTOR]** → Perubahan struktur kode tanpa mengubah fitur  
  - `[REFACTOR] Optimasi query transaksi untuk performa`  

---

## ⚙️ Cara Build & Run

### 📌 Prasyarat
- Windows 10/11  
- Visual Studio 2022 atau lebih baru  
- .NET 6.0 SDK → [Download di sini](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)  

### 📦 Build via Visual Studio
1. Clone repository:  
   ```bash
   git clone https://github.com/<username>/<repo-name>.git
   ```
2. Buka file `.sln` di Visual Studio.  
3. Pilih konfigurasi **Release** atau **Debug**.  
4. Tekan `Ctrl + F5` untuk menjalankan aplikasi.  

### 📦 Build via .NET CLI
1. Clone repository:  
   ```bash
   git clone https://github.com/<username>/<repo-name>.git
   cd <repo-name>
   ```
2. Restore dependencies:  
   ```bash
   dotnet restore
   ```
3. Build:  
   ```bash
   dotnet build
   ```
4. Jalankan:  
   ```bash
   dotnet run --project POS-App
   ```

---

## 🚀 Deployment

1. Pastikan konfigurasi `app.manifest` sudah sesuai environment (printer, dual monitor, API URL).  
2. Build aplikasi dalam mode Release.  
3. Gunakan **Publish** dari Visual Studio untuk menghasilkan installer/exe.  
4. Distribusikan installer ke mesin kasir yang sudah memenuhi requirement.  

---

## 🐞 Troubleshooting

- **Masalah Printer Bluetooth tidak terdeteksi**  
  Pastikan driver printer terinstall dan perangkat sudah dipairing di Windows.  

- **Dual Monitor tidak tampil**  
  Cek konfigurasi monitor di `SettingsDual` dan pastikan mode extended display aktif.  

- **API Service gagal sync**  
  Periksa URL API di konfigurasi dan koneksi internet. Gunakan log Serilog untuk debugging.  

---

## 👥 Kontribusi

Semua kontribusi internal dilakukan oleh tim pengembang **Dastrevas** sesuai arahan dan standar **PT. Gaspoll Management Center**.

---

## 📜 Lisensi

Proyek ini bersifat **proprietary** dan hanya digunakan oleh klien resmi **PT. Gaspoll Management Center**.  
Segala bentuk distribusi tanpa izin **dilarang**.
