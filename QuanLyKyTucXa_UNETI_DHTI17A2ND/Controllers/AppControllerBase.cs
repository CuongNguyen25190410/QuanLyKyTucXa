// Họ và tên: [Nguyễn Văn Cường]
// Mã sinh viên: [23203100019]
// Nội dung thực hiện: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Controllers;

/// <summary>Controller nền: cung cấp thông tin người đăng nhập lấy từ Session (không lấy từ URL/Form).</summary>
public abstract class AppControllerBase : Controller
{
    protected int? MaTaiKhoanHienTai => HttpContext.Session.GetInt32("MaTaiKhoan");
    protected string VaiTroHienTai => HttpContext.Session.GetString("VaiTro");
    protected bool LaAdmin => VaiTroHienTai == VaiTro.Admin;

    /// <summary>
    /// Lấy hồ sơ sinh viên của CHÍNH tài khoản đang đăng nhập.
    /// MaSinhVien luôn được xác định từ tài khoản/Session, không nhận từ URL
    /// nên sinh viên không thể xem hay sửa dữ liệu của người khác bằng cách đổi mã.
    /// </summary>
    protected async Task<SinhVien> LaySinhVienHienTaiAsync(AppDbContext db)
    {
        var ma = MaTaiKhoanHienTai;
        if (ma == null) return null;
        return await db.SinhViens.FirstOrDefaultAsync(s => s.MaTaiKhoan == ma.Value);
    }
}
