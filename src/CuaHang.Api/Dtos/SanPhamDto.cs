using System;

namespace CuaHang.Api.Dtos;

public record SanPhamRequest(
  string? Ten,
  decimal? DonGia,
  int? TonKho,
  int? DanhMucId,
  string? GhiChu
);
public record SanPhamResponse(
  int Id,
  int DanhMucId,
  string Ten,
  decimal DonGia,
  int TonKho,
  string? GhiChu,
  byte[]? RowVersion = null,
  string? AnhUrl = null
);

public record CapNhatTonKhoRequest(int? TonKho, string? RowVersion);
