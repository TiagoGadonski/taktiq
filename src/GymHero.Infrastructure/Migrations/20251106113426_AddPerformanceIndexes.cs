using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessions_OwnerId",
                table: "WorkoutSessions",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessions_OwnerId_CompletedAt",
                table: "WorkoutSessions",
                columns: new[] { "OwnerId", "CompletedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkoutSessions_OwnerId",
                table: "WorkoutSessions");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutSessions_OwnerId_CompletedAt",
                table: "WorkoutSessions");
        }
    }
}
