using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalManager.Api.Migrations.Sqlite
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
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CardRatio",
                table: "PersonalProfiles",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "Portrait");

            migrationBuilder.AddColumn<string>(
                name: "CardStyle",
                table: "PersonalProfiles",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "Visual");

            migrationBuilder.AddColumn<string>(
                name: "PortfolioMode",
                table: "PersonalProfiles",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "Designer");

            migrationBuilder.AddColumn<string>(
                name: "SkillDisplay",
                table: "PersonalProfiles",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "NameOnly");

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
