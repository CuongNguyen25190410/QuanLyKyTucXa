// Họ và tên: [ĐIỀN HỌ TÊN SV3]
// Mã sinh viên: [ĐIỀN MÃ SV3]
// Nội dung thực hiện: Module 3 - Quản lý sinh viên, đăng ký ở KTX, xem thông tin/lịch sử ở
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Models.ViewModels;

public class DangKyCreateVM
{
    [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu ở")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày bắt đầu")]
    public DateTime? NgayBatDau { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc dự kiến")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày kết thúc dự kiến")]
    public DateTime? NgayKetThucDuKien { get; set; }

    [Display(Name = "Nguyện vọng loại phòng")]
    public int? NguyenVongLoaiPhong { get; set; }

    [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
    [Display(Name = "Ghi chú")]
    public string GhiChu { get; set; }

    public List<SelectListItem> LoaiPhongs { get; set; } = new();
}

public class DangKyIndexVM
{
    public string TuKhoa { get; set; }
    public string TrangThai { get; set; }
    public Helpers.PagedResult<DangKyKTX> KetQua { get; set; }
}

public class DangKyDetailsVM
{
    public DangKyKTX DangKy { get; set; }
    public List<PhanPhong> LichSu { get; set; } = new();
    public bool LaAdmin { get; set; }
}
