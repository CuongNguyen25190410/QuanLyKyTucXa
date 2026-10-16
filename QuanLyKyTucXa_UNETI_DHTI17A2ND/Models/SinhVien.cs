// Họ và tên: [ĐIỀN HỌ TÊN SV3]
// Mã sinh viên: [ĐIỀN MÃ SV3]
// Nội dung thực hiện: Module 3 - Quản lý sinh viên, đăng ký ở KTX, xem thông tin/lịch sử ở
using System.ComponentModel.DataAnnotations;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

public class SinhVien
{
    [Key]
    [Required(ErrorMessage = "Mã sinh viên là bắt buộc")]
    [StringLength(20, ErrorMessage = "Mã sinh viên tối đa 20 ký tự")]
    [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "Mã sinh viên chỉ gồm chữ và số")]
    [Display(Name = "Mã sinh viên")]
    public string MaSinhVien { get; set; }

    // FK 1-1 tới TaiKhoan
    public int MaTaiKhoan { get; set; }

    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; }

    [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
    [DataType(DataType.Date)]
    [NgaySinhHopLe]
    [Display(Name = "Ngày sinh")]
    public DateTime NgaySinh { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn giới tính")]
    [StringLength(10)]
    [Display(Name = "Giới tính")]
    public string GioiTinh { get; set; }

    [StringLength(30, ErrorMessage = "Lớp tối đa 30 ký tự")]
    [Display(Name = "Lớp")]
    public string Lop { get; set; }

    [StringLength(100, ErrorMessage = "Khoa tối đa 100 ký tự")]
    [Display(Name = "Khoa")]
    public string Khoa { get; set; }

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 chữ số, bắt đầu bằng 0")]
    [Display(Name = "Số điện thoại")]
    public string SoDienThoai { get; set; }

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [StringLength(100)]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [StringLength(200, ErrorMessage = "Địa chỉ tối đa 200 ký tự")]
    [Display(Name = "Địa chỉ")]
    public string DiaChi { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; }

    // Navigation
    public TaiKhoan TaiKhoan { get; set; }
    public ICollection<DangKyKTX> DangKyKTXs { get; set; } = new List<DangKyKTX>();
}
