using CuaHang.Api.Common;
using CuaHang.Api.Data;
using CuaHang.Api.Dtos;
using CuaHang.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CuaHang.Api.Services;

public sealed class DonHangService(
    AppDbContext context,
    ThongBaoService thongBaoService)
{
    // Giữ overload cho các consumer nội bộ; endpoint HTTP luôn truyền khóa của client.
    public Task<DonHangResponse> DatHangAsync(
        DatHangRequest request, CancellationToken ct = default)
        => DatHangAsync(request, Guid.NewGuid().ToString("N"), "system", ct);

    public async Task<PaginateResponse<DonHangDanhSachResponse>> TimAsync(
        TrangThaiDon? trangThai,
        DateTime? tuNgay,
        DateTime? denNgay,
        int? khachHangId,
        int page,
        int pageSize,
        string? nguoiTao,
        bool quanTri,
        CancellationToken ct = default)
    {
        var query = context.DonHangs.AsNoTracking().AsQueryable();
        if (trangThai.HasValue)
            query = query.Where(x => x.TrangThai == trangThai.Value);
        if (tuNgay.HasValue)
            query = query.Where(x => x.NgayTao >= tuNgay.Value);
        if (denNgay.HasValue)
            query = query.Where(x => x.NgayTao < denNgay.Value);
        if (khachHangId.HasValue)
            query = query.Where(x => x.KhachHangId == khachHangId.Value);
        if (!quanTri)
            query = query.Where(x => x.NguoiTao == nguoiTao);

        var tongSo = await query.CountAsync(ct);
        var danhSach = await query
            .OrderByDescending(x => x.NgayTao)
            .ThenByDescending(x => x.Id)
            .Skip((int)((long)(page - 1) * pageSize))
            .Take(pageSize)
            .Select(x => new DonHangDanhSachResponse(
                x.Id,
                x.KhachHangId,
                x.KhachHang == null ? null : x.KhachHang.Ten,
                x.SanPhamId,
                x.SanPham == null ? null : x.SanPham.Ten,
                x.SoLuong,
                x.ThanhTien,
                x.TrangThai,
                x.NgayTao))
            .ToListAsync(ct);

        return new PaginateResponse<DonHangDanhSachResponse>(
            danhSach, page, pageSize, tongSo);
    }

    public async Task<DonHangResponse> DatHangAsync(
        DatHangRequest request,
        string idempotencyKey,
        string nguoiDat,
        CancellationToken ct = default)
    {
        var khachHangId = request.KhachHangId
            ?? throw new DuLieuKhongHopLeException("Thiếu khách hàng");
        var sanPhamId = request.SanPhamId
            ?? throw new DuLieuKhongHopLeException("Thiếu sản phẩm");
        var soLuong = request.SoLuong
            ?? throw new DuLieuKhongHopLeException("Thiếu số lượng");

        if (khachHangId <= 0 || sanPhamId <= 0 || soLuong <= 0)
            throw new DuLieuKhongHopLeException("Thông tin đặt hàng không hợp lệ");

        var daCo = await context.DonHangs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, ct);
        if (daCo is not null)
            return KiemTraDonLap(daCo, khachHangId, sanPhamId, soLuong, nguoiDat);

        var sanPham = await context.SanPhams
            .FirstOrDefaultAsync(x => x.Id == sanPhamId, ct)
            ?? throw new DuLieuKhongHopLeException("Sản phẩm không tồn tại");
        if (!await context.KhachHangs.AsNoTracking().AnyAsync(x => x.Id == khachHangId, ct))
            throw new DuLieuKhongHopLeException("Khách hàng không tồn tại");
        if (sanPham.TonKho < soLuong)
            throw new HetHangException("Số lượng tồn kho không đủ");

        var thanhTien = MayTinhTien.TinhThanhTien(sanPham.DonGia, soLuong);
        if (thanhTien > 9999999999999999.99m)
            throw new DuLieuKhongHopLeException("Thành tiền vượt giới hạn cho phép");

        var donHang = new DonHang
        {
            KhachHangId = khachHangId,
            SanPhamId = sanPhamId,
            SoLuong = soLuong,
            DonGia = sanPham.DonGia,
            ThanhTien = thanhTien,
            TrangThai = TrangThaiDon.Moi,
            IdempotencyKey = idempotencyKey
        };
        donHang.ChiTietDonHangs.Add(new ChiTietDonHang
        {
            SanPhamId = sanPham.Id,
            SoLuong = soLuong,
            DonGia = sanPham.DonGia,
            ThanhTien = thanhTien
        });

        sanPham.TonKho -= soLuong;
        context.DonHangs.Add(donHang);
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new XungDotException("Sản phẩm vừa được cập nhật, vui lòng thử lại");
        }
        catch (DbUpdateException ex) when (SqlLoi.LaViPhamChiMucDuyNhat(ex))
        {
            context.ChangeTracker.Clear();
            var daTao = await context.DonHangs.AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, ct);
            if (daTao is null)
                throw;
            return KiemTraDonLap(daTao, khachHangId, sanPhamId, soLuong, nguoiDat);
        }

        await thongBaoService.DonHangMoiAsync(donHang.Id, ct);
        return ChuyenDoi(donHang);
    }

    public async Task<DonHangResponse?> LayTheoMaAsync(int id, CancellationToken ct = default)
        => await context.DonHangs
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new DonHangResponse(
                x.Id,
                x.KhachHangId,
                x.SanPhamId,
                x.SoLuong,
                x.DonGia,
                x.ThanhTien,
                x.TrangThai,
                x.NgayTao,
                x.NgayCapNhat))
            .FirstOrDefaultAsync(ct);

    public Task<bool> LaNguoiTaoAsync(
        int id,
        string tenDangNhap,
        CancellationToken ct = default)
        => context.DonHangs.AsNoTracking()
            .AnyAsync(x => x.Id == id && x.NguoiTao == tenDangNhap, ct);

    public async Task HuyAsync(int id, CancellationToken ct = default)
    {
        var thongTin = await context.DonHangs
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new { x.SanPhamId, x.SoLuong })
            .FirstOrDefaultAsync(ct)
            ?? throw new KhongTimThayException("Không tìm thấy đơn hàng");

        var donHang = await context.DonHangs
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KhongTimThayException("Không tìm thấy đơn hàng");

        if (donHang.TrangThai == TrangThaiDon.DaHuy)
            throw new XungDotException("Đơn hàng đã được hủy");
        if (donHang.TrangThai is not (TrangThaiDon.Moi or TrangThaiDon.DaThanhToan))
            throw new XungDotException("Trạng thái đơn hàng không thể hủy");

        var chiTiet = await context.ChiTietDonHangs.AsNoTracking()
            .Where(x => x.DonHangId == id)
            .Select(x => new { x.SanPhamId, x.SoLuong })
            .ToListAsync(ct);
        if (chiTiet.Count == 0 && thongTin.SanPhamId is int sanPhamId)
            chiTiet.Add(new { SanPhamId = (int?)sanPhamId, SoLuong = thongTin.SoLuong });

        foreach (var item in chiTiet)
        {
            if (item.SanPhamId is not int itemSanPhamId)
                continue;
            var sanPham = await context.SanPhams.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == itemSanPhamId, ct);
            if (sanPham is not null)
                sanPham.TonKho = checked(sanPham.TonKho + item.SoLuong);
        }
        donHang.TrangThai = TrangThaiDon.DaHuy;

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new XungDotException("Đơn hàng hoặc tồn kho vừa được cập nhật");
        }
    }

    private static DonHangResponse ChuyenDoi(DonHang donHang)
        => new(
            donHang.Id,
            donHang.KhachHangId,
            donHang.SanPhamId,
            donHang.SoLuong,
            donHang.DonGia,
            donHang.ThanhTien,
            donHang.TrangThai,
            donHang.NgayTao,
            donHang.NgayCapNhat);

    private static DonHangResponse KiemTraDonLap(
        DonHang donHang, int khachHangId, int sanPhamId, int soLuong, string nguoiDat)
    {
        if (donHang.NguoiTao != nguoiDat
            || donHang.KhachHangId != khachHangId
            || donHang.SanPhamId != sanPhamId
            || donHang.SoLuong != soLuong)
            throw new XungDotException("Idempotency-Key đã được dùng cho yêu cầu khác");
        return ChuyenDoi(donHang);
    }
}
