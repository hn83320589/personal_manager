using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalManager.Api.Migrations.MySql
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

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "StartTime",
                table: "TimeEntries",
                type: "time(6)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10,
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "EndTime",
                table: "TimeEntries",
                type: "time(6)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10,
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Date",
                table: "TimeEntries",
                type: "date",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

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
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "WorkTasks",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "StartTime",
                table: "TimeEntries",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time(6)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "EndTime",
                table: "TimeEntries",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time(6)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Date",
                table: "TimeEntries",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Project",
                table: "TimeEntries",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_UserId",
                table: "TimeEntries",
                column: "UserId");
        }
    }
}
