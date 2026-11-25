using System;

namespace CuaHang.Api.Dtos;

public record PaginateResponse<T>
(
  IReadOnlyList<T> DanhSach,
  int Trang,
  int SoDong,
  int TongSo
);
