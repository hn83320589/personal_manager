using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalManager.Api.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class FileKindAndDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "FileUploads",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "FileUploads",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "Archive");

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "FileUploads",
                type: "INTEGER",
                nullable: true);

            // 依舊的 FileType 轉換成 Kind，再刪除舊欄位
            migrationBuilder.Sql(
                "UPDATE FileUploads SET Kind = CASE FileType " +
                "WHEN 'image' THEN 'Image' WHEN 'pdf' THEN 'Pdf' WHEN 'document' THEN 'Word' " +
                "WHEN 'presentation' THEN 'PowerPoint' ELSE 'Archive' END");

            migrationBuilder.DropColumn(
                name: "FileType",
                table: "FileUploads");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "FileUploads");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "FileUploads");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "FileUploads");

            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "FileUploads",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
