using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalManager.Api.Migrations.MySql
{
    /// <inheritdoc />
    public partial class BlockPortfolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortfolioAttachments");

            migrationBuilder.DropIndex(
                name: "IX_Portfolios_UserId",
                table: "Portfolios");

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Portfolios",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Fields",
                table: "Portfolios",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Links",
                table: "Portfolios",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Blocks",
                table: "Portfolios",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Portfolios",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CoverFocus",
                table: "Portfolios",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "50% 50%")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Covers",
                table: "Portfolios",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Period",
                table: "Portfolios",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Portfolios",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Portfolios",
                type: "varchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Portfolios",
                type: "int",
                nullable: true);

            // 舊作品：內容清單給空的 JSON 陣列、以 Id 產生不重複的網址、舊的描述移到摘要
            migrationBuilder.Sql(
                "UPDATE Portfolios SET Covers = '[]', Fields = '[]', Links = '[]', Blocks = '[]', " +
                "Slug = CONCAT('work-', Id), Summary = LEFT(Description, 500)");

            migrationBuilder.DropColumn(
                name: "Technologies",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "RepositoryUrl",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "ProjectUrl",
                table: "Portfolios");

            migrationBuilder.CreateTable(
                name: "PortfolioTags",
                columns: table => new
                {
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    TagsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioTags", x => new { x.PortfolioId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_PortfolioTags_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PortfolioTags_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Portfolios_UserId_Slug",
                table: "Portfolios",
                columns: new[] { "UserId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioTags_TagsId",
                table: "PortfolioTags",
                column: "TagsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortfolioTags");

            migrationBuilder.DropIndex(
                name: "IX_Portfolios_UserId_Slug",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "Blocks",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "CoverFocus",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "Covers",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "Links",
                table: "Portfolios");

            migrationBuilder.DropColumn(
                name: "Fields",
                table: "Portfolios");

            migrationBuilder.AddColumn<string>(
                name: "RepositoryUrl",
                table: "Portfolios",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Technologies",
                table: "Portfolios",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Portfolios",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Portfolios",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProjectUrl",
                table: "Portfolios",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PortfolioAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FileName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileUploadId = table.Column<int>(type: "int", nullable: true),
                    FileUrl = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioAttachments", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Portfolios_UserId",
                table: "Portfolios",
                column: "UserId");
        }
    }
}
