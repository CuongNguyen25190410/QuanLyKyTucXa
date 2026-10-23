// Họ và tên: [Trần Công Quang Đăng]
// Mã sinh viên: [23203100040]
// Nội dung thực hiện: Module 3 - Quản lý sinh viên, đăng ký ở KTX, xem thông tin/lịch sử ở
using System.ComponentModel.DataAnnotations;

namespace QuanLyKyTucXa_UNETI11_DHTI17A2ND.Helpers;

/// <summary>Ngày sinh hợp lệ: không ở tương lai và tuổi từ 15 đến 80.</summary>
public class NgaySinhHopLeAttribute : ValidationAttribute
{
    public NgaySinhHopLeAttribute() : base("Ngày sinh không hợp lệ (tuổi phải từ 15 đến 80 và không ở tương lai)")
    {
    }

    public override bool IsValid(object value)
    {
        if (value == null) return true;           // [Required] lo việc bắt buộc nhập
        if (value is not DateTime ngaySinh) return false;
        var homNay = DateTime.Today;
        if (ngaySinh.Date > homNay) return false;
        var tuoi = homNay.Year - ngaySinh.Year;
        if (ngaySinh.Date > homNay.AddYears(-tuoi)) tuoi--;
        return tuoi >= 15 && tuoi <= 80;
    }
}
