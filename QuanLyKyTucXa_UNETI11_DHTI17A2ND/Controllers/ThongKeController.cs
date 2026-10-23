// Họ và tên: [Hà Việt Anh]
// Mã sinh viên: [23203100046]
// Nội dung thực hiện: Module 4 - Quản lý đăng ký, phân phòng, chuyển phòng, trả phòng, Dashboard, thống kê
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Filters;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Models.ViewModels;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Services;

namespace QuanLyKyTucXa_UNETI11_DHTI17A2ND.Controllers;

// Dashboard và thống kê: số liệu luôn được truy vấn trực tiếp từ Database bằng LINQ
// tại thời điểm xem, không lưu tổng số cố định (đề mục 8.9).
[PhanQuyen(VaiTro.Admin)]
public class ThongKeController : AppControllerBase
{
    private readonly AppDbContext _db;
    private readonly PhongService _phongService;

    public ThongKeController(AppDbContext db, PhongService phongService)
    {
        _db = db;
        _phongService = phongService;
    }

    // ===================================================== DASHBOARD
    public async Task<IActionResult> Index()
    {
        var vm = new DashboardVM
        {
            TongSoPhong = await _db.PhongKTXs.CountAsync(),
            TongSoSinhVien = await _db.SinhViens.CountAsync(),
            TongSoDangKy = await _db.DangKyKTXs.CountAsync(),
            SoDangKyChoDuyet = await _db.DangKyKTXs.CountAsync(d => d.TrangThai == TrangThaiDangKy.ChoDuyet),
            SoSinhVienDangO = await _db.PhanPhongs.CountAsync(pp => pp.TrangThai == TrangThaiPhanPhong.DangO),

            // Tổng số chỗ = tổng SoNguoiToiDa của các phòng đang hoạt động
            TongSoCho = await _db.PhongKTXs
                .Where(p => p.TrangThai == TrangThaiPhong.DangSuDung)
                .SumAsync(p => p.LoaiPhong.SoNguoiToiDa),

            // Số chỗ đang sử dụng = số PhanPhong có trạng thái Đang ở
            TongSoChoDangSuDung = await _db.PhanPhongs.CountAsync(pp => pp.TrangThai == TrangThaiPhanPhong.DangO),

            // Phòng đã đủ người: phòng đang sử dụng có SoChoCon <= 0
            SoPhongDaDayNguoi = await _phongService.QueryPhong()
                .CountAsync(p => p.TrangThai == TrangThaiPhong.DangSuDung && p.SoChoCon <= 0)
        };

        // Số chỗ còn = Tổng số chỗ - Số chỗ đang sử dụng
        vm.TongSoChoConTrong = vm.TongSoCho - vm.TongSoChoDangSuDung;
        // Tỷ lệ sử dụng = đang sử dụng / tổng số chỗ x 100% (xử lý tổng số chỗ = 0)
        vm.TyLeSuDung = vm.TongSoCho == 0 ? 0 : Math.Round(vm.TongSoChoDangSuDung * 100.0 / vm.TongSoCho, 1);
        return View(vm);
    }

    // ===================================================== THỐNG KÊ CHI TIẾT
    public async Task<IActionResult> ChiTiet()
    {
        var vm = new ThongKeVM();

        // 1. Số phòng theo từng loại  (GroupBy + Count)
        vm.PhongTheoLoai = await _db.PhongKTXs
            .GroupBy(p => p.LoaiPhong.TenLoaiPhong)
            .Select(g => new ThongKeItemVM { Nhan = g.Key, GiaTri = g.Count() })
            .OrderByDescending(x => x.GiaTri)
            .ToListAsync();

        // 2 + 3 + 7. Mỗi phòng: số sinh viên đang ở, số chỗ còn, tỷ lệ sử dụng (OrderByDescending)
        vm.SinhVienTheoPhong = await _phongService.QueryPhong()
            .OrderByDescending(p => p.SoDangO).ThenBy(p => p.TenPhong)
            .ToListAsync();

        // 6. Phòng có số sinh viên đang ở nhiều nhất (có thể có nhiều phòng bằng nhau)
        var maxDangO = vm.SinhVienTheoPhong.Count == 0 ? 0 : vm.SinhVienTheoPhong.Max(p => p.SoDangO);
        vm.PhongDongNhat = maxDangO == 0
            ? new List<PhongItemVM>()
            : vm.SinhVienTheoPhong.Where(p => p.SoDangO == maxDangO).ToList();

        // 4. Số sinh viên theo từng tòa nhà
        vm.SinhVienTheoToa = await _db.PhanPhongs
            .Where(pp => pp.TrangThai == TrangThaiPhanPhong.DangO)
            .GroupBy(pp => pp.PhongKTX.ToaNha)
            .Select(g => new ThongKeItemVM { Nhan = g.Key, GiaTri = g.Count() })
            .OrderBy(x => x.Nhan)
            .ToListAsync();

        // 5. Số sinh viên theo từng loại phòng
        vm.SinhVienTheoLoaiPhong = await _db.PhanPhongs
            .Where(pp => pp.TrangThai == TrangThaiPhanPhong.DangO)
            .GroupBy(pp => pp.PhongKTX.LoaiPhong.TenLoaiPhong)
            .Select(g => new ThongKeItemVM { Nhan = g.Key, GiaTri = g.Count() })
            .OrderByDescending(x => x.GiaTri)
            .ToListAsync();

        // 6. Số đăng ký theo từng trạng thái (bổ sung trạng thái chưa có để bảng luôn đủ 4 dòng)
        var theoTrangThai = await _db.DangKyKTXs
            .GroupBy(d => d.TrangThai)
            .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
            .ToListAsync();
        vm.DangKyTheoTrangThai = TrangThaiDangKy.TatCa
            .Select(tt => new ThongKeItemVM { Nhan = tt, GiaTri = theoTrangThai.FirstOrDefault(x => x.TrangThai == tt)?.SoLuong ?? 0 })
            .ToList();

        // 8. Số sinh viên VÀO KTX theo tháng: tính theo ngày vào đầu tiên của mỗi đăng ký
        //    (lần chuyển phòng không được tính là "vào KTX" thêm lần nữa)
        var ngayVaoDau = await _db.PhanPhongs
            .GroupBy(pp => pp.MaDangKy)
            .Select(g => g.Min(x => x.NgayVao))
            .ToListAsync();
        vm.SinhVienVaoTheoThang = ngayVaoDau
            .GroupBy(d => new { d.Year, d.Month })
            .Select(g => new ThongKeThangVM { Nam = g.Key.Year, Thang = g.Key.Month, SoSinhVien = g.Count() })
            .OrderBy(x => x.Nam).ThenBy(x => x.Thang)
            .ToList();

        // 7. Tỷ lệ sử dụng chung
        var tongCho = await _db.PhongKTXs.Where(p => p.TrangThai == TrangThaiPhong.DangSuDung)
            .SumAsync(p => p.LoaiPhong.SoNguoiToiDa);
        var dangDung = await _db.PhanPhongs.CountAsync(pp => pp.TrangThai == TrangThaiPhanPhong.DangO);
        vm.TyLeSuDungChung = tongCho == 0 ? 0 : Math.Round(dangDung * 100.0 / tongCho, 1);

        return View(vm);
    }
}
