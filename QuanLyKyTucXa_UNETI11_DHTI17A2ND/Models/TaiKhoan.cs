// Họ và tên: [Nguyễn Văn Cường]
// Mã sinh viên: [23203100019]
// Nội dung thực hiện: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKyTucXa_UNETI11_DHTI17A2ND.Models;

/// <summary>Tài khoản đăng nhập. MatKhau lưu dạng băm (hash), không lưu mật khẩu thô.</summary>
public class TaiKhoan
{
    [Key]
    public int MaTaiKhoan { get; set; }

    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    [StringLength(50, ErrorMessage = "Tên đăng nhập tối đa 50 ký tự")]
    [Display(Name = "Tên đăng nhập")]
    public string TenDangNhap { get; set; }

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [StringLength(255)]
    [Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; }

    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; }

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [StringLength(100)]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Vai trò")]
    public string VaiTro { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; }

    // Navigation: TaiKhoan (1) - (1) SinhVien
    public SinhVien SinhVien { get; set; }
}
