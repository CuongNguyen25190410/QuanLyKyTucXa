// Họ và tên: [Nguyễn Văn Cường]
// Mã sinh viên: [23203100019]
// Nội dung thực hiện: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Filters;

/// <summary>
/// Kiểm tra đăng nhập + phân quyền TẠI CONTROLLER (đề mục 5.3), không chỉ ẩn menu ở View.
///  - [PhanQuyen]                    : chỉ cần đã đăng nhập.
///  - [PhanQuyen(VaiTro.Admin)]      : chỉ Admin.
///  - [PhanQuyen(VaiTro.SinhVien)]   : chỉ Sinh viên.
/// Vai trò KHÔNG lấy từ dữ liệu client gửi lên: filter đọc MaTaiKhoan trong Session
/// rồi tra lại tài khoản trong Database (vai trò + trạng thái khóa) ở mỗi request.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class PhanQuyenAttribute : ActionFilterAttribute
{
    private readonly string[] _vaiTros;

    public PhanQuyenAttribute(params string[] vaiTros)
    {
        _vaiTros = vaiTros ?? Array.Empty<string>();
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;
        var maTaiKhoan = http.Session.GetInt32("MaTaiKhoan");

        // 1. Chưa đăng nhập (hoặc Session đã bị xóa sau khi đăng xuất)
        if (maTaiKhoan == null)
        {
            string returnUrl = http.Request.Method == "GET" ? http.Request.Path + http.Request.QueryString : null;
            context.Result = new RedirectToActionResult("Login", "TaiKhoan", new { returnUrl });
            return;
        }

        // 2. Tra lại tài khoản trong Database
        var db = http.RequestServices.GetRequiredService<AppDbContext>();
        var taiKhoan = await db.TaiKhoans.AsNoTracking()
            .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan.Value);

        if (taiKhoan == null || taiKhoan.TrangThai != TrangThaiTaiKhoan.HoatDong)
        {
            http.Session.Clear();
            if (context.Controller is Controller c)
                c.TempData["Loi"] = "Tài khoản không tồn tại hoặc đã bị khóa. Vui lòng liên hệ quản trị viên.";
            context.Result = new RedirectToActionResult("Login", "TaiKhoan", null);
            return;
        }

        // đồng bộ lại Session theo dữ liệu thật trong DB
        if (http.Session.GetString("VaiTro") != taiKhoan.VaiTro)
            http.Session.SetString("VaiTro", taiKhoan.VaiTro);

        // 3. Kiểm tra vai trò
        if (_vaiTros.Length > 0 && !_vaiTros.Contains(taiKhoan.VaiTro))
        {
            context.Result = new RedirectToActionResult("TruyCapBiTuChoi", "TaiKhoan", null);
            return;
        }

        await next();
    }
}
