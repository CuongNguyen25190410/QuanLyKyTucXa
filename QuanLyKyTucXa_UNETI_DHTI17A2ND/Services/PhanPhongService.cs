// Họ và tên: [Hà Việt Anh]
// Mã sinh viên: [23203100046]
// Nội dung thực hiện: Module 4 - Quản lý đăng ký, phân phòng, chuyển phòng, trả phòng, Dashboard, thống kê
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

public record KetQuaNghiepVu(bool ThanhCong, string ThongBao)
{
    public static KetQuaNghiepVu Ok(string thongBao) => new(true, thongBao);
    public static KetQuaNghiepVu Loi(string thongBao) => new(false, thongBao);
}

/// <summary>
/// Nghiệp vụ phân phòng / chuyển phòng / trả phòng (đề mục 8).
/// Mỗi thao tác chạy trong một transaction mức Serializable và KIỂM TRA LẠI số chỗ
/// ngay trước SaveChanges, nhằm tránh trường hợp dữ liệu đã đổi giữa lúc mở View và lúc xác nhận.
/// </summary>
public class PhanPhongService
{
    private readonly AppDbContext _db;

    public PhanPhongService(AppDbContext db)
    {
        _db = db;
    }

    // ------------------------------------------------------------------ PHÂN PHÒNG
    public async Task<KetQuaNghiepVu> PhanPhongAsync(int maDangKy, int maPhong, DateTime ngayVao)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        var dangKy = await _db.DangKyKTXs.Include(d => d.SinhVien)
            .FirstOrDefaultAsync(d => d.MaDangKy == maDangKy);
        if (dangKy == null)
            return KetQuaNghiepVu.Loi("Không tìm thấy đăng ký KTX.");

        // 1. Đăng ký phải đang Chờ duyệt
        if (dangKy.TrangThai != TrangThaiDangKy.ChoDuyet)
            return KetQuaNghiepVu.Loi($"Đăng ký đang ở trạng thái \"{dangKy.TrangThai}\", chỉ phân phòng cho đăng ký \"{TrangThaiDangKy.ChoDuyet}\".");

        if (dangKy.SinhVien.TrangThai != TrangThaiSinhVien.HoatDong)
            return KetQuaNghiepVu.Loi("Sinh viên không còn ở trạng thái hoạt động.");

        // 2. Phòng tồn tại và đang hoạt động
        var phong = await _db.PhongKTXs.Include(p => p.LoaiPhong).FirstOrDefaultAsync(p => p.MaPhong == maPhong);
        if (phong == null)
            return KetQuaNghiepVu.Loi("Phòng được chọn không tồn tại.");
        if (phong.TrangThai != TrangThaiPhong.DangSuDung)
            return KetQuaNghiepVu.Loi($"Phòng {phong.TenPhong} đang tạm ngừng, không thể phân phòng.");

        // 3. Giới tính phù hợp
        if (dangKy.SinhVien.GioiTinh != phong.GioiTinhPhong)
            return KetQuaNghiepVu.Loi($"Phòng {phong.TenPhong} là phòng {phong.GioiTinhPhong}, không phù hợp với sinh viên giới tính {dangKy.SinhVien.GioiTinh}.");

        // 4. Sinh viên chưa ở phòng khác (một sinh viên không ở hai phòng đồng thời)
        var daCoPhong = await _db.PhanPhongs.AnyAsync(pp => pp.TrangThai == TrangThaiPhanPhong.DangO
                                                           && pp.DangKyKTX.MaSinhVien == dangKy.MaSinhVien);
        if (daCoPhong)
            return KetQuaNghiepVu.Loi("Sinh viên này đang ở một phòng khác. Hãy dùng chức năng Chuyển phòng.");

        // 5. Phòng còn chỗ: Số người đang ở < Số người tối đa của loại phòng
        var soDangO = await _db.PhanPhongs.CountAsync(pp => pp.MaPhong == maPhong && pp.TrangThai == TrangThaiPhanPhong.DangO);
        if (soDangO >= phong.LoaiPhong.SoNguoiToiDa)
            return KetQuaNghiepVu.Loi($"Phòng {phong.TenPhong} đã đủ người ({soDangO}/{phong.LoaiPhong.SoNguoiToiDa}), không thể phân thêm.");

        // 6. Ngày vào hợp lệ
        if (ngayVao.Date > dangKy.NgayKetThucDuKien.Date)
            return KetQuaNghiepVu.Loi("Ngày vào không được sau ngày kết thúc dự kiến của đăng ký.");

        _db.PhanPhongs.Add(new PhanPhong
        {
            MaDangKy = dangKy.MaDangKy,
            MaPhong = phong.MaPhong,
            NgayVao = ngayVao.Date,
            NgayRa = null,
            TrangThai = TrangThaiPhanPhong.DangO
        });
        dangKy.TrangThai = TrangThaiDangKy.DaPhanPhong;

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return KetQuaNghiepVu.Ok($"Đã phân sinh viên {dangKy.SinhVien.HoTen} vào phòng {phong.TenPhong}.");
    }

    // ------------------------------------------------------------------ CHUYỂN PHÒNG
    public async Task<KetQuaNghiepVu> ChuyenPhongAsync(int maPhanPhong, int maPhongMoi, DateTime ngayChuyen)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        var hienTai = await _db.PhanPhongs
            .Include(pp => pp.PhongKTX)
            .Include(pp => pp.DangKyKTX).ThenInclude(d => d.SinhVien)
            .FirstOrDefaultAsync(pp => pp.MaPhanPhong == maPhanPhong);
        if (hienTai == null)
            return KetQuaNghiepVu.Loi("Không tìm thấy bản ghi phân phòng.");

        // Sinh viên phải đang có phòng
        if (hienTai.TrangThai != TrangThaiPhanPhong.DangO)
            return KetQuaNghiepVu.Loi("Sinh viên hiện không ở phòng nào (bản ghi này đã chuyển hoặc đã trả phòng).");

        // Phòng mới tồn tại, đang hoạt động, khác phòng hiện tại
        var phongMoi = await _db.PhongKTXs.Include(p => p.LoaiPhong).FirstOrDefaultAsync(p => p.MaPhong == maPhongMoi);
        if (phongMoi == null)
            return KetQuaNghiepVu.Loi("Phòng mới không tồn tại.");
        if (phongMoi.MaPhong == hienTai.MaPhong)
            return KetQuaNghiepVu.Loi("Phòng mới trùng với phòng sinh viên đang ở. Hãy chọn phòng khác.");
        if (phongMoi.TrangThai != TrangThaiPhong.DangSuDung)
            return KetQuaNghiepVu.Loi($"Phòng {phongMoi.TenPhong} đang tạm ngừng, không thể chuyển vào.");

        // Giới tính phù hợp
        var sinhVien = hienTai.DangKyKTX.SinhVien;
        if (sinhVien.GioiTinh != phongMoi.GioiTinhPhong)
            return KetQuaNghiepVu.Loi($"Phòng {phongMoi.TenPhong} là phòng {phongMoi.GioiTinhPhong}, không phù hợp với sinh viên giới tính {sinhVien.GioiTinh}.");

        // Phòng mới còn chỗ
        var soDangO = await _db.PhanPhongs.CountAsync(pp => pp.MaPhong == maPhongMoi && pp.TrangThai == TrangThaiPhanPhong.DangO);
        if (soDangO >= phongMoi.LoaiPhong.SoNguoiToiDa)
            return KetQuaNghiepVu.Loi($"Phòng {phongMoi.TenPhong} đã đủ người ({soDangO}/{phongMoi.LoaiPhong.SoNguoiToiDa}).");

        if (ngayChuyen.Date < hienTai.NgayVao.Date)
            return KetQuaNghiepVu.Loi("Ngày chuyển không được trước ngày sinh viên vào phòng hiện tại.");

        // Bản ghi cũ -> Đã chuyển + NgayRa (KHÔNG sửa MaPhong, KHÔNG xóa để giữ lịch sử)
        hienTai.TrangThai = TrangThaiPhanPhong.DaChuyen;
        hienTai.NgayRa = ngayChuyen.Date;
        // Lưu bản ghi cũ trước để không vi phạm chỉ mục "mỗi đăng ký chỉ 1 bản ghi Đang ở"
        await _db.SaveChangesAsync();

        // Tạo bản ghi mới ở phòng mới
        _db.PhanPhongs.Add(new PhanPhong
        {
            MaDangKy = hienTai.MaDangKy,
            MaPhong = phongMoi.MaPhong,
            NgayVao = ngayChuyen.Date,
            NgayRa = null,
            TrangThai = TrangThaiPhanPhong.DangO
        });
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return KetQuaNghiepVu.Ok($"Đã chuyển {sinhVien.HoTen} từ phòng {hienTai.PhongKTX.TenPhong} sang phòng {phongMoi.TenPhong}.");
    }

    // ------------------------------------------------------------------ TRẢ PHÒNG
    public async Task<KetQuaNghiepVu> TraPhongAsync(int maPhanPhong, DateTime ngayRa)
    {
        //await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        // Nếu nơi gọi đã mở transaction (ví dụ duyệt đơn hủy phòng) thì dùng tiếp
        var tuMoTransaction = _db.Database.CurrentTransaction == null;
        await using var tx = tuMoTransaction
            ? await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
            : null;

        var hienTai = await _db.PhanPhongs
            .Include(pp => pp.PhongKTX)
            .Include(pp => pp.DangKyKTX).ThenInclude(d => d.SinhVien)
            .FirstOrDefaultAsync(pp => pp.MaPhanPhong == maPhanPhong);
        if (hienTai == null)
            return KetQuaNghiepVu.Loi("Không tìm thấy bản ghi phân phòng.");

        // Không trả phòng hai lần: phải đang ở
        if (hienTai.TrangThai != TrangThaiPhanPhong.DangO)
            return KetQuaNghiepVu.Loi("Không thể trả phòng: sinh viên không còn ở phòng này (đã trả phòng hoặc đã chuyển).");

        if (ngayRa.Date < hienTai.NgayVao.Date)
            return KetQuaNghiepVu.Loi("Ngày trả phòng không được trước ngày vào phòng.");

        hienTai.TrangThai = TrangThaiPhanPhong.DaTraPhong;
        hienTai.NgayRa = ngayRa.Date;
        hienTai.TrangThai = TrangThaiPhanPhong.DaTraPhong;
        hienTai.NgayRa = ngayRa.Date;
        hienTai.DangKyKTX.TrangThai = TrangThaiDangKy.DaTraPhong;

        // Đóng các đơn xin hủy phòng đang chờ của đăng ký này (phòng đã được trả rồi)
        var donCho = await _db.DonHuyPhongs
            .Where(d => d.MaDangKy == hienTai.MaDangKy && d.TrangThai == TrangThaiDonHuy.ChoDuyet)
            .ToListAsync();
        foreach (var d in donCho)
        {
            d.TrangThai = TrangThaiDonHuy.DaDuyet;
            d.NgayXuLy = DateTime.Now;
            d.GhiChuAdmin = "Phòng đã được trả.";
        }

        await _db.SaveChangesAsync();
        if (tuMoTransaction) await tx.CommitAsync();

        return KetQuaNghiepVu.Ok($"Sinh viên {hienTai.DangKyKTX.SinhVien.HoTen} đã trả phòng {hienTai.PhongKTX.TenPhong}.");
    }
}
