# Quy ước kỹ thuật: CuaHang API

Tài liệu này là hợp đồng làm việc của dự án. Mọi bài tập và mọi thay đổi code phải theo nó. Muốn đổi quy ước thì sửa tài liệu này trong cùng commit và ghi lý do ở mục 14.

## 1. Phiên bản công nghệ

| Thành phần                                       | Phiên bản                                  |
| ------------------------------------------------ | ------------------------------------------ |
| .NET                                             | 8.0                                        |
| Entity Framework Core và nhà cung cấp SQL Server | 8.0.10                                     |
| Công cụ `dotnet-ef`                              | 8.0.10, ghim theo dự án bằng tool manifest |
| Newtonsoft.Json                                  | 13.0.3                                     |
| Gắn Newtonsoft vào MVC                           | 8.0.13                                     |
| xUnit                                            | 2.9.2                                      |
| Cơ sở dữ liệu                                    | SQL Server 2022 (Docker)                   |

Không nâng phiên bản giữa chừng mà không ghi vào mục 14.

## 2. Cấu trúc thư mục

```
CuaHang.sln
├── src/CuaHang.Api/     Controllers, Services, Entities, Data, Dtos, Validation,
│                        Middleware, Security, Integrations, Background, Hubs,
│                        Reporting, Common, Program.cs
├── tests/CuaHang.Tests/ Unit, Integration
├── scripts/sql/         Kịch bản triển khai chạy lại được, đặt tên theo ngày
└── docs/                Tài liệu thiết kế và quy ước
```

Kiến trúc: **Controller → Service → `DbContext`**. Không có lớp Repository. Controller không chứa nghiệp vụ. Chỉ tạo interface ở chỗ cần thay bằng bản giả khi kiểm thử (dịch vụ gọi ngoài, bộ kiểm chữ ký, đồng hồ hệ thống).

## 3. Đặt tên

| Đối tượng           | Quy tắc                                         | Ví dụ                            |
| ------------------- | ----------------------------------------------- | -------------------------------- |
| Lớp thực thể        | Số ít, PascalCase                               | `DonHang`                        |
| Bảng                | Số nhiều                                        | `DonHangs`                       |
| Khóa chính          | `Id`                                            | `Id`                             |
| Khóa ngoại          | `<TênBảng>Id`                                   | `SanPhamId`                      |
| Cột thời gian       | `NgayTao`, `NgayCapNhat`                        |                                  |
| Cột người thực hiện | `NguoiTao`, `NguoiCapNhat`                      |                                  |
| DTO                 | Hậu tố `Request` hoặc `Response`, dùng `record` | `DatHangRequest`                 |
| Endpoint            | kebab-case, không dấu                           | `api/san-pham`                   |
| Thuộc tính JSON     | camelCase, tiếng Việt không dấu                 | `thanhTien`, `soLuong`           |
| Tên ca kiểm thử     | Nêu đúng điều khẳng định                        | `Khong_giam_gia_khi_duoi_nguong` |
| Tên migration       | Có nghĩa, nêu việc đã làm                       | `ThemCotDaXoaSanPham`            |

## 4. Endpoint và phản hồi

**Khuôn phản hồi (một khuôn cho cả hệ thống):**

- Thành công: `{ "thanhCong": true, "duLieu": ... }`
- Thất bại: `{ "thanhCong": false, "loi": "...", "chiTiet": [ { "truong": "...", "thongBao": "..." } ] }`. Mảng `chiTiet` chỉ có khi trả 400.
- JSON bỏ qua trường có giá trị null. Phía nhận phải chịu được việc thiếu khóa.

**Phân trang:** query `page` (từ 1, mặc định 1) và `pageSize` (1 đến 200, mặc định 20). Kết quả: `duLieu: { danhSach, trang, soDong, tongSo }`. `pageSize` ngoài khoảng trả 400.

**Mã trạng thái:**

| Tình huống                                                                                        | Mã                         |
| ------------------------------------------------------------------------------------------------- | -------------------------- |
| Thành công, có dữ liệu                                                                            | 200                        |
| Tạo mới                                                                                           | 201 kèm tiêu đề `Location` |
| Thành công, không có nội dung                                                                     | 204                        |
| Đầu vào sai, thiếu, vô lý; tham chiếu sai trong thân                                              | 400                        |
| Chưa đăng nhập, vé hỏng hoặc hết hạn                                                              | 401                        |
| Đã đăng nhập nhưng không đủ quyền                                                                 | 403                        |
| Tài nguyên trong đường dẫn không tồn tại                                                          | 404                        |
| Xung đột trạng thái hoặc quy tắc (trùng khóa, hết hàng, chuyển trạng thái sai, lệch `RowVersion`) | 409                        |
| Bị giới hạn tần suất                                                                              | 429                        |
| Lỗi không lường trước                                                                             | 500                        |

Thông báo 403 ngắn gọn, không nêu chi tiết. Phản hồi 500 chỉ ghi "Lỗi hệ thống"; vết ngăn xếp chỉ vào nhật ký.

## 5. Kiểm tra đầu vào

- Tắt bộ lọc tự động của `[ApiController]` bằng `SuppressModelStateInvalidFilter = true` (§5.4 của tài liệu nghiên cứu), và **mọi hành động nhận dữ liệu tự kiểm tra ngay ở đầu**.
- Bộ kiểm tra nằm ở `Validation/`, trả danh sách lỗi theo trường.
- Kiểm kiểu, độ dài, khoảng giá trị, và sự tồn tại của bản ghi được tham chiếu.
- Không tin dữ liệu gửi lên; không cho client tự đặt các trường nhạy cảm (`NguoiTao`, `DonGia` chụp lúc bán, `ThanhTien`).

## 6. Dãy trung gian (thứ tự chạy)

1. Xử lý ngoại lệ (ngoài cùng)
2. Gọi chéo nguồn (CORS), chỉ cho danh sách tên miền cụ thể
3. Nén phản hồi
4. Giới hạn tần suất
5. Xác thực
6. Phân quyền
7. Định tuyến tới controller

Đổi thứ tự là đổi hành vi, phải ghi vào mục 14.

## 7. Xác thực và phân quyền

- Vai trò: `QuanTri`, `NhanVien`. Chính sách `DuocSuaSanPham` yêu cầu claim `quyen = sua_san_pham` (cấp cho `QuanTri`).
- Quyền theo bản ghi: `NhanVien` chỉ thao tác trên đơn do mình tạo; `QuanTri` thao tác mọi đơn. Nguyên tắc: từ chối trước, từ chối thắng cho phép.
- Khóa ký, chuỗi kết nối, mật khẩu: không ở trong mã nguồn hay Git; đọc từ user-secrets hoặc biến môi trường.
- Mật khẩu chỉ lưu dạng băm có muối.
- Khi có nhiều lược đồ xác thực, dùng `[Authorize]` trần, không ghi kèm tên lược đồ.
- Phân quyền nằm ở tầng máy chủ. Mọi đường tải tệp và kết xuất đi qua cùng phân quyền với màn hình.
- Trước milestone M6, các endpoint chạy không xác thực trong môi trường phát triển.

## 8. Dữ liệu

| Hạng mục            | Quy ước                                                                                                       |
| ------------------- | ------------------------------------------------------------------------------------------------------------- |
| Tiền                | `decimal(18,2)` trong SQL, `decimal` trong C#, không bao giờ dùng `double`                                    |
| Làm tròn thành tiền | 2 chữ số thập phân, làm tròn xa số 0                                                                          |
| Chuỗi có dấu        | `nvarchar(n)` với `n` đủ dùng; chuỗi hằng trong T-SQL viết `N'...'`; không dùng `nvarchar(MAX)` khi không cần |
| Ngày giờ            | `datetime2`; mọi thời gian là UTC. Định dạng chuỗi trong JSON chốt ở bài BT015 và ghi vào mục 14              |
| Khóa chính          | Số tự tăng, không mang nghĩa nghiệp vụ                                                                        |
| Khóa ngoại          | Khai thật; hành vi xóa mặc định `Restrict`                                                                    |
| Xóa                 | Chỉ `SanPham` xóa mềm (`DaXoa`), dùng bộ lọc truy vấn toàn cục. `DonHang` không xóa, chỉ chuyển trạng thái    |
| Trạng thái đơn      | Lưu dạng chuỗi qua bộ chuyển đổi giá trị: `Moi`, `DaThanhToan`, `DangGiao`, `HoanTat`, `DaHuy`                |
| Đồng thời           | `RowVersion` trên `SanPham`, khai rõ bằng `IsRowVersion()`                                                    |

**Chuyển trạng thái đơn:**

| Từ                 | Sang được              |
| ------------------ | ---------------------- |
| `Moi`              | `DaThanhToan`, `DaHuy` |
| `DaThanhToan`      | `DangGiao`, `DaHuy`    |
| `DangGiao`         | `HoanTat`              |
| `HoanTat`, `DaHuy` | Không đi tiếp          |

## 9. Quy tắc nghiệp vụ cố định

- **Giảm giá theo số lượng:** dưới 10 không giảm; từ 10 đến 19 giảm 5%; từ 20 giảm 10%.
- `DonHang.DonGia` là bản chụp giá tại lúc bán. `ThanhTien = SoLuong × DonGia × (1 − tỷ lệ giảm)`.
- Tồn kho trừ ngay khi tạo đơn. Mua đúng bằng tồn được; vượt tồn trả 409.
- Hủy đơn cộng trả tồn trong cùng một giao dịch. Hủy đơn đã hủy trả 409.
- Doanh số không tính đơn `DaHuy`.

## 10. Truy cập dữ liệu

- Truy vấn chỉ đọc luôn có `AsNoTracking()`.
- Không trả thực thể ra ngoài; dùng DTO.
- Tránh N+1: dùng `Include` hoặc `Select` chiếu thẳng.
- Giữ `IQueryable` cho tới khi thật sự cần dữ liệu; không gọi hàm tự viết bên trong `Where`.
- SQL thô luôn truyền tham số, không ghép chuỗi từ dữ liệu người dùng.
- Giao dịch giữ càng ngắn càng tốt; không gọi dịch vụ ngoài bên trong giao dịch.
- Bảng được truy cập theo cùng một thứ tự trong mọi luồng xử lý (tránh khóa chết).
- Không dùng `WITH (NOLOCK)` làm giải pháp chung.

## 11. Bất đồng bộ, cấu hình, nhật ký

- Hàm có `await` thì khai `async` và trả `Task`. `await` suốt từ trên xuống; không dùng `.Result` hay `.Wait()`. Truyền `CancellationToken` xuống tầng dưới.
- Bật kiểm tra khả rỗng. Không dùng `!` để dập cảnh báo.
- Không bắt ngoại lệ rải rác; bắt tập trung ở middleware. Ngoại lệ nghiệp vụ dùng lớp riêng (`HetHangException`...).
- Cấu hình đọc theo thứ tự: `appsettings.json`, tệp theo môi trường, biến môi trường, tham số dòng lệnh.
- Tính năng rủi ro đặt sau khóa `TinhNang:*`, mặc định tắt, đọc lại mỗi lời gọi hoặc mỗi vòng quét.
- Nhật ký qua `ILogger<T>`, dùng giữ chỗ `{TenThamSo}`, không ghép chuỗi. Mức: Information cho mốc bình thường, Warning cho bất thường xử lý được, Error cho thất bại.
- Không ghi vào nhật ký: vé, mật khẩu, khóa, số định danh cá nhân đầy đủ.
- `EnableSensitiveDataLogging` chỉ bật ở máy cá nhân.

## 12. Migration và kịch bản SQL

- Migration có tên nêu việc đã làm. **Luôn đọc tệp vừa sinh trước khi áp.** Thấy lệnh `Drop` nào thì dừng lại kiểm tra.
- Mỗi milestone thêm migration riêng; không sửa migration đã áp.
- Triển khai thật dùng kịch bản trong `scripts/sql/`: chạy lại được nhiều lần, chỉ cộng thêm.
- Đầu mỗi kịch bản ghi: ngày, mục đích, tính chất, và **hành vi nào đổi, hành vi nào không** sau khi chạy.
- Không đổi kiểu cột đang có dữ liệu. Không thêm cột `NOT NULL` mà không có giá trị mặc định.
- Công cụ `dotnet-ef` chạy qua tool manifest: `dotnet tool run dotnet-ef ...`.

## 13. Kiểm thử, Git và tài liệu

**Kiểm thử**

- Kiểm thử đơn vị cho quy tắc tính toán, trường hợp biên, phân quyền; kiểm thử tích hợp cho luồng quan trọng.
- Mỗi ca dùng một tên cơ sở dữ liệu trong bộ nhớ riêng (sinh từ `Guid`).
- Phần dùng SQL thô và phần phụ thuộc ràng buộc phải thử trên SQL Server thật.
- Sửa một lỗi thì viết thêm một ca để nó không quay lại.
- **Không đưa lên nhánh nào làm đỏ bộ kiểm thử.** Nếu một ca cũ đỏ thì sửa mã của mình; chỉ khi hành vi cũ thật sự cần đổi mới bàn trước rồi sửa ca.

**Git**

- Không ghi nhận thay đổi thẳng lên nhánh chính; mỗi việc một nhánh.
- Trước khi đưa nhánh cho người khác xem: chạy `dotnet build` và `dotnet test`.
- Không đưa bí mật vào Git. Lịch sử Git giữ lại mọi thứ.

**Tài liệu**

- Danh mục API, bảng thứ tự middleware và quyết định thiết kế nằm trong `docs/` và được cập nhật cùng commit với thay đổi tương ứng.

## 14. Nhật ký thay đổi quy ước

| Ngày | Thay đổi  | Lý do |
| ---- | --------- | ----- |
| 2026-11-12 | JSON dùng camelCase; ngày giờ UTC theo ISO 8601 dạng `yyyy-MM-dd'T'HH:mm:ss.fff'Z'`. DTO đầu ra dùng record; tiền dùng `decimal`. | Chốt hợp đồng JSON ở BT015; hậu tố `Z` giúp client nhận biết thời gian UTC. |
