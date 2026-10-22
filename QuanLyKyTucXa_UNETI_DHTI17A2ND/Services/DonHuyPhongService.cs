using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

/// <summary>Nghiệp vụ đơn xin hủy phòng: sinh viên gửi/thu hồi, Admin duyệt/từ chối.</summary>
public class DonHuyPhongService
{
    private readonly AppDbContext _db;
    private readonly PhanPhongService _phanPhongService;

    public DonHuyPhongService(AppDbContext db, PhanPhongService phanPhongService)
    {
        _db = db;
        _phanPhongService = phanPhongService;
    }

    // ---------------------------------------------------------------- SINH VIÊN GỬI ĐƠN
    public async Task<KetQuaNghiepVu> GuiDonAsync(string maSinhVien, int maDangKy, DateTime ngayRaDuKien, string lyDo)
    {
        lyDo = lyDo?.Trim();
        if (string.IsNullOrEmpty(lyDo))
            return KetQuaNghiepVu.Loi("Vui lòng nhập lý do hủy phòng.");

        // Đăng ký phải thuộc về chính sinh viên này
        var dangKy = await _db.DangKyKTXs
            .FirstOrDefaultAsync(d => d.MaDangKy == maDangKy && d.MaSinhVien == maSinhVien);
        if (dangKy == null)
            return KetQuaNghiepVu.Loi("Không tìm thấy đăng ký của bạn.");

        if (dangKy.TrangThai != TrangThaiDangKy.DaPhanPhong)
            return KetQuaNghiepVu.Loi($"Chỉ gửi được đơn hủy phòng khi đăng ký ở trạng thái \"{TrangThaiDangKy.DaPhanPhong}\" (hiện tại: {dangKy.TrangThai}).");

        var hienTai = await _db.PhanPhongs
            .FirstOrDefaultAsync(pp => pp.MaDangKy == maDangKy && pp.TrangThai == TrangThaiPhanPhong.DangO);
        if (hienTai == null)
            return KetQuaNghiepVu.Loi("Bạn hiện không ở phòng nào.");

        if (await _db.DonHuyPhongs.AnyAsync(d => d.MaDangKy == maDangKy && d.TrangThai == TrangThaiDonHuy.ChoDuyet))
            return KetQuaNghiepVu.Loi("Bạn đã có một đơn hủy phòng đang chờ duyệt.");

        if (ngayRaDuKien.Date < DateTime.Today)
            return KetQuaNghiepVu.Loi("Ngày muốn trả phòng không được trước ngày hôm nay.");
        if (ngayRaDuKien.Date < hienTai.NgayVao.Date)
            return KetQuaNghiepVu.Loi("Ngày muốn trả phòng không được trước ngày bạn vào phòng.");

        _db.DonHuyPhongs.Add(new DonHuyPhong
        {
            MaDangKy = maDangKy,
            NgayGui = DateTime.Now,
            NgayRaDuKien = ngayRaDuKien.Date,
            LyDo = lyDo,
            TrangThai = TrangThaiDonHuy.ChoDuyet
        });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return KetQuaNghiepVu.Loi("Bạn đã có một đơn hủy phòng đang chờ duyệt.");
        }
        return KetQuaNghiepVu.Ok("Đã gửi đơn hủy phòng. Vui lòng chờ quản trị viên duyệt.");
    }

    // ---------------------------------------------------------------- SINH VIÊN THU HỒI ĐƠN
    public async Task<KetQuaNghiepVu> ThuHoiAsync(string maSinhVien, int maDon)
    {
        var don = await _db.DonHuyPhongs.Include(d => d.DangKyKTX)
            .FirstOrDefaultAsync(d => d.MaDon == maDon && d.DangKyKTX.MaSinhVien == maSinhVien);
        if (don == null)
            return KetQuaNghiepVu.Loi("Không tìm thấy đơn.");
        if (don.TrangThai != TrangThaiDonHuy.ChoDuyet)
            return KetQuaNghiepVu.Loi($"Đơn đã ở trạng thái \"{don.TrangThai}\", không thể thu hồi.");

        don.TrangThai = TrangThaiDonHuy.DaThuHoi;
        don.NgayXuLy = DateTime.Now;
        await _db.SaveChangesAsync();
        return KetQuaNghiepVu.Ok("Đã thu hồi đơn hủy phòng.");
    }

    // ---------------------------------------------------------------- ADMIN DUYỆT
    public async Task<KetQuaNghiepVu> DuyetAsync(int maDon, int maTaiKhoanAdmin, DateTime ngayRa, string ghiChu)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        var don = await _db.DonHuyPhongs.Include(d => d.DangKyKTX)
            .FirstOrDefaultAsync(d => d.MaDon == maDon);
        if (don == null)
            return KetQuaNghiepVu.Loi("Không tìm thấy đơn.");
        if (don.TrangThai != TrangThaiDonHuy.ChoDuyet)
            return KetQuaNghiepVu.Loi($"Đơn đã được xử lý (\"{don.TrangThai}\").");

        var hienTai = await _db.PhanPhongs
            .FirstOrDefaultAsync(pp => pp.MaDangKy == don.MaDangKy && pp.TrangThai == TrangThaiPhanPhong.DangO);
        if (hienTai == null)
            return KetQuaNghiepVu.Loi("Sinh viên không còn ở phòng nào. Hãy từ chối đơn này.");

        // Dùng lại nghiệp vụ trả phòng có sẵn (cùng transaction)
        var kq = await _phanPhongService.TraPhongAsync(hienTai.MaPhanPhong, ngayRa);
        if (!kq.ThanhCong) return kq;

        don.TrangThai = TrangThaiDonHuy.DaDuyet;
        don.NgayXuLy = DateTime.Now;
        don.MaTaiKhoanXuLy = maTaiKhoanAdmin;
        don.GhiChuAdmin = string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu.Trim();

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return KetQuaNghiepVu.Ok($"Đã duyệt đơn #{don.MaDon}. {kq.ThongBao}");
    }

    // ---------------------------------------------------------------- ADMIN TỪ CHỐI
    public async Task<KetQuaNghiepVu> TuChoiAsync(int maDon, int maTaiKhoanAdmin, string lyDoTuChoi)
    {
        if (string.IsNullOrWhiteSpace(lyDoTuChoi))
            return KetQuaNghiepVu.Loi("Vui lòng nhập lý do từ chối để sinh viên biết.");

        var don = await _db.DonHuyPhongs.FirstOrDefaultAsync(d => d.MaDon == maDon);
        if (don == null)
            return KetQuaNghiepVu.Loi("Không tìm thấy đơn.");
        if (don.TrangThai != TrangThaiDonHuy.ChoDuyet)
            return KetQuaNghiepVu.Loi($"Đơn đã được xử lý (\"{don.TrangThai}\").");

        don.TrangThai = TrangThaiDonHuy.TuChoi;
        don.NgayXuLy = DateTime.Now;
        don.MaTaiKhoanXuLy = maTaiKhoanAdmin;
        don.GhiChuAdmin = lyDoTuChoi.Trim();
        await _db.SaveChangesAsync();
        return KetQuaNghiepVu.Ok($"Đã từ chối đơn #{don.MaDon}.");
    }
}