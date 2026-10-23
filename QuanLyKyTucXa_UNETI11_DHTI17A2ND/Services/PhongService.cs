// Họ và tên: [Trần Hoàng Sơn]
// Mã sinh viên: [23203100010]
// Nội dung thực hiện: Module 2 - Quản lý phòng KTX, tìm kiếm, lọc, sắp xếp, phân trang
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Models.ViewModels;

namespace QuanLyKyTucXa_UNETI11_DHTI17A2ND.Services;

/// <summary>
/// Các truy vấn LINQ dùng chung về phòng. Điểm quan trọng nhất của đề:
///     SoChoCon = SoNguoiToiDa (của LoaiPhong) - số PhanPhong có TrangThai = "Đang ở"
/// được TÍNH khi truy vấn, không lưu và không cho nhập tay.
/// </summary>
public class PhongService
{
    private readonly AppDbContext _db;

    public PhongService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Truy vấn phòng kèm sức chứa, số người đang ở và số chỗ còn.
    /// Trả về IQueryable nên có thể nối tiếp Where/OrderBy/Skip/Take mà vẫn dịch ra SQL.
    /// </summary>
    public IQueryable<PhongItemVM> QueryPhong()
    {
        return _db.PhongKTXs.AsNoTracking().Select(p => new PhongItemVM
        {
            MaPhong = p.MaPhong,
            TenPhong = p.TenPhong,
            MaLoaiPhong = p.MaLoaiPhong ?? 0,
            TenLoaiPhong = p.LoaiPhong.TenLoaiPhong,
            DonGiaThang = p.LoaiPhong.DonGiaThang,
            ToaNha = p.ToaNha,
            Tang = p.Tang,
            GioiTinhPhong = p.GioiTinhPhong,
            TrangThai = p.TrangThai,
            SucChua = p.LoaiPhong.SoNguoiToiDa,
            SoDangO = p.PhanPhongs.Count(pp => pp.TrangThai == TrangThaiPhanPhong.DangO),
            SoChoCon = p.LoaiPhong.SoNguoiToiDa
                       - p.PhanPhongs.Count(pp => pp.TrangThai == TrangThaiPhanPhong.DangO)
        });
    }

    /// <summary>Các phòng Admin có thể chọn để phân/chuyển: đang sử dụng và còn chỗ (đề mục 8.7).</summary>
    public IQueryable<PhongItemVM> QueryPhongConCho()
    {
        return QueryPhong().Where(p => p.TrangThai == TrangThaiPhong.DangSuDung && p.SoChoCon > 0);
    }

    /// <summary>Số người đang ở = đếm PhanPhong có trạng thái Đang ở.</summary>
    public Task<int> DemSoNguoiDangOAsync(int maPhong)
    {
        return _db.PhanPhongs.CountAsync(pp => pp.MaPhong == maPhong && pp.TrangThai == TrangThaiPhanPhong.DangO);
    }

    /// <summary>Phòng đã đầy khi số người đang ở >= số người tối đa của loại phòng.</summary>
    public async Task<bool> PhongDayAsync(int maPhong)
    {
        var sucChua = await _db.PhongKTXs.Where(p => p.MaPhong == maPhong)
            .Select(p => p.LoaiPhong.SoNguoiToiDa).FirstOrDefaultAsync();
        var dangO = await DemSoNguoiDangOAsync(maPhong);
        return dangO >= sucChua;
    }

    /// <summary>Sinh viên đã có phòng (đang ở) hay chưa.</summary>
    public Task<bool> SinhVienDangOAsync(string maSinhVien)
    {
        return _db.PhanPhongs.AnyAsync(pp => pp.TrangThai == TrangThaiPhanPhong.DangO
                                             && pp.DangKyKTX.MaSinhVien == maSinhVien);
    }
}
