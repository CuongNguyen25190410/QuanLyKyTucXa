using System.ComponentModel.DataAnnotations;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

/// <summary>Đơn xin hủy phòng của sinh viên đang ở KTX, chờ Admin duyệt.</summary>
public class DonHuyPhong
{
    [Key]
    [Display(Name = "Mã đơn")]
    public int MaDon { get; set; }

    [Display(Name = "Mã đăng ký")]
    public int MaDangKy { get; set; }

    [Display(Name = "Ngày gửi")]
    public DateTime NgayGui { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Ngày muốn trả phòng")]
    public DateTime NgayRaDuKien { get; set; }

    [Required]
    [StringLength(500)]
    [Display(Name = "Lý do")]
    public string LyDo { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; }

    [Display(Name = "Ngày xử lý")]
    public DateTime? NgayXuLy { get; set; }

    public int? MaTaiKhoanXuLy { get; set; }

    [StringLength(500)]
    [Display(Name = "Phản hồi của quản trị viên")]
    public string GhiChuAdmin { get; set; }

    // Navigation
    public DangKyKTX DangKyKTX { get; set; }
    public TaiKhoan TaiKhoanXuLy { get; set; }
}