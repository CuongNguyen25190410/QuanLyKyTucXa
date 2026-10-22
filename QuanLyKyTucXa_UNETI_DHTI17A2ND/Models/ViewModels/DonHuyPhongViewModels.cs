
using System.ComponentModel.DataAnnotations;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Models.ViewModels;

public class DonHuyCreateVM
{
    public int MaDangKy { get; set; }
    public PhanPhong HienTai { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày muốn trả phòng")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày muốn trả phòng")]
    public DateTime? NgayRaDuKien { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập lý do hủy phòng")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Lý do phải từ 10 đến 500 ký tự")]
    [Display(Name = "Lý do hủy phòng")]
    public string LyDo { get; set; }
}

public class DonHuyIndexVM
{
    public string TuKhoa { get; set; }
    public string TrangThai { get; set; }
    public int SoChoDuyet { get; set; }
    public PagedResult<DonHuyPhong> KetQua { get; set; }
}

public class DonHuyXuLyVM
{
    public int MaDon { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Ngày trả phòng")]
    public DateTime? NgayRa { get; set; }

    [StringLength(500, ErrorMessage = "Tối đa 500 ký tự")]
    [Display(Name = "Phản hồi cho sinh viên")]
    public string GhiChuAdmin { get; set; }

    // Dữ liệu hiển thị (không nhận từ form)
    public DonHuyPhong Don { get; set; }
    public PhanPhong HienTai { get; set; }
    public int SoHoaDonChuaTT { get; set; }
    public decimal TongTienChuaTT { get; set; }
}