using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Constants;

namespace QuanLyKyTucXa_UNETI11_DHTI17A2ND.Models.ViewModels;

public class SoDoPhongVM
{
    public bool LaAdmin { get; set; }
    public string GioiTinhSinhVien { get; set; }
    public int? MaPhongCuaToi { get; set; }
    public List<SoDoToaVM> Toas { get; set; } = new();
    public List<SelectListItem> LoaiPhongs { get; set; } = new();

    /// tamngung | het | khacgioi | it (sắp đầy) | con
    public string LayTrangThai(PhongItemVM p)
    {
        if (p.TrangThai != TrangThaiPhong.DangSuDung) return "tamngung";
        if (p.SoChoCon <= 0) return "het";
        if (GioiTinhSinhVien != null && p.GioiTinhPhong != GioiTinhSinhVien) return "khacgioi";
        if (p.SoChoCon * 4 <= p.SucChua) return "it";
        return "con";
    }
}

public class SoDoToaVM
{
    public string TenToa { get; set; }
    public List<SoDoTangVM> Tangs { get; set; } = new();

    public int TongPhong => Tangs.Sum(t => t.Phongs.Count);

    public int PhongConCho => Tangs.Sum(t => t.Phongs.Count(p => p.TrangThai == TrangThaiPhong.DangSuDung && p.SoChoCon > 0));

    public int TongChoTrong => Tangs.Sum(t => t.Phongs
        .Where(p => p.TrangThai == TrangThaiPhong.DangSuDung).Sum(p => Math.Max(p.SoChoCon, 0)));

    public string GioiTinh
    {
        get
        {
            var ds = Tangs.SelectMany(t => t.Phongs).Select(p => p.GioiTinhPhong).Distinct().ToList();
            return ds.Count == 1 ? ds[0] : "Hỗn hợp";
        }
    }
}

public class SoDoTangVM
{
    public int Tang { get; set; }
    public List<PhongItemVM> Phongs { get; set; } = new();
}