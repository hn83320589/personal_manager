using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalManager.Api.Migrations.MySql
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
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "None")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateOnly>(
                name: "RecurrenceUntil",
                table: "CalendarEvents",
                type: "date",
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
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
