# Đo hiệu năng truy vấn (BT006)

Chưa có số đo thực tế trong repo. Không kết luận chỉ mục có lợi trước khi chạy các bước dưới đây trên cùng một bản sao CSDL.

## Dữ liệu và truy vấn

1. Dùng CSDL tên có `Dev`, `Test` hoặc `Bench`; không chạy seed trên production.
2. Tạo ít nhất một sản phẩm và một khách hàng, rồi chạy `scripts/sql/seed-lon.sql`.
3. Chọn cùng một sản phẩm và cùng khoảng ngày cho mọi lần đo:

```sql
SET STATISTICS IO ON;
SET STATISTICS TIME ON;

DECLARE @TuNgay datetime2 = DATEADD(MONTH, -12, SYSUTCDATETIME());
DECLARE @DenNgay datetime2 = SYSUTCDATETIME();
DECLARE @SanPhamId int = (SELECT TOP (1) Id FROM dbo.SanPhams ORDER BY Id);

SELECT
    sp.Id,
    sp.Ten,
    DATEFROMPARTS(YEAR(dh.NgayTao), MONTH(dh.NgayTao), 1) AS Thang,
    ISNULL(SUM(dh.ThanhTien), 0) AS DoanhSo,
    COUNT(DISTINCT dh.Id) AS SoDonHang
FROM dbo.SanPhams AS sp
LEFT JOIN (
    SELECT ct.SanPhamId, dh.Id, dh.NgayTao, ct.ThanhTien
    FROM dbo.ChiTietDonHangs AS ct
    INNER JOIN dbo.DonHangs AS dh ON dh.Id = ct.DonHangId
    WHERE dh.NgayTao >= @TuNgay
        AND dh.NgayTao < @DenNgay
        AND dh.TrangThai <> 'DaHuy'
) AS dh ON dh.SanPhamId = sp.Id
WHERE sp.Id = @SanPhamId
GROUP BY sp.Id, sp.Ten, YEAR(dh.NgayTao), MONTH(dh.NgayTao)
ORDER BY DoanhSo DESC;

SET STATISTICS TIME OFF;
SET STATISTICS IO OFF;
```

Chạy vài lần trước khi thêm chỉ mục; ghi logical reads, CPU/thời gian elapsed và phép toán chính từ actual execution plan. Sau đó chạy `scripts/sql/2026-11-12_chi-muc-donhang.sql`, lặp lại đúng truy vấn và điều kiện. Đo thêm cùng một lô ghi trước/sau để thấy chi phí cập nhật chỉ mục. Tắt chỉ mục chỉ trên bản sao thử nghiệm nếu phép đo không chứng minh lợi ích.

## Kết quả cần ghi nhận

| Thử nghiệm | Logical reads | CPU (ms) | Elapsed (ms) | Phép toán chính | Ghi chú |
|---|---:|---:|---:|---|---|
| Trước chỉ mục | Chưa đo | Chưa đo | Chưa đo | Chưa đo | Cùng dữ liệu và tham số |
| Sau chỉ mục | Chưa đo | Chưa đo | Chưa đo | Chưa đo | Cùng dữ liệu và tham số |
| Lô ghi trước/sau | Chưa đo | Chưa đo | Chưa đo | N/A | Cùng số dòng |

Chỉ mục hiện là ứng viên để đo, chưa có bằng chứng để tuyên bố giữ. Thử riêng chỉ mục `NgayTao` và không giữ thêm chỉ mục nếu không có cải thiện đo được.
