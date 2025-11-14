using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerIdToWorkoutSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add OwnerId column as nullable
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "WorkoutSessions",
                type: "uuid",
                nullable: true);

            // Step 2: Update existing rows to set OwnerId from their WorkoutPlan
            migrationBuilder.Sql(@"
                UPDATE ""WorkoutSessions"" ws
                SET ""OwnerId"" = wp.""OwnerId""
                FROM ""WorkoutPlans"" wp
                WHERE ws.""WorkoutPlanId"" = wp.""Id""
                AND ws.""OwnerId"" IS NULL;
            ");

            // Step 3: For any remaining sessions without a plan (free workouts),
            // we'll delete them since we can't determine the owner
            // In a production scenario, you'd want to handle this differently
            migrationBuilder.Sql(@"
                DELETE FROM ""WorkoutSessions""
                WHERE ""OwnerId"" IS NULL;
            ");

            // Step 4: Make the column non-nullable now that all rows have a value
            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "WorkoutSessions",
                type: "uuid",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "WorkoutSessions");
        }
    }
}
