using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAssessmentProtocols : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssessmentProtocols",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ProtocolType = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Instructions = table.Column<string>(type: "text", nullable: true),
                    Equipment = table.Column<string>(type: "text", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    MeasurementFields = table.Column<string>(type: "text", nullable: false),
                    NormativeData = table.Column<string>(type: "text", nullable: true),
                    CalculationFormula = table.Column<string>(type: "text", nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentProtocols", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentProtocols_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssessmentResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProtocolId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentAssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    TestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Measurements = table.Column<string>(type: "text", nullable: false),
                    CalculatedScore = table.Column<double>(type: "double precision", nullable: true),
                    ResultUnit = table.Column<string>(type: "text", nullable: true),
                    Classification = table.Column<string>(type: "text", nullable: true),
                    TrainerNotes = table.Column<string>(type: "text", nullable: true),
                    Recommendations = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentResults_AssessmentProtocols_ProtocolId",
                        column: x => x.ProtocolId,
                        principalTable: "AssessmentProtocols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentResults_StudentAssessments_StudentAssessmentId",
                        column: x => x.StudentAssessmentId,
                        principalTable: "StudentAssessments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssessmentResults_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentResults_Users_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentProtocols_CreatedByUserId",
                table: "AssessmentProtocols",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResults_ProtocolId",
                table: "AssessmentResults",
                column: "ProtocolId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResults_StudentAssessmentId",
                table: "AssessmentResults",
                column: "StudentAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResults_StudentId",
                table: "AssessmentResults",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResults_TrainerId",
                table: "AssessmentResults",
                column: "TrainerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentResults");

            migrationBuilder.DropTable(
                name: "AssessmentProtocols");
        }
    }
}
