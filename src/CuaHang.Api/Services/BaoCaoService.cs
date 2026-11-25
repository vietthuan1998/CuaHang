using CuaHang.Api.Data;
using CuaHang.Api.Dtos;
using CuaHang.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CuaHang.Api.Services;

public sealed class BaoCaoService(AppDbContext context)
{
    public async Task<PaginateResponse<BaoCaoSanPhamThangResponse>> DoanhSoTheoSanPhamTheoThangAsync(
        DateTime tuNgay,
        DateTime denNgay,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var truyVan = context.BaoCaoSanPhamThangs.FromSqlInterpolated($"""
            SELECT
                sp.Id AS SanPhamId,
                sp.Ten,
                CASE
                    WHEN dh.Id IS NULL THEN CAST(NULL AS datetime2)
                    ELSE DATEFROMPARTS(YEAR(dh.NgayTao), MONTH(dh.NgayTao), 1)
                END AS Thang,
                ISNULL(SUM(dh.ThanhTien), 0) AS DoanhSo,
                COUNT(DISTINCT dh.Id) AS SoDonHang
            FROM dbo.SanPhams AS sp
            LEFT JOIN (
                SELECT ct.SanPhamId, dh.Id, dh.NgayTao, ct.ThanhTien
                FROM dbo.ChiTietDonHangs AS ct
                INNER JOIN dbo.DonHangs AS dh ON dh.Id = ct.DonHangId
                WHERE dh.NgayTao >= {tuNgay}
                    AND dh.NgayTao < {denNgay}
                    AND dh.TrangThai <> 'DaHuy'
            ) AS dh ON dh.SanPhamId = sp.Id
            GROUP BY
                sp.Id,
                sp.Ten,
                YEAR(dh.NgayTao),
                MONTH(dh.NgayTao)
            """);

        var tongSo = await truyVan.CountAsync(ct);
        var danhSach = await truyVan
            .OrderByDescending(x => x.DoanhSo)
            .ThenBy(x => x.SanPhamId)
            .ThenBy(x => x.Thang)
            .Skip((int)((long)(page - 1) * pageSize))
            .Take(pageSize)
            .Select(x => new BaoCaoSanPhamThangResponse(
                x.SanPhamId,
                x.Ten,
                x.Thang,
                x.DoanhSo,
                x.SoDonHang))
            .ToListAsync(ct);

        return new PaginateResponse<BaoCaoSanPhamThangResponse>(
            danhSach, page, pageSize, tongSo);
    }

    public async Task<PaginateResponse<BaoCaoSanPhamThangResponse>> DoanhSoTheoSanPhamTheoThangLinqAsync(
        DateTime tuNgay,
        DateTime denNgay,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var donHangQuery = context.DonHangs
            .AsNoTracking()
            .Where(x => x.NgayTao >= tuNgay
                && x.NgayTao < denNgay
                && x.TrangThai != TrangThaiDon.DaHuy);

        var dongDonHangQuery =
            from chiTiet in context.ChiTietDonHangs.AsNoTracking()
            join donHang in donHangQuery on chiTiet.DonHangId equals donHang.Id
            select new
            {
                chiTiet.SanPhamId,
                chiTiet.ThanhTien,
                donHang.Id,
                donHang.NgayTao
            };
        var truyVan = from sanPham in context.SanPhams.AsNoTracking()
                      join dongDonHang in dongDonHangQuery
                          on (int?)sanPham.Id equals dongDonHang.SanPhamId into dongDonHangs
                      from dongDonHang in dongDonHangs.DefaultIfEmpty()
                      group new { sanPham, dongDonHang } by new
                      {
                          sanPham.Id,
                          sanPham.Ten,
                          Nam = dongDonHang == null ? (int?)null : dongDonHang.NgayTao.Year,
                          ThangSo = dongDonHang == null ? (int?)null : dongDonHang.NgayTao.Month
                      }
            into nhom
                      select new
                      {
                          nhom.Key.Id,
                          nhom.Key.Ten,
                          nhom.Key.Nam,
                          nhom.Key.ThangSo,
                          DoanhSo = nhom.Sum(x =>
                              x.dongDonHang == null ? (decimal?)null : x.dongDonHang.ThanhTien) ?? 0m,
                          SoDonHang = nhom
                              .Where(x => x.dongDonHang != null)
                              .Select(x => x.dongDonHang.Id)
                              .Distinct()
                              .Count()
                      };

        var tongSo = await truyVan.CountAsync(ct);
        var danhSach = await truyVan
            .OrderByDescending(x => x.DoanhSo)
            .ThenBy(x => x.Id)
            .ThenBy(x => x.Nam)
            .ThenBy(x => x.ThangSo)
            .Skip((int)((long)(page - 1) * pageSize))
            .Take(pageSize)
            .Select(x => new BaoCaoSanPhamThangResponse(
                x.Id,
                x.Ten,
                x.Nam.HasValue
                    ? new DateTime(x.Nam.Value, x.ThangSo.GetValueOrDefault(), 1)
                    : null,
                x.DoanhSo,
                x.SoDonHang))
            .ToListAsync(ct);

        return new PaginateResponse<BaoCaoSanPhamThangResponse>(
            danhSach, page, pageSize, tongSo);
    }

    // The interpolated value is sent as a SQL parameter, including quotes or SQL-like input.
    public async Task<IReadOnlyList<BaoCaoDonHangResponse>> TimTheoTrangThaiAsync(
        string trangThai, CancellationToken ct = default)
        => await context.DonHangs
            .FromSqlInterpolated($"SELECT * FROM DonHangs WHERE TrangThai = {trangThai}")
            .AsNoTracking()
            .Select(x => new BaoCaoDonHangResponse(x.Id, x.TrangThai, x.ThanhTien, x.NgayTao))
            .ToListAsync(ct);
}
