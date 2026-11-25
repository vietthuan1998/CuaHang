using System;
using CuaHang.Api.Data;
using CuaHang.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CuaHang.Api.Services;

public class NhatKyService
{
  private readonly AppDbContext _context;

  public NhatKyService(AppDbContext context) => _context = context;

  public async Task<PaginateResponse<NhatKyResponse>> TimAsync(
      string? tenBang, string? banGhiId, int page, int pageSize, CancellationToken ct)
  {
    var q = _context.NhatKys.AsNoTracking();
    if (!string.IsNullOrEmpty(tenBang)) q = q.Where(x => x.TenBang == tenBang);
    if (!string.IsNullOrEmpty(banGhiId)) q = q.Where(x => x.BanGhiId == banGhiId);

    var tongSo = await q.CountAsync(ct);

    var danhSach = await q
        .OrderByDescending(x => x.NgayTao)
        .ThenByDescending(x => x.Id)
        .Skip((int)((long)(page - 1) * pageSize))
        .Take(pageSize)
        .Select(x => new NhatKyResponse(
            x.Id, x.NguoiThucHien, x.HanhDong, x.TenBang, x.BanGhiId,
            x.GiaTriTruoc, x.GiaTriSau, x.NgayTao))
        .ToListAsync(ct);

    return new PaginateResponse<NhatKyResponse>(danhSach, page, pageSize, tongSo);
  }
}
