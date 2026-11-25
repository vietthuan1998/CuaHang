using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuaHang.Api.Migrations
{
    /// <inheritdoc />
    public partial class TaoBangNhatKy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DaXoa",
                table: "SanPhams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayCapNhat",
                table: "SanPhams",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayTao",
                table: "SanPhams",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "NguoiCapNhat",
                table: "SanPhams",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiTao",
                table: "SanPhams",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SanPhamId",
                table: "DonHangs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayCapNhat",
                table: "DonHangs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiCapNhat",
                table: "DonHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiTao",
                table: "DonHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NhatKys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiThucHien = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HanhDong = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    TenBang = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    BangGhiId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    GiaTriTruoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GiaTriSau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKys", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NhatKys_TenBang_BangGhiId_NgayTao",
                table: "NhatKys",
                columns: new[] { "TenBang", "BangGhiId", "NgayTao" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NhatKys");

            migrationBuilder.DropColumn(
                name: "DaXoa",
                table: "SanPhams");

            migrationBuilder.DropColumn(
                name: "NgayCapNhat",
                table: "SanPhams");

            migrationBuilder.DropColumn(
                name: "NgayTao",
                table: "SanPhams");

            migrationBuilder.DropColumn(
                name: "NguoiCapNhat",
                table: "SanPhams");

            migrationBuilder.DropColumn(
                name: "NguoiTao",
                table: "SanPhams");

            migrationBuilder.DropColumn(
                name: "NgayCapNhat",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "NguoiCapNhat",
                table: "DonHangs");

            migrationBuilder.DropColumn(
                name: "NguoiTao",
                table: "DonHangs");

            migrationBuilder.AlterColumn<int>(
                name: "SanPhamId",
                table: "DonHangs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
