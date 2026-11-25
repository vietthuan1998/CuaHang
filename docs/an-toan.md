# Rà soát an toàn (BT016)

Trạng thái dưới đây phản ánh những gì hiện có trong mã nguồn. Chưa thể xác nhận trạng thái của bí mật trong lịch sử Git vì thư mục làm việc hiện không có repository Git; không đọc hoặc chép nội dung tệp `.env` vào tài liệu này.

| Mục | Trạng thái | Bằng chứng | Việc còn lại |
|---|---|---|---|
| SQL có tham số | Đã có ở các truy vấn hiện tại | EF Core LINQ và `FromSqlInterpolated` trong `src/CuaHang.Api/Services/BaoCaoService.cs` | Tiếp tục kiểm tra mọi truy vấn mới; chưa có kiểm thử chạy tiêm SQL trên SQL Server |
| Không tin dữ liệu gửi lên | Đang làm | Validator sản phẩm và kiểm tra đầu vào đặt hàng trong controller | Bổ sung kiểm thử tích hợp cho mọi nhánh API và xác thực |
| Bí mật trong mã nguồn/Git | Chưa thể xác minh | Khóa ký JWT, tài khoản bootstrap và bí mật webhook chỉ được đọc từ cấu hình môi trường; chưa có repository Git ở workspace để rà lịch sử | Cấu hình `Jwt__SigningKey` (ít nhất 32 byte), `Webhook__SigningSecret`, `Auth__AdminUsername`/`Auth__AdminPassword`, `Auth__StaffUsername`/`Auth__StaffPassword` bằng user-secrets/biến môi trường; quét lịch sử Git và xoay khóa nếu từng bị commit |
| Phản hồi lỗi không lộ chi tiết | Đã có | `XuLyNgoaiLeHandler` trả lỗi hệ thống chung cho ngoại lệ không dự kiến | Bổ sung test tích hợp cho 500 và rà mọi middleware |
| Kiểm tra tệp tải lên | Đã triển khai ở mã nguồn | Endpoint ảnh chỉ nhận JPEG/PNG/WebP có chữ ký tệp khớp, giới hạn 5 MB và lưu trong `App_Data` ngoài web root; chỉ người có quyền sửa sản phẩm được tải lên | Cần chạy migration và kiểm thử lưu trữ trên môi trường triển khai; hóa đơn PDF được tạo từ dữ liệu đơn, không nhận PDF tùy ý |
| Phân quyền phía máy chủ | Đã triển khai ở mã nguồn | `Program.cs` kiểm issuer, audience, hạn, chữ ký, khóa; policy `DuocSuaSanPham`; chủ sở hữu đơn được lọc ở máy chủ | Phải cấu hình secret, áp migration và kiểm thử bằng SQL Server trước khi triển khai |
| CORS giới hạn nguồn | Đã cấu hình Development | `Cors:AllowedOrigins` chỉ cho `http://localhost:5158` trong `appsettings.Development.json` | Cấu hình riêng origin thật ở môi trường triển khai; production hiện không cho phép origin nào |
| Băm mật khẩu có muối | Đã triển khai ở mã nguồn | `XacThucService` dùng `PasswordHasher<NguoiDung>`; migration tài khoản cần áp dụng | Nạp tài khoản qua biến môi trường hoặc endpoint QuanTri; không dùng mật khẩu mặc định |

## Cấu hình chạy local

Ứng dụng dừng khi không có khóa ký JWT hợp lệ; đây là kiểm tra bảo mật có chủ ý, không phải lỗi biên dịch. Không thêm khóa vào `appsettings*.json` hay Git. Cấu hình khóa riêng cho máy phát triển bằng .NET user-secrets:

```sh
dotnet user-secrets init --project src/CuaHang.Api
dotnet user-secrets set "Jwt:SigningKey" "<khóa ngẫu nhiên dài tối thiểu 32 byte>" --project src/CuaHang.Api
```

Môi trường triển khai phải cung cấp `Jwt__SigningKey` qua secret store/biến môi trường. Issuer và audience mặc định nằm trong `appsettings.json`; tài khoản bootstrap và webhook cũng phải cấu hình riêng khi sử dụng.
