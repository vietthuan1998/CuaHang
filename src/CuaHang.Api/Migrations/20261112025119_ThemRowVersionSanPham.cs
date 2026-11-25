using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuaHang.Api.Migrations
{
    /// <inheritdoc />
    public partial class ThemRowVersionSanPham : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SanPhams",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SanPhams");
        }
    }
}
