using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeRepsAndLoadOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutExercises_WorkoutPlans_WorkoutPlanId1",
                table: "WorkoutExercises");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutExercises_WorkoutPlanId1",
                table: "WorkoutExercises");

            migrationBuilder.DropColumn(
                name: "WorkoutPlanId1",
                table: "WorkoutExercises");

            migrationBuilder.AlterColumn<int>(
                name: "Reps",
                table: "WorkoutSets",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<double>(
                name: "Load",
                table: "WorkoutSets",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Reps",
                table: "WorkoutSets",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Load",
                table: "WorkoutSets",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkoutPlanId1",
                table: "WorkoutExercises",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutExercises_WorkoutPlanId1",
                table: "WorkoutExercises",
                column: "WorkoutPlanId1");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutExercises_WorkoutPlans_WorkoutPlanId1",
                table: "WorkoutExercises",
                column: "WorkoutPlanId1",
                principalTable: "WorkoutPlans",
                principalColumn: "Id");
        }
    }
}
