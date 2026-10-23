// Họ và tên: [Hà Việt Anh]
// Mã sinh viên: [23203100046]
// Nội dung thực hiện: Module 4 - Quản lý đăng ký, phân phòng, chuyển phòng, trả phòng, Dashboard, thống kê
namespace QuanLyKyTucXa_UNETI11_DHTI17A2ND.Models.ViewModels;

public class DashboardVM
{
    public int TongSoPhong { get; set; }
    public int TongSoSinhVien { get; set; }
    public int TongSoDangKy { get; set; }
    public int SoDangKyChoDuyet { get; set; }
    public int SoSinhVienDangO { get; set; }
    public int TongSoCho { get; set; }
    public int TongSoChoDangSuDung { get; set; }
    public int TongSoChoConTrong { get; set; }
    public int SoPhongDaDayNguoi { get; set; }
    public double TyLeSuDung { get; set; }
}

public class ThongKeItemVM
{
    public string Nhan { get; set; }
    public int GiaTri { get; set; }
}

public class ThongKeThangVM
{
    public int Nam { get; set; }
    public int Thang { get; set; }
    public int SoSinhVien { get; set; }
}

public class ThongKeVM
{
    public List<ThongKeItemVM> PhongTheoLoai { get; set; } = new();
    public List<PhongItemVM> SinhVienTheoPhong { get; set; } = new();   // gồm số người đang ở, chỗ còn, tỷ lệ
    public List<ThongKeItemVM> SinhVienTheoToa { get; set; } = new();
    public List<ThongKeItemVM> SinhVienTheoLoaiPhong { get; set; } = new();
    public List<ThongKeItemVM> DangKyTheoTrangThai { get; set; } = new();
    public List<PhongItemVM> PhongDongNhat { get; set; } = new();       // phòng có nhiều sinh viên đang ở nhất
    public List<ThongKeThangVM> SinhVienVaoTheoThang { get; set; } = new();
    public double TyLeSuDungChung { get; set; }
}
