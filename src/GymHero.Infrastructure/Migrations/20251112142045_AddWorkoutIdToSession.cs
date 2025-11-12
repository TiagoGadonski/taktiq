using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutIdToSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WorkoutId",
                table: "WorkoutSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessions_WorkoutId",
                table: "WorkoutSessions",
                column: "WorkoutId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutSessions_Workouts_WorkoutId",
                table: "WorkoutSessions",
                column: "WorkoutId",
                principalTable: "Workouts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutSessions_Workouts_WorkoutId",
                table: "WorkoutSessions");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutSessions_WorkoutId",
                table: "WorkoutSessions");

            migrationBuilder.DropColumn(
                name: "WorkoutId",
                table: "WorkoutSessions");
        }
    }
}
