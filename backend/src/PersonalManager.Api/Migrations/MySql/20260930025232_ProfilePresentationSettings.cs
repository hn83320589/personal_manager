using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalManager.Api.Migrations.MySql
{
    /// <inheritdoc />
    public partial class ProfilePresentationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PersonalProfiles_UserId",
                table: "PersonalProfiles");

            migrationBuilder.AddColumn<string>(
                name: "AvailabilityStatus",
                table: "PersonalProfiles",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CardRatio",
                table: "PersonalProfiles",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CardStyle",
                table: "PersonalProfiles",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PortfolioMode",
                table: "PersonalProfiles",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SkillDisplay",
                table: "PersonalProfiles",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalProfiles_UserId",
                table: "PersonalProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PersonalProfiles_UserId",
                table: "PersonalProfiles");

            migrationBuilder.DropColumn(
                name: "AvailabilityStatus",
                table: "PersonalProfiles");

            migrationBuilder.DropColumn(
                name: "CardRatio",
                table: "PersonalProfiles");

            migrationBuilder.DropColumn(
                name: "CardStyle",
                table: "PersonalProfiles");

            migrationBuilder.DropColumn(
                name: "PortfolioMode",
                table: "PersonalProfiles");

            migrationBuilder.DropColumn(
                name: "SkillDisplay",
                table: "PersonalProfiles");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalProfiles_UserId",
                table: "PersonalProfiles",
                column: "UserId");
        }
    }
}
