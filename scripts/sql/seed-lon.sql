/*
Ngày: 2026-11-12
Mục đích: Tạo dữ liệu thử BT006 để đo truy vấn báo cáo trên SQL Server.
Tính chất: Chỉ thêm tối đa 200.000 đơn đánh dấu benchmark; chạy lại không nhân đôi.
Hành vi đổi: Cơ sở dữ liệu thử có thêm đơn benchmark trải đều trong 24 tháng.
Hành vi không đổi: Không sửa đơn hiện có, không xóa dữ liệu, không áp dụng lên production.
*/

IF DB_NAME() NOT LIKE N'%Dev%' AND DB_NAME() NOT LIKE N'%Test%' AND DB_NAME() NOT LIKE N'%Bench%'
    THROW 51020, 'Chi chay seed-lon tren co so du lieu Dev, Test hoac Bench.', 1;

DECLARE @SanPhamId int =
    (SELECT TOP (1) Id FROM dbo.SanPhams WHERE DaXoa = 0 ORDER BY Id);
DECLARE @KhachHangId int =
    (SELECT TOP (1) Id FROM dbo.KhachHangs ORDER BY Id);

IF @SanPhamId IS NULL OR @KhachHangId IS NULL
    THROW 51021, 'Can co it nhat mot san pham va mot khach hang truoc khi seed.', 1;

IF NOT EXISTS (
    SELECT 1 FROM dbo.DonHangs WHERE NguoiTao = N'benchmark-bt006'
)
BEGIN
    BEGIN TRANSACTION;

    WITH So AS
    (
        SELECT TOP (200000)
            ROW_NUMBER() OVER (ORDER BY a.object_id, b.object_id) AS N
        FROM sys.all_objects AS a
        CROSS JOIN sys.all_objects AS b
    )
    INSERT INTO dbo.DonHangs
        (KhachHangId, SanPhamId, SoLuong, DonGia, ThanhTien, TrangThai,
         NgayTao, NgayCapNhat, NguoiTao, NguoiCapNhat)
    SELECT
        @KhachHangId,
        @SanPhamId,
        CONVERT(int, 1 + (So.N % 20)),
        sp.DonGia,
        CONVERT(decimal(18, 2), sp.DonGia * (1 + (So.N % 20))),
        CASE WHEN So.N % 50 = 0 THEN 'DaHuy' ELSE 'DaThanhToan' END,
        DATEADD(DAY, -CONVERT(int, So.N % 730), SYSUTCDATETIME()),
        NULL,
        N'benchmark-bt006',
        NULL
    FROM So
    CROSS JOIN dbo.SanPhams AS sp
    WHERE sp.Id = @SanPhamId;

    INSERT INTO dbo.ChiTietDonHangs
        (DonHangId, SanPhamId, SoLuong, DonGia, ThanhTien)
    SELECT Id, SanPhamId, SoLuong, DonGia, ThanhTien
    FROM dbo.DonHangs AS dh
    WHERE dh.NguoiTao = N'benchmark-bt006'
      AND NOT EXISTS (
          SELECT 1 FROM dbo.ChiTietDonHangs AS ct
          WHERE ct.DonHangId = dh.Id
      );

    COMMIT;
END;
