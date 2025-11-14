using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUserActivityLogsForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_UserActivityLogs_Users_UserId",
                table: "UserActivityLogs");

            // Recreate the foreign key with ON DELETE SET NULL behavior
            // This allows user deletion while preserving activity logs for audit purposes
            migrationBuilder.AddForeignKey(
                name: "FK_UserActivityLogs_Users_UserId",
                table: "UserActivityLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the modified foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_UserActivityLogs_Users_UserId",
                table: "UserActivityLogs");

            // Recreate the original foreign key (without explicit ON DELETE behavior)
            migrationBuilder.AddForeignKey(
                name: "FK_UserActivityLogs_Users_UserId",
                table: "UserActivityLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
