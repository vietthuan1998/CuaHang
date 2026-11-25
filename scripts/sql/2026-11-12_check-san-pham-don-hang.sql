/*
Ngày: 2026-11-12
Mục đích: Bổ sung CHECK cho giá, tồn kho và số lượng đơn hàng sau khi rà dữ liệu.
Tính chất: Cộng thêm, chạy lại được; chạy trên bản sao CSDL trước khi triển khai.
Hành vi đổi: SQL Server từ chối giá âm, tồn kho âm và số lượng đơn không dương.
Hành vi không đổi: Không sửa/xóa dữ liệu hợp lệ hay thay đổi kiểu cột, khóa và quan hệ.
*/

IF EXISTS (SELECT 1 FROM dbo.SanPhams WHERE DonGia < 0)
    THROW 51010, 'SanPhams co DonGia am; chua them rang buoc.', 1;
IF EXISTS (SELECT 1 FROM dbo.SanPhams WHERE TonKho < 0)
    THROW 51011, 'SanPhams co TonKho am; chua them rang buoc.', 1;
IF EXISTS (SELECT 1 FROM dbo.DonHangs WHERE SoLuong <= 0)
    THROW 51012, 'DonHangs co SoLuong khong duong; chua them rang buoc.', 1;

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_SanPhams_DonGia' AND parent_object_id = OBJECT_ID(N'dbo.SanPhams'))
    ALTER TABLE dbo.SanPhams WITH CHECK ADD CONSTRAINT CK_SanPhams_DonGia CHECK (DonGia >= 0);
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_SanPhams_TonKho' AND parent_object_id = OBJECT_ID(N'dbo.SanPhams'))
    ALTER TABLE dbo.SanPhams WITH CHECK ADD CONSTRAINT CK_SanPhams_TonKho CHECK (TonKho >= 0);
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_DonHangs_SoLuong' AND parent_object_id = OBJECT_ID(N'dbo.DonHangs'))
    ALTER TABLE dbo.DonHangs WITH CHECK ADD CONSTRAINT CK_DonHangs_SoLuong CHECK (SoLuong > 0);
