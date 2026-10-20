using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

/// <summary>Nghiệp vụ hóa đơn tiền phòng + thanh toán QR qua cổng thanh toán.</summary>
public class HoaDonService
{
    public static readonly TimeSpan ThoiHanLink = TimeSpan.FromMinutes(30);

    private readonly AppDbContext _db;
    private readonly ICongThanhToan _cong;

    public HoaDonService(AppDbContext db, ICongThanhToan cong)
    {
        _db = db;
        _cong = cong;
    }

    /// <summary>Nội dung chuyển khoản ngắn gọn (tối đa 9 ký tự: HD + mã hóa đơn).</summary>
    public static string NoiDungChuyenKhoan(int maHoaDon) => $"HD{maHoaDon}";

    // ------------------------------------------------------------------ ADMIN: tạo hóa đơn theo tháng
    public async Task<int> TaoHoaDonThangAsync(int thang, int nam)
    {
        var cuoiThang = new DateTime(nam, thang, 1).AddMonths(1).AddDays(-1);

        var dangO = await _db.PhanPhongs.AsNoTracking()
            .Where(pp => pp.TrangThai == TrangThaiPhanPhong.DangO && pp.NgayVao <= cuoiThang)
            .Select(pp => new { pp.MaDangKy, pp.MaPhong, Gia = pp.PhongKTX.LoaiPhong.DonGiaThang })
            .ToListAsync();

        var daCo = (await _db.HoaDons.Where(h => h.Thang == thang && h.Nam == nam)
            .Select(h => h.MaDangKy).ToListAsync()).ToHashSet();

        var han = new DateTime(nam, thang, 10);
        var dem = 0;
        foreach (var x in dangO)
        {
            if (daCo.Contains(x.MaDangKy)) continue;
            _db.HoaDons.Add(new HoaDon
            {
                MaDangKy = x.MaDangKy,
                MaPhong = x.MaPhong,
                Thang = thang,
                Nam = nam,
                SoTien = x.Gia,
                NgayTao = DateTime.Now,
                HanThanhToan = han,
                TrangThai = TrangThaiHoaDon.ChuaThanhToan
            });
            dem++;
        }
        if (dem > 0) await _db.SaveChangesAsync();
        return dem;
    }

    // ------------------------------------------------------------------ SINH VIÊN: tạo/lấy mã QR
    public async Task<(HoaDon HoaDon, string Loi)> KhoiTaoThanhToanAsync(int maHoaDon, string maSinhVien, string returnUrl, string cancelUrl)
    {
        var hd = await _db.HoaDons
            .Include(h => h.PhongKTX)
            .Include(h => h.DangKyKTX).ThenInclude(d => d.SinhVien)
            .FirstOrDefaultAsync(h => h.MaHoaDon == maHoaDon);

        // Hóa đơn phải thuộc về chính sinh viên đang đăng nhập
        if (hd == null || hd.DangKyKTX.MaSinhVien != maSinhVien) return (null, "Không tìm thấy hóa đơn.");
        if (hd.TrangThai == TrangThaiHoaDon.DaThanhToan) return (hd, null);
        if (hd.TrangThai != TrangThaiHoaDon.ChuaThanhToan) return (hd, "Hóa đơn đã bị hủy, không thể thanh toán.");

        // Kiểm tra link cũ đã được trả chưa trước khi tạo link mới (tránh trả tiền hai lần)
        await DongBoAsync(hd);
        if (hd.TrangThai == TrangThaiHoaDon.DaThanhToan) return (hd, null);

        // Link cũ còn hạn thì dùng lại
        if (hd.OrderCode != null && hd.QrCode != null && hd.LinkHetHan > DateTime.Now.AddMinutes(1))
            return (hd, null);

        var orderCode = SinhOrderCode();
        var hetHan = DateTime.Now.Add(ThoiHanLink);
        var ket = await _cong.TaoLinkAsync(
            orderCode, (long)hd.SoTien, NoiDungChuyenKhoan(hd.MaHoaDon),
            $"Tien phong KTX {hd.Thang:00}/{hd.Nam}", hd.DangKyKTX.SinhVien.HoTen,
            returnUrl, cancelUrl, hetHan);

        hd.OrderCode = orderCode;
        hd.PaymentLinkId = ket.PaymentLinkId;
        hd.QrCode = ket.QrCode;
        hd.CheckoutUrl = ket.CheckoutUrl;
        hd.LinkHetHan = hetHan;
        await _db.SaveChangesAsync();
        return (hd, null);
    }

    // ------------------------------------------------------------------ Đồng bộ trạng thái với cổng thanh toán
    public async Task DongBoAsync(HoaDon hd)
    {
        if (hd.TrangThai != TrangThaiHoaDon.ChuaThanhToan || hd.OrderCode == null) return;

        string st;
        try { st = await _cong.LayTrangThaiAsync(hd.OrderCode.Value); }
        catch (LoiCongThanhToanException) { return; }

        if (st == "PAID")
        {
            await DanhDauDaThanhToanAsync(hd, _cong.Ten, null);
        }
        else if (st == "CANCELLED" || st == "EXPIRED")
        {
            hd.QrCode = null;          // lần mở trang sau sẽ tạo mã mới
            hd.LinkHetHan = null;
            await _db.SaveChangesAsync();
        }
    }

    // ------------------------------------------------------------------ Webhook payOS (đã kiểm tra chữ ký ở Controller)
    public async Task<bool> XacNhanTuWebhookAsync(long orderCode, long soTien, string maGiaoDich)
    {
        var hd = await _db.HoaDons.FirstOrDefaultAsync(h => h.OrderCode == orderCode);
        if (hd == null) return false;
        if (hd.TrangThai == TrangThaiHoaDon.DaThanhToan) return true;
        if (hd.TrangThai != TrangThaiHoaDon.ChuaThanhToan) return false;
        if ((long)hd.SoTien != soTien) return false;         // số tiền phải khớp hóa đơn

        await DanhDauDaThanhToanAsync(hd, "payOS", maGiaoDich);
        return true;
    }

    // ------------------------------------------------------------------ Giả lập (chỉ khi dùng MockGateway)
    public async Task<KetQuaNghiepVu> GiaLapThanhToanAsync(int maHoaDon, string maSinhVien)
    {
        if (!_cong.LaGiaLap) return KetQuaNghiepVu.Loi("Chức năng giả lập chỉ dùng ở chế độ Mock.");

        var hd = await _db.HoaDons.Include(h => h.DangKyKTX).FirstOrDefaultAsync(h => h.MaHoaDon == maHoaDon);
        if (hd == null || hd.DangKyKTX.MaSinhVien != maSinhVien) return KetQuaNghiepVu.Loi("Không tìm thấy hóa đơn.");
        if (hd.TrangThai != TrangThaiHoaDon.ChuaThanhToan) return KetQuaNghiepVu.Loi("Hóa đơn không ở trạng thái chờ thanh toán.");

        await DanhDauDaThanhToanAsync(hd, "Giả lập", "MOCK-" + DateTime.Now.Ticks);
        return KetQuaNghiepVu.Ok("Đã giả lập thanh toán thành công.");
    }

    // ------------------------------------------------------------------ ADMIN: xác nhận thu thủ công / hủy hóa đơn
    public async Task<KetQuaNghiepVu> XacNhanThuCongAsync(int maHoaDon)
    {
        var hd = await _db.HoaDons.FirstOrDefaultAsync(h => h.MaHoaDon == maHoaDon);
        if (hd == null) return KetQuaNghiepVu.Loi("Không tìm thấy hóa đơn.");
        if (hd.TrangThai != TrangThaiHoaDon.ChuaThanhToan) return KetQuaNghiepVu.Loi("Chỉ xác nhận được hóa đơn chưa thanh toán.");

        await DanhDauDaThanhToanAsync(hd, "Thu trực tiếp", null);
        return KetQuaNghiepVu.Ok($"Đã xác nhận thu hóa đơn #{hd.MaHoaDon}.");
    }

    public async Task<KetQuaNghiepVu> HuyAsync(int maHoaDon)
    {
        var hd = await _db.HoaDons.FirstOrDefaultAsync(h => h.MaHoaDon == maHoaDon);
        if (hd == null) return KetQuaNghiepVu.Loi("Không tìm thấy hóa đơn.");

        await DongBoAsync(hd);   // nếu sinh viên vừa trả xong thì không hủy nữa
        if (hd.TrangThai != TrangThaiHoaDon.ChuaThanhToan) return KetQuaNghiepVu.Loi($"Hóa đơn đang ở trạng thái \"{hd.TrangThai}\", không thể hủy.");

        hd.TrangThai = TrangThaiHoaDon.DaHuy;
        await _db.SaveChangesAsync();
        return KetQuaNghiepVu.Ok($"Đã hủy hóa đơn #{hd.MaHoaDon}.");
    }

    // ------------------------------------------------------------------ Hàm hỗ trợ
    private async Task DanhDauDaThanhToanAsync(HoaDon hd, string phuongThuc, string maGiaoDich)
    {
        hd.TrangThai = TrangThaiHoaDon.DaThanhToan;
        hd.NgayThanhToan = DateTime.Now;
        hd.PhuongThuc = phuongThuc;
        hd.MaGiaoDich = maGiaoDich;
        await _db.SaveChangesAsync();
    }

    /// <summary>Mã đơn duy nhất (15 chữ số, nhỏ hơn giới hạn 9007199254740991 của payOS).</summary>
    private static long SinhOrderCode() => long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff"));
}