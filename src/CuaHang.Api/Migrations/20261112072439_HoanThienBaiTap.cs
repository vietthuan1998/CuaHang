using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuaHang.Api.Migrations
{
    /// <inheritdoc />
    public partial class HoanThienBaiTap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnhTenTep",
                table: "SanPhams",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChiTietDonHangs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DonHangId = table.Column<int>(type: "int", nullable: false),
                    SanPhamId = table.Column<int>(type: "int", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ThanhTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonHangs", x => x.Id);
                    table.CheckConstraint("CK_ChiTietDonHangs_DonGia", "[DonGia] >= 0");
                    table.CheckConstraint("CK_ChiTietDonHangs_SoLuong", "[SoLuong] > 0");
                    table.ForeignKey(
                        name: "FK_ChiTietDonHangs_DonHangs_DonHangId",
                        column: x => x.DonHangId,
                        principalTable: "DonHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHangs_SanPhams_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "SanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DongBoLichSus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KyBatDauUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    MaTrangThaiNgoai = table.Column<int>(type: "int", nullable: true),
                    NgayCapNhatUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DongBoLichSus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HopThuDis",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoaiSuKien = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    SoLanThu = table.Column<int>(type: "int", nullable: false),
                    GuiSauUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayTaoUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HopThuDis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDungs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MatKhauBam = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    VaiTro = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    HoatDong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDungs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThanhToanWebhooks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaThanhToanNgoai = table.Column<string>(type: "varchar(120)", unicode: false, maxLength: 120, nullable: false),
                    DonHangId = table.Column<int>(type: "int", nullable: false),
                    NgayNhanUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhToanWebhooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThanhToanWebhooks_DonHangs_DonHangId",
                        column: x => x.DonHangId,
                        principalTable: "DonHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_DonHangId",
                table: "ChiTietDonHangs",
                column: "DonHangId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_SanPhamId",
                table: "ChiTietDonHangs",
                column: "SanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_DongBoLichSus_KyBatDauUtc",
                table: "DongBoLichSus",
                column: "KyBatDauUtc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HopThuDis_TrangThai_GuiSauUtc",
                table: "HopThuDis",
                columns: new[] { "TrangThai", "GuiSauUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDungs_TenDangNhap",
                table: "NguoiDungs",
                column: "TenDangNhap",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToanWebhooks_DonHangId",
                table: "ThanhToanWebhooks",
                column: "DonHangId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToanWebhooks_MaThanhToanNgoai",
                table: "ThanhToanWebhooks",
                column: "MaThanhToanNgoai",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDonHangs");

            migrationBuilder.DropTable(
                name: "DongBoLichSus");

            migrationBuilder.DropTable(
                name: "HopThuDis");

            migrationBuilder.DropTable(
                name: "NguoiDungs");

            migrationBuilder.DropTable(
                name: "ThanhToanWebhooks");

            migrationBuilder.DropColumn(
                name: "AnhTenTep",
                table: "SanPhams");
        }
    }
}
