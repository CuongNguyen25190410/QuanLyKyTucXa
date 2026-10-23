using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Helpers;

namespace QuanLyKyTucXa_UNETI11_DHTI17A2ND.Models.ViewModels;

public class HoaDonIndexVM
{
    public string TuKhoa { get; set; }
    public int? Thang { get; set; }
    public int? Nam { get; set; }
    public string TrangThai { get; set; }
    public PagedResult<HoaDon> KetQua { get; set; }
    public decimal TongDaThu { get; set; }
    public decimal TongChuaThu { get; set; }
    public int ThangMacDinh { get; set; }
    public int NamMacDinh { get; set; }
}