# Giải thích LINQ và các điểm cần nắm khi bảo vệ

## 1. Công thức quan trọng nhất
```
SoChoCon = SoNguoiToiDa (của LoaiPhong) − số PhanPhong có TrangThai = "Đang ở" trong phòng
```
**Vì sao không lưu `SoChoCon` cho Admin nhập?** Vì đó là dữ liệu *suy ra được*. Nếu lưu riêng, nó sẽ lệch với dữ liệu thật mỗi khi
phân/chuyển/trả phòng (hoặc khi ai đó nhập sai) → phòng có thể bị xếp quá sức chứa hoặc báo "hết chỗ" sai. Tính từ `PhanPhong` thì luôn đúng.

Vị trí code: `Services/PhongService.cs` → `QueryPhong()`:
```csharp
_db.PhongKTXs.Select(p => new PhongItemVM {
    SucChua  = p.LoaiPhong.SoNguoiToiDa,
    SoDangO  = p.PhanPhongs.Count(pp => pp.TrangThai == TrangThaiPhanPhong.DangO),
    SoChoCon = p.LoaiPhong.SoNguoiToiDa - p.PhanPhongs.Count(pp => pp.TrangThai == TrangThaiPhanPhong.DangO)
});
```
Kết quả là `IQueryable` nên có thể nối tiếp `Where/OrderBy/Skip/Take` và EF Core dịch tất cả thành **một câu SQL**.

## 2. Kiểm tra phòng đầy / còn chỗ
- Phòng đầy: `soDangO >= phong.LoaiPhong.SoNguoiToiDa` (`PhanPhongService.PhanPhongAsync`, `PhongService.PhongDayAsync`).
- Danh sách phòng chọn được: `QueryPhongConCho()` = `Where(TrangThai == "Đang sử dụng" && SoChoCon > 0)`.
- Sinh viên đã có phòng: `PhanPhongs.AnyAsync(pp => pp.TrangThai == "Đang ở" && pp.DangKyKTX.MaSinhVien == maSV)`.

## 3. Danh sách phòng: Tìm kiếm → Lọc → Sắp xếp → Phân trang
File `Controllers/PhongKTXController.cs`, action `Index`. Thứ tự xử lý:
1. `QueryPhong()` – truy vấn gốc (chưa chạy SQL).
2. **Where** tìm kiếm: `TenPhong.Contains(k) || ToaNha.Contains(k)`.
3. **Where** lọc: loại phòng, tòa, tầng, giới tính, trạng thái, còn/hết chỗ (`SoChoCon > 0` / `<= 0`).
4. **OrderBy / OrderByDescending** (+ `ThenBy` để thứ tự ổn định giữa các trang).
5. **Skip/Take** trong `PagedResult<T>.CreateAsync` (đếm tổng bằng `CountAsync` rồi lấy đúng một trang).
6. Hiển thị. Form lọc dùng phương thức GET nên mọi điều kiện nằm trên query string; `Views/Shared/_Pager.cshtml` giữ nguyên các tham số này khi tạo link chuyển trang.

Đổi số bản ghi mỗi trang: sửa hằng `PageSize` (đầu `PhongKTXController`). Thêm điều kiện lọc: thêm thuộc tính vào `PhongFilterVM`, thêm một `if (...) q = q.Where(...)` và một ô `<select>` trong `Views/PhongKTX/Index.cshtml`.

## 4. Thống kê (`ThongKeController`)
| Yêu cầu | LINQ |
|---|---|
| Số phòng theo loại | `GroupBy(p => p.LoaiPhong.TenLoaiPhong)` + `Count()` |
| SV đang ở / chỗ còn / tỷ lệ từng phòng | `QueryPhong().OrderByDescending(p => p.SoDangO)` |
| SV theo tòa | `PhanPhongs.Where(Đang ở).GroupBy(pp => pp.PhongKTX.ToaNha)` |
| SV theo loại phòng | `GroupBy(pp => pp.PhongKTX.LoaiPhong.TenLoaiPhong)` |
| Đăng ký theo trạng thái | `GroupBy(d => d.TrangThai)` |
| Phòng đông nhất | `Max(p => p.SoDangO)` rồi `Where(p => p.SoDangO == max)` |
| Tổng số chỗ | `PhongKTXs.Where(Đang sử dụng).SumAsync(p => p.LoaiPhong.SoNguoiToiDa)` |
| Chỗ đang sử dụng | `PhanPhongs.CountAsync(pp => pp.TrangThai == "Đang ở")` |
| SV vào KTX theo tháng | `GroupBy(pp => pp.MaDangKy).Select(g => g.Min(NgayVao))` rồi nhóm theo (Năm, Tháng) |

## 5. Các câu hỏi hay gặp và hướng trả lời
- **Làm sao một sinh viên không thể có hai phòng "Đang ở"?** Ba lớp: (1) service kiểm tra `AnyAsync` trước khi phân; (2) transaction mức Serializable + kiểm tra lại ngay trước `SaveChanges`; (3) chỉ mục duy nhất có điều kiện `MaDangKy WHERE TrangThai = 'Đang ở'` ở CSDL.
- **Vì sao chuyển phòng phải tạo bản ghi mới?** Để giữ lịch sử: bản ghi cũ thành "Đã chuyển" + `NgayRa`, bản ghi mới "Đang ở" → biết SV từng ở phòng nào, khi nào, chuyển bao nhiêu lần. Sửa thẳng `MaPhong` sẽ mất dấu phòng cũ.
- **Vì sao tách `DangKyKTX` và `PhanPhong`?** Đăng ký là *yêu cầu xin ở* (có thể bị hủy), `PhanPhong` là *sự việc ở thực tế*. Không đặt `MaPhong` trong `SinhVien`.
- **Phân quyền thực hiện ở đâu?** `Filters/PhanQuyenAttribute.cs`, gắn trên Controller/Action. Filter đọc `MaTaiKhoan` từ Session rồi tra lại tài khoản trong DB (vai trò + trạng thái khóa); vai trò không bao giờ lấy từ Form/URL. Ẩn menu chỉ là giao diện, không phải bảo mật.
- **Sinh viên không xem được dữ liệu người khác thế nào?** Không Action nào của sinh viên nhận mã sinh viên; mã được lấy từ tài khoản đăng nhập (`LaySinhVienHienTaiAsync`). Với `DangKyKTX/Details/{id}` có kiểm tra `dangKy.MaSinhVien == sv.MaSinhVien`, sai thì trả 404.
- **Mật khẩu lưu thế nào?** Băm bằng `PasswordHasher<TaiKhoan>` (không lưu mật khẩu thô); đăng nhập dùng `VerifyHashedPassword`.
- **Code First và Migration là gì?** Viết Entity + `AppDbContext` trước; `dotnet ef migrations add` sinh mã tạo/sửa bảng; `dotnet ef database update` (hoặc `MigrateAsync`) áp dụng vào SQL Server.
- **Model Binding / Data Annotation / Validation:** form được ánh xạ vào Model/ViewModel; `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, `[RegularExpression]` kiểm tra đơn giản; quy tắc phụ thuộc DB (trùng tên, còn chỗ...) kiểm tra bằng LINQ trong Controller/Service rồi `ModelState.AddModelError`.
- **`ValidateAntiForgeryToken`:** chống CSRF cho mọi thao tác POST.
