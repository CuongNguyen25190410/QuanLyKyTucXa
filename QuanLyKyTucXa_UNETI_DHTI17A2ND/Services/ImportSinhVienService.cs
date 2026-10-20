using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models.ViewModels;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

/// <summary>Nhập hàng loạt sinh viên + tài khoản từ file Excel (.xlsx).</summary>
public class ImportSinhVienService
{
    public const string MatKhauMacDinh = "Sv@123456";
    public const int ToiDaDong = 1000;

    // Thứ tự cột trong file mẫu
    private static readonly string[] TieuDe =
        { "Mã sinh viên", "Họ tên", "Ngày sinh", "Giới tính", "Lớp", "Khoa", "Số điện thoại", "Email", "Địa chỉ", "Mật khẩu" };

    private static readonly string[] DinhDangNgay =
        { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "yyyy-MM-dd", "dd.MM.yyyy", "d.M.yyyy" };

    private readonly AppDbContext _db;
    private readonly IPasswordHasher<TaiKhoan> _hasher;

    public ImportSinhVienService(AppDbContext db, IPasswordHasher<TaiKhoan> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    private sealed class DongHopLe
    {
        public int Dong;
        public SinhVien SinhVien;
        public TaiKhoan TaiKhoan;
        public string MatKhau;
    }

    // ===================================================================== FILE MẪU
    public byte[] TaoFileMau()
    {
        using var wb = new XLWorkbook();

        var ws = wb.Worksheets.Add("SinhVien");
        // Các cột dạng chữ để Excel không làm mất số 0 đầu (số điện thoại) hay đổi mã sinh viên thành số
        foreach (var cot in new[] { 1, 5, 6, 7, 9, 10 }) ws.Column(cot).Style.NumberFormat.Format = "@";
        ws.Column(3).Style.DateFormat.Format = "dd/MM/yyyy";

        for (int i = 0; i < TieuDe.Length; i++)
        {
            var c = ws.Cell(1, i + 1);
            c.SetValue(TieuDe[i]);
            c.Style.Font.Bold = true;
            c.Style.Fill.BackgroundColor = XLColor.LightBlue;
        }
        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(1);

        var hd = wb.Worksheets.Add("HuongDan");
        var dong = new[]
        {
            "HƯỚNG DẪN NHẬP DANH SÁCH SINH VIÊN",
            "1. Nhập dữ liệu ở sheet \"SinhVien\", mỗi sinh viên một dòng, bắt đầu từ dòng 2. Không đổi tên/thứ tự các cột.",
            "2. Cột bắt buộc: Mã sinh viên, Họ tên, Ngày sinh, Giới tính, Số điện thoại, Email. Lớp, Khoa, Địa chỉ, Mật khẩu có thể để trống.",
            "3. Mã sinh viên: chỉ gồm chữ và số, tối đa 20 ký tự. Mã này cũng là TÊN ĐĂNG NHẬP của sinh viên.",
            "4. Ngày sinh: dd/MM/yyyy (ví dụ 15/03/2005), tuổi từ 15 đến 80.",
            "5. Giới tính: Nam hoặc Nữ.",
            "6. Số điện thoại: 10 chữ số, bắt đầu bằng 0.",
            "7. Mật khẩu: để trống thì dùng mật khẩu mặc định (" + MatKhauMacDinh + "); nếu nhập thì từ 6 đến 100 ký tự.",
            "8. Tối đa " + ToiDaDong + " dòng mỗi lần nhập. Chỉ nhận file .xlsx.",
            "",
            "VÍ DỤ (không có trong sheet SinhVien, hãy tự nhập):",
            "22103100101 | Nguyễn Văn A | 15/03/2005 | Nam | DHTI17A2ND | Công nghệ thông tin | 0987654321 | a@gmail.com | Nam Định | (trống)"
        };
        for (int i = 0; i < dong.Length; i++) hd.Cell(i + 1, 1).SetValue(dong[i]);
        hd.Cell(1, 1).Style.Font.Bold = true;
        hd.Column(1).Width = 140;

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ===================================================================== NHẬP FILE
    public async Task<ImportSinhVienKetQua> XuLyAsync(Stream stream, string tenFile, bool chiKiemTra, bool dungNeuCoLoi)
    {
        var kq = new ImportSinhVienKetQua { TenFile = tenFile, ChiKiemTra = chiKiemTra, DungNeuCoLoi = dungNeuCoLoi };

        XLWorkbook wb;
        try { wb = new XLWorkbook(stream); }
        catch (Exception)   // file hỏng / không phải .xlsx thật: thư viện ném nhiều loại ngoại lệ khác nhau
        {
            kq.LoiChung = "Không đọc được file. Hãy dùng file .xlsx (Excel 2007 trở lên) đúng định dạng.";
            return kq;
        }

        using (wb)
        {
            var ws = wb.Worksheets.FirstOrDefault();
            if (ws == null) { kq.LoiChung = "File không có sheet nào."; return kq; }

            // Kiểm tra dòng tiêu đề đúng mẫu
            for (int i = 0; i < 8; i++)
            {
                if (Chuan(Doc(ws.Cell(1, i + 1))) != Chuan(TieuDe[i]))
                {
                    kq.LoiChung = $"Dòng tiêu đề không đúng mẫu (cột {i + 1} phải là \"{TieuDe[i]}\"). Hãy tải file mẫu và nhập theo mẫu.";
                    return kq;
                }
            }

            var cacDong = ws.RowsUsed().Where(r => r.RowNumber() > 1).ToList();
            if (cacDong.Count > ToiDaDong)
            {
                kq.LoiChung = $"File có {cacDong.Count} dòng, vượt giới hạn {ToiDaDong} dòng mỗi lần. Hãy chia nhỏ file.";
                return kq;
            }

            var maCo = (await _db.SinhViens.Select(s => s.MaSinhVien).ToListAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var tenDangNhapCo = (await _db.TaiKhoans.Select(t => t.TenDangNhap).ToListAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var trongFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var hopLe = new List<DongHopLe>();

            foreach (var r in cacDong)
            {
                var so = r.RowNumber();
                var ma = Doc(r.Cell(1));
                var hoTen = Regex.Replace(Doc(r.Cell(2)), @"\s+", " ");
                var cellNgay = r.Cell(3);
                var gioiTinh = ChuanGioiTinh(Doc(r.Cell(4)));
                var lop = Doc(r.Cell(5));
                var khoa = Doc(r.Cell(6));
                var sdt = ChuanSoDienThoai(Doc(r.Cell(7)));
                var email = Doc(r.Cell(8));
                var diaChi = Doc(r.Cell(9));
                var matKhau = Doc(r.Cell(10));

                // Bỏ qua dòng trống hoàn toàn
                if (new[] { ma, hoTen, gioiTinh, lop, khoa, sdt, email, diaChi, matKhau }.All(string.IsNullOrEmpty)
                    && cellNgay.IsEmpty())
                    continue;

                kq.TongDong++;

                var ngay = DocNgay(cellNgay);
                var ngayHong = !cellNgay.IsEmpty() && ngay == null;

                var vm = new SinhVienFormVM
                {
                    MaSinhVien = ma,
                    HoTen = hoTen,
                    NgaySinh = ngay,
                    GioiTinh = gioiTinh,
                    Lop = lop.Length == 0 ? null : lop,
                    Khoa = khoa.Length == 0 ? null : khoa,
                    SoDienThoai = sdt,
                    Email = email,
                    DiaChi = diaChi.Length == 0 ? null : diaChi,
                    TrangThai = TrangThaiSinhVien.HoatDong,
                    TenDangNhap = ma,
                    MatKhau = matKhau.Length == 0 ? MatKhauMacDinh : matKhau
                };

                // Dùng lại đúng các quy tắc kiểm tra của form "Thêm sinh viên" (DataAnnotations)
                var ketQuaKiemTra = new List<ValidationResult>();
                Validator.TryValidateObject(vm, new ValidationContext(vm), ketQuaKiemTra, true);
                var loi = ketQuaKiemTra.Select(x => x.ErrorMessage).ToList();

                if (ngayHong)
                {
                    loi.RemoveAll(x => x.Contains("Ngày sinh là bắt buộc"));
                    loi.Add("Ngày sinh không đúng định dạng (dùng dd/MM/yyyy)");
                }
                if (!string.IsNullOrEmpty(gioiTinh) && !GioiTinh.TatCa.Contains(gioiTinh))
                    loi.Add("Giới tính phải là Nam hoặc Nữ");

                if (!string.IsNullOrEmpty(ma))
                {
                    if (maCo.Contains(ma) || tenDangNhapCo.Contains(ma))
                        loi.Add("Mã sinh viên/tên đăng nhập đã tồn tại trong hệ thống");
                    else if (!trongFile.Add(ma))
                        loi.Add("Trùng mã sinh viên với một dòng phía trên trong file");
                }

                if (loi.Count > 0)
                {
                    kq.Loi.Add(new ImportDongLoi { Dong = so, MaSinhVien = ma, HoTen = hoTen, LyDo = string.Join("; ", loi.Distinct()) });
                    continue;
                }

                var tk = new TaiKhoan
                {
                    TenDangNhap = ma,
                    HoTen = hoTen,
                    Email = email,
                    VaiTro = VaiTro.SinhVien,
                    TrangThai = TrangThaiTaiKhoan.HoatDong
                };
                var sv = new SinhVien
                {
                    MaSinhVien = ma,
                    HoTen = hoTen,
                    NgaySinh = ngay.Value.Date,
                    GioiTinh = gioiTinh,
                    Lop = vm.Lop,
                    Khoa = vm.Khoa,
                    SoDienThoai = sdt,
                    Email = email,
                    DiaChi = vm.DiaChi,
                    TrangThai = TrangThaiSinhVien.HoatDong,
                    TaiKhoan = tk
                };
                hopLe.Add(new DongHopLe { Dong = so, SinhVien = sv, TaiKhoan = tk, MatKhau = vm.MatKhau });
                kq.HopLe.Add(new ImportDongHopLe { Dong = so, MaSinhVien = ma, HoTen = hoTen, GioiTinh = gioiTinh, Lop = lop });
            }

            kq.SoHopLe = hopLe.Count;

            if (kq.TongDong == 0)
            {
                kq.LoiChung = "File không có dòng dữ liệu nào (dữ liệu bắt đầu từ dòng 2).";
                return kq;
            }

            var duocLuu = !chiKiemTra && hopLe.Count > 0 && !(dungNeuCoLoi && kq.Loi.Count > 0);
            if (!duocLuu) return kq;

            // Băm mật khẩu song song cho nhanh (mỗi lần băm khá tốn thời gian)
            Parallel.ForEach(hopLe, d => d.TaiKhoan.MatKhau = _hasher.HashPassword(d.TaiKhoan, d.MatKhau));

            try
            {
                _db.SinhViens.AddRange(hopLe.Select(d => d.SinhVien));
                await _db.SaveChangesAsync();      // một lần lưu = một transaction: hoặc tạo hết, hoặc không tạo gì
                kq.DaLuu = true;
                kq.SoDaTao = hopLe.Count;
            }
            catch (DbUpdateException)
            {
                kq.LoiChung = "Lưu dữ liệu thất bại (có thể vừa có người khác tạo trùng mã sinh viên). Chưa có tài khoản nào được tạo, vui lòng thử lại.";
            }
        }
        return kq;
    }

    // ===================================================================== HÀM HỖ TRỢ
    private static string Doc(IXLCell c) => c == null || c.IsEmpty() ? "" : (c.GetString() ?? "").Trim();

    private static string Chuan(string s) => (s ?? "").Normalize(NormalizationForm.FormC).Trim().ToLowerInvariant();

    private static string ChuanGioiTinh(string s)
    {
        return Chuan(s) switch
        {
            "nam" => GioiTinh.Nam,
            "nữ" or "nu" => GioiTinh.Nu,
            _ => (s ?? "").Trim()
        };
    }

    /// <summary>Excel hay làm mất số 0 đầu của số điện thoại: tự thêm lại, bỏ khoảng trắng/dấu chấm/gạch ngang, đổi +84 thành 0.</summary>
    private static string ChuanSoDienThoai(string s)
    {
        var t = Regex.Replace(s ?? "", @"[\s.\-]", "");
        var m = Regex.Match(t, @"^\+?84(\d{9})$");
        if (m.Success) return "0" + m.Groups[1].Value;
        if (Regex.IsMatch(t, @"^[1-9]\d{8}$")) return "0" + t;
        return t;
    }

    private static DateTime? DocNgay(IXLCell c)
    {
        if (c == null || c.IsEmpty()) return null;
        if (c.DataType == XLDataType.DateTime) return c.GetDateTime();
        if (c.DataType == XLDataType.Number)
        {
            try { return DateTime.FromOADate(c.GetDouble()); } catch (ArgumentException) { return null; }
        }
        var s = c.GetString().Trim();
        return DateTime.TryParseExact(s, DinhDangNgay, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
    }
}