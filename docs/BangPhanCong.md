# Bảng phân công công việc

> Điền thông tin thật của nhóm. Nội dung phải khớp với phần chú thích đầu file mã nguồn và lịch sử Commit trên GitHub (đề mục 17, 20, 22.1).

| SV | Họ và tên | Mã sinh viên | Module | Controller / View / Model chính | Chức năng thực hiện | Commit tiêu biểu |
|---|---|---|---|---|---|---|
| 1 | | | 1 | `TaiKhoanController`, `LoaiPhongController`, `PhanQuyenAttribute`, `Models/TaiKhoan`, `Models/LoaiPhong`, `Views/TaiKhoan`, `Views/LoaiPhong` | Tài khoản, đăng nhập/đăng xuất, Session, phân quyền, CRUD loại phòng | |
| 2 | | | 2 | `PhongKTXController`, `PhongService`, `PagedResult`, `Models/PhongKTX`, `Views/PhongKTX` | CRUD phòng, tìm kiếm, lọc, sắp xếp, phân trang, tính số chỗ còn | |
| 3 | | | 3 | `SinhVienController`, `DangKyKTXController`, `CaNhanController`, `Models/SinhVien`, `Models/DangKyKTX`, `Views/SinhVien`, `Views/DangKyKTX`, `Views/CaNhan` | Quản lý sinh viên, đăng ký KTX, xem thông tin/lịch sử ở | |
| 4 | | | 4 | `PhanPhongController`, `PhanPhongService`, `ThongKeController`, `Models/PhanPhong`, `Views/PhanPhong`, `Views/ThongKe` | Duyệt/phân phòng, chuyển phòng, trả phòng, Dashboard, thống kê | |

## Mẫu Commit
`[Mã SV] [Module] Nội dung công việc` — ví dụ: `[22103100001] [PhongKTX] Them chuc nang phan trang`

## Quy trình làm việc nhóm (đề mục 20)
Thiết kế CSDL → thống nhất Entity → tạo Project → DbContext/Migration → phân chia Module → code → commit/push → pull/tích hợp → kiểm thử → hoàn thiện.
Không dồn toàn bộ công việc vào một Commit cuối.
