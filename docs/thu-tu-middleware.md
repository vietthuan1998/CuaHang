# Thứ tự middleware

Thứ tự mong muốn theo quy ước §6 của `quy-uoc.md`:

| Thứ tự | Middleware | Hiện có | Lý do đứng ở vị trí này | Dự kiến thêm |
| --- | --- | --- | --- | --- |
| 1 | `UseExceptionHandler` | Có | Bọc toàn bộ middleware phía sau để chuẩn hóa lỗi. | Đã thêm trong BT014 |
| 2 | `UseRouting` | Có | Tạo endpoint metadata để CORS và rate limiter đọc được chính sách của endpoint. | Đã có |
| 3 | CORS | Có, chỉ `http://localhost:5158` trong Development | Chỉ thêm header cho origin được cấu hình; không bật credentials. | Cấu hình origin triển khai trước khi mở client thật |
| 4 | Nén phản hồi | Có | Nén nội dung phản hồi sau khi CORS quyết định header. | Đã thêm trong BT010 |
| 5 | Giới hạn tần suất | Có policy `dang-nhap` (5 lần/phút/IP) | Từ chối đăng nhập quá dày trước khi xử lý endpoint. Cần gắn policy lên API đăng nhập ở BT007. | Đã thêm policy trong BT010 |
| 6 | Xác thực | Chưa | Tạo principal trước khi phân quyền. | BT007 / M6 |
| 7 | Phân quyền | Có `UseAuthorization`; chưa có `UseAuthentication` | Kiểm tra quyền sau xác thực. | BT007 / M6 |
| 8 | Endpoint controller | Có `MapControllers` | Thực thi controller sau routing, CORS, giới hạn và phân quyền. | Đã có |

Trong `Program.cs`, `UseRouting` được gọi tường minh trước CORS và rate limiter vì hai middleware này cần endpoint metadata. Policy `dang-nhap` chưa có tác dụng cho đến khi endpoint đăng nhập được gắn `[EnableRateLimiting("dang-nhap")]`.
