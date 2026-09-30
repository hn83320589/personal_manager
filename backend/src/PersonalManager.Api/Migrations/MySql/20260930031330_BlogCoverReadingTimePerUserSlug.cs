using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalManager.Api.Migrations.MySql
{
    /// <inheritdoc />
    public partial class BlogCoverReadingTimePerUserSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogPostTags_Tags_TagEntitiesId",
                table: "BlogPostTags");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_PublishedAt",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_Slug",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_Status",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_UserId",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "BlogPosts");

            migrationBuilder.RenameColumn(
                name: "TagEntitiesId",
                table: "BlogPostTags",
                newName: "TagsId");

            migrationBuilder.RenameIndex(
                name: "IX_BlogPostTags_TagEntitiesId",
                table: "BlogPostTags",
                newName: "IX_BlogPostTags_TagsId");

            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                table: "BlogPosts",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ReadingMinutes",
                table: "BlogPosts",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_UserId_Slug",
                table: "BlogPosts",
                columns: new[] { "UserId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_UserId_Status_PublishedAt",
                table: "BlogPosts",
                columns: new[] { "UserId", "Status", "PublishedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPostTags_Tags_TagsId",
                table: "BlogPostTags",
                column: "TagsId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogPostTags_Tags_TagsId",
                table: "BlogPostTags");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_UserId_Slug",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_UserId_Status_PublishedAt",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "ReadingMinutes",
                table: "BlogPosts");

            migrationBuilder.RenameColumn(
                name: "TagsId",
                table: "BlogPostTags",
                newName: "TagEntitiesId");

            migrationBuilder.RenameIndex(
                name: "IX_BlogPostTags_TagsId",
                table: "BlogPostTags",
                newName: "IX_BlogPostTags_TagEntitiesId");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "BlogPosts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "BlogPosts",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_PublishedAt",
                table: "BlogPosts",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_Slug",
                table: "BlogPosts",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_Status",
                table: "BlogPosts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_UserId",
                table: "BlogPosts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPostTags_Tags_TagEntitiesId",
                table: "BlogPostTags",
                column: "TagEntitiesId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
