// Họ và tên: [ĐIỀN HỌ TÊN SV4]
// Mã sinh viên: [ĐIỀN MÃ SV4]
// Nội dung thực hiện: Module 4 - Quản lý đăng ký, phân phòng, chuyển phòng, trả phòng, Dashboard, thống kê
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Filters;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models.ViewModels;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Controllers;

// Phân phòng - chuyển phòng - trả phòng: chỉ Admin
[PhanQuyen(VaiTro.Admin)]
public class PhanPhongController : AppControllerBase
{
    private const int PageSize = 10;

    private readonly AppDbContext _db;
    private readonly PhongService _phongService;
    private readonly PhanPhongService _phanPhongService;

    public PhanPhongController(AppDbContext db, PhongService phongService, PhanPhongService phanPhongService)
    {
        _db = db;
        _phongService = phongService;
        _phanPhongService = phanPhongService;
    }

    // ===================================================== DANH SÁCH SINH VIÊN ĐANG Ở / LỊCH SỬ
    // trangThai: "Đang ở" (mặc định) | "Đã chuyển" | "Đã trả phòng" | "all"
    public async Task<IActionResult> Index(string tuKhoa, string trangThai = TrangThaiPhanPhong.DangO, int page = 1)
    {
        var q = _db.PhanPhongs.AsNoTracking()
            .Include(pp => pp.DangKyKTX).ThenInclude(d => d.SinhVien)
            .Include(pp => pp.PhongKTX).ThenInclude(p => p.LoaiPhong)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            tuKhoa = tuKhoa.Trim();
            q = q.Where(pp => pp.DangKyKTX.SinhVien.HoTen.Contains(tuKhoa)
                              || pp.DangKyKTX.MaSinhVien.Contains(tuKhoa)
                              || pp.PhongKTX.TenPhong.Contains(tuKhoa));
        }
        if (!string.IsNullOrEmpty(trangThai) && trangThai != "all")
            q = q.Where(pp => pp.TrangThai == trangThai);

        q = q.OrderBy(pp => pp.PhongKTX.TenPhong).ThenBy(pp => pp.DangKyKTX.SinhVien.HoTen);

        var vm = new PhanPhongIndexVM
        {
            TuKhoa = tuKhoa,
            TrangThai = trangThai,
            KetQua = await PagedResult<PhanPhong>.CreateAsync(q, page, PageSize)
        };
        return View(vm);
    }

    // ===================================================== PHÂN PHÒNG
    [HttpGet]
    public async Task<IActionResult> Create(int? maDangKy)
    {
        if (maDangKy == null) return NotFound();
        var dangKy = await LayDangKyAsync(maDangKy.Value);
        if (dangKy == null) return NotFound();

        if (dangKy.TrangThai != TrangThaiDangKy.ChoDuyet)
        {
            TempData["Loi"] = $"Đăng ký này đang ở trạng thái \"{dangKy.TrangThai}\", chỉ phân phòng cho đăng ký \"{TrangThaiDangKy.ChoDuyet}\".";
            return RedirectToAction("Index", "DangKyKTX");
        }

        var vm = new PhanPhongCreateVM
        {
            MaDangKy = dangKy.MaDangKy,
            DangKy = dangKy,
            NgayVao = DateTime.Today
        };
        await NapPhongCoTheChonAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PhanPhongCreateVM vm)
    {
        var dangKy = await LayDangKyAsync(vm.MaDangKy);
        if (dangKy == null) return NotFound();

        if (ModelState.IsValid)
        {
            // Mọi quy tắc nghiệp vụ được kiểm tra lại trong service, ngay trước SaveChanges
            var kq = await _phanPhongService.PhanPhongAsync(vm.MaDangKy, vm.MaPhong.Value, vm.NgayVao.Value);
            if (kq.ThanhCong)
            {
                TempData["ThanhCong"] = kq.ThongBao;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, kq.ThongBao);   // báo rõ nguyên nhân
        }

        vm.DangKy = dangKy;
        await NapPhongCoTheChonAsync(vm);
        return View(vm);
    }

    // ===================================================== CHUYỂN PHÒNG
    [HttpGet]
    public async Task<IActionResult> Chuyen(int? id)
    {
        if (id == null) return NotFound();
        var hienTai = await LayPhanPhongAsync(id.Value);
        if (hienTai == null) return NotFound();

        if (hienTai.TrangThai != TrangThaiPhanPhong.DangO)
        {
            TempData["Loi"] = "Sinh viên không còn ở phòng này nên không thể chuyển phòng.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new ChuyenPhongVM { MaPhanPhong = hienTai.MaPhanPhong, HienTai = hienTai, NgayChuyen = DateTime.Today };
        await NapPhongMoiAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Chuyen(ChuyenPhongVM vm)
    {
        var hienTai = await LayPhanPhongAsync(vm.MaPhanPhong);
        if (hienTai == null) return NotFound();

        if (ModelState.IsValid)
        {
            var kq = await _phanPhongService.ChuyenPhongAsync(vm.MaPhanPhong, vm.MaPhongMoi.Value, vm.NgayChuyen.Value);
            if (kq.ThanhCong)
            {
                TempData["ThanhCong"] = kq.ThongBao;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, kq.ThongBao);
        }

        vm.HienTai = hienTai;
        await NapPhongMoiAsync(vm);
        return View(vm);
    }

    // ===================================================== TRẢ PHÒNG
    [HttpGet]
    public async Task<IActionResult> Tra(int? id)
    {
        if (id == null) return NotFound();
        var hienTai = await LayPhanPhongAsync(id.Value);
        if (hienTai == null) return NotFound();

        if (hienTai.TrangThai != TrangThaiPhanPhong.DangO)
        {
            TempData["Loi"] = "Sinh viên không còn ở phòng này (đã trả phòng hoặc đã chuyển), không thể trả phòng lần nữa.";
            return RedirectToAction(nameof(Index));
        }
        return View(new TraPhongVM { MaPhanPhong = hienTai.MaPhanPhong, HienTai = hienTai, NgayRa = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Tra(TraPhongVM vm)
    {
        var hienTai = await LayPhanPhongAsync(vm.MaPhanPhong);
        if (hienTai == null) return NotFound();

        if (ModelState.IsValid)
        {
            var kq = await _phanPhongService.TraPhongAsync(vm.MaPhanPhong, vm.NgayRa.Value);
            if (kq.ThanhCong)
            {
                TempData["ThanhCong"] = kq.ThongBao;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, kq.ThongBao);
        }

        vm.HienTai = hienTai;
        return View(vm);
    }

    // ===================================================== HÀM HỖ TRỢ
    private Task<DangKyKTX> LayDangKyAsync(int maDangKy)
    {
        return _db.DangKyKTXs.AsNoTracking()
            .Include(d => d.SinhVien)
            .Include(d => d.LoaiPhongNguyenVong)
            .FirstOrDefaultAsync(d => d.MaDangKy == maDangKy);
    }

    private Task<PhanPhong> LayPhanPhongAsync(int maPhanPhong)
    {
        return _db.PhanPhongs.AsNoTracking()
            .Include(pp => pp.PhongKTX).ThenInclude(p => p.LoaiPhong)
            .Include(pp => pp.DangKyKTX).ThenInclude(d => d.SinhVien)
            .FirstOrDefaultAsync(pp => pp.MaPhanPhong == maPhanPhong);
    }

    /// <summary>Danh sách phòng Admin có thể chọn: loại bỏ phòng Tạm ngừng và phòng đã đủ sức chứa.</summary>
    private async Task NapPhongCoTheChonAsync(PhanPhongCreateVM vm)
    {
        var ds = await _phongService.QueryPhongConCho().ToListAsync();
        var gioiTinh = vm.DangKy.SinhVien.GioiTinh;
        var nguyenVong = vm.DangKy.NguyenVongLoaiPhong;

        // Ưu tiên: phòng đúng giới tính -> đúng nguyện vọng loại phòng -> tên phòng
        vm.PhongCoTheChon = ds
            .OrderByDescending(p => p.GioiTinhPhong == gioiTinh)
            .ThenByDescending(p => nguyenVong.HasValue && p.MaLoaiPhong == nguyenVong.Value)
            .ThenBy(p => p.TenPhong)
            .ToList();
    }

    private async Task NapPhongMoiAsync(ChuyenPhongVM vm)
    {
        var maPhongHienTai = vm.HienTai.MaPhong;
        var gioiTinh = vm.HienTai.DangKyKTX.SinhVien.GioiTinh;

        var ds = await _phongService.QueryPhongConCho()
            .Where(p => p.MaPhong != maPhongHienTai)          // phòng mới phải khác phòng đang ở
            .ToListAsync();

        vm.PhongCoTheChon = ds
            .OrderByDescending(p => p.GioiTinhPhong == gioiTinh)
            .ThenBy(p => p.TenPhong)
            .Select(p => new SelectListItem(
                $"{p.TenPhong} - {p.TenLoaiPhong} - Tòa {p.ToaNha} - Tầng {p.Tang} - {p.GioiTinhPhong} - còn {p.SoChoCon}/{p.SucChua} chỗ",
                p.MaPhong.ToString()))
            .ToList();
    }
}
