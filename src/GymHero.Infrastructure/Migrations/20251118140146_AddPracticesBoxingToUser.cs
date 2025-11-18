using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPracticesBoxingToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserActivityLogs_Users_UserId",
                table: "UserActivityLogs");

            migrationBuilder.AddColumn<bool>(
                name: "PracticesBoxing",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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
            migrationBuilder.DropForeignKey(
                name: "FK_UserActivityLogs_Users_UserId",
                table: "UserActivityLogs");

            migrationBuilder.DropColumn(
                name: "PracticesBoxing",
                table: "Users");

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivityLogs_Users_UserId",
                table: "UserActivityLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
