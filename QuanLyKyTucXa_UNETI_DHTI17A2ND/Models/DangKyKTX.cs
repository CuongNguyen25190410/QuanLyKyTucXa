// Họ và tên: [ĐIỀN HỌ TÊN SV3]
// Mã sinh viên: [ĐIỀN MÃ SV3]
// Nội dung thực hiện: Module 3 - Quản lý sinh viên, đăng ký ở KTX, xem thông tin/lịch sử ở
using System.ComponentModel.DataAnnotations;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

/// <summary>
/// Một yêu cầu xin ở KTX. Đăng ký CHƯA đồng nghĩa với việc đã được xếp phòng:
/// việc xếp phòng được lưu ở bảng PhanPhong (đề mục 7.5).
/// </summary>
public class DangKyKTX
{
    [Key]
    [Display(Name = "Mã đăng ký")]
    public int MaDangKy { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Mã sinh viên")]
    public string MaSinhVien { get; set; }

    [Display(Name = "Ngày đăng ký")]
    public DateTime NgayDangKy { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Ngày bắt đầu")]
    public DateTime NgayBatDau { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Ngày kết thúc dự kiến")]
    public DateTime NgayKetThucDuKien { get; set; }

    [Display(Name = "Nguyện vọng loại phòng")]
    public int? NguyenVongLoaiPhong { get; set; }
    [Display(Name = "Phòng nguyện vọng")]
    public int? NguyenVongPhong { get; set; }

    [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
    [Display(Name = "Ghi chú")]
    public string GhiChu { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; }
    public PhongKTX PhongNguyenVong { get; set; }
    // Navigation
    public SinhVien SinhVien { get; set; }
    public LoaiPhong LoaiPhongNguyenVong { get; set; }
    public ICollection<PhanPhong> PhanPhongs { get; set; } = new List<PhanPhong>();
    public ICollection<DonHuyPhong> DonHuyPhongs { get; set; } = new List<DonHuyPhong>();
}
