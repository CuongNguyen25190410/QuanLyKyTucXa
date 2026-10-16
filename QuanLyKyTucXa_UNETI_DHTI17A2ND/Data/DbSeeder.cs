// Họ và tên: [ĐIỀN HỌ TÊN CẢ NHÓM]
// Mã sinh viên: [ĐIỀN MÃ CẢ NHÓM]
// Nội dung thực hiện: Thành phần dùng chung của Project (cấu hình, DbContext, dữ liệu mẫu, layout...)
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;

/// <summary>
/// Dữ liệu mẫu theo đề mục 15: 4 loại phòng, 20 phòng, 30 sinh viên, 2 Admin, tài khoản sinh viên,
/// 30 đăng ký KTX, dữ liệu phân phòng có phòng còn nhiều chỗ / còn 1 chỗ / đã đầy / tạm ngừng,
/// có sinh viên đã chuyển phòng, đã trả phòng, đăng ký chờ duyệt và đã hủy.
/// Chỉ nạp khi Database chưa có tài khoản nào.
/// </summary>
public static class DbSeeder
{
    public const string MatKhauAdmin = "Admin@123";
    public const string MatKhauSinhVien = "Sv@123456";

    public static async Task SeedAsync(AppDbContext db, IPasswordHasher<TaiKhoan> hasher)
    {
        if (await db.TaiKhoans.AnyAsync()) return;

        var homNay = DateTime.Today;

        // ------------------------------------------------------------ 1. Tài khoản Admin
        TaiKhoan TaoTaiKhoan(string user, string pass, string hoTen, string email, string vaiTro, string trangThai = TrangThaiTaiKhoan.HoatDong)
        {
            var tk = new TaiKhoan { TenDangNhap = user, HoTen = hoTen, Email = email, VaiTro = vaiTro, TrangThai = trangThai };
            tk.MatKhau = hasher.HashPassword(tk, pass);
            return tk;
        }

        db.TaiKhoans.Add(TaoTaiKhoan("admin", MatKhauAdmin, "Quản trị viên", "admin@uneti.edu.vn", VaiTro.Admin));
        db.TaiKhoans.Add(TaoTaiKhoan("admin2", MatKhauAdmin, "Nguyễn Thị Quản Lý", "quanly@uneti.edu.vn", VaiTro.Admin));

        // ------------------------------------------------------------ 2. Loại phòng
        var lp4 = new LoaiPhong { TenLoaiPhong = "Phòng 4 người", SoNguoiToiDa = 4, DonGiaThang = 600000, MoTa = "Phòng tiêu chuẩn 4 giường tầng, có quạt và tủ cá nhân.", TrangThai = TrangThaiLoaiPhong.HoatDong };
        var lp6 = new LoaiPhong { TenLoaiPhong = "Phòng 6 người", SoNguoiToiDa = 6, DonGiaThang = 450000, MoTa = "Phòng 6 giường tầng, tiết kiệm chi phí.", TrangThai = TrangThaiLoaiPhong.HoatDong };
        var lp8 = new LoaiPhong { TenLoaiPhong = "Phòng 8 người", SoNguoiToiDa = 8, DonGiaThang = 350000, MoTa = "Phòng lớn 8 giường tầng, giá rẻ nhất.", TrangThai = TrangThaiLoaiPhong.HoatDong };
        var lp2 = new LoaiPhong { TenLoaiPhong = "Phòng dịch vụ 2 người", SoNguoiToiDa = 2, DonGiaThang = 1200000, MoTa = "Phòng 2 người, có điều hòa và nhà vệ sinh riêng.", TrangThai = TrangThaiLoaiPhong.HoatDong };
        db.LoaiPhongs.AddRange(lp4, lp6, lp8, lp2);

        // ------------------------------------------------------------ 3. 20 phòng KTX (tên, loại, tòa, tầng, giới tính, trạng thái)
        var danhSachPhong = new (string Ten, LoaiPhong Loai, string Toa, int Tang, string GioiTinh, string TrangThai)[]
        {
            // Tòa A - Nam
            ("A101", lp4, "A", 1, GioiTinh.Nam, TrangThaiPhong.DangSuDung),
            ("A102", lp4, "A", 1, GioiTinh.Nam, TrangThaiPhong.DangSuDung),
            ("A103", lp6, "A", 1, GioiTinh.Nam, TrangThaiPhong.DangSuDung),
            ("A201", lp6, "A", 2, GioiTinh.Nam, TrangThaiPhong.DangSuDung),
            ("A202", lp8, "A", 2, GioiTinh.Nam, TrangThaiPhong.DangSuDung),
            ("A203", lp2, "A", 2, GioiTinh.Nam, TrangThaiPhong.DangSuDung),
            ("A301", lp4, "A", 3, GioiTinh.Nam, TrangThaiPhong.DangSuDung),
            ("A302", lp6, "A", 3, GioiTinh.Nam, TrangThaiPhong.TamNgung),     // phòng tạm ngừng (sửa chữa)
            // Tòa B - Nữ
            ("B101", lp4, "B", 1, GioiTinh.Nu, TrangThaiPhong.DangSuDung),
            ("B102", lp4, "B", 1, GioiTinh.Nu, TrangThaiPhong.DangSuDung),
            ("B103", lp6, "B", 1, GioiTinh.Nu, TrangThaiPhong.DangSuDung),
            ("B201", lp6, "B", 2, GioiTinh.Nu, TrangThaiPhong.DangSuDung),
            ("B202", lp8, "B", 2, GioiTinh.Nu, TrangThaiPhong.DangSuDung),
            ("B203", lp2, "B", 2, GioiTinh.Nu, TrangThaiPhong.DangSuDung),
            ("B301", lp4, "B", 3, GioiTinh.Nu, TrangThaiPhong.DangSuDung),
            ("B302", lp6, "B", 3, GioiTinh.Nu, TrangThaiPhong.TamNgung),      // phòng tạm ngừng
            // Tòa C - hỗn hợp
            ("C101", lp4, "C", 1, GioiTinh.Nam, TrangThaiPhong.DangSuDung),
            ("C102", lp6, "C", 1, GioiTinh.Nu, TrangThaiPhong.DangSuDung),
            ("C201", lp8, "C", 2, GioiTinh.Nam, TrangThaiPhong.DangSuDung),
            ("C202", lp2, "C", 2, GioiTinh.Nu, TrangThaiPhong.DangSuDung)
        };

        var phongTheoTen = new Dictionary<string, PhongKTX>();
        foreach (var p in danhSachPhong)
        {
            var phong = new PhongKTX
            {
                TenPhong = p.Ten,
                LoaiPhong = p.Loai,
                ToaNha = p.Toa,
                Tang = p.Tang,
                GioiTinhPhong = p.GioiTinh,
                TrangThai = p.TrangThai,
                MoTa = p.TrangThai == TrangThaiPhong.TamNgung ? "Đang sửa chữa hệ thống điện nước." : $"Phòng {p.Ten}, tòa {p.Toa}, tầng {p.Tang}."
            };
            phongTheoTen[p.Ten] = phong;
            db.PhongKTXs.Add(phong);
        }

        // ------------------------------------------------------------ 4. 30 sinh viên + tài khoản sinh viên
        // SV01-SV16: Nam, SV17-SV30: Nữ. Mã sinh viên: 221031000NN
        var tenSinhVien = new[]
        {
            "Nguyễn Văn An", "Trần Minh Quân", "Lê Hoàng Nam", "Phạm Đức Anh", "Hoàng Văn Bình", "Vũ Quang Huy",
            "Đặng Thành Long", "Bùi Tuấn Kiệt", "Đỗ Mạnh Hùng", "Ngô Gia Bảo", "Dương Văn Khoa", "Lý Trọng Nghĩa",
            "Phan Thế Vinh", "Trịnh Công Sơn", "Hồ Nhật Minh", "Tạ Quốc Việt",
            "Nguyễn Thị Lan", "Trần Thu Hà", "Lê Thị Mai", "Phạm Ngọc Anh", "Hoàng Thị Hoa", "Vũ Thùy Linh",
            "Đặng Khánh Linh", "Bùi Thị Thảo", "Đỗ Thu Trang", "Ngô Phương Thảo", "Dương Thị Hạnh", "Lý Mỹ Duyên",
            "Phan Thanh Hằng", "Trịnh Bảo Ngọc"
        };
        var lopSinhVien = new[] { "DHTI17A1ND", "DHTI17A2ND", "DHTI18A1ND" };
        var tinh = new[] { "Nam Định", "Thái Bình", "Hà Nam", "Ninh Bình", "Hải Dương", "Nghệ An" };

        var sinhViens = new List<SinhVien>();
        for (int i = 0; i < 30; i++)
        {
            var ma = $"221031000{i + 1:00}";
            var gioiTinh = i < 16 ? GioiTinh.Nam : GioiTinh.Nu;
            // SV30: tài khoản bị khóa (để kiểm thử đăng nhập tài khoản bị khóa)
            var khoa = i == 29;
            var tk = TaoTaiKhoan(ma, MatKhauSinhVien, tenSinhVien[i], $"sv{ma}@student.uneti.edu.vn",
                VaiTro.SinhVien, khoa ? TrangThaiTaiKhoan.Khoa : TrangThaiTaiKhoan.HoatDong);

            var sv = new SinhVien
            {
                MaSinhVien = ma,
                TaiKhoan = tk,
                HoTen = tenSinhVien[i],
                NgaySinh = new DateTime(2003 + (i % 3), 1 + (i % 12), 1 + (i % 27)),
                GioiTinh = gioiTinh,
                Lop = lopSinhVien[i % 3],
                Khoa = "Công nghệ thông tin",
                SoDienThoai = $"09{(12345000 + i * 137):00000000}".Substring(0, 10),
                Email = $"sv{ma}@student.uneti.edu.vn",
                DiaChi = tinh[i % tinh.Length],
                TrangThai = TrangThaiSinhVien.HoatDong
            };
            sinhViens.Add(sv);
            db.SinhViens.Add(sv);
        }

        await db.SaveChangesAsync();   // lưu để có khóa chính trước khi tạo đăng ký/phân phòng

        // ------------------------------------------------------------ 5. Đăng ký KTX + phân phòng
        // Mỗi bộ: (chỉ số sinh viên 1..30, trạng thái đăng ký, danh sách phòng đã ở theo thứ tự, trạng thái từng bản ghi)
        // Quy ước tháng: ngày vào cách hôm nay N tháng, để thống kê "vào KTX theo tháng" có nhiều cột.
        void ThemDangKy(int sttSv, string trangThaiDangKy, int thangTruoc, LoaiPhong nguyenVong,
                        params (string Phong, string TrangThai, int NgayVaoThangTruoc, int? NgayRaThangTruoc)[] lichSu)
        {
            var sv = sinhViens[sttSv - 1];
            var ngayBatDau = homNay.AddMonths(-thangTruoc);
            var dk = new DangKyKTX
            {
                MaSinhVien = sv.MaSinhVien,
                NgayDangKy = ngayBatDau.AddDays(-5),
                NgayBatDau = ngayBatDau,
                NgayKetThucDuKien = ngayBatDau.AddMonths(8),
                NguyenVongLoaiPhong = nguyenVong?.MaLoaiPhong,
                LoaiPhongNguyenVong = nguyenVong,
                GhiChu = trangThaiDangKy == TrangThaiDangKy.DaHuy ? "Sinh viên đổi kế hoạch, không còn nhu cầu ở KTX." : null,
                TrangThai = trangThaiDangKy
            };
            db.DangKyKTXs.Add(dk);

            foreach (var ls in lichSu)
            {
                db.PhanPhongs.Add(new PhanPhong
                {
                    DangKyKTX = dk,
                    PhongKTX = phongTheoTen[ls.Phong],
                    NgayVao = homNay.AddMonths(-ls.NgayVaoThangTruoc),
                    NgayRa = ls.NgayRaThangTruoc.HasValue ? homNay.AddMonths(-ls.NgayRaThangTruoc.Value) : null,
                    TrangThai = ls.TrangThai
                });
            }
        }

        const string O = TrangThaiPhanPhong.DangO;
        const string C = TrangThaiPhanPhong.DaChuyen;
        const string T = TrangThaiPhanPhong.DaTraPhong;
        const string DP = TrangThaiDangKy.DaPhanPhong;

        // --- Phòng ĐÃ ĐẦY: A101 (4/4), B101 (4/4), B203 (2/2)
        ThemDangKy(1, DP, 5, lp4, ("A101", O, 5, null));
        ThemDangKy(2, DP, 5, lp4, ("A101", O, 5, null));
        ThemDangKy(3, DP, 4, lp4, ("A101", O, 4, null));
        ThemDangKy(4, DP, 4, lp4, ("A101", O, 4, null));
        ThemDangKy(17, DP, 5, lp4, ("B101", O, 5, null));
        ThemDangKy(18, DP, 5, lp4, ("B101", O, 5, null));
        ThemDangKy(19, DP, 3, lp4, ("B101", O, 3, null));
        ThemDangKy(20, DP, 3, lp4, ("B101", O, 3, null));
        ThemDangKy(26, DP, 2, lp2, ("B203", O, 2, null));
        ThemDangKy(27, DP, 2, lp2, ("B203", O, 2, null));

        // --- Phòng CÒN ĐÚNG 1 CHỖ: A102 (3/4), B102 (3/4)
        ThemDangKy(5, DP, 4, lp4, ("A102", O, 4, null));
        ThemDangKy(6, DP, 4, lp4, ("A102", O, 4, null));
        ThemDangKy(7, DP, 3, lp4, ("A102", O, 3, null));
        ThemDangKy(21, DP, 4, lp4, ("B102", O, 4, null));
        ThemDangKy(22, DP, 3, lp4, ("B102", O, 3, null));
        ThemDangKy(23, DP, 3, lp4, ("B102", O, 3, null));

        // --- Phòng CÒN NHIỀU CHỖ: A103 (2/6), A201 (2/6), C201 (2/8), B103 (2/6)
        ThemDangKy(8, DP, 2, lp6, ("A103", O, 2, null));
        ThemDangKy(9, DP, 1, lp6, ("A103", O, 1, null));
        ThemDangKy(10, DP, 2, lp6, ("A201", O, 2, null));
        ThemDangKy(11, DP, 1, lp8, ("C201", O, 1, null));
        ThemDangKy(12, DP, 1, lp8, ("C201", O, 1, null));
        ThemDangKy(24, DP, 2, lp6, ("B103", O, 2, null));
        ThemDangKy(25, DP, 1, lp6, ("B103", O, 1, null));

        // --- Sinh viên 13 ĐÃ CHUYỂN PHÒNG: A103 (đã chuyển) -> A201 (đang ở)  => lịch sử 2 bản ghi
        ThemDangKy(13, DP, 4, lp6, ("A103", C, 4, 2), ("A201", O, 2, null));

        // --- Đăng ký ĐÃ TRẢ PHÒNG: SV14 (A102), SV28 (B103)
        ThemDangKy(14, TrangThaiDangKy.DaTraPhong, 6, lp4, ("A102", T, 6, 3));
        ThemDangKy(28, TrangThaiDangKy.DaTraPhong, 6, lp6, ("B103", T, 6, 4));

        // --- Đăng ký CHỜ DUYỆT: SV15 (Nam), SV16 (Nam), SV29 (Nữ)
        ThemDangKy(15, TrangThaiDangKy.ChoDuyet, 0, lp4);
        ThemDangKy(16, TrangThaiDangKy.ChoDuyet, 0, lp8);
        ThemDangKy(29, TrangThaiDangKy.ChoDuyet, 0, lp2);

        // --- Đăng ký ĐÃ HỦY: SV30 (tài khoản bị khóa)
        ThemDangKy(30, TrangThaiDangKy.DaHuy, 1, lp6);

        await db.SaveChangesAsync();
    }
}
