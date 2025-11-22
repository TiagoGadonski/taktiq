using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeTrainerSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add index for public profile trainers (most common query)
            migrationBuilder.CreateIndex(
                name: "IX_Users_IsPublicProfile_Role",
                table: "Users",
                columns: new[] { "IsPublicProfile", "Role" },
                filter: "IsPublicProfile = true AND Role = 'PersonalTrainer'");

            // Add index for specialization filtering
            migrationBuilder.CreateIndex(
                name: "IX_Users_Specialization",
                table: "Users",
                column: "Specialization",
                filter: "Specialization IS NOT NULL");

            // Add index for location filtering
            migrationBuilder.CreateIndex(
                name: "IX_Users_Location",
                table: "Users",
                column: "Location",
                filter: "Location IS NOT NULL");

            // Add composite index for common filter combinations
            migrationBuilder.CreateIndex(
                name: "IX_Users_PublicProfile_Location_Specialization",
                table: "Users",
                columns: new[] { "IsPublicProfile", "Location", "Specialization" },
                filter: "IsPublicProfile = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_IsPublicProfile_Role",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Specialization",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Location",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PublicProfile_Location_Specialization",
                table: "Users");
        }
    }
}
