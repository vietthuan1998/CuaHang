/*
Ngày: 2026-11-12
Mục đích: Thêm mã phiên bản cho SanPhams để phát hiện cập nhật đồng thời.
Tính chất: Cộng thêm, chạy lại được; áp dụng sau migration hiện tại.
Hành vi đổi: Cập nhật cạnh tranh trên cùng sản phẩm có thể bị từ chối thay vì ghi đè.
Hành vi không đổi: Không sửa giá trị, kiểu cột, khóa hay quan hệ hiện có.
*/

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112025119_ThemRowVersionSanPham'
)
BEGIN
    ALTER TABLE [SanPhams] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112025119_ThemRowVersionSanPham'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261112025119_ThemRowVersionSanPham', N'8.0.33');
END;
GO

COMMIT;
GO
