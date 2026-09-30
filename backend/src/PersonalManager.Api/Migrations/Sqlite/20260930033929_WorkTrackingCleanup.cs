using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalManager.Api.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class WorkTrackingCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_UserId",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "ActualHours",
                table: "WorkTasks");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "WorkTasks");

            migrationBuilder.DropColumn(
                name: "Project",
                table: "TimeEntries");

            migrationBuilder.RenameColumn(
                name: "Task",
                table: "TimeEntries",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "TimeEntries",
                newName: "DurationMinutes");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_UserId_Date",
                table: "TimeEntries",
                columns: new[] { "UserId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_WorkTaskId",
                table: "TimeEntries",
                column: "WorkTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_WorkTasks_WorkTaskId",
                table: "TimeEntries",
                column: "WorkTaskId",
                principalTable: "WorkTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_WorkTasks_WorkTaskId",
                table: "TimeEntries");

            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_UserId_Date",
                table: "TimeEntries");

            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_WorkTaskId",
                table: "TimeEntries");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "TimeEntries",
                newName: "Task");

            migrationBuilder.RenameColumn(
                name: "DurationMinutes",
                table: "TimeEntries",
                newName: "Duration");

            migrationBuilder.AddColumn<double>(
                name: "ActualHours",
                table: "WorkTasks",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "WorkTasks",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Project",
                table: "TimeEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_UserId",
                table: "TimeEntries",
                column: "UserId");
        }
    }
}
