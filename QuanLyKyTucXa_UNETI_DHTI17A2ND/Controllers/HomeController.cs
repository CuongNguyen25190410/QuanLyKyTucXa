// Họ và tên: [ĐIỀN HỌ TÊN CẢ NHÓM]
// Mã sinh viên: [ĐIỀN MÃ CẢ NHÓM]
// Nội dung thực hiện: Thành phần dùng chung của Project (cấu hình, DbContext, dữ liệu mẫu, layout...)
using Microsoft.AspNetCore.Mvc;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using Microsoft.EntityFrameworkCore;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Controllers;

public class HomeController : AppControllerBase
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    // Trang chủ: ai cũng xem được; nội dung thay đổi theo vai trò (lấy từ Session)
    public async Task<IActionResult> Index()
    {
        ViewBag.SoPhongConCho = await _db.PhongKTXs
            .CountAsync(p => p.TrangThai == TrangThaiPhong.DangSuDung
                             && p.LoaiPhong.SoNguoiToiDa > p.PhanPhongs.Count(pp => pp.TrangThai == TrangThaiPhanPhong.DangO));
        return View();
    }

    public IActionResult Error()
    {
        return View();
    }
}
