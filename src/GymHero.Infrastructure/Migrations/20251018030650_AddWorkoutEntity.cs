using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutExercises_WorkoutPlans_WorkoutPlanId",
                table: "WorkoutExercises");

            migrationBuilder.RenameColumn(
                name: "WorkoutPlanId",
                table: "WorkoutExercises",
                newName: "WorkoutId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkoutExercises_WorkoutPlanId",
                table: "WorkoutExercises",
                newName: "IX_WorkoutExercises_WorkoutId");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "WorkoutPlans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "WorkoutPlans",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "WorkoutExercises",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RestSeconds",
                table: "WorkoutExercises",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetRepsRange",
                table: "WorkoutExercises",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetRpe",
                table: "WorkoutExercises",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkoutPlanId1",
                table: "WorkoutExercises",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Workouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workouts_WorkoutPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "WorkoutPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // DATA MIGRATION: Create a default Workout for each existing WorkoutPlan
            // This ensures all existing WorkoutExercises get properly linked to a Workout
            migrationBuilder.Sql(@"
                INSERT INTO ""Workouts"" (""Id"", ""PlanId"", ""Name"", ""DayOfWeek"", ""Order"", ""CreatedAt"")
                SELECT
                    gen_random_uuid(),
                    wp.""Id"",
                    'Treino Completo',
                    NULL,
                    1,
                    NOW()
                FROM ""WorkoutPlans"" wp
                WHERE EXISTS (
                    SELECT 1 FROM ""WorkoutExercises"" we WHERE we.""WorkoutId"" = wp.""Id""
                );

                -- Update all WorkoutExercises to point to the new default Workout
                UPDATE ""WorkoutExercises"" we
                SET ""WorkoutId"" = w.""Id""
                FROM ""Workouts"" w
                WHERE w.""PlanId"" = we.""WorkoutId"";
            ");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutExercises_WorkoutPlanId1",
                table: "WorkoutExercises",
                column: "WorkoutPlanId1");

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_PlanId",
                table: "Workouts",
                column: "PlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutExercises_WorkoutPlans_WorkoutPlanId1",
                table: "WorkoutExercises",
                column: "WorkoutPlanId1",
                principalTable: "WorkoutPlans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutExercises_Workouts_WorkoutId",
                table: "WorkoutExercises",
                column: "WorkoutId",
                principalTable: "Workouts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutExercises_WorkoutPlans_WorkoutPlanId1",
                table: "WorkoutExercises");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutExercises_Workouts_WorkoutId",
                table: "WorkoutExercises");

            migrationBuilder.DropTable(
                name: "Workouts");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutExercises_WorkoutPlanId1",
                table: "WorkoutExercises");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "WorkoutPlans");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "WorkoutPlans");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "WorkoutExercises");

            migrationBuilder.DropColumn(
                name: "RestSeconds",
                table: "WorkoutExercises");

            migrationBuilder.DropColumn(
                name: "TargetRepsRange",
                table: "WorkoutExercises");

            migrationBuilder.DropColumn(
                name: "TargetRpe",
                table: "WorkoutExercises");

            migrationBuilder.DropColumn(
                name: "WorkoutPlanId1",
                table: "WorkoutExercises");

            migrationBuilder.RenameColumn(
                name: "WorkoutId",
                table: "WorkoutExercises",
                newName: "WorkoutPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkoutExercises_WorkoutId",
                table: "WorkoutExercises",
                newName: "IX_WorkoutExercises_WorkoutPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutExercises_WorkoutPlans_WorkoutPlanId",
                table: "WorkoutExercises",
                column: "WorkoutPlanId",
                principalTable: "WorkoutPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
