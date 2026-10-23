// Họ và tên: [CẢ NHÓM]
// Mã sinh viên: [CẢ NHÓM]
// Nội dung thực hiện: Thành phần dùng chung của Project (cấu hình, DbContext, dữ liệu mẫu, layout...)
using Microsoft.AspNetCore.Mvc;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Data;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Models;
namespace QuanLyKyTucXa_UNETI11_DHTI17A2ND.Controllers;

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

        // Thông tin lưu trú của sinh viên đang đăng nhập (lấy theo Session, không nhận mã từ URL)
        if (MaTaiKhoanHienTai != null && VaiTroHienTai == VaiTro.SinhVien)
        {
            var sv = await LaySinhVienHienTaiAsync(_db);
            if (sv != null)
            {
                ViewBag.PhongHienTai = await _db.PhanPhongs.AsNoTracking()
                    .Include(pp => pp.PhongKTX).ThenInclude(p => p.LoaiPhong)
                    .Include(pp => pp.DangKyKTX)
                    .FirstOrDefaultAsync(pp => pp.TrangThai == TrangThaiPhanPhong.DangO
                                               && pp.DangKyKTX.MaSinhVien == sv.MaSinhVien);

                ViewBag.DangKyChoDuyet = await _db.DangKyKTXs.AsNoTracking()
                    .FirstOrDefaultAsync(d => d.MaSinhVien == sv.MaSinhVien
                                              && d.TrangThai == TrangThaiDangKy.ChoDuyet);
            }
        }

        return View();
    }

    public IActionResult Error()
    {
        return View();
    }
}
