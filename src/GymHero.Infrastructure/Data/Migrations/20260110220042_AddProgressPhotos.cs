using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProgressPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProgressPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UploaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhotoType = table.Column<int>(type: "integer", nullable: false),
                    BodyAngle = table.Column<int>(type: "integer", nullable: false),
                    PhotoDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WeightKg = table.Column<double>(type: "double precision", nullable: true),
                    BodyFatPercentage = table.Column<double>(type: "double precision", nullable: true),
                    MuscleMassKg = table.Column<double>(type: "double precision", nullable: true),
                    ChestCm = table.Column<double>(type: "double precision", nullable: true),
                    WaistCm = table.Column<double>(type: "double precision", nullable: true),
                    HipsCm = table.Column<double>(type: "double precision", nullable: true),
                    LeftArmCm = table.Column<double>(type: "double precision", nullable: true),
                    RightArmCm = table.Column<double>(type: "double precision", nullable: true),
                    LeftThighCm = table.Column<double>(type: "double precision", nullable: true),
                    RightThighCm = table.Column<double>(type: "double precision", nullable: true),
                    LeftCalfCm = table.Column<double>(type: "double precision", nullable: true),
                    RightCalfCm = table.Column<double>(type: "double precision", nullable: true),
                    TrainerNotes = table.Column<string>(type: "text", nullable: true),
                    StudentNotes = table.Column<string>(type: "text", nullable: true),
                    Caption = table.Column<string>(type: "text", nullable: true),
                    IsVisibleToStudent = table.Column<bool>(type: "boolean", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    StudentAssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressPhotos_Medias_MediaId",
                        column: x => x.MediaId,
                        principalTable: "Medias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgressPhotos_StudentAssessments_StudentAssessmentId",
                        column: x => x.StudentAssessmentId,
                        principalTable: "StudentAssessments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProgressPhotos_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgressPhotos_Users_UploaderId",
                        column: x => x.UploaderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressPhotos_MediaId",
                table: "ProgressPhotos",
                column: "MediaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressPhotos_StudentAssessmentId",
                table: "ProgressPhotos",
                column: "StudentAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressPhotos_StudentId",
                table: "ProgressPhotos",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressPhotos_UploaderId",
                table: "ProgressPhotos",
                column: "UploaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgressPhotos");
        }
    }
}
