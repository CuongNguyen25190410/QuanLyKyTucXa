// Họ và tên: [ĐIỀN HỌ TÊN SV3]
// Mã sinh viên: [ĐIỀN MÃ SV3]
// Nội dung thực hiện: Module 3 - Quản lý sinh viên, đăng ký ở KTX, xem thông tin/lịch sử ở
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Filters;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models.ViewModels;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Controllers;

// Quản lý hồ sơ sinh viên: chỉ Admin. Sinh viên xem thông tin của mình ở CaNhanController.
[PhanQuyen(VaiTro.Admin)]
public class SinhVienController : AppControllerBase
{
    private const int PageSize = 10;

    private readonly AppDbContext _db;
    private readonly IPasswordHasher<TaiKhoan> _hasher;

    public SinhVienController(AppDbContext db, IPasswordHasher<TaiKhoan> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<IActionResult> Index(string tuKhoa, string gioiTinh, string trangThai, int page = 1)
    {
        var q = _db.SinhViens.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            tuKhoa = tuKhoa.Trim();
            q = q.Where(s => s.MaSinhVien.Contains(tuKhoa) || s.HoTen.Contains(tuKhoa) || s.Lop.Contains(tuKhoa));
        }
        if (!string.IsNullOrEmpty(gioiTinh)) q = q.Where(s => s.GioiTinh == gioiTinh);
        if (!string.IsNullOrEmpty(trangThai)) q = q.Where(s => s.TrangThai == trangThai);
        q = q.OrderBy(s => s.MaSinhVien);

        ViewBag.TuKhoa = tuKhoa;
        ViewBag.GioiTinh = gioiTinh;
        ViewBag.TrangThai = trangThai;

        var ketQua = await PagedResult<SinhVien>.CreateAsync(q, page, PageSize);

        // Sinh viên nào đang ở phòng (một truy vấn gom cho cả trang, tránh truy vấn lặp từng dòng)
        var cacMa = ketQua.Items.Select(s => s.MaSinhVien).ToList();
        ViewBag.DangO = await _db.PhanPhongs
            .Where(pp => pp.TrangThai == TrangThaiPhanPhong.DangO && cacMa.Contains(pp.DangKyKTX.MaSinhVien))
            .Select(pp => new { pp.DangKyKTX.MaSinhVien, pp.PhongKTX.TenPhong })
            .ToDictionaryAsync(x => x.MaSinhVien, x => x.TenPhong);

        return View(ketQua);
    }

    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();
        var sv = await _db.SinhViens.AsNoTracking().Include(s => s.TaiKhoan)
            .FirstOrDefaultAsync(s => s.MaSinhVien == id);
        if (sv == null) return NotFound();

        var vm = new SinhVienDetailsVM
        {
            SinhVien = sv,
            TaiKhoan = sv.TaiKhoan,
            DangKys = await _db.DangKyKTXs.AsNoTracking().Include(d => d.LoaiPhongNguyenVong)
                .Where(d => d.MaSinhVien == id).OrderByDescending(d => d.NgayDangKy).ToListAsync(),
            LichSuPhanPhong = await _db.PhanPhongs.AsNoTracking()
                .Include(pp => pp.PhongKTX).ThenInclude(p => p.LoaiPhong)
                .Where(pp => pp.DangKyKTX.MaSinhVien == id)
                .OrderByDescending(pp => pp.NgayVao).ThenByDescending(pp => pp.MaPhanPhong).ToListAsync()
        };
        // Số lần chuyển phòng = số bản ghi trạng thái "Đã chuyển"
        vm.SoLanChuyenPhong = vm.LichSuPhanPhong.Count(pp => pp.TrangThai == TrangThaiPhanPhong.DaChuyen);
        return View(vm);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new SinhVienFormVM { TrangThai = TrangThaiSinhVien.HoatDong, GioiTinh = GioiTinh.Nam });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SinhVienFormVM vm)
    {
        // Tài khoản đăng nhập là bắt buộc khi tạo mới sinh viên
        if (string.IsNullOrWhiteSpace(vm.TenDangNhap))
            ModelState.AddModelError(nameof(vm.TenDangNhap), "Tên đăng nhập là bắt buộc.");
        if (string.IsNullOrWhiteSpace(vm.MatKhau))
            ModelState.AddModelError(nameof(vm.MatKhau), "Mật khẩu là bắt buộc.");

        if (!string.IsNullOrWhiteSpace(vm.MaSinhVien)
            && await _db.SinhViens.AnyAsync(s => s.MaSinhVien == vm.MaSinhVien.Trim()))
            ModelState.AddModelError(nameof(vm.MaSinhVien), "Mã sinh viên đã tồn tại.");

        if (!string.IsNullOrWhiteSpace(vm.TenDangNhap)
            && await _db.TaiKhoans.AnyAsync(t => t.TenDangNhap == vm.TenDangNhap.Trim()))
            ModelState.AddModelError(nameof(vm.TenDangNhap), "Tên đăng nhập đã tồn tại.");

        KiemTraGiaTriChon(vm);
        if (!ModelState.IsValid) return View(vm);

        var taiKhoan = new TaiKhoan
        {
            TenDangNhap = vm.TenDangNhap.Trim(),
            HoTen = vm.HoTen.Trim(),
            Email = vm.Email.Trim(),
            VaiTro = VaiTro.SinhVien,
            TrangThai = vm.TrangThai == TrangThaiSinhVien.HoatDong ? TrangThaiTaiKhoan.HoatDong : TrangThaiTaiKhoan.Khoa
        };
        taiKhoan.MatKhau = _hasher.HashPassword(taiKhoan, vm.MatKhau);

        var sv = new SinhVien
        {
            MaSinhVien = vm.MaSinhVien.Trim(),
            HoTen = vm.HoTen.Trim(),
            NgaySinh = vm.NgaySinh.Value.Date,
            GioiTinh = vm.GioiTinh,
            Lop = vm.Lop,
            Khoa = vm.Khoa,
            SoDienThoai = vm.SoDienThoai,
            Email = vm.Email.Trim(),
            DiaChi = vm.DiaChi,
            TrangThai = vm.TrangThai,
            TaiKhoan = taiKhoan       // EF tự lưu TaiKhoan trước rồi gán MaTaiKhoan cho SinhVien
        };

        _db.SinhViens.Add(sv);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = $"Đã thêm sinh viên {sv.HoTen} và tạo tài khoản {taiKhoan.TenDangNhap}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();
        var sv = await _db.SinhViens.Include(s => s.TaiKhoan).FirstOrDefaultAsync(s => s.MaSinhVien == id);
        if (sv == null) return NotFound();

        return View(new SinhVienFormVM
        {
            LaSua = true,
            MaSinhVien = sv.MaSinhVien,
            HoTen = sv.HoTen,
            NgaySinh = sv.NgaySinh,
            GioiTinh = sv.GioiTinh,
            Lop = sv.Lop,
            Khoa = sv.Khoa,
            SoDienThoai = sv.SoDienThoai,
            Email = sv.Email,
            DiaChi = sv.DiaChi,
            TrangThai = sv.TrangThai,
            TenDangNhap = sv.TaiKhoan?.TenDangNhap
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, SinhVienFormVM vm)
    {
        if (id != vm.MaSinhVien) return NotFound();
        var sv = await _db.SinhViens.Include(s => s.TaiKhoan).FirstOrDefaultAsync(s => s.MaSinhVien == id);
        if (sv == null) return NotFound();

        vm.LaSua = true;
        vm.TenDangNhap = sv.TaiKhoan?.TenDangNhap;
        KiemTraGiaTriChon(vm);

        // Không cho "Ngừng hoạt động" khi sinh viên còn đang ở phòng
        if (vm.TrangThai == TrangThaiSinhVien.NgungHoatDong
            && await _db.PhanPhongs.AnyAsync(pp => pp.TrangThai == TrangThaiPhanPhong.DangO && pp.DangKyKTX.MaSinhVien == id))
            ModelState.AddModelError(nameof(vm.TrangThai), "Sinh viên đang ở phòng KTX, hãy trả phòng trước khi chuyển sang Ngừng hoạt động.");

        if (!ModelState.IsValid) return View(vm);

        sv.HoTen = vm.HoTen.Trim();
        sv.NgaySinh = vm.NgaySinh.Value.Date;
        sv.GioiTinh = vm.GioiTinh;
        sv.Lop = vm.Lop;
        sv.Khoa = vm.Khoa;
        sv.SoDienThoai = vm.SoDienThoai;
        sv.Email = vm.Email.Trim();
        sv.DiaChi = vm.DiaChi;
        sv.TrangThai = vm.TrangThai;

        // Đồng bộ tài khoản: ngừng hoạt động -> khóa tài khoản (dữ liệu lịch sử vẫn giữ nguyên)
        if (sv.TaiKhoan != null)
        {
            sv.TaiKhoan.HoTen = sv.HoTen;
            sv.TaiKhoan.Email = sv.Email;
            sv.TaiKhoan.TrangThai = sv.TrangThai == TrangThaiSinhVien.HoatDong ? TrangThaiTaiKhoan.HoatDong : TrangThaiTaiKhoan.Khoa;
        }

        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã cập nhật sinh viên.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();
        var sv = await _db.SinhViens.AsNoTracking().FirstOrDefaultAsync(s => s.MaSinhVien == id);
        if (sv == null) return NotFound();
        ViewBag.SoDangKy = await _db.DangKyKTXs.CountAsync(d => d.MaSinhVien == id);
        return View(sv);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var sv = await _db.SinhViens.Include(s => s.TaiKhoan).FirstOrDefaultAsync(s => s.MaSinhVien == id);
        if (sv == null) return NotFound();

        // Không xóa sinh viên đã có đăng ký/phân phòng: sẽ làm mất lịch sử (đề mục 9.2)
        var soDangKy = await _db.DangKyKTXs.CountAsync(d => d.MaSinhVien == id);
        if (soDangKy > 0)
        {
            TempData["Loi"] = $"Không thể xóa sinh viên {sv.HoTen}: đã có {soDangKy} đăng ký KTX. Hãy chuyển trạng thái sang \"Ngừng hoạt động\" để khóa tài khoản và giữ lịch sử.";
            return RedirectToAction(nameof(Index));
        }

        var taiKhoan = sv.TaiKhoan;
        _db.SinhViens.Remove(sv);
        if (taiKhoan != null) _db.TaiKhoans.Remove(taiKhoan);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã xóa sinh viên.";
        return RedirectToAction(nameof(Index));
    }

    private void KiemTraGiaTriChon(SinhVienFormVM vm)
    {
        if (!string.IsNullOrEmpty(vm.GioiTinh) && !GioiTinh.TatCa.Contains(vm.GioiTinh))
            ModelState.AddModelError(nameof(vm.GioiTinh), "Giới tính không hợp lệ.");
        if (vm.TrangThai != TrangThaiSinhVien.HoatDong && vm.TrangThai != TrangThaiSinhVien.NgungHoatDong)
            ModelState.AddModelError(nameof(vm.TrangThai), "Trạng thái không hợp lệ.");
    }
}
