// Họ và tên: [ĐIỀN HỌ TÊN SV4]
// Mã sinh viên: [ĐIỀN MÃ SV4]
// Nội dung thực hiện: Module 4 - Quản lý đăng ký, phân phòng, chuyển phòng, trả phòng, Dashboard, thống kê
using System.ComponentModel.DataAnnotations;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

/// <summary>
/// Lịch sử ở thực tế. Chuyển phòng/trả phòng KHÔNG sửa MaPhong hay xóa bản ghi,
/// mà đổi trạng thái bản ghi cũ, ghi NgayRa và (nếu chuyển) tạo bản ghi mới.
/// </summary>
public class PhanPhong
{
    [Key]
    [Display(Name = "Mã phân phòng")]
    public int MaPhanPhong { get; set; }

    [Display(Name = "Mã đăng ký")]
    public int MaDangKy { get; set; }

    [Display(Name = "Phòng")]
    public int MaPhong { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Ngày vào")]
    public DateTime NgayVao { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Ngày ra")]
    public DateTime? NgayRa { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; }

    // Navigation
    public DangKyKTX DangKyKTX { get; set; }
    public PhongKTX PhongKTX { get; set; }
}
