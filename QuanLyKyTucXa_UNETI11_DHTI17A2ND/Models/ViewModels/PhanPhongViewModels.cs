// Họ và tên: [Hà Việt Anh]
// Mã sinh viên: [23203100046]
// Nội dung thực hiện: Module 4 - Quản lý đăng ký, phân phòng, chuyển phòng, trả phòng, Dashboard, thống kê
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyKyTucXa_UNETI11_DHTI17A2ND.Models.ViewModels;

/// <summary>Màn hình Admin chọn phòng cho một đăng ký Chờ duyệt (đề mục 8.7).</summary>
public class PhanPhongCreateVM
{
    public int MaDangKy { get; set; }
    public DangKyKTX DangKy { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn một phòng")]
    [Display(Name = "Phòng")]
    public int? MaPhong { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày vào")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày vào")]
    public DateTime? NgayVao { get; set; }

    /// <summary>Các phòng có thể chọn: đang sử dụng và còn chỗ.</summary>
    public List<PhongItemVM> PhongCoTheChon { get; set; } = new();
}

public class ChuyenPhongVM
{
    public int MaPhanPhong { get; set; }
    public PhanPhong HienTai { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn phòng mới")]
    [Display(Name = "Phòng mới")]
    public int? MaPhongMoi { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày chuyển")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày chuyển")]
    public DateTime? NgayChuyen { get; set; }

    public List<SelectListItem> PhongCoTheChon { get; set; } = new();
}

public class TraPhongVM
{
    public int MaPhanPhong { get; set; }
    public PhanPhong HienTai { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày trả phòng")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày trả phòng")]
    public DateTime? NgayRa { get; set; }
}

public class PhanPhongIndexVM
{
    public string TuKhoa { get; set; }
    public string TrangThai { get; set; }
    public Helpers.PagedResult<PhanPhong> KetQua { get; set; }
}
