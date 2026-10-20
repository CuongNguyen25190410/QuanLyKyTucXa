// Họ và tên: [ĐIỀN HỌ TÊN CẢ NHÓM]
// Mã sinh viên: [ĐIỀN MÃ CẢ NHÓM]
// Nội dung thực hiện: Thành phần dùng chung của Project (cấu hình, DbContext, dữ liệu mẫu, layout...)
namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;

// Tập giá trị trạng thái dùng thống nhất toàn Project (đề mục 9.1):
// mọi Controller/View đều dùng các hằng số này, không gõ chuỗi trực tiếp.

public static class VaiTro
{
    public const string Admin = "Admin";
    public const string SinhVien = "Sinh viên";
}

public static class TrangThaiTaiKhoan
{
    public const string HoatDong = "Hoạt động";
    public const string Khoa = "Khóa";
}

public static class TrangThaiLoaiPhong
{
    public const string HoatDong = "Hoạt động";
    public const string NgungSuDung = "Ngừng sử dụng";
}

public static class TrangThaiPhong
{
    public const string DangSuDung = "Đang sử dụng";
    public const string TamNgung = "Tạm ngừng";
    public static readonly string[] TatCa = { DangSuDung, TamNgung };
}

public static class GioiTinh
{
    public const string Nam = "Nam";
    public const string Nu = "Nữ";
    public static readonly string[] TatCa = { Nam, Nu };
}

public static class TrangThaiSinhVien
{
    public const string HoatDong = "Đang hoạt động";
    public const string NgungHoatDong = "Ngừng hoạt động";
}

public static class TrangThaiDangKy
{
    public const string ChoDuyet = "Chờ duyệt";
    public const string DaPhanPhong = "Đã phân phòng";
    public const string DaTraPhong = "Đã trả phòng";
    public const string DaHuy = "Đã hủy";
    public static readonly string[] TatCa = { ChoDuyet, DaPhanPhong, DaTraPhong, DaHuy };
}

public static class TrangThaiPhanPhong
{
    public const string DangO = "Đang ở";
    public const string DaChuyen = "Đã chuyển";
    public const string DaTraPhong = "Đã trả phòng";
    public static readonly string[] TatCa = { DangO, DaChuyen, DaTraPhong };
}
public static class TrangThaiHoaDon
{
    public const string ChuaThanhToan = "Chưa thanh toán";
    public const string DaThanhToan = "Đã thanh toán";
    public const string DaHuy = "Đã hủy";
    public static readonly string[] TatCa = { ChuaThanhToan, DaThanhToan, DaHuy };
}