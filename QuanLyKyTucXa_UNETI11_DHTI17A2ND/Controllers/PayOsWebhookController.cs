using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QuanLyKyTucXa_UNETI11_DHTI17A2ND.Services;

namespace QuanLyKyTucXa_UNETI11_DHTI17A2ND.Controllers;

/// <summary>Nhận thông báo thanh toán từ payOS. Không dùng Session; bảo mật bằng chữ ký HMAC-SHA256.</summary>
[ApiController]
[Route("payos/webhook")]
public class PayOsWebhookController : ControllerBase
{
    private readonly HoaDonService _hoaDon;
    private readonly PayOsOptions _opt;

    public PayOsWebhookController(HoaDonService hoaDon, IOptions<PayOsOptions> opt)
    {
        _hoaDon = hoaDon;
        _opt = opt.Value;
    }

    [HttpPost]
    public async Task<IActionResult> Nhan([FromBody] JsonElement body)
    {
        if (!body.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object
            || !body.TryGetProperty("signature", out var sig))
            return BadRequest(new { success = false });

        if (!PayOsGateway.KiemTraChuKyWebhook(data, sig.GetString(), _opt.ChecksumKey))
            return BadRequest(new { success = false, message = "Chữ ký không hợp lệ" });

        var code = data.TryGetProperty("code", out var c) ? c.GetString() : null;
        if (code == "00"
            && data.TryGetProperty("orderCode", out var oc) && oc.ValueKind == JsonValueKind.Number
            && data.TryGetProperty("amount", out var am) && am.ValueKind == JsonValueKind.Number)
        {
            var reference = data.TryGetProperty("reference", out var r) ? r.GetString() : null;
            await _hoaDon.XacNhanTuWebhookAsync(oc.GetInt64(), am.GetInt64(), reference);
        }

        // Luôn trả 200 khi chữ ký đúng (kể cả mã đơn mẫu khi payOS kiểm tra webhook)
        return Ok(new { success = true });
    }
}