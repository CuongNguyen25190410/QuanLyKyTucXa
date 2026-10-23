using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Models;

namespace QuanLyKyTucXa_UNETI11_DHTI17A2ND.Data;

/// Thêm phòng mẫu: Tòa + Tầng + Số phòng (01..99) -> A101, B214, C305...
/// 3 tòa x 5 tầng x 14 phòng = 210 phòng. Phòng đã có sẵn sẽ được bỏ qua.
/// Tòa A: Nam. Tòa B: Nữ. Tòa C: hỗn hợp (số phòng lẻ = Nam, chẵn = Nữ).
public static class PhongMauSeeder
{
    private static readonly string[] DanhSachToa = { "A", "B", "C" };
    private const int SoTang = 5;
    private const int SoPhongMoiTang = 14;     // tối đa 99

    private static readonly int[] MauSucChua = { 4, 4, 6, 6, 8, 8, 4, 6, 2, 4, 6, 8, 2, 6 };

    public static async Task SeedAsync(AppDbContext db)
    {
        var loaiPhongs = await db.LoaiPhongs.ToListAsync();
        LoaiPhong Loai(int sucChua) => loaiPhongs.FirstOrDefault(l => l.SoNguoiToiDa == sucChua);

        if (new[] { 2, 4, 6, 8 }.Any(n => Loai(n) == null)) return;

        var daCo = (await db.PhongKTXs.Select(p => p.TenPhong).ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var moi = new List<PhongKTX>();
        foreach (var toa in DanhSachToa)
        {
            for (int tang = 1; tang <= SoTang; tang++)
            {
                for (int so = 1; so <= SoPhongMoiTang; so++)
                {
                    var ten = $"{toa}{tang}{so:00}";
                    if (daCo.Contains(ten)) continue;

                    var gioiTinh = toa switch
                    {
                        "A" => GioiTinh.Nam,
                        "B" => GioiTinh.Nu,
                        _ => so % 2 == 1 ? GioiTinh.Nam : GioiTinh.Nu
                    };

                    var tamNgung = (tang * 7 + so) % 23 == 0;
                    var loai = Loai(MauSucChua[(so - 1 + tang) % MauSucChua.Length]);

                    moi.Add(new PhongKTX
                    {
                        TenPhong = ten,
                        LoaiPhong = loai,
                        ToaNha = toa,
                        Tang = tang,
                        GioiTinhPhong = gioiTinh,
                        TrangThai = tamNgung ? TrangThaiPhong.TamNgung : TrangThaiPhong.DangSuDung,
                        MoTa = tamNgung
                            ? "Đang sửa chữa hệ thống điện nước."
                            : $"Phòng {ten}, tòa {toa}, tầng {tang}."
                    });
                }
            }
        }

        if (moi.Count == 0) return;
        db.PhongKTXs.AddRange(moi);
        await db.SaveChangesAsync();
    }
}