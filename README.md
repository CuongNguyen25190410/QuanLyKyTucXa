# Hệ thống quản lý ký túc xá và phân phòng sinh viên

ASP.NET Core 10 MVC · Entity Framework Core 10 (Code First + Migration) · SQL Server · LINQ · Session

> Tên Project/Solution/Namespace: `QuanLyKyTucXa_UNETI_DHTI17A2ND`. Nếu cần đổi lại, đổi tên thư mục, file `.sln`, `.csproj` rồi dùng Find & Replace trong Visual Studio để thay namespace.

## 1. Yêu cầu môi trường
- .NET 10 SDK
- SQL Server (LocalDB đi kèm Visual Studio là đủ) hoặc SQL Server Express
- Công cụ EF: `dotnet tool install --global dotnet-ef` (chạy lại `dotnet tool update --global dotnet-ef` nếu đã có bản cũ)
- Cần Internet khi chạy để tải Bootstrap/jQuery từ CDN (giao diện + validate phía client)

## 2. Cài đặt và chạy
1. Mở `appsettings.json`, sửa `ConnectionStrings:DefaultConnection` cho đúng SQL Server của máy bạn.
   Mặc định: `Server=(localdb)\MSSQLLocalDB;Database=QuanLyKyTucXa_UNETI_DHTI17A2ND;...`
2. **Tạo Migration (làm một lần, rồi commit thư mục `Migrations/` lên GitHub):**
   ```bash
   cd QuanLyKyTucXa_UNETI_DHTI17A2ND

   dotnet ef migrations add KhoiTaoCoSoDuLieu

   dotnet ef migrations add ThemBangHoaDon
   
   dotnet ef database update
   ```
3. Chạy: `dotnet run` (hoặc F5 trong Visual Studio) rồi mở địa chỉ hiển thị (mặc định http://localhost:5080).

Khi ứng dụng khởi động, `Program.cs` tự gọi `Database.MigrateAsync()` (nếu đã có Migration) và nạp dữ liệu mẫu nếu
Database chưa có tài khoản nào (`Data/DbSeeder.cs`).
   ```bash

> ⚠️ Hãy tạo Migration (bước 2) **trước lần chạy đầu tiên**. Nếu chạy khi chưa có Migration, ứng dụng dùng
> `EnsureCreated` để dự phòng; lúc đó muốn chuyển sang Migration phải xóa Database cũ (`dotnet ef database drop`) rồi làm lại bước 2.
>
> Khi sau này thay đổi Entity: tạo Migration **mới** có tên thể hiện nội dung (ví dụ `ThemCotGhiChuPhanPhong`),
> không xóa lịch sử Migration (đề mục 10.1).
   ```

4. **Thao tác trên git:( nhập lệnh trên cmd or terminal trong thư mục muốn lưu)**
   ```bash
      >>>tải code về : 
                          git clone https://github.com/CuongNguyen25190410/QuanLyKyTucXa.git
      >>>cập nhất code mới nhất từ git về máy : 
                          git pull origin main
      >>>cập nhật từ máy lên git : 
                          git add .
                          git commit -m "nội dung cập nhật "
                          git pull --rebase origin main
                          git push origin main
   ```
 
                        

## 3. Tài khoản kiểm thử (dữ liệu mẫu)
| Vai trò | Tên đăng nhập | Mật khẩu |
|---|---|---|
| Admin | `admin`, `admin2` | `Admin@123` |
| Sinh viên | `22103100001` … `22103100029` | `Sv@123456` |
| Sinh viên bị khóa | `22103100030` | `Sv@123456` (đăng nhập sẽ báo tài khoản bị khóa) |

Một số sinh viên hữu ích khi thử nghiệm:
- `22103100001`–`04` Nam, đang ở **A101 (phòng đầy)**.
- `22103100005`–`07` Nam, đang ở **A102 (còn đúng 1 chỗ)**.
- `22103100013` Nam, **đã chuyển phòng** A103 → A201 (xem lịch sử).
- `22103100014`, `22103100028`: **đã trả phòng**.
- `22103100015`, `22103100016` (Nam), `22103100029` (Nữ): đăng ký **Chờ duyệt** → dùng để thử phân phòng.
- Phòng **A302, B302 tạm ngừng**; phòng đầy: **A101, B101, B203**; còn 1 chỗ: **A102, B102**.

## 4. Cấu trúc thư mục
```
Constants/      Hằng số trạng thái dùng chung (VaiTro, TrangThaiPhong, TrangThaiDangKy, ...)
Models/         6 Entity: TaiKhoan, LoaiPhong, PhongKTX, SinhVien, DangKyKTX, PhanPhong
Models/ViewModels/  ViewModel cho từng màn hình
Data/           AppDbContext (cấu hình quan hệ, chỉ mục), DbSeeder (dữ liệu mẫu)
Services/       PhongService (LINQ tính số chỗ còn), PhanPhongService (phân/chuyển/trả phòng)
Filters/        PhanQuyenAttribute (kiểm tra đăng nhập + phân quyền ở Controller)
Helpers/        PagedResult (phân trang), Fmt (định dạng ngày/tiền), NgaySinhHopLe
Controllers/    TaiKhoan, LoaiPhong, PhongKTX, SinhVien, DangKyKTX, PhanPhong, ThongKe, CaNhan, Home
Views/          Razor View theo từng Controller
docs/           Thiết kế CSDL, kịch bản kiểm thử, giải thích LINQ, bảng phân công
```

## 5. Phân chia module 
| Module | Nội dung | File chính |
|---|---|---|
| 1 | Tài khoản, đăng nhập, phân quyền, loại phòng | `TaiKhoanController`, `LoaiPhongController`, `PhanQuyenAttribute`, `Models/TaiKhoan`, `Models/LoaiPhong` |
| 2 | Quản lý phòng, tìm kiếm, lọc, sắp xếp, phân trang | `PhongKTXController`, `PhongService`, `PagedResult`, `Views/PhongKTX/*` |
| 3 | Sinh viên, đăng ký KTX, thông tin/lịch sử ở | `SinhVienController`, `DangKyKTXController`, `CaNhanController` |
| 4 | Phân phòng, chuyển/trả phòng, Dashboard, thống kê | `PhanPhongController`, `PhanPhongService`, `ThongKeController` |

