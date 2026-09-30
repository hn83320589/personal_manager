using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalManager.Api.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class ModelManagedIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_DueDate",
                table: "WorkTasks",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_Priority",
                table: "WorkTasks",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_Status",
                table: "WorkTasks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_UserId",
                table: "WorkTasks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkExperiences_IsCurrent",
                table: "WorkExperiences",
                column: "IsCurrent");

            migrationBuilder.CreateIndex(
                name: "IX_WorkExperiences_IsPublic_SortOrder",
                table: "WorkExperiences",
                columns: new[] { "IsPublic", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkExperiences_StartDate_EndDate",
                table: "WorkExperiences",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkExperiences_UserId",
                table: "WorkExperiences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_DueDate",
                table: "TodoItems",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_Priority",
                table: "TodoItems",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_Status",
                table: "TodoItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_UserId",
                table: "TodoItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_UserId",
                table: "TimeEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserId_Name",
                table: "Tags",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Skills_Category",
                table: "Skills",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_IsPublic_SortOrder",
                table: "Skills",
                columns: new[] { "IsPublic", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Skills_UserId",
                table: "Skills",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UserId",
                table: "Projects",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Portfolios_IsPublic_IsFeatured",
                table: "Portfolios",
                columns: new[] { "IsPublic", "IsFeatured" });

            migrationBuilder.CreateIndex(
                name: "IX_Portfolios_SortOrder",
                table: "Portfolios",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Portfolios_UserId",
                table: "Portfolios",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalProfiles_UserId",
                table: "PersonalProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Token",
                table: "PasswordResetTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuestBookEntries_CreatedAt",
                table: "GuestBookEntries",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_GuestBookEntries_Email",
                table: "GuestBookEntries",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_GuestBookEntries_TargetUserId_IsApproved",
                table: "GuestBookEntries",
                columns: new[] { "TargetUserId", "IsApproved" });

            migrationBuilder.CreateIndex(
                name: "IX_FileUploads_UserId",
                table: "FileUploads",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Educations_IsPublic_SortOrder",
                table: "Educations",
                columns: new[] { "IsPublic", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Educations_UserId",
                table: "Educations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMethods_IsPublic_SortOrder",
                table: "ContactMethods",
                columns: new[] { "IsPublic", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ContactMethods_Type",
                table: "ContactMethods",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMethods_UserId",
                table: "ContactMethods",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_IsPublic",
                table: "CalendarEvents",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_StartTime_EndTime",
                table: "CalendarEvents",
                columns: new[] { "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_UserId",
                table: "CalendarEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_Category",
                table: "BlogPosts",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_PublishedAt",
                table: "BlogPosts",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_Status",
                table: "BlogPosts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_UserId",
                table: "BlogPosts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_ViewCount",
                table: "BlogPosts",
                column: "ViewCount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkTasks_DueDate",
                table: "WorkTasks");

            migrationBuilder.DropIndex(
                name: "IX_WorkTasks_Priority",
                table: "WorkTasks");

            migrationBuilder.DropIndex(
                name: "IX_WorkTasks_Status",
                table: "WorkTasks");

            migrationBuilder.DropIndex(
                name: "IX_WorkTasks_UserId",
                table: "WorkTasks");

            migrationBuilder.DropIndex(
                name: "IX_WorkExperiences_IsCurrent",
                table: "WorkExperiences");

            migrationBuilder.DropIndex(
                name: "IX_WorkExperiences_IsPublic_SortOrder",
                table: "WorkExperiences");

            migrationBuilder.DropIndex(
                name: "IX_WorkExperiences_StartDate_EndDate",
                table: "WorkExperiences");

            migrationBuilder.DropIndex(
                name: "IX_WorkExperiences_UserId",
                table: "WorkExperiences");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsActive",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_DueDate",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_Priority",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_Status",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_UserId",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_UserId",
                table: "TimeEntries");

            migrationBuilder.DropIndex(
                name: "IX_Tags_UserId_Name",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Skills_Category",
                table: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_Skills_IsPublic_SortOrder",
                table: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_Skills_UserId",
                table: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_Projects_UserId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Portfolios_IsPublic_IsFeatured",
                table: "Portfolios");

            migrationBuilder.DropIndex(
                name: "IX_Portfolios_SortOrder",
                table: "Portfolios");

            migrationBuilder.DropIndex(
                name: "IX_Portfolios_UserId",
                table: "Portfolios");

            migrationBuilder.DropIndex(
                name: "IX_PersonalProfiles_UserId",
                table: "PersonalProfiles");

            migrationBuilder.DropIndex(
                name: "IX_PasswordResetTokens_Token",
                table: "PasswordResetTokens");

            migrationBuilder.DropIndex(
                name: "IX_GuestBookEntries_CreatedAt",
                table: "GuestBookEntries");

            migrationBuilder.DropIndex(
                name: "IX_GuestBookEntries_Email",
                table: "GuestBookEntries");

            migrationBuilder.DropIndex(
                name: "IX_GuestBookEntries_TargetUserId_IsApproved",
                table: "GuestBookEntries");

            migrationBuilder.DropIndex(
                name: "IX_FileUploads_UserId",
                table: "FileUploads");

            migrationBuilder.DropIndex(
                name: "IX_Educations_IsPublic_SortOrder",
                table: "Educations");

            migrationBuilder.DropIndex(
                name: "IX_Educations_UserId",
                table: "Educations");

            migrationBuilder.DropIndex(
                name: "IX_ContactMethods_IsPublic_SortOrder",
                table: "ContactMethods");

            migrationBuilder.DropIndex(
                name: "IX_ContactMethods_Type",
                table: "ContactMethods");

            migrationBuilder.DropIndex(
                name: "IX_ContactMethods_UserId",
                table: "ContactMethods");

            migrationBuilder.DropIndex(
                name: "IX_CalendarEvents_IsPublic",
                table: "CalendarEvents");

            migrationBuilder.DropIndex(
                name: "IX_CalendarEvents_StartTime_EndTime",
                table: "CalendarEvents");

            migrationBuilder.DropIndex(
                name: "IX_CalendarEvents_UserId",
                table: "CalendarEvents");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_Category",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_PublishedAt",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_Status",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_UserId",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_ViewCount",
                table: "BlogPosts");
        }
    }
}
