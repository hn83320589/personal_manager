using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalManager.Api.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class CalendarRecurrence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecurrenceRule",
                table: "CalendarEvents");

            migrationBuilder.AddColumn<string>(
                name: "Recurrence",
                table: "CalendarEvents",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "None");

            migrationBuilder.AddColumn<DateOnly>(
                name: "RecurrenceUntil",
                table: "CalendarEvents",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Recurrence",
                table: "CalendarEvents");

            migrationBuilder.DropColumn(
                name: "RecurrenceUntil",
                table: "CalendarEvents");

            migrationBuilder.AddColumn<string>(
                name: "RecurrenceRule",
                table: "CalendarEvents",
                type: "TEXT",
                nullable: false,
                defaultValue: "None");
        }
    }
}
