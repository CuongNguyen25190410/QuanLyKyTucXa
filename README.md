# Hệ thống quản lý ký túc xá và phân phòng sinh viên

ASP.NET Core 10 MVC · Entity Framework Core 10 (Code First + Migration) · SQL Server · LINQ · Session

> Tên Project/Solution/Namespace hiện tại: `QuanLyKyTucXa_UNETI_DHTI17A2ND`.
> Theo đề (mục 3.1) tên phải có dạng `QuanLyKyTucXa_UNETI[STT nhóm]_[MãLớp]`. Nếu đổi tên, hãy đổi đồng bộ thư mục, file `.sln`, `.csproj`
> và namespace (Find & Replace trong Visual Studio).

## 1. Yêu cầu môi trường
- .NET 10 SDK
- SQL Server (LocalDB đi kèm Visual Studio là đủ) hoặc SQL Server Express
- Công cụ EF Core: `dotnet tool install --global dotnet-ef` (nếu đã có bản cũ: `dotnet tool update --global dotnet-ef`)
- Internet khi chạy để tải Bootstrap/jQuery từ CDN (giao diện + validate phía client)
- Các package (EF Core SqlServer 10, ClosedXML) đã khai báo trong `.csproj`, `dotnet restore` sẽ tự tải, **không cần** `dotnet add package`.

## 2. Cài đặt và chạy (sau khi clone)
1. Mở `QuanLyKyTucXa_UNETI_DHTI17A2ND/appsettings.json`, sửa `ConnectionStrings:DefaultConnection` cho đúng SQL Server của máy.
   Mặc định: `Server=(localdb)\MSSQLLocalDB;Database=QuanLyKyTucXa_UNETI_DHTI17A2ND;Trusted_Connection=True;...`
2. Tạo Database từ Migration có sẵn trong thư mục `Migrations/`:
   ```bash
   cd QuanLyKyTucXa_UNETI_DHTI17A2ND
   dotnet restore
   dotnet ef database update
   ```
3. Chạy: `dotnet run` (hoặc F5 trong Visual Studio), rồi mở http://localhost:5080 (hoặc https://localhost:7080).

Khi khởi động, `Program.cs` tự gọi `Database.MigrateAsync()` và nạp dữ liệu mẫu nếu Database chưa có tài khoản nào
(`Data/DbSeeder.cs`, sau đó `Data/PhongMauSeeder.cs` bổ sung thêm phòng, tổng khoảng 210 phòng để có nhiều trang). Vì vậy bước 2 có thể bỏ qua, ứng dụng vẫn tự tạo Database.

> **Không chạy lại** `dotnet ef migrations add ...` cho các migration đã có (`KhoiTaoCoSoDuLieu`, `ThemBangHoaDon`, `ThemBangDonHuyPhong`,
> `ThemNguyenVongPhongVaoDangKy`) vì sẽ sinh migration thừa hoặc báo trùng tên. Migration đã được commit cùng mã nguồn.
>
> Khi **thay đổi Entity** về sau, tạo migration **mới** có tên thể hiện nội dung, ví dụ:
> ```bash
> dotnet ef migrations add ThemCotGhiChuPhanPhong
> dotnet ef database update
> ```
> Không xóa lịch sử Migration (đề mục 10.1). Muốn làm lại từ đầu trên máy cá nhân: `dotnet ef database drop` rồi `dotnet ef database update`.

### Cấu hình thanh toán (chức năng nâng cao, không bắt buộc)
- Mặc định `ThanhToan:NhaCungCap = "Mock"`: thanh toán giả lập để demo (sinh viên có nút "giả lập thanh toán").
- Muốn dùng payOS thật: đặt `ThanhToan:NhaCungCap = "PayOS"` và điền `PayOS:ClientId/ApiKey/ChecksumKey` trong
  `appsettings.Development.json` (file này đã nằm trong `.gitignore`, **không commit khóa lên GitHub**).

## 3. Tài khoản kiểm thử (dữ liệu mẫu)
| Vai trò | Tên đăng nhập | Mật khẩu |
|---|---|---|
| Admin | `admin`, `admin2` | `Admin@123` |
| Sinh viên | `22103100001` … `22103100029` | `Sv@123456` |
| Sinh viên bị khóa | `22103100030` | `Sv@123456` (đăng nhập sẽ báo tài khoản bị khóa) |

Một số sinh viên hữu ích khi thử nghiệm:
- `22103100001`–`04` (Nam): đang ở **A101 (phòng đầy)**.
- `22103100005`–`07` (Nam): đang ở **A102 (còn đúng 1 chỗ)**.
- `22103100013` (Nam): **đã chuyển phòng** A103 → A201 (xem lịch sử).
- `22103100014`, `22103100028`: **đã trả phòng**.
- `22103100015`, `22103100016` (Nam), `22103100029` (Nữ): đăng ký **Chờ duyệt** → dùng để thử phân phòng.
- `22103100030`: tài khoản bị khóa, đăng ký đã hủy.
- Phòng **A302, B302 tạm ngừng**; phòng đầy: **A101, B101, B203**; còn 1 chỗ: **A102, B102**.

## 4. Cấu trúc thư mục
```
Constants/          Hằng số trạng thái dùng chung (VaiTro, TrangThaiPhong, TrangThaiDangKy, ...)
Models/             Entity: TaiKhoan, LoaiPhong, PhongKTX, SinhVien, DangKyKTX, PhanPhong
                    + mở rộng: HoaDon, DonHuyPhong
Models/ViewModels/  ViewModel cho từng màn hình
Data/               AppDbContext (quan hệ, chỉ mục), DbSeeder, PhongMauSeeder (dữ liệu mẫu)
Migrations/         Migration EF Core (phải commit lên GitHub)
Services/           PhongService (LINQ tính số chỗ còn), PhanPhongService (phân/chuyển/trả phòng),
                    DonHuyPhongService, HoaDonService, CongThanhToan, ImportSinhVienService
Filters/            PhanQuyenAttribute (kiểm tra đăng nhập + phân quyền ở Controller)
Helpers/            PagedResult (phân trang), Fmt (định dạng ngày/tiền), NgaySinhHopLe
Controllers/        TaiKhoan, LoaiPhong, PhongKTX, SinhVien, DangKyKTX, PhanPhong, ThongKe, CaNhan, Home
                    + mở rộng: DonHuyPhong, ThanhToan, ImportSinhVien, PayOsWebhook
Views/              Razor View theo từng Controller
docs/               ThietKeCSDL, KichBanKiemThu, GiaiThichLINQ, BangPhanCong
```

## 5. Phân chia module
| Module | Nội dung | File chính |
|---|---|---|
| 1 | Tài khoản, đăng nhập, phân quyền, loại phòng | `TaiKhoanController`, `LoaiPhongController`, `PhanQuyenAttribute`, `Models/TaiKhoan`, `Models/LoaiPhong` |
| 2 | Quản lý phòng, tìm kiếm, lọc, sắp xếp, phân trang | `PhongKTXController`, `PhongService`, `PagedResult`, `Views/PhongKTX/*` |
| 3 | Sinh viên, đăng ký KTX, thông tin/lịch sử ở | `SinhVienController`, `DangKyKTXController`, `CaNhanController` |
| 4 | Phân phòng, chuyển/trả phòng, Dashboard, thống kê | `PhanPhongController`, `PhanPhongService`, `ThongKeController` |

Chức năng nâng cao (không bắt buộc): hóa đơn + thanh toán QR (`ThanhToan`, `HoaDonService`), đơn xin hủy phòng (`DonHuyPhong`),
nhập sinh viên từ Excel (`ImportSinhVien`), sơ đồ phòng (`PhongKTX/SoDo`).
Bảng phân công chi tiết (họ tên, mã sinh viên, commit tiêu biểu) xem `docs/BangPhanCong.md`.

## 6. Làm việc với Git/GitHub
```bash
# Tải code về
git clone https://github.com/CuongNguyen25190410/QuanLyKyTucXa.git

# Cập nhật code mới nhất từ GitHub về máy
git pull origin main

# Đẩy code từ máy lên GitHub
git add .
git commit -m "[Mã SV] [Module] Nội dung công việc"
git pull --rebase origin main
git push origin main
```
Mẫu commit theo đề (mục 20): `[22103100001] [PhongKTX] Them chuc nang phan trang`.
Nếu `git pull --rebase` báo xung đột: sửa file xung đột, `git add <file>`, rồi `git rebase --continue` (hoặc `git rebase --abort` để hủy).
Không để lại ký hiệu `<<<<<<<`, `=======`, `>>>>>>>` trong mã nguồn.