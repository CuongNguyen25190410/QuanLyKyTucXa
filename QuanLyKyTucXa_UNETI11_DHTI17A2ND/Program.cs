
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Models;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Services;

// ==============================================
// 1. CẤU HÌNH NGÔN NGỮ
// ==============================================

CultureInfo.DefaultThreadCurrentCulture =
    CultureInfo.InvariantCulture;

CultureInfo.DefaultThreadCurrentUICulture =
    CultureInfo.InvariantCulture;

// ==============================================
// 2. KHỞI TẠO WEB APPLICATION
// ==============================================

var builder = WebApplication.CreateBuilder(args);

// Đăng ký MVC và Việt hóa lỗi nhập liệu
builder.Services.AddControllersWithViews(options =>
{
    var mp = options.ModelBindingMessageProvider;

    mp.SetValueMustNotBeNullAccessor(
        _ => "Vui lòng nhập giá trị.");

    mp.SetMissingBindRequiredValueAccessor(
        f => $"Thiếu giá trị cho {f}.");

    mp.SetMissingKeyOrValueAccessor(
        () => "Vui lòng nhập giá trị.");

    mp.SetAttemptedValueIsInvalidAccessor(
        (v, f) => $"Giá trị '{v}' không hợp lệ cho {f}.");

    mp.SetNonPropertyAttemptedValueIsInvalidAccessor(
        v => $"Giá trị '{v}' không hợp lệ.");

    mp.SetUnknownValueIsInvalidAccessor(
        f => $"Giá trị không hợp lệ cho {f}.");

    mp.SetNonPropertyUnknownValueIsInvalidAccessor(
        () => "Giá trị không hợp lệ.");

    mp.SetValueIsInvalidAccessor(
        v => $"Giá trị '{v}' không hợp lệ.");

    mp.SetValueMustBeANumberAccessor(
        f => $"{f} phải là một số.");

    mp.SetNonPropertyValueMustBeANumberAccessor(
        () => "Giá trị phải là một số.");
});

// ==============================================
// 3. KẾT NỐI DATABASE SQL SERVER
// ==============================================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Chưa cấu hình DefaultConnection trong appsettings.json.");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// ==============================================
// 4. CẤU HÌNH SESSION
// ==============================================

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ==============================================
// 5. MÃ HÓA MẬT KHẨU
// ==============================================

builder.Services.AddSingleton<
    IPasswordHasher<TaiKhoan>,
    PasswordHasher<TaiKhoan>>();

// ==============================================
// 6. ĐĂNG KÝ CÁC SERVICE NGHIỆP VỤ
// ==============================================

builder.Services.AddScoped<PhongService>();

builder.Services.AddScoped<PhanPhongService>();

builder.Services.AddScoped<DonHuyPhongService>();

builder.Services.AddScoped<ImportSinhVienService>();

builder.Services.AddScoped<HoaDonService>();

// ==============================================
// 7. CẤU HÌNH THANH TOÁN PAYOS / MOCK
// ==============================================

builder.Services.Configure<PayOsOptions>(
    builder.Configuration.GetSection("PayOS"));

builder.Services.AddHttpClient<PayOsGateway>(client =>
{
    client.BaseAddress =
        new Uri("https://api-merchant.payos.vn/");

    client.Timeout = TimeSpan.FromSeconds(20);
});

builder.Services.AddScoped<MockGateway>();

builder.Services.AddScoped<ICongThanhToan>(sp =>
{
    var nhaCungCap =
        builder.Configuration["ThanhToan:NhaCungCap"];

    if (string.Equals(
        nhaCungCap,
        "PayOS",
        StringComparison.OrdinalIgnoreCase))
    {
        return sp.GetRequiredService<PayOsGateway>();
    }

    return sp.GetRequiredService<MockGateway>();
});

// ==============================================
// 8. BUILD APPLICATION
// ==============================================

var app = builder.Build();

// ==============================================
// 9. KHỞI TẠO DATABASE BẰNG MIGRATION
// ==============================================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    var hasher = scope.ServiceProvider
        .GetRequiredService<IPasswordHasher<TaiKhoan>>();

    // In tên database thực tế đang kết nối
    Console.WriteLine("===================================");

    Console.WriteLine("DANG KHOI TAO DATABASE");

    Console.WriteLine(
        "Database: " +
        db.Database.GetDbConnection().Database);

    Console.WriteLine("===================================");

    try
    {
        // Kiểm tra các Migration của dự án
        var migrations = db.Database
            .GetMigrations()
            .ToList();

        if (migrations.Count == 0)
        {
            throw new InvalidOperationException(
                "Du an chua co Migration. " +
                "Hay tao Migration truoc khi chay.");
        }

        // Tạo database mới hoặc áp dụng Migration còn thiếu.
        // Không sử dụng EnsureCreated vì có thể gây
        // xung đột với lịch sử Migration.
        Console.WriteLine("Dang ap dung Migration...");

        await db.Database.MigrateAsync();

        Console.WriteLine("Migration thanh cong!");

        // Nạp tài khoản, sinh viên, phòng mẫu
        Console.WriteLine("Dang nap du lieu mau...");

        await DbSeeder.SeedAsync(db, hasher);

        // Nạp thêm phòng mẫu
        await PhongMauSeeder.SeedAsync(db);

        Console.WriteLine("Nap du lieu hoan tat!");
    }
    catch (Exception ex)
    {
        Console.WriteLine("===================================");

        Console.WriteLine("LOI KHOI TAO DATABASE:");

        Console.WriteLine(ex.ToString());

        Console.WriteLine("===================================");

        // Giữ nguyên lỗi để không chạy website
        // khi database chưa được khởi tạo thành công.
        throw;
    }
}

// ==============================================
// 10. CẤU HÌNH MIDDLEWARE
// ==============================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

// Chuyển HTTP sang HTTPS khi có cấu hình HTTPS
app.UseHttpsRedirection();

// Cho phép truy cập CSS, JS, hình ảnh
app.UseStaticFiles();

// Cấu hình Routing
app.UseRouting();

// Session phải đặt sau UseRouting
app.UseSession();

// ==============================================
// 11. CẤU HÌNH ROUTE
// ==============================================

// Route sử dụng Attribute
app.MapControllers();

// Route MVC mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TaiKhoan}/{action=Login}/{id?}");

// ==============================================
// 12. CHẠY WEBSITE
// ==============================================

app.Run();