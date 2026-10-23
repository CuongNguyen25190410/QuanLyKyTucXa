using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Filters;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Helpers;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Models;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Models.ViewModels;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Services;

namespace QuanLyKyTucXa_UNETI11_DHTI17A2ND.Controllers;

[PhanQuyen]
public class DonHuyPhongController : AppControllerBase
{
    private const int PageSize = 10;
    private readonly AppDbContext _db;
    private readonly DonHuyPhongService _service;

    public DonHuyPhongController(AppDbContext db, DonHuyPhongService service)
    {
        _db = db;
        _service = service;
    }

    // =========================================================== PHÍA SINH VIÊN
    [PhanQuyen(VaiTro.SinhVien)]
    public async Task<IActionResult> CuaToi()
    {
        var sv = await LaySinhVienHienTaiAsync(_db);
        if (sv == null) return View(new List<DonHuyPhong>());

        var ds = await _db.DonHuyPhongs.AsNoTracking()
            .Include(d => d.DangKyKTX).ThenInclude(k => k.PhanPhongs).ThenInclude(pp => pp.PhongKTX)
            .Where(d => d.DangKyKTX.MaSinhVien == sv.MaSinhVien)   // chỉ đơn của chính mình
            .OrderByDescending(d => d.NgayGui)
            .ToListAsync();
        return View(ds);
    }

    [PhanQuyen(VaiTro.SinhVien)]
    [HttpGet]
    public async Task<IActionResult> Create(int? maDangKy)
    {
        var sv = await LaySinhVienHienTaiAsync(_db);
        if (sv == null) return NotFound();

        var vm = new DonHuyCreateVM { NgayRaDuKien = DateTime.Today };
        var loi = await NapAsync(vm, sv.MaSinhVien, maDangKy);
        if (loi != null)
        {
            TempData["Loi"] = loi;
            return RedirectToAction("CuaToi", "DangKyKTX");
        }
        return View(vm);
    }

    [PhanQuyen(VaiTro.SinhVien)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DonHuyCreateVM vm)
    {
        var sv = await LaySinhVienHienTaiAsync(_db);
        if (sv == null) return NotFound();

        var loi = await NapAsync(vm, sv.MaSinhVien, vm.MaDangKy);
        if (loi != null)
        {
            TempData["Loi"] = loi;
            return RedirectToAction("CuaToi", "DangKyKTX");
        }

        if (ModelState.IsValid)
        {
            var kq = await _service.GuiDonAsync(sv.MaSinhVien, vm.MaDangKy, vm.NgayRaDuKien.Value, vm.LyDo);
            if (kq.ThanhCong)
            {
                TempData["ThanhCong"] = kq.ThongBao;
                return RedirectToAction(nameof(CuaToi));
            }
            ModelState.AddModelError(string.Empty, kq.ThongBao);
        }
        return View(vm);
    }

    [PhanQuyen(VaiTro.SinhVien)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThuHoi(int id)
    {
        var sv = await LaySinhVienHienTaiAsync(_db);
        if (sv == null) return NotFound();

        var kq = await _service.ThuHoiAsync(sv.MaSinhVien, id);
        TempData[kq.ThanhCong ? "ThanhCong" : "Loi"] = kq.ThongBao;
        return RedirectToAction(nameof(CuaToi));
    }

    // =========================================================== PHÍA ADMIN
    // trangThai: "Chờ duyệt" (mặc định) | "Đã duyệt" | "Từ chối" | "Đã thu hồi" | "all"
    [PhanQuyen(VaiTro.Admin)]
    public async Task<IActionResult> Index(string tuKhoa, string trangThai = TrangThaiDonHuy.ChoDuyet, int page = 1)
    {
        var q = _db.DonHuyPhongs.AsNoTracking()
            .Include(d => d.DangKyKTX).ThenInclude(k => k.SinhVien)
            .Include(d => d.DangKyKTX).ThenInclude(k => k.PhanPhongs).ThenInclude(pp => pp.PhongKTX)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            tuKhoa = tuKhoa.Trim();
            q = q.Where(d => d.DangKyKTX.MaSinhVien.Contains(tuKhoa) || d.DangKyKTX.SinhVien.HoTen.Contains(tuKhoa));
        }
        if (!string.IsNullOrEmpty(trangThai) && trangThai != "all")
            q = q.Where(d => d.TrangThai == trangThai);

        q = q.OrderBy(d => d.TrangThai == TrangThaiDonHuy.ChoDuyet ? 0 : 1).ThenByDescending(d => d.NgayGui);

        var vm = new DonHuyIndexVM
        {
            TuKhoa = tuKhoa,
            TrangThai = trangThai,
            SoChoDuyet = await _db.DonHuyPhongs.CountAsync(d => d.TrangThai == TrangThaiDonHuy.ChoDuyet),
            KetQua = await PagedResult<DonHuyPhong>.CreateAsync(q, page, PageSize)
        };
        return View(vm);
    }

    [PhanQuyen(VaiTro.Admin)]
    [HttpGet]
    public async Task<IActionResult> XuLy(int? id)
    {
        if (id == null) return NotFound();
        var vm = new DonHuyXuLyVM { MaDon = id.Value };
        if (!await NapXuLyAsync(vm)) return NotFound();
        vm.NgayRa = vm.Don.NgayRaDuKien;
        return View(vm);
    }

    [PhanQuyen(VaiTro.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duyet(DonHuyXuLyVM vm)
    {
        if (vm.NgayRa == null)
            ModelState.AddModelError(nameof(vm.NgayRa), "Vui lòng chọn ngày trả phòng.");

        if (ModelState.IsValid)
        {
            var kq = await _service.DuyetAsync(vm.MaDon, MaTaiKhoanHienTai.Value, vm.NgayRa.Value, vm.GhiChuAdmin);
            if (kq.ThanhCong)
            {
                TempData["ThanhCong"] = kq.ThongBao;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, kq.ThongBao);
        }

        if (!await NapXuLyAsync(vm)) return NotFound();
        return View("XuLy", vm);
    }

    [PhanQuyen(VaiTro.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TuChoi(DonHuyXuLyVM vm)
    {
        var kq = await _service.TuChoiAsync(vm.MaDon, MaTaiKhoanHienTai.Value, vm.GhiChuAdmin);
        if (kq.ThanhCong)
        {
            TempData["ThanhCong"] = kq.ThongBao;
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError(string.Empty, kq.ThongBao);

        if (!await NapXuLyAsync(vm)) return NotFound();
        return View("XuLy", vm);
    }

    // =========================================================== HÀM HỖ TRỢ
    /// <summary>Nạp thông tin phòng hiện tại cho form gửi đơn. Trả về thông báo lỗi nếu không đủ điều kiện, ngược lại null.</summary>
    private async Task<string> NapAsync(DonHuyCreateVM vm, string maSinhVien, int? maDangKy)
    {
        var q = _db.DangKyKTXs.AsNoTracking()
            .Where(d => d.MaSinhVien == maSinhVien && d.TrangThai == TrangThaiDangKy.DaPhanPhong);
        if (maDangKy != null) q = q.Where(d => d.MaDangKy == maDangKy.Value);

        var dangKy = await q.FirstOrDefaultAsync();
        if (dangKy == null)
            return "Bạn không có đăng ký nào đang được phân phòng để hủy.";

        var hienTai = await _db.PhanPhongs.AsNoTracking().Include(pp => pp.PhongKTX)
            .FirstOrDefaultAsync(pp => pp.MaDangKy == dangKy.MaDangKy && pp.TrangThai == TrangThaiPhanPhong.DangO);
        if (hienTai == null)
            return "Bạn hiện không ở phòng nào.";

        if (await _db.DonHuyPhongs.AnyAsync(d => d.MaDangKy == dangKy.MaDangKy && d.TrangThai == TrangThaiDonHuy.ChoDuyet))
            return "Bạn đã có đơn hủy phòng đang chờ duyệt. Hãy thu hồi đơn đó trước nếu muốn gửi lại.";

        vm.MaDangKy = dangKy.MaDangKy;
        vm.HienTai = hienTai;
        return null;
    }

    /// <summary>Nạp dữ liệu hiển thị cho trang xử lý đơn của Admin.</summary>
    private async Task<bool> NapXuLyAsync(DonHuyXuLyVM vm)
    {
        var don = await _db.DonHuyPhongs.AsNoTracking()
            .Include(d => d.TaiKhoanXuLy)
            .Include(d => d.DangKyKTX).ThenInclude(k => k.SinhVien)
            .FirstOrDefaultAsync(d => d.MaDon == vm.MaDon);
        if (don == null) return false;

        vm.Don = don;
        vm.HienTai = await _db.PhanPhongs.AsNoTracking()
            .Include(pp => pp.PhongKTX).ThenInclude(p => p.LoaiPhong)
            .FirstOrDefaultAsync(pp => pp.MaDangKy == don.MaDangKy && pp.TrangThai == TrangThaiPhanPhong.DangO);

        // Hóa đơn chưa thanh toán: chỉ để admin cân nhắc, không chặn việc duyệt
        var chuaTT = await _db.HoaDons.AsNoTracking()
            .Where(h => h.MaDangKy == don.MaDangKy && h.TrangThai == TrangThaiHoaDon.ChuaThanhToan)
            .Select(h => h.SoTien).ToListAsync();
        vm.SoHoaDonChuaTT = chuaTT.Count;
        vm.TongTienChuaTT = chuaTT.Sum();
        return true;
    }
}