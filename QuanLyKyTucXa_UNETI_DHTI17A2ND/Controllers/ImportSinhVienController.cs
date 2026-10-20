using Microsoft.AspNetCore.Mvc;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Filters;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models.ViewModels;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Controllers;

// Chỉ Admin được nhập hàng loạt sinh viên
[PhanQuyen(VaiTro.Admin)]
public class ImportSinhVienController : AppControllerBase
{
    private const int GioiHanByte = 5 * 1024 * 1024;   // 5 MB

    private readonly ImportSinhVienService _service;

    public ImportSinhVienController(ImportSinhVienService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new ImportSinhVienVM { MatKhauMacDinh = ImportSinhVienService.MatKhauMacDinh });
    }

    // Tải file Excel mẫu
    [HttpGet]
    public IActionResult Mau()
    {
        return File(_service.TaoFileMau(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Mau_Nhap_Sinh_Vien.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(GioiHanByte)]
    public async Task<IActionResult> Index(IFormFile file, bool chiKiemTra, bool dungNeuCoLoi)
    {
        var vm = new ImportSinhVienVM
        {
            ChiKiemTra = chiKiemTra,
            DungNeuCoLoi = dungNeuCoLoi,
            MatKhauMacDinh = ImportSinhVienService.MatKhauMacDinh
        };

        if (file == null || file.Length == 0)
        {
            vm.ThongBaoLoi = "Vui lòng chọn file Excel (.xlsx).";
            return View(vm);
        }
        if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            vm.ThongBaoLoi = "Chỉ nhận file .xlsx. Nếu bạn có file .xls, hãy mở bằng Excel rồi lưu lại dạng .xlsx.";
            return View(vm);
        }
        if (file.Length > GioiHanByte)
        {
            vm.ThongBaoLoi = "File quá lớn (tối đa 5 MB).";
            return View(vm);
        }

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;

        vm.KetQua = await _service.XuLyAsync(ms, file.FileName, chiKiemTra, dungNeuCoLoi);
        return View(vm);
    }
}