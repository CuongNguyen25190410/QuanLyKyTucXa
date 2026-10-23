using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKyTucXa_UNETI11_DHTI17A2ND.Models;

/// <summary>Hóa đơn tiền phòng theo tháng của một đăng ký (một sinh viên).</summary>
public class HoaDon
{
    [Key]
    [Display(Name = "Mã hóa đơn")]
    public int MaHoaDon { get; set; }

    [Display(Name = "Mã đăng ký")]
    public int MaDangKy { get; set; }

    [Display(Name = "Phòng")]
    public int MaPhong { get; set; }

    [Range(1, 12)]
    [Display(Name = "Tháng")]
    public int Thang { get; set; }

    [Range(2000, 2100)]
    [Display(Name = "Năm")]
    public int Nam { get; set; }

    [Column(TypeName = "decimal(18,0)")]
    [Display(Name = "Số tiền")]
    public decimal SoTien { get; set; }

    [Display(Name = "Ngày tạo")]
    public DateTime NgayTao { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Hạn thanh toán")]
    public DateTime HanThanhToan { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; }

    // ---- Thông tin giao dịch qua cổng thanh toán
    public long? OrderCode { get; set; }

    [StringLength(64)]
    public string PaymentLinkId { get; set; }

    [StringLength(600)]
    public string QrCode { get; set; }

    [StringLength(300)]
    public string CheckoutUrl { get; set; }

    public DateTime? LinkHetHan { get; set; }

    [Display(Name = "Ngày thanh toán")]
    public DateTime? NgayThanhToan { get; set; }

    [StringLength(30)]
    [Display(Name = "Hình thức")]
    public string PhuongThuc { get; set; }

    [StringLength(100)]
    [Display(Name = "Mã giao dịch")]
    public string MaGiaoDich { get; set; }

    // Navigation
    public DangKyKTX DangKyKTX { get; set; }
    public PhongKTX PhongKTX { get; set; }
}