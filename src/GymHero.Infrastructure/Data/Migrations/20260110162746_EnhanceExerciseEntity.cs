using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceExerciseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use SQL direto para fazer a conversão com USING
            migrationBuilder.Sql(@"
                ALTER TABLE ""Exercises""
                ALTER COLUMN ""MuscleGroup"" TYPE integer
                USING (CASE ""MuscleGroup""
                    WHEN 'Chest' THEN 0
                    WHEN 'Back' THEN 1
                    WHEN 'Shoulders' THEN 2
                    WHEN 'Biceps' THEN 3
                    WHEN 'Triceps' THEN 4
                    WHEN 'Forearms' THEN 5
                    WHEN 'Core' THEN 6
                    WHEN 'Abs' THEN 6
                    WHEN 'Quadriceps' THEN 7
                    WHEN 'Hamstrings' THEN 8
                    WHEN 'Glutes' THEN 9
                    WHEN 'Calves' THEN 10
                    WHEN 'FullBody' THEN 11
                    WHEN 'Full Body' THEN 11
                    ELSE 0
                END)::integer;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Exercises""
                ALTER COLUMN ""Equipment"" TYPE integer
                USING (CASE
                    WHEN ""Equipment"" IS NULL THEN 0
                    WHEN ""Equipment"" = 'Barbell' THEN 0
                    WHEN ""Equipment"" = 'Dumbbell' THEN 1
                    WHEN ""Equipment"" = 'Machine' THEN 2
                    WHEN ""Equipment"" = 'Cable' THEN 3
                    WHEN ""Equipment"" = 'Bodyweight' THEN 4
                    WHEN ""Equipment"" = 'Kettlebell' THEN 5
                    WHEN ""Equipment"" = 'Resistance Band' THEN 6
                    WHEN ""Equipment"" = 'Pull-up Bar' THEN 7
                    WHEN ""Equipment"" = 'Medicine Ball' THEN 8
                    WHEN ""Equipment"" = 'TRX' THEN 9
                    WHEN ""Equipment"" = 'None' THEN 4
                    ELSE 0
                END)::integer;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Exercises""
                ALTER COLUMN ""Category"" TYPE integer
                USING (CASE
                    WHEN ""Category"" IS NULL THEN 0
                    WHEN ""Category"" = 'Strength' THEN 0
                    WHEN ""Category"" = 'Hypertrophy' THEN 1
                    WHEN ""Category"" = 'Endurance' THEN 2
                    WHEN ""Category"" = 'Power' THEN 3
                    WHEN ""Category"" = 'Cardio' THEN 4
                    WHEN ""Category"" = 'Flexibility' THEN 5
                    WHEN ""Category"" = 'Balance' THEN 6
                    WHEN ""Category"" = 'Mobility' THEN 7
                    WHEN ""Category"" = 'Sports' THEN 8
                    WHEN ""Category"" = 'PostureCorrection' THEN 9
                    WHEN ""Category"" = 'Calisthenics' THEN 10
                    WHEN ""Category"" = 'HIIT' THEN 11
                    WHEN ""Category"" = 'Stretching' THEN 12
                    ELSE 0
                END)::integer;
            ");

            migrationBuilder.AddColumn<string>(
                name: "CommonMistakes",
                table: "Exercises",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Exercises",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Difficulty",
                table: "Exercises",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "Exercises",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Exercises",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryMuscles",
                table: "Exercises",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "Exercises",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tips",
                table: "Exercises",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Exercises",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Category",
                table: "Exercises",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_CreatedByUserId",
                table: "Exercises",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Difficulty",
                table: "Exercises",
                column: "Difficulty");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Equipment",
                table: "Exercises",
                column: "Equipment");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_IsPublic",
                table: "Exercises",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_LocationPublic",
                table: "Exercises",
                columns: new[] { "WorkoutLocation", "IsPublic" });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_MuscleGroup",
                table: "Exercises",
                column: "MuscleGroup");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Users_CreatedByUserId",
                table: "Exercises",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Users_CreatedByUserId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_Category",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_CreatedByUserId",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_Difficulty",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_Equipment",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_IsPublic",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_LocationPublic",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_MuscleGroup",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "CommonMistakes",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "SecondaryMuscles",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "Tips",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Exercises");

            migrationBuilder.AlterColumn<string>(
                name: "MuscleGroup",
                table: "Exercises",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Equipment",
                table: "Exercises",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Exercises",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
