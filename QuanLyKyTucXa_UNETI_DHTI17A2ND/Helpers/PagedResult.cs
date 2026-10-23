// Họ và tên: [Trần Hoàng Sơn]
// Mã sinh viên: [23203100010]
// Nội dung thực hiện: Module 2 - Quản lý phòng KTX, tìm kiếm, lọc, sắp xếp, phân trang
using Microsoft.EntityFrameworkCore;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;

public class PagedResultBase
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
}

/// <summary>
/// Kết quả phân trang. Phân trang được thực hiện NGAY TRÊN truy vấn EF Core
/// (CountAsync + Skip/Take), không tải toàn bộ dữ liệu rồi cắt trang.
/// </summary>
public class PagedResult<T> : PagedResultBase
{
    public List<T> Items { get; set; } = new();

    public static async Task<PagedResult<T>> CreateAsync(IQueryable<T> query, int page, int pageSize)
    {
        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        if (page > totalPages) page = totalPages;   // vượt trang cuối -> về trang cuối
        if (page < 1) page = 1;

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResult<T> { Items = items, Page = page, PageSize = pageSize, TotalItems = total };
    }
}
