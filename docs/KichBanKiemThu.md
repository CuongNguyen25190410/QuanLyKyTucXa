# Kịch bản kiểm thử (đề mục 21)

Dùng dữ liệu mẫu có sẵn. Ghi lại kết quả thực tế và chụp ảnh/quay video làm minh chứng.

## A. Đăng nhập / phân quyền (Module 1)
| # | Thao tác | Kết quả mong đợi |
|---|---|---|
| A1 | Đăng nhập `admin` / `Admin@123` | Vào Dashboard, menu hiển thị chức năng Admin |
| A2 | Đăng nhập `22103100001` / `Sv@123456` | Vào trang chủ, menu chỉ có chức năng Sinh viên |
| A3 | Đăng nhập sai mật khẩu | Báo "Mật khẩu không đúng." |
| A4 | Đăng nhập tên không tồn tại | Báo "Tên đăng nhập không tồn tại." |
| A5 | Đăng nhập `22103100030` / `Sv@123456` | Báo "Tài khoản đã bị khóa..." |
| A6 | Để trống ô đăng nhập | Validation Message hiện dưới từng ô |
| A7 | Đăng nhập bằng sinh viên rồi gõ trực tiếp `/LoaiPhong`, `/ThongKe`, `/SinhVien`, `/PhanPhong`, `/TaiKhoan` | Chuyển sang trang 403 "Không có quyền truy cập" (chứng minh phân quyền ở Controller) |
| A8 | Đăng xuất, bấm nút Back của trình duyệt rồi mở lại `/PhongKTX` | Bị chuyển về trang đăng nhập (Session đã bị xóa) |
| A9 | Admin khóa tài khoản sinh viên đang đăng nhập ở trình duyệt khác, sinh viên bấm sang trang khác | Bị đăng xuất, báo tài khoản bị khóa |

## B. Loại phòng / Phòng (Module 1, 2)
| # | Thao tác | Kết quả mong đợi |
|---|---|---|
| B1 | Thêm loại phòng trùng tên "Phòng 4 người" | Báo "Tên loại phòng đã tồn tại." |
| B2 | Thêm loại phòng SoNguoiToiDa = 0 hoặc đơn giá = 0 | Báo lỗi khoảng giá trị |
| B3 | Xóa loại phòng "Phòng 4 người" | Bị chặn, gợi ý chuyển "Ngừng sử dụng" |
| B4 | Thêm phòng trùng tên "A101" | Báo "Tên phòng đã tồn tại." |
| B5 | Thêm phòng tầng 0 / bỏ trống tòa nhà | Báo lỗi validation |
| B6 | Xóa phòng A101 (đã có lịch sử) | Bị chặn |
| B7 | Tạm ngừng phòng A101 (đang có người) | Bị chặn, báo số sinh viên đang ở |
| B8 | Danh sách phòng, không lọc | 20 phòng chia 3 trang (8 phòng/trang) |
| B9 | Tìm "A1" kết hợp lọc Tòa A + Loại "Phòng 4 người" | Chỉ A101, A102 |
| B10 | Chọn lọc **Còn chỗ**, sắp xếp **Số chỗ còn giảm dần**, sang trang 2 | Điều kiện giữ nguyên; thứ tự đúng |
| B11 | Lọc **Hết chỗ** | Đúng 3 phòng: A101, B101, B203 |
| B12 | Tìm từ khóa không có kết quả (vd "ZZZ") | Hiện thông báo "Không tìm thấy...", không lỗi |
| B13 | Trang đầu / giữa / cuối | Nút Đầu/Trước/Sau/Cuối đúng; vượt trang cuối tự về trang cuối |

## C. Sinh viên / Đăng ký (Module 3)
| # | Thao tác | Kết quả mong đợi |
|---|---|---|
| C1 | Admin thêm sinh viên trùng mã `22103100001` | Báo "Mã sinh viên đã tồn tại." |
| C2 | Thêm sinh viên email sai, SĐT sai, ngày sinh ở tương lai | Báo lỗi từng trường |
| C3 | Sinh viên `22103100001` (đã được phân phòng) bấm Đăng ký | Báo đang được phân phòng, không cho đăng ký |
| C4 | Sinh viên `22103100015` (đã có đăng ký chờ duyệt) bấm Đăng ký | Báo đã có đăng ký đang chờ duyệt |
| C5 | Sinh viên mới: ngày kết thúc ≤ ngày bắt đầu / ngày bắt đầu trong quá khứ | Báo lỗi ngày |
| C6 | Sinh viên hủy đăng ký Chờ duyệt | Trạng thái → Đã hủy |
| C7 | Sinh viên cố hủy đăng ký đã phân phòng (gửi POST thủ công) | Bị từ chối |
| C8 | Sinh viên A mở `/DangKyKTX/Details/<mã đăng ký của sinh viên B>` (đổi ID trên URL) | 404, không xem được |
| C9 | Trang "Thông tin cá nhân" / "Lịch sử ở" | Chỉ hiển thị dữ liệu của chính mình |
| C10 | Admin xóa đăng ký đã phân phòng | Bị chặn |
| C11 | Admin xóa sinh viên đã có đăng ký | Bị chặn, gợi ý "Ngừng hoạt động" (khóa tài khoản, giữ lịch sử) |

## D. Phân phòng / Chuyển / Trả (Module 4)
| # | Thao tác | Kết quả mong đợi |
|---|---|---|
| D1 | Phân SV `…015` (Nam) vào phòng còn chỗ phù hợp | Thành công; đăng ký → Đã phân phòng; PhanPhong "Đang ở" |
| D2 | Phân SV `…016` (Nam) vào **A102 (còn 1 chỗ)** | Thành công; A102 chuyển sang **Hết chỗ** (3/4 → 4/4) |
| D3 | Phân SV `…029` (Nữ) vào A101 hoặc phòng Nam | Báo sai giới tính |
| D4 | Sau D2, phân thêm một SV Nam khác vào A102 | Báo "Phòng đã đủ người" |
| D5 | Gửi POST phân phòng cho phòng tạm ngừng A302 | Báo phòng tạm ngừng |
| D6 | Phân phòng cho đăng ký đã Đã phân phòng / Đã hủy | Bị từ chối |
| D7 | Chuyển SV `…001` từ A101 (đầy) sang A103 | Thành công; A101 còn 1 chỗ trống; có 2 bản ghi trong lịch sử |
| D8 | Chuyển vào chính phòng đang ở | Báo "trùng phòng đang ở" |
| D9 | Chuyển vào phòng đầy (vd B101 / A101 sau khi đầy) | Báo "Phòng đã đủ người" |
| D10 | Chuyển SV Nam sang phòng Nữ | Báo sai giới tính |
| D11 | Trả phòng SV `…001` | PhanPhong + DangKy → Đã trả phòng; chỗ trống tăng 1; Dashboard giảm "chỗ đang sử dụng" |
| D12 | Trả phòng lần hai (mở lại link/POST cũ) | Báo không còn ở phòng, không trả hai lần |
| D13 | Xem lịch sử SV `…013` | Thấy A103 (Đã chuyển) → A201 (Đang ở); "Số lần chuyển phòng = 1" |

## E. Dashboard / Thống kê
| # | Thao tác | Kết quả mong đợi |
|---|---|---|
| E1 | Mở Dashboard | 9 chỉ số; Tổng chỗ = tổng sức chứa các phòng đang sử dụng (88 với dữ liệu mẫu); chỗ đang dùng = số PhanPhong "Đang ở" (24 ban đầu); chỗ còn = tổng − đang dùng |
| E2 | Sau mỗi thao tác D*, mở lại Dashboard / Thống kê | Số liệu thay đổi đúng (không lưu số cố định) |
| E3 | Mở Thống kê chi tiết | Đủ: phòng theo loại, SV theo phòng/tòa/loại phòng, đăng ký theo trạng thái, phòng đông nhất, tỷ lệ sử dụng, SV vào theo tháng |

## F. Kiểm thử tích hợp (đề mục 21.2)
Một kịch bản xuyên suốt: Admin tạo sinh viên + tài khoản → sinh viên đăng nhập, đăng ký KTX → Admin duyệt/phân phòng → Admin chuyển phòng → Admin trả phòng → sinh viên xem lịch sử → Admin kiểm tra Dashboard/Thống kê.
