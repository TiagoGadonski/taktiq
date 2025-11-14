using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeWorkoutPlanIdOptionalInWorkoutSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutSessions_WorkoutPlans_WorkoutPlanId",
                table: "WorkoutSessions");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkoutPlanId",
                table: "WorkoutSessions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutSessions_WorkoutPlans_WorkoutPlanId",
                table: "WorkoutSessions",
                column: "WorkoutPlanId",
                principalTable: "WorkoutPlans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutSessions_WorkoutPlans_WorkoutPlanId",
                table: "WorkoutSessions");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkoutPlanId",
                table: "WorkoutSessions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutSessions_WorkoutPlans_WorkoutPlanId",
                table: "WorkoutSessions",
                column: "WorkoutPlanId",
                principalTable: "WorkoutPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
