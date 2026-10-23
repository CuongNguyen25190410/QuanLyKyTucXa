using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace QuanLyKyTucXa_UNETI11_DHTI17A2ND.Services;

public class PayOsOptions
{
    public string ClientId { get; set; }
    public string ApiKey { get; set; }
    public string ChecksumKey { get; set; }
}

public class LoiCongThanhToanException : Exception
{
    public LoiCongThanhToanException(string message) : base(message) { }
}

public record KetQuaTaoLink(string PaymentLinkId, string QrCode, string CheckoutUrl);

/// <summary>Giao diện chung cho cổng thanh toán: muốn dùng VNPAY/MoMo... chỉ cần viết thêm một lớp cài đặt.</summary>
public interface ICongThanhToan
{
    string Ten { get; }
    bool LaGiaLap { get; }

    Task<KetQuaTaoLink> TaoLinkAsync(long orderCode, long soTien, string noiDung, string tenMon,
        string tenNguoiMua, string returnUrl, string cancelUrl, DateTime hetHanLuc);

    /// <summary>PENDING | PAID | CANCELLED | EXPIRED ...</summary>
    Task<string> LayTrangThaiAsync(long orderCode);
}

// ===================================================================== payOS
public class PayOsGateway : ICongThanhToan
{
    private readonly HttpClient _http;
    private readonly PayOsOptions _opt;

    public PayOsGateway(HttpClient http, IOptions<PayOsOptions> opt)
    {
        _http = http;
        _opt = opt.Value;
    }

    public string Ten => "payOS";
    public bool LaGiaLap => false;

    public static string KyHmac(string key, string data)
    {
        using var h = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(h.ComputeHash(Encoding.UTF8.GetBytes(data))).ToLowerInvariant();
    }

    /// <summary>Kiểm tra chữ ký webhook: sắp xếp khóa của "data" theo alphabet, nối key=value bằng &amp;, HMAC-SHA256.</summary>
    public static bool KiemTraChuKyWebhook(JsonElement data, string signature, string checksumKey)
    {
        if (string.IsNullOrWhiteSpace(checksumKey) || string.IsNullOrWhiteSpace(signature)) return false;

        var sb = new StringBuilder();
        var dau = true;
        foreach (var p in data.EnumerateObject().OrderBy(x => x.Name, StringComparer.Ordinal))
        {
            string v = p.Value.ValueKind switch
            {
                JsonValueKind.Null or JsonValueKind.Undefined => "",
                JsonValueKind.String => p.Value.GetString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => p.Value.GetRawText()
            };
            if (!dau) sb.Append('&');
            dau = false;
            sb.Append(p.Name).Append('=').Append(v);
        }
        return string.Equals(KyHmac(checksumKey, sb.ToString()), signature, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<KetQuaTaoLink> TaoLinkAsync(long orderCode, long soTien, string noiDung, string tenMon,
        string tenNguoiMua, string returnUrl, string cancelUrl, DateTime hetHanLuc)
    {
        KiemTraCauHinh();

        var duLieuKy = $"amount={soTien}&cancelUrl={cancelUrl}&description={noiDung}&orderCode={orderCode}&returnUrl={returnUrl}";
        var body = new
        {
            orderCode,
            amount = soTien,
            description = noiDung,
            buyerName = tenNguoiMua,
            items = new[] { new { name = tenMon, quantity = 1, price = soTien } },
            cancelUrl,
            returnUrl,
            expiredAt = new DateTimeOffset(hetHanLuc).ToUnixTimeSeconds(),
            signature = KyHmac(_opt.ChecksumKey, duLieuKy)
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, "v2/payment-requests")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
        req.Headers.Add("x-client-id", _opt.ClientId);
        req.Headers.Add("x-api-key", _opt.ApiKey);

        using var res = await _http.SendAsync(req);
        var root = DocPhanHoi(await res.Content.ReadAsStringAsync());
        KiemTraThanhCong(root);

        var d = root.GetProperty("data");
        return new KetQuaTaoLink(
            d.GetProperty("paymentLinkId").GetString(),
            d.GetProperty("qrCode").GetString(),
            d.GetProperty("checkoutUrl").GetString());
    }

    public async Task<string> LayTrangThaiAsync(long orderCode)
    {
        KiemTraCauHinh();

        using var req = new HttpRequestMessage(HttpMethod.Get, $"v2/payment-requests/{orderCode}");
        req.Headers.Add("x-client-id", _opt.ClientId);
        req.Headers.Add("x-api-key", _opt.ApiKey);

        using var res = await _http.SendAsync(req);
        var root = DocPhanHoi(await res.Content.ReadAsStringAsync());
        KiemTraThanhCong(root);
        return root.GetProperty("data").GetProperty("status").GetString();
    }

    private void KiemTraCauHinh()
    {
        if (string.IsNullOrWhiteSpace(_opt.ClientId) || string.IsNullOrWhiteSpace(_opt.ApiKey) || string.IsNullOrWhiteSpace(_opt.ChecksumKey))
            throw new LoiCongThanhToanException("Chưa cấu hình payOS (ClientId / ApiKey / ChecksumKey) trong appsettings.");
    }

    private static JsonElement DocPhanHoi(string json)
    {
        try { return JsonDocument.Parse(json).RootElement.Clone(); }
        catch (JsonException) { throw new LoiCongThanhToanException("Phản hồi từ payOS không hợp lệ."); }
    }

    private static void KiemTraThanhCong(JsonElement root)
    {
        var code = root.TryGetProperty("code", out var c) ? c.ToString() : null;
        if (code == "00") return;
        var desc = root.TryGetProperty("desc", out var d) ? d.ToString() : "Lỗi không xác định";
        throw new LoiCongThanhToanException("payOS: " + desc);
    }
}

// ===================================================================== Giả lập (demo, không cần tài khoản)
public class MockGateway : ICongThanhToan
{
    public string Ten => "Giả lập";
    public bool LaGiaLap => true;

    public Task<KetQuaTaoLink> TaoLinkAsync(long orderCode, long soTien, string noiDung, string tenMon,
        string tenNguoiMua, string returnUrl, string cancelUrl, DateTime hetHanLuc)
    {
        // Mã QR giả: không dùng để chuyển tiền thật
        return Task.FromResult(new KetQuaTaoLink($"MOCK{orderCode}", $"GIA-LAP|{noiDung}|{soTien}", null));
    }

    public Task<string> LayTrangThaiAsync(long orderCode) => Task.FromResult("PENDING");
}