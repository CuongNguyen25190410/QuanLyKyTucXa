// Họ và tên: [ĐIỀN HỌ TÊN CẢ NHÓM]
// Mã sinh viên: [ĐIỀN MÃ CẢ NHÓM]
// Nội dung thực hiện: Thành phần dùng chung của Project (cấu hình, DbContext, dữ liệu mẫu, layout...)
using System.Globalization;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;

/// <summary>Định dạng ngày, tiền và màu nhãn trạng thái dùng chung cho các View.</summary>
public static class Fmt
{
    private static readonly CultureInfo Vn = CultureInfo.GetCultureInfo("vi-VN");

    public static string Ngay(DateTime? d) => d.HasValue ? d.Value.ToString("dd/MM/yyyy") : "—";

    public static string NgayGio(DateTime? d) => d.HasValue ? d.Value.ToString("dd/MM/yyyy HH:mm") : "—";

    public static string Tien(decimal d) => d.ToString("N0", Vn) + " đ";

    public static string Badge(string trangThai) => trangThai switch
    {
        TrangThaiPhong.DangSuDung => "bg-success",
        TrangThaiPhong.TamNgung => "bg-secondary",
        TrangThaiTaiKhoan.HoatDong => "bg-success",
        TrangThaiTaiKhoan.Khoa => "bg-danger",
        TrangThaiSinhVien.HoatDong => "bg-success",
        TrangThaiSinhVien.NgungHoatDong => "bg-secondary",
        TrangThaiDangKy.ChoDuyet => "bg-warning text-dark",
        TrangThaiDangKy.DaPhanPhong => "bg-success",
        TrangThaiDangKy.DaTraPhong => "bg-secondary",
        TrangThaiDangKy.DaHuy => "bg-danger",
        TrangThaiPhanPhong.DangO => "bg-success",
        TrangThaiPhanPhong.DaChuyen => "bg-info text-dark",
        _ => "bg-secondary"
    };
}
