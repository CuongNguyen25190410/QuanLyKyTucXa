// Họ và tên: [Trần Công Quang Đăng]
// Mã sinh viên: [23203100040]
// Nội dung thực hiện: Module 3 - Quản lý sinh viên, đăng ký ở KTX, xem thông tin/lịch sử ở
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Filters;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Helpers;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Models;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Models.ViewModels;

namespace QuanLyKyTucXa_UNETI11_DHTI17A2ND.Controllers;

[PhanQuyen]
public class DangKyKTXController : AppControllerBase
{
    private const int PageSize = 10;
    private readonly AppDbContext _db;

    public DangKyKTXController(AppDbContext db)
    {
        _db = db;
    }

    // =========================================================== PHÍA SINH VIÊN
    /// <summary>Các đăng ký của chính sinh viên đang đăng nhập (mã sinh viên lấy từ Session/tài khoản).</summary>
    [PhanQuyen(VaiTro.SinhVien)]
    public async Task<IActionResult> CuaToi()
    {

        var sv = await LaySinhVienHienTaiAsync(_db);
        if (sv == null) return View(new List<DangKyKTX>());

        var ds = await _db.DangKyKTXs.AsNoTracking()
            .Include(d => d.PhongNguyenVong)
            .Include(d => d.LoaiPhongNguyenVong)
            .Include(d => d.DonHuyPhongs)
            .Include(d => d.PhanPhongs).ThenInclude(pp => pp.PhongKTX)
            .Where(d => d.MaSinhVien == sv.MaSinhVien)          // ràng buộc theo tài khoản đăng nhập
            .OrderByDescending(d => d.NgayDangKy)
            .ToListAsync();
        return View(ds);
    }

    [PhanQuyen(VaiTro.SinhVien)]
    [HttpGet]
    public async Task<IActionResult> Create(int? maLoaiPhong, int? maPhong)
    {
        var vm = new DangKyCreateVM
        {
            NgayBatDau = DateTime.Today,
            NgayKetThucDuKien = DateTime.Today.AddMonths(4),
            NguyenVongLoaiPhong = maLoaiPhong
        };

        // Sinh viên chọn một phòng cụ thể trên sơ đồ
        if (maPhong.HasValue)
        {
            var phong = await _db.PhongKTXs.AsNoTracking().Include(p => p.LoaiPhong)
                .FirstOrDefaultAsync(p => p.MaPhong == maPhong.Value);
            if (phong != null)
            {
                vm.NguyenVongPhong = phong.MaPhong;
                vm.PhongNguyenVong = phong;
                vm.NguyenVongLoaiPhong = phong.MaLoaiPhong;   // loại phòng theo đúng phòng đã chọn
            }
        }

        await NapLoaiPhongAsync(vm);
        return View(vm);
    }

    [PhanQuyen(VaiTro.SinhVien)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DangKyCreateVM vm)
    {
        // Mã sinh viên được xác định từ tài khoản đăng nhập, KHÔNG nhận từ Form/URL
        var sv = await LaySinhVienHienTaiAsync(_db);
        if (sv == null)
            ModelState.AddModelError(string.Empty, "Tài khoản của bạn chưa có hồ sơ sinh viên. Vui lòng liên hệ quản trị viên.");
        else
        {
            // 1. Sinh viên tồn tại và đang hoạt động
            if (sv.TrangThai != TrangThaiSinhVien.HoatDong)
                ModelState.AddModelError(string.Empty, "Hồ sơ sinh viên của bạn không ở trạng thái hoạt động.");

            // 2. Không có đăng ký khác đang Đã phân phòng
            if (await _db.DangKyKTXs.AnyAsync(d => d.MaSinhVien == sv.MaSinhVien && d.TrangThai == TrangThaiDangKy.DaPhanPhong))
                ModelState.AddModelError(string.Empty, "Bạn đang được phân phòng KTX, không thể đăng ký thêm. Hãy trả phòng trước nếu muốn đăng ký lại.");

            // 3. Không tạo nhiều đăng ký Chờ duyệt đồng thời
            if (await _db.DangKyKTXs.AnyAsync(d => d.MaSinhVien == sv.MaSinhVien && d.TrangThai == TrangThaiDangKy.ChoDuyet))
                ModelState.AddModelError(string.Empty, "Bạn đã có một đăng ký đang chờ duyệt. Hãy hủy đăng ký đó trước khi gửi đăng ký mới.");
        }

        // 4. Kiểm tra ngày
        if (vm.NgayBatDau.HasValue)
        {
            if (vm.NgayBatDau.Value.Date < DateTime.Today)
                ModelState.AddModelError(nameof(vm.NgayBatDau), "Ngày bắt đầu không được trước ngày hôm nay.");
            else if (vm.NgayBatDau.Value.Date > DateTime.Today.AddYears(1))
                ModelState.AddModelError(nameof(vm.NgayBatDau), "Ngày bắt đầu không được quá 1 năm kể từ hôm nay.");
        }
        if (vm.NgayBatDau.HasValue && vm.NgayKetThucDuKien.HasValue
            && vm.NgayKetThucDuKien.Value.Date <= vm.NgayBatDau.Value.Date)
            ModelState.AddModelError(nameof(vm.NgayKetThucDuKien), "Ngày kết thúc dự kiến phải sau ngày bắt đầu.");
        // 4b. Phòng nguyện vọng (chọn từ sơ đồ): phải tồn tại, đang sử dụng, đúng giới tính
        if (vm.NguyenVongPhong.HasValue)
        {
            var phong = await _db.PhongKTXs.AsNoTracking().Include(p => p.LoaiPhong)
                .FirstOrDefaultAsync(p => p.MaPhong == vm.NguyenVongPhong.Value);
            if (phong == null)
            {
                ModelState.AddModelError(string.Empty, "Phòng bạn chọn không tồn tại.");
                vm.NguyenVongPhong = null;
            }
            else
            {
                vm.PhongNguyenVong = phong;
                if (phong.TrangThai != TrangThaiPhong.DangSuDung)
                    ModelState.AddModelError(string.Empty, $"Phòng {phong.TenPhong} đang tạm ngừng, không thể chọn.");
                else if (sv != null && phong.GioiTinhPhong != sv.GioiTinh)
                    ModelState.AddModelError(string.Empty, $"Phòng {phong.TenPhong} là phòng {phong.GioiTinhPhong}, không phù hợp với giới tính của bạn.");

                vm.NguyenVongLoaiPhong = phong.MaLoaiPhong;   // luôn đồng bộ loại phòng theo phòng đã chọn
            }
        }
        // 5. Nguyện vọng loại phòng (nếu chọn) phải tồn tại
        if (vm.NguyenVongLoaiPhong.HasValue
            && !await _db.LoaiPhongs.AnyAsync(l => l.MaLoaiPhong == vm.NguyenVongLoaiPhong))
            ModelState.AddModelError(nameof(vm.NguyenVongLoaiPhong), "Loại phòng không tồn tại.");

        if (!ModelState.IsValid)
        {
            await NapLoaiPhongAsync(vm);
            return View(vm);
        }

        var dangKy = new DangKyKTX
        {
            NguyenVongPhong = vm.NguyenVongPhong,
            MaSinhVien = sv.MaSinhVien,
            NgayDangKy = DateTime.Now,                    // do hệ thống xác định
            NgayBatDau = vm.NgayBatDau.Value.Date,
            NgayKetThucDuKien = vm.NgayKetThucDuKien.Value.Date,
            NguyenVongLoaiPhong = vm.NguyenVongLoaiPhong,
            GhiChu = vm.GhiChu,
            TrangThai = TrangThaiDangKy.ChoDuyet           // trạng thái do hệ thống quyết định
        };

        try
        {
            _db.DangKyKTXs.Add(dangKy);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Chỉ mục duy nhất ở CSDL chặn trường hợp gửi trùng đồng thời (bấm hai lần)
            ModelState.AddModelError(string.Empty, "Bạn đã có đăng ký đang hiệu lực. Vui lòng kiểm tra lại.");
            await NapLoaiPhongAsync(vm);
            return View(vm);
        }

        TempData["ThanhCong"] = "Đã gửi đăng ký ở KTX. Vui lòng chờ quản trị viên duyệt.";
        return RedirectToAction(nameof(CuaToi));
    }

    // =========================================================== HỦY (sinh viên hủy của mình, Admin hủy bất kỳ)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Huy(int id)
    {
        var dangKy = await _db.DangKyKTXs.FirstOrDefaultAsync(d => d.MaDangKy == id);
        if (dangKy == null) return NotFound();

        if (!LaAdmin)
        {
            // Sinh viên chỉ được hủy đăng ký của chính mình
            var sv = await LaySinhVienHienTaiAsync(_db);
            if (sv == null || dangKy.MaSinhVien != sv.MaSinhVien) return NotFound();
        }

        // Chỉ hủy được khi còn Chờ duyệt
        if (dangKy.TrangThai != TrangThaiDangKy.ChoDuyet)
        {
            TempData["Loi"] = $"Chỉ hủy được đăng ký ở trạng thái \"{TrangThaiDangKy.ChoDuyet}\" (hiện tại: {dangKy.TrangThai}).";
        }
        else
        {
            dangKy.TrangThai = TrangThaiDangKy.DaHuy;
            await _db.SaveChangesAsync();
            TempData["ThanhCong"] = "Đã hủy đăng ký.";
        }
        return LaAdmin ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(CuaToi));
    }

    // =========================================================== CHI TIẾT (dùng chung, có kiểm tra chủ sở hữu)
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var dangKy = await _db.DangKyKTXs.AsNoTracking()
            .Include(d => d.PhongNguyenVong)
            .Include(d => d.SinhVien)
            .Include(d => d.LoaiPhongNguyenVong)
            .FirstOrDefaultAsync(d => d.MaDangKy == id);
        if (dangKy == null) return NotFound();

        if (!LaAdmin)
        {
            // Đổi mã trên URL cũng không xem được đăng ký của người khác
            var sv = await LaySinhVienHienTaiAsync(_db);
            if (sv == null || dangKy.MaSinhVien != sv.MaSinhVien) return NotFound();
        }

        var vm = new DangKyDetailsVM
        {
            DangKy = dangKy,
            LaAdmin = LaAdmin,
            LichSu = await _db.PhanPhongs.AsNoTracking()
                .Include(pp => pp.PhongKTX).ThenInclude(p => p.LoaiPhong)
                .Where(pp => pp.MaDangKy == id)
                .OrderBy(pp => pp.NgayVao).ThenBy(pp => pp.MaPhanPhong).ToListAsync()
        };
        return View(vm);
    }

    // =========================================================== PHÍA ADMIN
    [PhanQuyen(VaiTro.Admin)]
    public async Task<IActionResult> Index(string tuKhoa, string trangThai, int page = 1)
    {
        var q = _db.DangKyKTXs.AsNoTracking()
            .Include(d => d.PhongNguyenVong)
            .Include(d => d.SinhVien)
            .Include(d => d.LoaiPhongNguyenVong)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            tuKhoa = tuKhoa.Trim();
            q = q.Where(d => d.MaSinhVien.Contains(tuKhoa) || d.SinhVien.HoTen.Contains(tuKhoa));
        }
        if (!string.IsNullOrEmpty(trangThai)) q = q.Where(d => d.TrangThai == trangThai);

        // Đăng ký chờ duyệt lên đầu để Admin xử lý trước
        q = q.OrderBy(d => d.TrangThai == TrangThaiDangKy.ChoDuyet ? 0 : 1).ThenByDescending(d => d.NgayDangKy);

        var vm = new DangKyIndexVM
        {
            TuKhoa = tuKhoa,
            TrangThai = trangThai,
            KetQua = await PagedResult<DangKyKTX>.CreateAsync(q, page, PageSize)
        };
        return View(vm);
    }

    [PhanQuyen(VaiTro.Admin)]
    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var dangKy = await _db.DangKyKTXs.AsNoTracking().Include(d => d.SinhVien)
            .FirstOrDefaultAsync(d => d.MaDangKy == id);
        if (dangKy == null) return NotFound();
        return View(dangKy);
    }

    [PhanQuyen(VaiTro.Admin)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var dangKy = await _db.DangKyKTXs.FirstOrDefaultAsync(d => d.MaDangKy == id);
        if (dangKy == null) return NotFound();

        // Đăng ký đã được phân phòng (hoặc có lịch sử phân phòng) thì không được xóa
        var coLichSu = await _db.PhanPhongs.AnyAsync(pp => pp.MaDangKy == id);
        if (coLichSu || dangKy.TrangThai == TrangThaiDangKy.DaPhanPhong || dangKy.TrangThai == TrangThaiDangKy.DaTraPhong)
        {
            TempData["Loi"] = "Không thể xóa đăng ký đã được phân phòng hoặc đã có lịch sử ở. Dữ liệu này cần được giữ lại.";
            return RedirectToAction(nameof(Index));
        }

        _db.DangKyKTXs.Remove(dangKy);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã xóa đăng ký.";
        return RedirectToAction(nameof(Index));
    }

    private async Task NapLoaiPhongAsync(DangKyCreateVM vm)
    {
        var ds = await _db.LoaiPhongs.AsNoTracking()
            .Where(l => l.TrangThai == TrangThaiLoaiPhong.HoatDong)
            .OrderBy(l => l.SoNguoiToiDa)
            .Select(l => new { l.MaLoaiPhong, l.TenLoaiPhong, l.SoNguoiToiDa, l.DonGiaThang })
            .ToListAsync();
        vm.LoaiPhongs = ds.Select(l => new SelectListItem(
            $"{l.TenLoaiPhong} ({l.SoNguoiToiDa} người - {Fmt.Tien(l.DonGiaThang)}/tháng)",
            l.MaLoaiPhong.ToString())).ToList();
    }
}
