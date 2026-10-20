namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Models.ViewModels;

public class ImportSinhVienVM
{
    public bool ChiKiemTra { get; set; }
    public bool DungNeuCoLoi { get; set; } = true;
    public string ThongBaoLoi { get; set; }
    public string MatKhauMacDinh { get; set; }
    public ImportSinhVienKetQua KetQua { get; set; }
}

public class ImportSinhVienKetQua
{
    public string TenFile { get; set; }
    public bool ChiKiemTra { get; set; }
    public bool DungNeuCoLoi { get; set; }
    public string LoiChung { get; set; }

    public int TongDong { get; set; }
    public int SoHopLe { get; set; }
    public bool DaLuu { get; set; }
    public int SoDaTao { get; set; }

    public List<ImportDongLoi> Loi { get; set; } = new();
    public List<ImportDongHopLe> HopLe { get; set; } = new();
}

public class ImportDongLoi
{
    public int Dong { get; set; }
    public string MaSinhVien { get; set; }
    public string HoTen { get; set; }
    public string LyDo { get; set; }
}

public class ImportDongHopLe
{
    public int Dong { get; set; }
    public string MaSinhVien { get; set; }
    public string HoTen { get; set; }
    public string GioiTinh { get; set; }
    public string Lop { get; set; }
}