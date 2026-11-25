/*
Ngày: 2026-11-12
Mục đích: Tạo chỉ mục ứng viên cho phép đo truy vấn BT006.
Tính chất: Cộng thêm, chạy lại được; chỉ chạy trên cơ sở dữ liệu thử.
Hành vi đổi: Có thể giảm đọc khi lọc đơn theo khoảng ngày; cần đo trước khi giữ.
Hành vi không đổi: Không sửa/xóa dữ liệu hay thay đổi khóa và quan hệ.
*/

IF DB_NAME() NOT LIKE N'%Dev%' AND DB_NAME() NOT LIKE N'%Test%' AND DB_NAME() NOT LIKE N'%Bench%'
    THROW 51022, 'Chi chay script chi muc tren co so du lieu Dev, Test hoac Bench.', 1;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_DonHangs_NgayTao_TrangThai'
      AND object_id = OBJECT_ID(N'dbo.DonHangs')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_DonHangs_NgayTao_TrangThai
        ON dbo.DonHangs (NgayTao, TrangThai);
END;
