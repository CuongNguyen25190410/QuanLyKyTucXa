// Họ và tên: [Trần Hoàng Sơn]
// Mã sinh viên: [23203100010]
// Nội dung thực hiện: Module 2 - Quản lý phòng KTX, tìm kiếm, lọc, sắp xếp, phân trang
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Filters;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Helpers;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Models;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Models.ViewModels;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Services;

namespace QuanLyKyTucXa_UNETI11_DHTI17A2ND.Controllers;

// Cả Admin và Sinh viên đều phải đăng nhập mới xem được danh sách phòng.
// Các thao tác thay đổi dữ liệu có thêm [PhanQuyen(VaiTro.Admin)] ở từng Action.
[PhanQuyen]
public class PhongKTXController : AppControllerBase
{
    /// <summary>Số bản ghi mỗi trang (đổi ở đây khi giảng viên yêu cầu).</summary>
    private const int PageSize = 8;

    private readonly AppDbContext _db;
    private readonly PhongService _phongService;

    public PhongKTXController(AppDbContext db, PhongService phongService)
    {
        _db = db;
        _phongService = phongService;
    }

    // ===================================================================== DANH SÁCH
    // Thứ tự xử lý:  Truy vấn -> Tìm kiếm -> Lọc -> Sắp xếp -> Phân trang -> Hiển thị
    public async Task<IActionResult> Index([FromQuery] PhongFilterVM filter)
    {
        var laAdmin = LaAdmin;

        // 1. Truy vấn gốc (đã kèm SucChua, SoDangO, SoChoCon tính bằng LINQ)
        var q = _phongService.QueryPhong();

        // Sinh viên chỉ thấy phòng đang sử dụng
        if (!laAdmin) q = q.Where(p => p.TrangThai == TrangThaiPhong.DangSuDung);

        // 2. Tìm kiếm: tên phòng hoặc tòa nhà
        if (!string.IsNullOrWhiteSpace(filter.TuKhoa))
        {
            var k = filter.TuKhoa.Trim();
            q = q.Where(p => p.TenPhong.Contains(k) || p.ToaNha.Contains(k));
        }

        // 3. Lọc (kết hợp được nhiều điều kiện cùng lúc)
        if (filter.MaLoaiPhong.HasValue)
            q = q.Where(p => p.MaLoaiPhong == filter.MaLoaiPhong.Value);
        if (!string.IsNullOrEmpty(filter.ToaNha))
            q = q.Where(p => p.ToaNha == filter.ToaNha);
        if (filter.Tang.HasValue)
            q = q.Where(p => p.Tang == filter.Tang.Value);
        if (!string.IsNullOrEmpty(filter.GioiTinh))
            q = q.Where(p => p.GioiTinhPhong == filter.GioiTinh);
        if (laAdmin && !string.IsNullOrEmpty(filter.TrangThai))
            q = q.Where(p => p.TrangThai == filter.TrangThai);
        if (filter.ConCho == "con")
            q = q.Where(p => p.SoChoCon > 0);           // dựa trên số người đang ở thực tế
        else if (filter.ConCho == "het")
            q = q.Where(p => p.SoChoCon <= 0);

        // 4. Sắp xếp (luôn thêm ThenBy theo mã phòng để thứ tự ổn định giữa các trang)
        q = filter.SapXep switch
        {
            "ten_desc"  => q.OrderByDescending(p => p.TenPhong).ThenBy(p => p.MaPhong),
            "tang_asc"  => q.OrderBy(p => p.Tang).ThenBy(p => p.TenPhong),
            "tang_desc" => q.OrderByDescending(p => p.Tang).ThenBy(p => p.TenPhong),
            "cho_asc"   => q.OrderBy(p => p.SoChoCon).ThenBy(p => p.TenPhong),
            "cho_desc"  => q.OrderByDescending(p => p.SoChoCon).ThenBy(p => p.TenPhong),
            _           => q.OrderBy(p => p.TenPhong).ThenBy(p => p.MaPhong)
        };

        // 5. Phân trang ngay trên truy vấn (CountAsync + Skip/Take)
        var ketQua = await PagedResult<PhongItemVM>.CreateAsync(q, filter.Page, PageSize);
        filter.Page = ketQua.Page;

        // Dữ liệu cho các ô select lọc (lấy dữ liệu thô từ DB, tạo SelectListItem trong bộ nhớ)
        var loaiPhongs = await _db.LoaiPhongs.AsNoTracking().OrderBy(l => l.SoNguoiToiDa)
            .Select(l => new { l.MaLoaiPhong, l.TenLoaiPhong }).ToListAsync();
        var toaNhas = await _db.PhongKTXs.AsNoTracking().Select(p => p.ToaNha).Distinct().OrderBy(t => t).ToListAsync();
        var tangs = await _db.PhongKTXs.AsNoTracking().Select(p => p.Tang).Distinct().OrderBy(t => t).ToListAsync();

        var vm = new PhongIndexVM
        {
            Filter = filter,
            KetQua = ketQua,
            LaAdmin = laAdmin,
            LoaiPhongs = loaiPhongs.Select(l => new SelectListItem(l.TenLoaiPhong, l.MaLoaiPhong.ToString())).ToList(),
            ToaNhas = toaNhas.Select(t => new SelectListItem(t, t)).ToList(),
            Tangs = tangs.Select(t => new SelectListItem("Tầng " + t, t.ToString())).ToList()
        };
        return View(vm);
    }

    // ===================================================================== CHI TIẾT
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var phong = await _phongService.QueryPhong().FirstOrDefaultAsync(p => p.MaPhong == id);
        if (phong == null) return NotFound();
        if (!LaAdmin && phong.TrangThai != TrangThaiPhong.DangSuDung) return NotFound();

        var vm = new PhongDetailsVM
        {
            Phong = phong,
            LaAdmin = LaAdmin,
            MoTa = await _db.PhongKTXs.Where(p => p.MaPhong == id).Select(p => p.MoTa).FirstOrDefaultAsync()
        };

        // Chỉ Admin được xem danh sách sinh viên đang ở trong phòng
        if (LaAdmin)
        {
            vm.NguoiDangO = await _db.PhanPhongs.AsNoTracking()
                .Include(pp => pp.DangKyKTX).ThenInclude(d => d.SinhVien)
                .Where(pp => pp.MaPhong == id && pp.TrangThai == TrangThaiPhanPhong.DangO)
                .OrderBy(pp => pp.NgayVao).ToListAsync();
        }
        return View(vm);
    }

    // ===================================================================== THÊM
    [PhanQuyen(VaiTro.Admin)]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await NapDuLieuSelectAsync(null);
        return View(new PhongKTX { TrangThai = TrangThaiPhong.DangSuDung, Tang = 1 });
    }

    [PhanQuyen(VaiTro.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TenPhong,MaLoaiPhong,ToaNha,Tang,GioiTinhPhong,TrangThai,MoTa")] PhongKTX phong)
    {
        await KiemTraTrungVaLoaiPhongAsync(phong, null);

        if (!ModelState.IsValid)
        {
            await NapDuLieuSelectAsync(phong.MaLoaiPhong);   // nạp lại dữ liệu select khi trả View do lỗi
            return View(phong);
        }

        phong.TenPhong = phong.TenPhong.Trim();
        phong.ToaNha = phong.ToaNha.Trim();
        _db.PhongKTXs.Add(phong);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = $"Đã thêm phòng {phong.TenPhong}.";
        return RedirectToAction(nameof(Index));
    }

    // ===================================================================== SỬA
    [PhanQuyen(VaiTro.Admin)]
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var phong = await _db.PhongKTXs.FindAsync(id.Value);
        if (phong == null) return NotFound();
        await NapDuLieuSelectAsync(phong.MaLoaiPhong);
        ViewBag.SoDangO = await _phongService.DemSoNguoiDangOAsync(phong.MaPhong);
        return View(phong);
    }

    [PhanQuyen(VaiTro.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("MaPhong,TenPhong,MaLoaiPhong,ToaNha,Tang,GioiTinhPhong,TrangThai,MoTa")] PhongKTX phong)
    {
        if (id != phong.MaPhong) return NotFound();
        var goc = await _db.PhongKTXs.FindAsync(id);
        if (goc == null) return NotFound();

        await KiemTraTrungVaLoaiPhongAsync(phong, id);

        // Các ràng buộc khi phòng đang có người ở (để số chỗ còn không bị âm / dữ liệu không mâu thuẫn)
        var soDangO = await _phongService.DemSoNguoiDangOAsync(id);
        if (soDangO > 0)
        {
            if (phong.GioiTinhPhong != goc.GioiTinhPhong)
                ModelState.AddModelError(nameof(phong.GioiTinhPhong), $"Phòng đang có {soDangO} sinh viên ở, không thể đổi giới tính phòng.");
            if (phong.TrangThai == TrangThaiPhong.TamNgung)
                ModelState.AddModelError(nameof(phong.TrangThai), $"Phòng đang có {soDangO} sinh viên ở, hãy chuyển phòng/trả phòng hết trước khi tạm ngừng.");
        }
        if (phong.MaLoaiPhong.HasValue)
        {
            var sucChuaMoi = await _db.LoaiPhongs.Where(l => l.MaLoaiPhong == phong.MaLoaiPhong)
                .Select(l => (int?)l.SoNguoiToiDa).FirstOrDefaultAsync();
            if (sucChuaMoi.HasValue && sucChuaMoi.Value < soDangO)
                ModelState.AddModelError(nameof(phong.MaLoaiPhong), $"Loại phòng mới chỉ chứa {sucChuaMoi} người, nhưng phòng đang có {soDangO} người ở.");
        }

        if (!ModelState.IsValid)
        {
            await NapDuLieuSelectAsync(phong.MaLoaiPhong);
            ViewBag.SoDangO = soDangO;
            return View(phong);
        }

        goc.TenPhong = phong.TenPhong.Trim();
        goc.MaLoaiPhong = phong.MaLoaiPhong;
        goc.ToaNha = phong.ToaNha.Trim();
        goc.Tang = phong.Tang;
        goc.GioiTinhPhong = phong.GioiTinhPhong;
        goc.TrangThai = phong.TrangThai;
        goc.MoTa = phong.MoTa;
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã cập nhật phòng.";
        return RedirectToAction(nameof(Index));
    }

    // ===================================================================== SƠ ĐỒ PHÒNG
    public async Task<IActionResult> SoDo()
    {
        var ds = await _phongService.QueryPhong()
            .OrderBy(p => p.ToaNha).ThenBy(p => p.Tang).ThenBy(p => p.TenPhong)
            .ToListAsync();

        string gioiTinhSv = null;
        int? maPhongCuaToi = null;
        if (!LaAdmin)
        {
            var sv = await LaySinhVienHienTaiAsync(_db);
            if (sv != null)
            {
                gioiTinhSv = sv.GioiTinh;
                maPhongCuaToi = await _db.PhanPhongs
                    .Where(pp => pp.TrangThai == TrangThaiPhanPhong.DangO && pp.DangKyKTX.MaSinhVien == sv.MaSinhVien)
                    .Select(pp => (int?)pp.MaPhong)
                    .FirstOrDefaultAsync();
            }
        }

        var loaiPhongs = await _db.LoaiPhongs.AsNoTracking().OrderBy(l => l.SoNguoiToiDa)
            .Select(l => new { l.MaLoaiPhong, l.TenLoaiPhong }).ToListAsync();

        var vm = new SoDoPhongVM
        {
            LaAdmin = LaAdmin,
            GioiTinhSinhVien = gioiTinhSv,
            MaPhongCuaToi = maPhongCuaToi,
            LoaiPhongs = loaiPhongs.Select(l => new SelectListItem(l.TenLoaiPhong, l.MaLoaiPhong.ToString())).ToList(),
            Toas = ds.GroupBy(p => p.ToaNha).OrderBy(g => g.Key)
                .Select(g => new SoDoToaVM
                {
                    TenToa = g.Key,
                    Tangs = g.GroupBy(p => p.Tang).OrderByDescending(t => t.Key)
                        .Select(t => new SoDoTangVM { Tang = t.Key, Phongs = t.OrderBy(p => p.TenPhong).ToList() })
                        .ToList()
                }).ToList()
        };
        return View(vm);
    }
    // ===================================================================== ĐỔI TRẠNG THÁI NHANH
    [PhanQuyen(VaiTro.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiTrangThai(int id, string returnQuery)
    {
        var phong = await _db.PhongKTXs.FindAsync(id);
        if (phong == null) return NotFound();

        if (phong.TrangThai == TrangThaiPhong.DangSuDung)
        {
            var soDangO = await _phongService.DemSoNguoiDangOAsync(id);
            if (soDangO > 0)
            {
                TempData["Loi"] = $"Phòng {phong.TenPhong} đang có {soDangO} sinh viên ở, không thể tạm ngừng.";
                return Redirect(Url.Action(nameof(Index)) + returnQuery);
            }
            phong.TrangThai = TrangThaiPhong.TamNgung;
        }
        else
        {
            phong.TrangThai = TrangThaiPhong.DangSuDung;
        }
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = $"Phòng {phong.TenPhong} chuyển sang \"{phong.TrangThai}\".";
        return Redirect(Url.Action(nameof(Index)) + returnQuery);   // giữ nguyên điều kiện tìm kiếm/lọc/trang
    }

    // ===================================================================== XÓA
    [PhanQuyen(VaiTro.Admin)]
    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var phong = await _phongService.QueryPhong().FirstOrDefaultAsync(p => p.MaPhong == id);
        if (phong == null) return NotFound();
        ViewBag.SoLanPhanPhong = await _db.PhanPhongs.CountAsync(pp => pp.MaPhong == id);
        return View(phong);
    }

    [PhanQuyen(VaiTro.Admin)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var phong = await _db.PhongKTXs.FindAsync(id);
        if (phong == null) return NotFound();

        // Không xóa phòng đã phát sinh phân phòng: sẽ làm mất lịch sử
        var soLan = await _db.PhanPhongs.CountAsync(pp => pp.MaPhong == id);
        if (soLan > 0)
        {
            TempData["Loi"] = $"Không thể xóa phòng {phong.TenPhong}: đã có {soLan} bản ghi lịch sử phân phòng. Hãy chuyển phòng sang trạng thái \"Tạm ngừng\".";
            return RedirectToAction(nameof(Index));
        }

        _db.PhongKTXs.Remove(phong);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = $"Đã xóa phòng {phong.TenPhong}.";
        return RedirectToAction(nameof(Index));
    }

    // ===================================================================== HÀM HỖ TRỢ
    /// <summary>Kiểm tra bằng LINQ các quy tắc phụ thuộc Database: tên phòng không trùng, loại phòng tồn tại.</summary>
    private async Task KiemTraTrungVaLoaiPhongAsync(PhongKTX phong, int? maPhongDangSua)
    {
        if (!string.IsNullOrWhiteSpace(phong.TenPhong))
        {
            var ten = phong.TenPhong.Trim();
            var trung = await _db.PhongKTXs.AnyAsync(p => p.TenPhong == ten && p.MaPhong != (maPhongDangSua ?? 0));
            if (trung) ModelState.AddModelError(nameof(phong.TenPhong), "Tên phòng đã tồn tại.");
        }
        if (phong.MaLoaiPhong.HasValue && !await _db.LoaiPhongs.AnyAsync(l => l.MaLoaiPhong == phong.MaLoaiPhong))
            ModelState.AddModelError(nameof(phong.MaLoaiPhong), "Loại phòng không tồn tại.");

        if (!string.IsNullOrEmpty(phong.GioiTinhPhong) && !GioiTinh.TatCa.Contains(phong.GioiTinhPhong))
            ModelState.AddModelError(nameof(phong.GioiTinhPhong), "Giới tính phòng không hợp lệ.");
        if (!string.IsNullOrEmpty(phong.TrangThai) && !TrangThaiPhong.TatCa.Contains(phong.TrangThai))
            ModelState.AddModelError(nameof(phong.TrangThai), "Trạng thái không hợp lệ.");
    }

    private async Task NapDuLieuSelectAsync(int? maLoaiPhongDangChon)
    {
        // Chỉ cho chọn loại phòng đang hoạt động (cộng thêm loại đang gán cho phòng khi sửa)
        var loaiPhongs = await _db.LoaiPhongs.AsNoTracking()
            .Where(l => l.TrangThai == TrangThaiLoaiPhong.HoatDong || l.MaLoaiPhong == maLoaiPhongDangChon)
            .OrderBy(l => l.SoNguoiToiDa)
            .Select(l => new { l.MaLoaiPhong, l.TenLoaiPhong, l.SoNguoiToiDa })
            .ToListAsync();
        ViewBag.LoaiPhongs = loaiPhongs
            .Select(l => new SelectListItem($"{l.TenLoaiPhong} ({l.SoNguoiToiDa} người)", l.MaLoaiPhong.ToString()))
            .ToList();
        ViewBag.GioiTinhs = GioiTinh.TatCa.Select(g => new SelectListItem(g, g)).ToList();
        ViewBag.TrangThais = TrangThaiPhong.TatCa.Select(t => new SelectListItem(t, t)).ToList();
    }
}
