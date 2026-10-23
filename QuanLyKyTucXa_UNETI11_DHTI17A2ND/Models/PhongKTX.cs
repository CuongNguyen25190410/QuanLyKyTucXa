// Họ và tên: [Trần Hoàng Sơn]
// Mã sinh viên: [23203100010]
// Nội dung thực hiện: Module 2 - Quản lý phòng KTX, tìm kiếm, lọc, sắp xếp, phân trang
using System.ComponentModel.DataAnnotations;

namespace QuanLyKyTucXa_UNETI11_DHTI17A2ND.Models;

/// <summary>
/// Phòng ký túc xá. KHÔNG có trường SoChoCon: số chỗ còn được tính bằng LINQ
/// = LoaiPhong.SoNguoiToiDa - (số PhanPhong có TrangThai = "Đang ở").
/// </summary>
public class PhongKTX
{
    [Key]
    [Display(Name = "Mã phòng")]
    public int MaPhong { get; set; }

    [Required(ErrorMessage = "Tên phòng là bắt buộc")]
    [StringLength(50, ErrorMessage = "Tên phòng tối đa 50 ký tự")]
    [Display(Name = "Tên phòng")]
    public string TenPhong { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn loại phòng")]
    [Display(Name = "Loại phòng")]
    public int? MaLoaiPhong { get; set; }

    [Required(ErrorMessage = "Tòa nhà là bắt buộc")]
    [StringLength(50, ErrorMessage = "Tòa nhà tối đa 50 ký tự")]
    [Display(Name = "Tòa nhà")]
    public string ToaNha { get; set; }

    [Range(1, 30, ErrorMessage = "Tầng phải từ 1 đến 30")]
    [Display(Name = "Tầng")]
    public int Tang { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn giới tính phòng")]
    [StringLength(10)]
    [Display(Name = "Giới tính phòng")]
    public string GioiTinhPhong { get; set; }

    [Required(ErrorMessage = "Trạng thái là bắt buộc")]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; }

    [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
    [Display(Name = "Mô tả")]
    public string MoTa { get; set; }

    // Navigation
    public LoaiPhong LoaiPhong { get; set; }
    public ICollection<PhanPhong> PhanPhongs { get; set; } = new List<PhanPhong>();
}
