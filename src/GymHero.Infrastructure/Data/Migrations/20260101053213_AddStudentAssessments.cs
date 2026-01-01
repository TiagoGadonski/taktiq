using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentAssessments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentType = table.Column<string>(type: "text", nullable: false),
                    AssessmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ForwardHead = table.Column<string>(type: "text", nullable: true),
                    RoundedShoulders = table.Column<string>(type: "text", nullable: true),
                    AnteriorPelvicTilt = table.Column<string>(type: "text", nullable: true),
                    PosteriorPelvicTilt = table.Column<string>(type: "text", nullable: true),
                    KneeValgus = table.Column<string>(type: "text", nullable: true),
                    KneeVarus = table.Column<string>(type: "text", nullable: true),
                    FlatFeet = table.Column<string>(type: "text", nullable: true),
                    Scoliosis = table.Column<string>(type: "text", nullable: true),
                    BodyFatPercentage = table.Column<double>(type: "double precision", nullable: true),
                    MuscleMass = table.Column<double>(type: "double precision", nullable: true),
                    FlexibilityScore = table.Column<double>(type: "double precision", nullable: true),
                    StrengthScore = table.Column<double>(type: "double precision", nullable: true),
                    CardioScore = table.Column<double>(type: "double precision", nullable: true),
                    CustomFields = table.Column<string>(type: "text", nullable: true),
                    TrainerNotes = table.Column<string>(type: "text", nullable: true),
                    Recommendations = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentAssessments_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentAssessments_Users_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentAssessments_StudentId",
                table: "StudentAssessments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAssessments_TrainerId",
                table: "StudentAssessments",
                column: "TrainerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentAssessments");
        }
    }
}
