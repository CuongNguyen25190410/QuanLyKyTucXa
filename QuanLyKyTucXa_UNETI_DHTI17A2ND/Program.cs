// Họ và tên: [ĐIỀN HỌ TÊN CẢ NHÓM]
// Mã sinh viên: [ĐIỀN MÃ CẢ NHÓM]
// Nội dung thực hiện: Thành phần dùng chung của Project (cấu hình, DbContext, dữ liệu mẫu, layout...)
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

// Dùng culture Invariant để model binding số/ngày không phụ thuộc cấu hình máy
// (hiển thị ngày, tiền theo định dạng Việt Nam được xử lý riêng trong Helpers/Fmt.cs).
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    // Việt hóa các thông báo lỗi model binding mặc định
    var mp = options.ModelBindingMessageProvider;
    mp.SetValueMustNotBeNullAccessor(_ => "Vui lòng nhập giá trị.");
    mp.SetMissingBindRequiredValueAccessor(f => $"Thiếu giá trị cho {f}.");
    mp.SetMissingKeyOrValueAccessor(() => "Vui lòng nhập giá trị.");
    mp.SetAttemptedValueIsInvalidAccessor((v, f) => $"Giá trị '{v}' không hợp lệ cho {f}.");
    mp.SetNonPropertyAttemptedValueIsInvalidAccessor(v => $"Giá trị '{v}' không hợp lệ.");
    mp.SetUnknownValueIsInvalidAccessor(f => $"Giá trị không hợp lệ cho {f}.");
    mp.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Giá trị không hợp lệ.");
    mp.SetValueIsInvalidAccessor(v => $"Giá trị '{v}' không hợp lệ.");
    mp.SetValueMustBeANumberAccessor(f => $"{f} phải là một số.");
    mp.SetNonPropertyValueMustBeANumberAccessor(() => "Giá trị phải là một số.");
});

// Entity Framework Core 10 + SQL Server (chuỗi kết nối nằm trong appsettings.json)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session: lưu MaTaiKhoan, HoTen, VaiTro sau khi đăng nhập
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Băm mật khẩu bằng PasswordHasher của ASP.NET Core
builder.Services.AddSingleton<IPasswordHasher<TaiKhoan>, PasswordHasher<TaiKhoan>>();

// Các service nghiệp vụ
builder.Services.AddScoped<PhongService>();
builder.Services.AddScoped<PhanPhongService>();

builder.Services.AddScoped<ImportSinhVienService>();
// ---- Thanh toán QR (payOS hoặc giả lập) ----
// ThanhToan:NhaCungCap = "PayOS" (cần khóa trong appsettings.Development.json) hoặc "Mock" (demo, mặc định)
builder.Services.Configure<PayOsOptions>(builder.Configuration.GetSection("PayOS"));
builder.Services.AddHttpClient<PayOsGateway>(c =>
{
    c.BaseAddress = new Uri("https://api-merchant.payos.vn/");
    c.Timeout = TimeSpan.FromSeconds(20);
});
builder.Services.AddScoped<MockGateway>();
builder.Services.AddScoped<ICongThanhToan>(sp =>
    string.Equals(builder.Configuration["ThanhToan:NhaCungCap"], "PayOS", StringComparison.OrdinalIgnoreCase)
        ? sp.GetRequiredService<PayOsGateway>()
        : sp.GetRequiredService<MockGateway>());
builder.Services.AddScoped<HoaDonService>();

var app = builder.Build();

// Tạo/cập nhật CSDL từ Migration và nạp dữ liệu mẫu (an toàn khi chạy lại nhiều lần)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<TaiKhoan>>();

    if (db.Database.GetMigrations().Any())
        await db.Database.MigrateAsync();          // cách chuẩn: Code First + Migration (gồm cả bảng HoaDon)
    else
        await db.Database.EnsureCreatedAsync();    // dự phòng khi chưa tạo Migration

    await DbSeeder.SeedAsync(db, hasher);          // tài khoản, sinh viên, phòng mẫu ban đầu
    await PhongMauSeeder.SeedAsync(db);            // thêm phòng cho đủ ~300 phòng (100 phòng/tòa)
}

<<<<<<< HEAD
=======
//Tạo CSDL từ Migration (nếu đã có Migration) và nạp dữ liệu mẫu lần đầu
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<TaiKhoan>>();

    if (db.Database.GetMigrations().Any())
        await db.Database.MigrateAsync();          // cách chuẩn: Code First + Migration
    else
        await db.Database.EnsureCreatedAsync();    // dự phòng khi chưa tạo Migration (xem README)

    await DbSeeder.SeedAsync(db, hasher);
}

>>>>>>> c2d0a27 (update)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();          // phải đứng sau UseRouting và trước các endpoint MVC

app.MapControllers();      // cho route thuộc tính, ví dụ /payos/webhook
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TaiKhoan}/{action=Login}/{id?}");

app.Run();