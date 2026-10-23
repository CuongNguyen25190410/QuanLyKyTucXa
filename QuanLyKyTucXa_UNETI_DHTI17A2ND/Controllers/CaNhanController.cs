// Họ và tên: [Trần Công Quang Đăng]
// Mã sinh viên: [23203100040]
// Nội dung thực hiện: Module 3 - Quản lý sinh viên, đăng ký ở KTX, xem thông tin/lịch sử ở
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Filters;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models.ViewModels;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Controllers;

/// <summary>
/// Trang dành cho sinh viên xem DỮ LIỆU CỦA CHÍNH MÌNH. Không Action nào nhận mã sinh viên từ URL:
/// mọi truy vấn đều ràng buộc theo tài khoản đang đăng nhập nên không thể xem chéo dữ liệu.
/// </summary>
[PhanQuyen(VaiTro.SinhVien)]
public class CaNhanController : AppControllerBase
{
    private readonly AppDbContext _db;

    public CaNhanController(AppDbContext db)
    {
        _db = db;
    }

    // Tổng hợp: tài khoản + hồ sơ + đăng ký hiện tại + phòng đang ở (đề mục 7.4, 7.6)
    public async Task<IActionResult> Index()
    {
        var sv = await _db.SinhViens.AsNoTracking().Include(s => s.TaiKhoan)
            .FirstOrDefaultAsync(s => s.MaTaiKhoan == MaTaiKhoanHienTai.Value);
        if (sv == null) return View(new CaNhanVM());

        var vm = new CaNhanVM
        {
            SinhVien = sv,
            TaiKhoan = sv.TaiKhoan,
            SoLanDangKy = await _db.DangKyKTXs.CountAsync(d => d.MaSinhVien == sv.MaSinhVien),
            DangKyChoDuyet = await _db.DangKyKTXs.AsNoTracking().Include(d => d.LoaiPhongNguyenVong)
                .FirstOrDefaultAsync(d => d.MaSinhVien == sv.MaSinhVien && d.TrangThai == TrangThaiDangKy.ChoDuyet),
            DangKyHienTai = await _db.DangKyKTXs.AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaSinhVien == sv.MaSinhVien && d.TrangThai == TrangThaiDangKy.DaPhanPhong),
            PhongHienTai = await _db.PhanPhongs.AsNoTracking()
                .Include(pp => pp.PhongKTX).ThenInclude(p => p.LoaiPhong)
                .Include(pp => pp.DangKyKTX)
                .FirstOrDefaultAsync(pp => pp.TrangThai == TrangThaiPhanPhong.DangO && pp.DangKyKTX.MaSinhVien == sv.MaSinhVien)
        };
        return View(vm);
    }

    // Lịch sử ở KTX: toàn bộ các lần ở phòng, kể cả phòng đã chuyển/đã trả
    public async Task<IActionResult> LichSu()
    {
        var sv = await LaySinhVienHienTaiAsync(_db);
        if (sv == null) return View(new List<PhanPhong>());

        var ds = await _db.PhanPhongs.AsNoTracking()
            .Include(pp => pp.PhongKTX).ThenInclude(p => p.LoaiPhong)
            .Include(pp => pp.DangKyKTX)
            .Where(pp => pp.DangKyKTX.MaSinhVien == sv.MaSinhVien)
            .OrderByDescending(pp => pp.NgayVao).ThenByDescending(pp => pp.MaPhanPhong)
            .ToListAsync();
        return View(ds);
    }
}
