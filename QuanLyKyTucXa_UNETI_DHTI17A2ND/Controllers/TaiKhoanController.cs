// Họ và tên: [ĐIỀN HỌ TÊN SV1]
// Mã sinh viên: [ĐIỀN MÃ SV1]
// Nội dung thực hiện: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
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

public class TaiKhoanController : AppControllerBase
{
    private const int PageSize = 10;

    private readonly AppDbContext _db;
    private readonly IPasswordHasher<TaiKhoan> _hasher;

    public TaiKhoanController(AppDbContext db, IPasswordHasher<TaiKhoan> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    // ================================================================ ĐĂNG NHẬP / ĐĂNG XUẤT
    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        if (MaTaiKhoanHienTai != null) return RedirectToAction("Index", "Home");
        return View(new LoginVM { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // Kiểm tra tài khoản bằng EF Core + LINQ
        var taiKhoan = await _db.TaiKhoans.FirstOrDefaultAsync(t => t.TenDangNhap == vm.TenDangNhap.Trim());
        if (taiKhoan == null)
        {
            ModelState.AddModelError(nameof(vm.TenDangNhap), "Tên đăng nhập không tồn tại.");
            return View(vm);
        }

        var kq = _hasher.VerifyHashedPassword(taiKhoan, taiKhoan.MatKhau, vm.MatKhau);
        if (kq == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(nameof(vm.MatKhau), "Mật khẩu không đúng.");
            return View(vm);
        }

        // Tài khoản bị khóa không được đăng nhập
        if (taiKhoan.TrangThai != TrangThaiTaiKhoan.HoatDong)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");
            return View(vm);
        }

        // Lưu tối thiểu: mã tài khoản, họ tên, vai trò vào Session
        HttpContext.Session.Clear();
        HttpContext.Session.SetInt32("MaTaiKhoan", taiKhoan.MaTaiKhoan);
        HttpContext.Session.SetString("HoTen", taiKhoan.HoTen);
        HttpContext.Session.SetString("VaiTro", taiKhoan.VaiTro);

        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return LocalRedirect(vm.ReturnUrl);

        return taiKhoan.VaiTro == VaiTro.Admin
            ? RedirectToAction("Index", "ThongKe")
            : RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();     // xóa Session: không dùng lại được Session cũ
        TempData["ThanhCong"] = "Bạn đã đăng xuất.";
        return RedirectToAction(nameof(Login));
    }

    public IActionResult TruyCapBiTuChoi()
    {
        Response.StatusCode = 403;
        return View();
    }

    // ================================================================ ĐỔI MẬT KHẨU (mọi người dùng đã đăng nhập)
    [PhanQuyen]
    [HttpGet]
    public IActionResult DoiMatKhau() => View(new DoiMatKhauVM());

    [PhanQuyen]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiMatKhau(DoiMatKhauVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var taiKhoan = await _db.TaiKhoans.FindAsync(MaTaiKhoanHienTai.Value);
        if (taiKhoan == null) return NotFound();

        if (_hasher.VerifyHashedPassword(taiKhoan, taiKhoan.MatKhau, vm.MatKhauCu) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(nameof(vm.MatKhauCu), "Mật khẩu hiện tại không đúng.");
            return View(vm);
        }

        taiKhoan.MatKhau = _hasher.HashPassword(taiKhoan, vm.MatKhauMoi);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đổi mật khẩu thành công.";
        return RedirectToAction("Index", "Home");
    }

    // ================================================================ QUẢN LÝ TÀI KHOẢN (chỉ Admin)
    [PhanQuyen(VaiTro.Admin)]
    public async Task<IActionResult> Index(string tuKhoa, string vaiTro, string trangThai, int page = 1)
    {
        var q = _db.TaiKhoans.AsNoTracking().Include(t => t.SinhVien).AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            tuKhoa = tuKhoa.Trim();
            q = q.Where(t => t.TenDangNhap.Contains(tuKhoa) || t.HoTen.Contains(tuKhoa) || t.Email.Contains(tuKhoa));
        }
        if (!string.IsNullOrEmpty(vaiTro)) q = q.Where(t => t.VaiTro == vaiTro);
        if (!string.IsNullOrEmpty(trangThai)) q = q.Where(t => t.TrangThai == trangThai);

        q = q.OrderBy(t => t.VaiTro).ThenBy(t => t.TenDangNhap);

        ViewBag.TuKhoa = tuKhoa;
        ViewBag.VaiTro = vaiTro;
        ViewBag.TrangThai = trangThai;
        return View(await PagedResult<TaiKhoan>.CreateAsync(q, page, PageSize));
    }

    [PhanQuyen(VaiTro.Admin)]
    [HttpGet]
    public IActionResult Create() => View(new TaiKhoanFormVM { VaiTro = VaiTro.Admin });

    [PhanQuyen(VaiTro.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaiKhoanFormVM vm)
    {
        if (string.IsNullOrWhiteSpace(vm.MatKhau))
            ModelState.AddModelError(nameof(vm.MatKhau), "Mật khẩu là bắt buộc.");

        if (!string.IsNullOrWhiteSpace(vm.TenDangNhap)
            && await _db.TaiKhoans.AnyAsync(t => t.TenDangNhap == vm.TenDangNhap.Trim()))
            ModelState.AddModelError(nameof(vm.TenDangNhap), "Tên đăng nhập đã tồn tại.");

        if (!ModelState.IsValid) return View(vm);

        var taiKhoan = new TaiKhoan
        {
            TenDangNhap = vm.TenDangNhap.Trim(),
            HoTen = vm.HoTen.Trim(),
            Email = vm.Email.Trim(),
            VaiTro = VaiTro.Admin,                 // vai trò do hệ thống quyết định, không nhận từ Form
            TrangThai = TrangThaiTaiKhoan.HoatDong
        };
        taiKhoan.MatKhau = _hasher.HashPassword(taiKhoan, vm.MatKhau);

        _db.TaiKhoans.Add(taiKhoan);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = $"Đã thêm tài khoản Admin {taiKhoan.TenDangNhap}.";
        return RedirectToAction(nameof(Index));
    }

    [PhanQuyen(VaiTro.Admin)]
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var tk = await _db.TaiKhoans.FindAsync(id.Value);
        if (tk == null) return NotFound();

        return View(new TaiKhoanFormVM
        {
            MaTaiKhoan = tk.MaTaiKhoan,
            TenDangNhap = tk.TenDangNhap,
            HoTen = tk.HoTen,
            Email = tk.Email,
            VaiTro = tk.VaiTro
        });
    }

    [PhanQuyen(VaiTro.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TaiKhoanFormVM vm)
    {
        if (id != vm.MaTaiKhoan) return NotFound();
        var tk = await _db.TaiKhoans.FindAsync(id);
        if (tk == null) return NotFound();

        // Tên đăng nhập và vai trò không đổi khi sửa -> bỏ qua kiểm tra các trường này
        ModelState.Remove(nameof(vm.TenDangNhap));
        vm.TenDangNhap = tk.TenDangNhap;
        vm.VaiTro = tk.VaiTro;

        if (!ModelState.IsValid) return View(vm);

        tk.HoTen = vm.HoTen.Trim();
        tk.Email = vm.Email.Trim();
        if (!string.IsNullOrWhiteSpace(vm.MatKhau))
            tk.MatKhau = _hasher.HashPassword(tk, vm.MatKhau);

        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = "Đã cập nhật tài khoản.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Khóa / mở khóa tài khoản. Khóa KHÔNG xóa dữ liệu: lịch sử đăng ký và phân phòng vẫn được giữ.</summary>
    [PhanQuyen(VaiTro.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiTrangThai(int id)
    {
        var tk = await _db.TaiKhoans.FindAsync(id);
        if (tk == null) return NotFound();

        if (tk.MaTaiKhoan == MaTaiKhoanHienTai)
        {
            TempData["Loi"] = "Không thể tự khóa tài khoản đang đăng nhập.";
            return RedirectToAction(nameof(Index));
        }

        tk.TrangThai = tk.TrangThai == TrangThaiTaiKhoan.HoatDong ? TrangThaiTaiKhoan.Khoa : TrangThaiTaiKhoan.HoatDong;
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = tk.TrangThai == TrangThaiTaiKhoan.Khoa
            ? $"Đã khóa tài khoản {tk.TenDangNhap}."
            : $"Đã mở khóa tài khoản {tk.TenDangNhap}.";
        return RedirectToAction(nameof(Index));
    }

    [PhanQuyen(VaiTro.Admin)]
    [HttpGet]
    public async Task<IActionResult> DatLaiMatKhau(int? id)
    {
        if (id == null) return NotFound();
        var tk = await _db.TaiKhoans.FindAsync(id.Value);
        if (tk == null) return NotFound();
        return View(new DatLaiMatKhauVM { MaTaiKhoan = tk.MaTaiKhoan, TenDangNhap = tk.TenDangNhap });
    }

    [PhanQuyen(VaiTro.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DatLaiMatKhau(DatLaiMatKhauVM vm)
    {
        var tk = await _db.TaiKhoans.FindAsync(vm.MaTaiKhoan);
        if (tk == null) return NotFound();
        vm.TenDangNhap = tk.TenDangNhap;
        if (!ModelState.IsValid) return View(vm);

        tk.MatKhau = _hasher.HashPassword(tk, vm.MatKhauMoi);
        await _db.SaveChangesAsync();
        TempData["ThanhCong"] = $"Đã đặt lại mật khẩu cho {tk.TenDangNhap}.";
        return RedirectToAction(nameof(Index));
    }
}
