using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutPlanComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkoutPlanComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutPlanComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutPlanComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkoutPlanComments_WorkoutPlanComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "WorkoutPlanComments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkoutPlanComments_WorkoutPlans_WorkoutPlanId",
                        column: x => x.WorkoutPlanId,
                        principalTable: "WorkoutPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlanComments_ParentCommentId",
                table: "WorkoutPlanComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlanComments_UserId",
                table: "WorkoutPlanComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlanComments_WorkoutPlanId",
                table: "WorkoutPlanComments",
                column: "WorkoutPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkoutPlanComments");
        }
    }
}
