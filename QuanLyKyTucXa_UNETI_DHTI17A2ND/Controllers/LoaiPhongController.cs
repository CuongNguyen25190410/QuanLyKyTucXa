// Họ và tên: [ĐIỀN HỌ TÊN SV1]
// Mã sinh viên: [ĐIỀN MÃ SV1]
// Nội dung thực hiện: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Filters;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Controllers;

// Toàn bộ CRUD loại phòng chỉ dành cho Admin: kiểm tra quyền trước khi chạm vào dữ liệu
[PhanQuyen(VaiTro.Admin)]
public class LoaiPhongController : AppControllerBase
{
    private const int PageSize = 10;
    private readonly AppDbContext _db;

    public LoaiPhongController(AppDbContext db)
    {
        _db = db;
    }

    private static List<SelectListItem> DanhSachTrangThai() => new()
    {
        new SelectListItem(TrangThaiLoaiPhong.HoatDong, TrangThaiLoaiPhong.HoatDong),
        new SelectListItem(TrangThaiLoaiPhong.NgungSuDung, TrangThaiLoaiPhong.NgungSuDung)
    };

    public async Task<IActionResult> Index(string tuKhoa, string trangThai, int page = 1)
    {
        var q = _db.LoaiPhongs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            tuKhoa = tuKhoa.Trim();
            q = q.Where(l => l.TenLoaiPhong.Contains(tuKhoa));
        }
        if (!string.IsNullOrEmpty(trangThai)) q = q.Where(l => l.TrangThai == trangThai);
        q = q.OrderBy(l => l.SoNguoiToiDa).ThenBy(l => l.TenLoaiPhong);

        ViewBag.TuKhoa = tuKhoa;
        ViewBag.TrangThai = trangThai;
        ViewBag.TrangThais = DanhSachTrangThai();
        // số phòng thuộc từng loại, để hiển thị cạnh danh sách
        ViewBag.SoPhong = await _db.PhongKTXs.GroupBy(p => p.MaLoaiPhong)
            .Select(g => new { Ma = g.Key, So = g.Count() })
            .ToDictionaryAsync(x => x.Ma ?? 0, x => x.So);

        return View(await PagedResult<LoaiPhong>.CreateAsync(q, page, PageSize));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var lp = await _db.LoaiPhongs.AsNoTracking().FirstOrDefaultAsync(l => l.MaLoaiPhong == id);
        if (lp == null) return NotFound();
        ViewBag.SoPhong = await _db.PhongKTXs.CountAsync(p => p.MaLoaiPhong == id);
        return View(lp);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.TrangThais = DanhSachTrangThai();
        return View(new LoaiPhong { TrangThai = TrangThaiLoaiPhong.HoatDong, SoNguoiToiDa = 4 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TenLoaiPhong,SoNguoiToiDa,DonGiaThang,MoTa,TrangThai")] LoaiPhong lp)
    {
        // Kiểm tra trùng tên bằng LINQ (Data Annotation không làm được việc này)
        if (!string.IsNullOrWhiteSpace(lp.TenLoaiPhong)
            && await _db.LoaiPhongs.AnyAsync(l => l.TenLoaiPhong == lp.TenLoaiPhong.Trim()))
            ModelState.AddModelError(nameof(lp.TenLoaiPhong), "Tên loại phòng đã tồn tại.");

        if (!ModelState.IsValid)
        {
            ViewBag.TrangThais = DanhSachTrangThai();
            return View(lp);
        }

        lp.TenLoaiPhong = lp.TenLoaiPhong.Trim();
        _db.LoaiPhongs.Add(lp);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã thêm loại phòng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var lp = await _db.LoaiPhongs.FindAsync(id.Value);
        if (lp == null) return NotFound();
        ViewBag.TrangThais = DanhSachTrangThai();
        return View(lp);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("MaLoaiPhong,TenLoaiPhong,SoNguoiToiDa,DonGiaThang,MoTa,TrangThai")] LoaiPhong lp)
    {
        if (id != lp.MaLoaiPhong) return NotFound();
        var goc = await _db.LoaiPhongs.FindAsync(id);
        if (goc == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(lp.TenLoaiPhong)
            && await _db.LoaiPhongs.AnyAsync(l => l.TenLoaiPhong == lp.TenLoaiPhong.Trim() && l.MaLoaiPhong != id))
            ModelState.AddModelError(nameof(lp.TenLoaiPhong), "Tên loại phòng đã tồn tại.");

        // Không giảm sức chứa xuống thấp hơn số người đang thực tế ở (tránh SoChoCon < 0)
        if (ModelState.IsValid)
        {
            // Lấy số người đang ở của từng phòng thuộc loại này rồi lấy giá trị lớn nhất
            // (tránh viết MAX(COUNT(...)) lồng nhau mà SQL Server không cho phép)
            var soNguoiTungPhong = await _db.PhongKTXs.Where(p => p.MaLoaiPhong == id)
                .Select(p => p.PhanPhongs.Count(pp => pp.TrangThai == TrangThaiPhanPhong.DangO))
                .ToListAsync();
            var dongNhatHienTai = soNguoiTungPhong.Count == 0 ? 0 : soNguoiTungPhong.Max();
            if (lp.SoNguoiToiDa < dongNhatHienTai)
                ModelState.AddModelError(nameof(lp.SoNguoiToiDa),
                    $"Có phòng đang có {dongNhatHienTai} người ở, số người tối đa không được nhỏ hơn {dongNhatHienTai}.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.TrangThais = DanhSachTrangThai();
            return View(lp);
        }

        goc.TenLoaiPhong = lp.TenLoaiPhong.Trim();
        goc.SoNguoiToiDa = lp.SoNguoiToiDa;
        goc.DonGiaThang = lp.DonGiaThang;
        goc.MoTa = lp.MoTa;
        goc.TrangThai = lp.TrangThai;
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã cập nhật loại phòng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var lp = await _db.LoaiPhongs.AsNoTracking().FirstOrDefaultAsync(l => l.MaLoaiPhong == id);
        if (lp == null) return NotFound();
        ViewBag.SoPhong = await _db.PhongKTXs.CountAsync(p => p.MaLoaiPhong == id);
        return View(lp);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var lp = await _db.LoaiPhongs.FindAsync(id);
        if (lp == null) return NotFound();

        // Không xóa nếu làm dữ liệu liên quan không hợp lệ (đề mục 5.4 và 9.2)
        var soPhong = await _db.PhongKTXs.CountAsync(p => p.MaLoaiPhong == id);
        var soDangKy = await _db.DangKyKTXs.CountAsync(d => d.NguyenVongLoaiPhong == id);
        if (soPhong > 0 || soDangKy > 0)
        {
            TempData["Loi"] = $"Không thể xóa loại phòng \"{lp.TenLoaiPhong}\": đang có {soPhong} phòng và {soDangKy} đăng ký sử dụng loại này. " +
                              "Hãy chuyển trạng thái sang \"Ngừng sử dụng\" thay vì xóa.";
            return RedirectToAction(nameof(Index));
        }

        _db.LoaiPhongs.Remove(lp);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã xóa loại phòng.";
        return RedirectToAction(nameof(Index));
    }
}
