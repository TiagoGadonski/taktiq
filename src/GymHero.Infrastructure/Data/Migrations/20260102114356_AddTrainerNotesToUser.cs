using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainerNotesToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkoutSessionFeedbacks_SessionId",
                table: "WorkoutSessionFeedbacks");

            migrationBuilder.AddColumn<string>(
                name: "TrainerNotes",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessionFeedbacks_SessionId",
                table: "WorkoutSessionFeedbacks",
                column: "SessionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkoutSessionFeedbacks_SessionId",
                table: "WorkoutSessionFeedbacks");

            migrationBuilder.DropColumn(
                name: "TrainerNotes",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessionFeedbacks_SessionId",
                table: "WorkoutSessionFeedbacks",
                column: "SessionId");
        }
    }
}
