using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Filters;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models.ViewModels;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Controllers;

[PhanQuyen]
public class ThanhToanController : AppControllerBase
{
    private const int PageSize = 10;

    private readonly AppDbContext _db;
    private readonly HoaDonService _hoaDon;
    private readonly ICongThanhToan _cong;

    public ThanhToanController(AppDbContext db, HoaDonService hoaDon, ICongThanhToan cong)
    {
        _db = db;
        _hoaDon = hoaDon;
        _cong = cong;
    }

    // =========================================================== PHÍA SINH VIÊN
    [PhanQuyen(VaiTro.SinhVien)]
    public async Task<IActionResult> CuaToi()
    {
        var sv = await LaySinhVienHienTaiAsync(_db);
        if (sv == null) return View(new List<HoaDon>());

        var ds = await _db.HoaDons.AsNoTracking()
            .Include(h => h.PhongKTX)
            .Where(h => h.DangKyKTX.MaSinhVien == sv.MaSinhVien)      // chỉ hóa đơn của chính mình
            .OrderByDescending(h => h.Nam).ThenByDescending(h => h.Thang)
            .ToListAsync();
        return View(ds);
    }

    // Trang hiển thị mã QR thanh toán
    [PhanQuyen(VaiTro.SinhVien)]
    public async Task<IActionResult> QR(int id)
    {
        var sv = await LaySinhVienHienTaiAsync(_db);
        if (sv == null) return NotFound();

        try
        {
            var (hd, loi) = await _hoaDon.KhoiTaoThanhToanAsync(
                id, sv.MaSinhVien,
                Url.Action(nameof(KetQua), "ThanhToan", new { id }, Request.Scheme),
                Url.Action(nameof(CuaToi), "ThanhToan", null, Request.Scheme));

            if (hd == null) return NotFound();
            if (loi != null)
            {
                TempData["Loi"] = loi;
                return RedirectToAction(nameof(CuaToi));
            }
            if (hd.TrangThai == TrangThaiHoaDon.DaThanhToan)
            {
                TempData["ThanhCong"] = $"Hóa đơn tháng {hd.Thang:00}/{hd.Nam} đã được thanh toán.";
                return RedirectToAction(nameof(CuaToi));
            }

            ViewBag.GiaLap = _cong.LaGiaLap;
            return View(hd);
        }
        catch (LoiCongThanhToanException ex)
        {
            TempData["Loi"] = "Không tạo được mã thanh toán: " + ex.Message;
            return RedirectToAction(nameof(CuaToi));
        }
    }

    // Trang QR gọi định kỳ (AJAX) để biết đã thanh toán chưa
    [PhanQuyen(VaiTro.SinhVien)]
    public async Task<IActionResult> TrangThai(int id)
    {
        var sv = await LaySinhVienHienTaiAsync(_db);
        if (sv == null) return NotFound();

        var hd = await _db.HoaDons.Include(h => h.DangKyKTX)
            .FirstOrDefaultAsync(h => h.MaHoaDon == id && h.DangKyKTX.MaSinhVien == sv.MaSinhVien);
        if (hd == null) return NotFound();

        await _hoaDon.DongBoAsync(hd);
        return Json(new { daThanhToan = hd.TrangThai == TrangThaiHoaDon.DaThanhToan, trangThai = hd.TrangThai });
    }

    // Địa chỉ cổng thanh toán đưa sinh viên quay lại sau khi trả tiền
    [PhanQuyen(VaiTro.SinhVien)]
    public async Task<IActionResult> KetQua(int id)
    {
        var sv = await LaySinhVienHienTaiAsync(_db);
        if (sv == null) return NotFound();

        var hd = await _db.HoaDons.Include(h => h.DangKyKTX)
            .FirstOrDefaultAsync(h => h.MaHoaDon == id && h.DangKyKTX.MaSinhVien == sv.MaSinhVien);
        if (hd == null) return NotFound();

        await _hoaDon.DongBoAsync(hd);
        if (hd.TrangThai == TrangThaiHoaDon.DaThanhToan)
            TempData["ThanhCong"] = $"Thanh toán hóa đơn tháng {hd.Thang:00}/{hd.Nam} thành công. Cảm ơn bạn!";
        else
            TempData["Loi"] = "Chưa nhận được thanh toán. Nếu bạn đã chuyển tiền, vui lòng chờ ít phút rồi tải lại trang.";
        return RedirectToAction(nameof(CuaToi));
    }

    // Chỉ dùng khi cấu hình "Mock" để demo
    [PhanQuyen(VaiTro.SinhVien)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GiaLap(int id)
    {
        var sv = await LaySinhVienHienTaiAsync(_db);
        if (sv == null) return NotFound();

        var kq = await _hoaDon.GiaLapThanhToanAsync(id, sv.MaSinhVien);
        TempData[kq.ThanhCong ? "ThanhCong" : "Loi"] = kq.ThongBao;
        return RedirectToAction(nameof(CuaToi));
    }

    // =========================================================== PHÍA ADMIN
    [PhanQuyen(VaiTro.Admin)]
    public async Task<IActionResult> Index(string tuKhoa, int? thang, int? nam, string trangThai, int page = 1)
    {
        var q = _db.HoaDons.AsNoTracking()
            .Include(h => h.PhongKTX)
            .Include(h => h.DangKyKTX).ThenInclude(d => d.SinhVien)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            tuKhoa = tuKhoa.Trim();
            q = q.Where(h => h.DangKyKTX.MaSinhVien.Contains(tuKhoa)
                             || h.DangKyKTX.SinhVien.HoTen.Contains(tuKhoa)
                             || h.PhongKTX.TenPhong.Contains(tuKhoa));
        }
        if (thang.HasValue) q = q.Where(h => h.Thang == thang.Value);
        if (nam.HasValue) q = q.Where(h => h.Nam == nam.Value);
        if (!string.IsNullOrEmpty(trangThai)) q = q.Where(h => h.TrangThai == trangThai);

        var tongDaThu = await q.Where(h => h.TrangThai == TrangThaiHoaDon.DaThanhToan).SumAsync(h => (decimal?)h.SoTien) ?? 0;
        var tongChuaThu = await q.Where(h => h.TrangThai == TrangThaiHoaDon.ChuaThanhToan).SumAsync(h => (decimal?)h.SoTien) ?? 0;

        q = q.OrderByDescending(h => h.Nam).ThenByDescending(h => h.Thang).ThenBy(h => h.PhongKTX.TenPhong);

        var vm = new HoaDonIndexVM
        {
            TuKhoa = tuKhoa,
            Thang = thang,
            Nam = nam,
            TrangThai = trangThai,
            TongDaThu = tongDaThu,
            TongChuaThu = tongChuaThu,
            ThangMacDinh = DateTime.Today.Month,
            NamMacDinh = DateTime.Today.Year,
            KetQua = await PagedResult<HoaDon>.CreateAsync(q, page, PageSize)
        };
        return View(vm);
    }

    [PhanQuyen(VaiTro.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TaoHoaDonThang(int thang, int nam)
    {
        if (thang < 1 || thang > 12 || nam < 2020 || nam > 2100)
        {
            TempData["Loi"] = "Tháng hoặc năm không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var n = await _hoaDon.TaoHoaDonThangAsync(thang, nam);
            TempData["ThanhCong"] = n > 0
                ? $"Đã tạo {n} hóa đơn cho tháng {thang:00}/{nam}."
                : $"Không có hóa đơn mới cần tạo cho tháng {thang:00}/{nam} (đã tạo đủ hoặc chưa có sinh viên đang ở).";
        }
        catch (DbUpdateException)
        {
            TempData["Loi"] = "Có hóa đơn bị trùng khi tạo. Vui lòng thử lại.";
        }
        return RedirectToAction(nameof(Index), new { thang, nam });
    }

    [PhanQuyen(VaiTro.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XacNhan(int id, string returnQuery)
    {
        var kq = await _hoaDon.XacNhanThuCongAsync(id);
        TempData[kq.ThanhCong ? "ThanhCong" : "Loi"] = kq.ThongBao;
        return QuayVeDanhSach(returnQuery);
    }

    [PhanQuyen(VaiTro.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Huy(int id, string returnQuery)
    {
        var kq = await _hoaDon.HuyAsync(id);
        TempData[kq.ThanhCong ? "ThanhCong" : "Loi"] = kq.ThongBao;
        return QuayVeDanhSach(returnQuery);
    }
    [PhanQuyen(VaiTro.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KhoiPhuc(int id, string returnQuery)
    {
        var kq = await _hoaDon.KhoiPhucAsync(id);
        TempData[kq.ThanhCong ? "ThanhCong" : "Loi"] = kq.ThongBao;
        return QuayVeDanhSach(returnQuery);
    }
    private IActionResult QuayVeDanhSach(string returnQuery)
    {
        if (string.IsNullOrEmpty(returnQuery) || !returnQuery.StartsWith("?")) returnQuery = "";
        return Redirect(Url.Action(nameof(Index)) + returnQuery);
    }
}