// Họ và tên: [ĐIỀN HỌ TÊN CẢ NHÓM]
// Mã sinh viên: [ĐIỀN MÃ CẢ NHÓM]
// Nội dung thực hiện: Thành phần dùng chung của Project (cấu hình, DbContext, dữ liệu mẫu, layout...)
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TaiKhoan> TaiKhoans { get; set; }
    public DbSet<LoaiPhong> LoaiPhongs { get; set; }
    public DbSet<PhongKTX> PhongKTXs { get; set; }
    public DbSet<SinhVien> SinhViens { get; set; }
    public DbSet<DangKyKTX> DangKyKTXs { get; set; }
    public DbSet<PhanPhong> PhanPhongs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- TaiKhoan ----
        modelBuilder.Entity<TaiKhoan>(e =>
        {
            e.ToTable("TaiKhoan");
            e.HasIndex(x => x.TenDangNhap).IsUnique();          // tên đăng nhập không trùng
        });

        // ---- LoaiPhong ----
        modelBuilder.Entity<LoaiPhong>(e =>
        {
            e.ToTable("LoaiPhong");
            e.HasIndex(x => x.TenLoaiPhong).IsUnique();         // tên loại phòng không trùng
        });

        // ---- PhongKTX: LoaiPhong (1) - (n) PhongKTX ----
        modelBuilder.Entity<PhongKTX>(e =>
        {
            e.ToTable("PhongKTX");
            e.HasIndex(x => x.TenPhong).IsUnique();             // tên phòng không trùng
            e.HasOne(x => x.LoaiPhong)
             .WithMany(l => l.PhongKTXs)
             .HasForeignKey(x => x.MaLoaiPhong)
             .OnDelete(DeleteBehavior.Restrict);                // không xóa dây chuyền
        });

        // ---- SinhVien: TaiKhoan (1) - (1) SinhVien ----
        modelBuilder.Entity<SinhVien>(e =>
        {
            e.ToTable("SinhVien");
            e.HasKey(x => x.MaSinhVien);
            e.HasOne(x => x.TaiKhoan)
             .WithOne(t => t.SinhVien)
             .HasForeignKey<SinhVien>(x => x.MaTaiKhoan)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- DangKyKTX: SinhVien (1) - (n) DangKyKTX ----
        modelBuilder.Entity<DangKyKTX>(e =>
        {
            e.ToTable("DangKyKTX");
            e.HasOne(x => x.SinhVien)
             .WithMany(s => s.DangKyKTXs)
             .HasForeignKey(x => x.MaSinhVien)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.LoaiPhongNguyenVong)
             .WithMany()
             .HasForeignKey(x => x.NguyenVongLoaiPhong)
             .OnDelete(DeleteBehavior.Restrict);

            // Chốt chặn ở tầng CSDL: mỗi sinh viên chỉ có tối đa 1 đăng ký đang hiệu lực
            // (Chờ duyệt hoặc Đã phân phòng). Đã hủy / Đã trả phòng thì không giới hạn.
            e.HasIndex(x => x.MaSinhVien)
             .IsUnique()
             .HasFilter("[TrangThai] <> N'Đã hủy' AND [TrangThai] <> N'Đã trả phòng'");
        });

        // ---- PhanPhong: DangKyKTX (1)-(n) PhanPhong, PhongKTX (1)-(n) PhanPhong ----
        modelBuilder.Entity<PhanPhong>(e =>
        {
            e.ToTable("PhanPhong");
            e.HasOne(x => x.DangKyKTX)
             .WithMany(d => d.PhanPhongs)
             .HasForeignKey(x => x.MaDangKy)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.PhongKTX)
             .WithMany(p => p.PhanPhongs)
             .HasForeignKey(x => x.MaPhong)
             .OnDelete(DeleteBehavior.Restrict);

            // Tại một thời điểm một đăng ký (tức một sinh viên) chỉ có tối đa 1 bản ghi "Đang ở".
            e.HasIndex(x => x.MaDangKy)
             .IsUnique()
             .HasFilter("[TrangThai] = N'Đang ở'");
        });
    }
}
