using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuaHang.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChuanHoaNhatKyVaChiMucSanPham : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_SanPhams_DanhMucId'
                      AND object_id = OBJECT_ID(N'dbo.SanPhams'))
                BEGIN
                    DROP INDEX [IX_SanPhams_DanhMucId] ON [dbo].[SanPhams];
                END;
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.NhatKys', N'U') IS NOT NULL
                   AND OBJECT_ID(N'dbo.NhatKyThaoTacs', N'U') IS NULL
                BEGIN
                    EXEC sp_rename N'dbo.NhatKys', N'NhatKyThaoTacs';
                END;

                IF OBJECT_ID(N'dbo.NhatKyThaoTacs', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[NhatKyThaoTacs] (
                        [Id] bigint IDENTITY(1, 1) NOT NULL,
                        [NguoiThucHien] nvarchar(50) NOT NULL,
                        [HanhDong] varchar(50) NOT NULL,
                        [TenBang] varchar(50) NOT NULL,
                        [BanGhiId] varchar(50) NOT NULL,
                        [GiaTriTruoc] nvarchar(max) NOT NULL,
                        [GiaTriSau] nvarchar(max) NOT NULL,
                        [NgayTao] datetime2 NOT NULL,
                        CONSTRAINT [PK_NhatKyThaoTacs_Repair] PRIMARY KEY ([Id])
                    );
                END
                ELSE
                BEGIN
                    IF COL_LENGTH(N'dbo.NhatKyThaoTacs', N'BanGhiId') IS NULL
                       AND COL_LENGTH(N'dbo.NhatKyThaoTacs', N'BangGhiId') IS NOT NULL
                    BEGIN
                        EXEC sp_rename
                            N'dbo.NhatKyThaoTacs.BangGhiId',
                            N'BanGhiId',
                            N'COLUMN';
                    END;

                    IF EXISTS (
                        SELECT 1
                        FROM sys.columns
                        WHERE object_id = OBJECT_ID(N'dbo.NhatKyThaoTacs')
                          AND name = N'Id'
                          AND (system_type_id <> TYPE_ID(N'bigint') OR is_identity = 0))
                    BEGIN
                        CREATE TABLE [dbo].[NhatKyThaoTacs_New] (
                            [Id] bigint IDENTITY(1, 1) NOT NULL,
                            [NguoiThucHien] nvarchar(50) NOT NULL,
                            [HanhDong] varchar(50) NOT NULL,
                            [TenBang] varchar(50) NOT NULL,
                            [BanGhiId] varchar(50) NOT NULL,
                            [GiaTriTruoc] nvarchar(max) NOT NULL,
                            [GiaTriSau] nvarchar(max) NOT NULL,
                            [NgayTao] datetime2 NOT NULL,
                            CONSTRAINT [PK_NhatKyThaoTacs_New] PRIMARY KEY ([Id])
                        );

                        SET IDENTITY_INSERT [dbo].[NhatKyThaoTacs_New] ON;
                        INSERT INTO [dbo].[NhatKyThaoTacs_New]
                            ([Id], [NguoiThucHien], [HanhDong], [TenBang], [BanGhiId],
                             [GiaTriTruoc], [GiaTriSau], [NgayTao])
                        SELECT [Id], [NguoiThucHien], [HanhDong], [TenBang], [BanGhiId],
                               [GiaTriTruoc], [GiaTriSau], [NgayTao]
                        FROM [dbo].[NhatKyThaoTacs];
                        SET IDENTITY_INSERT [dbo].[NhatKyThaoTacs_New] OFF;

                        DROP TABLE [dbo].[NhatKyThaoTacs];
                        EXEC sp_rename N'dbo.NhatKyThaoTacs_New', N'NhatKyThaoTacs';
                    END;
                END;

                IF OBJECT_ID(N'dbo.NhatKyThaoTacs', N'U') IS NOT NULL
                   AND NOT EXISTS (
                       SELECT 1
                       FROM sys.key_constraints
                       WHERE parent_object_id = OBJECT_ID(N'dbo.NhatKyThaoTacs')
                         AND type = N'PK')
                BEGIN
                    ALTER TABLE [dbo].[NhatKyThaoTacs]
                        ADD CONSTRAINT [PK_NhatKyThaoTacs_Repair] PRIMARY KEY ([Id]);
                END;

                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_NhatKys_TenBang_BangGhiId_NgayTao'
                      AND object_id = OBJECT_ID(N'dbo.NhatKyThaoTacs'))
                BEGIN
                    DROP INDEX [IX_NhatKys_TenBang_BangGhiId_NgayTao]
                        ON [dbo].[NhatKyThaoTacs];
                END;

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_NhatKyThaoTacs_TenBang_BanGhiId_NgayTao'
                      AND object_id = OBJECT_ID(N'dbo.NhatKyThaoTacs'))
                BEGIN
                    CREATE INDEX [IX_NhatKyThaoTacs_TenBang_BanGhiId_NgayTao]
                        ON [dbo].[NhatKyThaoTacs] ([TenBang], [BanGhiId], [NgayTao]);
                END;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "NguoiTao",
                table: "SanPhams",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NguoiCapNhat",
                table: "SanPhams",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NguoiTao",
                table: "DonHangs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NguoiCapNhat",
                table: "DonHangs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_SanPhams_DanhMucId_Ten'
                      AND object_id = OBJECT_ID(N'dbo.SanPhams'))
                BEGIN
                    CREATE UNIQUE INDEX [IX_SanPhams_DanhMucId_Ten]
                        ON [dbo].[SanPhams] ([DanhMucId], [Ten])
                        WHERE [DaXoa] = 0;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_SanPhams_DanhMucId_Ten'
                      AND object_id = OBJECT_ID(N'dbo.SanPhams'))
                BEGIN
                    DROP INDEX [IX_SanPhams_DanhMucId_Ten] ON [dbo].[SanPhams];
                END;
                """);

            migrationBuilder.DropPrimaryKey(
                name: "PK_NhatKyThaoTacs",
                table: "NhatKyThaoTacs");

            migrationBuilder.RenameTable(
                name: "NhatKyThaoTacs",
                newName: "NhatKys");

            migrationBuilder.RenameColumn(
                name: "BanGhiId",
                table: "NhatKys",
                newName: "BangGhiId");

            migrationBuilder.RenameIndex(
                name: "IX_NhatKyThaoTacs_TenBang_BanGhiId_NgayTao",
                table: "NhatKys",
                newName: "IX_NhatKys_TenBang_BangGhiId_NgayTao");

            migrationBuilder.AlterColumn<string>(
                name: "NguoiTao",
                table: "SanPhams",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NguoiCapNhat",
                table: "SanPhams",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NguoiTao",
                table: "DonHangs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NguoiCapNhat",
                table: "DonHangs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.Sql("""
                CREATE TABLE [dbo].[NhatKys_New] (
                    [Id] int IDENTITY(1, 1) NOT NULL,
                    [NguoiThucHien] nvarchar(50) NOT NULL,
                    [HanhDong] varchar(50) NOT NULL,
                    [TenBang] varchar(50) NOT NULL,
                    [BangGhiId] varchar(50) NOT NULL,
                    [GiaTriTruoc] nvarchar(max) NOT NULL,
                    [GiaTriSau] nvarchar(max) NOT NULL,
                    [NgayTao] datetime2 NOT NULL,
                    CONSTRAINT [PK_NhatKys] PRIMARY KEY ([Id])
                );

                SET IDENTITY_INSERT [dbo].[NhatKys_New] ON;
                INSERT INTO [dbo].[NhatKys_New]
                    ([Id], [NguoiThucHien], [HanhDong], [TenBang], [BangGhiId],
                     [GiaTriTruoc], [GiaTriSau], [NgayTao])
                SELECT [Id], [NguoiThucHien], [HanhDong], [TenBang], [BangGhiId],
                       [GiaTriTruoc], [GiaTriSau], [NgayTao]
                FROM [dbo].[NhatKys];
                SET IDENTITY_INSERT [dbo].[NhatKys_New] OFF;

                DROP TABLE [dbo].[NhatKys];
                EXEC sp_rename N'dbo.NhatKys_New', N'NhatKys';

                CREATE INDEX [IX_NhatKys_TenBang_BangGhiId_NgayTao]
                    ON [dbo].[NhatKys] ([TenBang], [BangGhiId], [NgayTao]);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_SanPhams_DanhMucId'
                      AND object_id = OBJECT_ID(N'dbo.SanPhams'))
                BEGIN
                    CREATE INDEX [IX_SanPhams_DanhMucId]
                        ON [dbo].[SanPhams] ([DanhMucId]);
                END;
                """);
        }
    }
}
