// Họ và tên: [ĐIỀN HỌ TÊN SV2]
// Mã sinh viên: [ĐIỀN MÃ SV2]
// Nội dung thực hiện: Module 2 - Quản lý phòng KTX, tìm kiếm, lọc, sắp xếp, phân trang
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Models.ViewModels;

/// <summary>Một dòng trong danh sách phòng, đã kèm các giá trị tính toán.</summary>
public class PhongItemVM
{
    public int MaPhong { get; set; }
    public string TenPhong { get; set; }
    public int MaLoaiPhong { get; set; }
    public string TenLoaiPhong { get; set; }
    public decimal DonGiaThang { get; set; }
    public string ToaNha { get; set; }
    public int Tang { get; set; }
    public string GioiTinhPhong { get; set; }
    public string TrangThai { get; set; }

    /// <summary>Sức chứa = SoNguoiToiDa của loại phòng.</summary>
    public int SucChua { get; set; }

    /// <summary>Số PhanPhong đang ở trong phòng.</summary>
    public int SoDangO { get; set; }

    /// <summary>SoChoCon = SucChua - SoDangO (tính toán, không lưu).</summary>
    public int SoChoCon { get; set; }
}

/// <summary>Điều kiện truy vấn danh sách phòng; nhận từ query string để giữ nguyên khi chuyển trang.</summary>
public class PhongFilterVM
{
    public string TuKhoa { get; set; }
    public int? MaLoaiPhong { get; set; }
    public string ToaNha { get; set; }
    public int? Tang { get; set; }
    public string GioiTinh { get; set; }
    public string TrangThai { get; set; }
    public string ConCho { get; set; }              // "con" | "het" | rỗng
    public string SapXep { get; set; } = "ten_asc"; // ten_asc, ten_desc, tang_asc, tang_desc, cho_asc, cho_desc
    public int Page { get; set; } = 1;
}

public class PhongIndexVM
{
    public PhongFilterVM Filter { get; set; } = new();
    public PagedResult<PhongItemVM> KetQua { get; set; }
    public List<SelectListItem> LoaiPhongs { get; set; } = new();
    public List<SelectListItem> ToaNhas { get; set; } = new();
    public List<SelectListItem> Tangs { get; set; } = new();
    public bool LaAdmin { get; set; }
}

public class PhongDetailsVM
{
    public PhongItemVM Phong { get; set; }
    public string MoTa { get; set; }
    public List<PhanPhong> NguoiDangO { get; set; } = new();   // chỉ Admin được xem chi tiết người ở
    public bool LaAdmin { get; set; }
}
