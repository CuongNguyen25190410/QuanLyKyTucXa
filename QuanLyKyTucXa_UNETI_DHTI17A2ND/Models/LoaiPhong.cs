// Họ và tên: [ĐIỀN HỌ TÊN SV1]
// Mã sinh viên: [ĐIỀN MÃ SV1]
// Nội dung thực hiện: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

public class LoaiPhong
{
    [Key]
    [Display(Name = "Mã loại phòng")]
    public int MaLoaiPhong { get; set; }

    [Required(ErrorMessage = "Tên loại phòng là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên loại phòng tối đa 100 ký tự")]
    [Display(Name = "Tên loại phòng")]
    public string TenLoaiPhong { get; set; }

    [Range(1, 100, ErrorMessage = "Số người tối đa phải lớn hơn 0 (tối đa 100)")]
    [Display(Name = "Số người tối đa")]
    public int SoNguoiToiDa { get; set; }

    [Range(1, 1000000000, ErrorMessage = "Đơn giá tháng phải lớn hơn 0")]
    [Column(TypeName = "decimal(18,0)")]
    [Display(Name = "Đơn giá/tháng")]
    public decimal DonGiaThang { get; set; }

    [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
    [Display(Name = "Mô tả")]
    public string MoTa { get; set; }

    [Required(ErrorMessage = "Trạng thái là bắt buộc")]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; }

    // LoaiPhong (1) - (n) PhongKTX
    public ICollection<PhongKTX> PhongKTXs { get; set; } = new List<PhongKTX>();
}
