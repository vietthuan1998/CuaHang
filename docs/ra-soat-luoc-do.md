# Rà soát lược đồ (BT017)

## Kết quả rà soát

- `SanPhams.DonGia`: `decimal(18,2)`, đúng quy ước tiền.
- `DonHangs.DonGia` và `ThanhTien`: `decimal(18,2)`, không dùng số thực dấu phẩy động.
- `NgayTao`/`NgayCapNhat`: `datetime2`; ứng dụng gán thời gian UTC.
- Các khóa chính là số tự tăng; khóa ngoại được khai báo. Chi tiết đơn hàng bị xóa cùng đơn (`Cascade`); liên kết tới sản phẩm/khách hàng dùng `Restrict`.
- `SanPhams.Ten` dài tối đa 200, `GhiChu` tối đa 500; các cột trạng thái và bảng có kiểu hữu hạn.
- Dữ liệu đơn hàng giữ `DonGia` chụp lúc bán và `ThanhTien`, nên thay đổi giá sản phẩm không làm đổi lịch sử.
- Các thực thể danh mục, khách hàng, sản phẩm và đơn hàng được tách riêng. Bảng `ChiTietDonHangs` lưu dòng sản phẩm, số lượng, giá chụp lúc bán và thành tiền; migration backfill các đơn cũ nhưng giữ cột cũ để tương thích API.
- Năm điều kiện miền giá trị được khai trong cấu hình EF và migration: giá sản phẩm không âm, tồn kho không âm, số lượng đơn/chi tiết dương, giá đơn/chi tiết không âm.

## Cách triển khai an toàn

Kịch bản [`2026-11-12_check-san-pham-don-hang.sql`](../scripts/sql/2026-11-12_check-san-pham-don-hang.sql) kiểm tra dữ liệu vi phạm trước khi thêm CHECK và bỏ qua constraint đã tồn tại để chạy lại được. Chạy kịch bản trên bản sao CSDL trước; chưa áp dụng lên CSDL dùng chung.

Migration `HoanThienBaiTap` tạo các bảng tài khoản, đồng bộ, webhook/outbox, chi tiết đơn hàng và cột tên ảnh; đồng thời backfill đơn cũ. Tất cả migration vẫn cần được duyệt và áp dụng trên bản sao CSDL trước khi nghiệm thu. Không migration nào được áp dụng lên CSDL dùng chung trong quá trình này.

BT017 chỉ được nghiệm thu sau khi kịch bản đã chạy trên bản sao, ba ca dữ liệu sai bị từ chối và một ca dữ liệu hợp lệ được chấp nhận. Chưa có bằng chứng chạy CSDL trong repo.
