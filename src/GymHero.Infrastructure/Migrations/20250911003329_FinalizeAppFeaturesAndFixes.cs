using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeAppFeaturesAndFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_Users_OwnerId",
                table: "Challenges");

            migrationBuilder.DropIndex(
                name: "IX_ChallengeProgresses_ChallengeId",
                table: "ChallengeProgresses");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Challenges",
                newName: "CreatorId");

            migrationBuilder.RenameIndex(
                name: "IX_Challenges_OwnerId",
                table: "Challenges",
                newName: "IX_Challenges_CreatorId");

            migrationBuilder.AddColumn<Guid>(
                name: "PersonalTrainerId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ParticipantId",
                table: "ChallengeProgresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "BadgeDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    TriggerType = table.Column<string>(type: "text", nullable: false),
                    ThresholdValue = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BadgeDefinitions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_PersonalTrainerId",
                table: "Users",
                column: "PersonalTrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeProgresses_ChallengeId",
                table: "ChallengeProgresses",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeProgresses_ParticipantId",
                table: "ChallengeProgresses",
                column: "ParticipantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChallengeProgresses_Users_ParticipantId",
                table: "ChallengeProgresses",
                column: "ParticipantId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_Users_CreatorId",
                table: "Challenges",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_PersonalTrainerId",
                table: "Users",
                column: "PersonalTrainerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChallengeProgresses_Users_ParticipantId",
                table: "ChallengeProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_Users_CreatorId",
                table: "Challenges");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_PersonalTrainerId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "BadgeDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_Users_PersonalTrainerId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_ChallengeProgresses_ChallengeId",
                table: "ChallengeProgresses");

            migrationBuilder.DropIndex(
                name: "IX_ChallengeProgresses_ParticipantId",
                table: "ChallengeProgresses");

            migrationBuilder.DropColumn(
                name: "PersonalTrainerId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ParticipantId",
                table: "ChallengeProgresses");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Challenges",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Challenges_CreatorId",
                table: "Challenges",
                newName: "IX_Challenges_OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeProgresses_ChallengeId",
                table: "ChallengeProgresses",
                column: "ChallengeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_Users_OwnerId",
                table: "Challenges",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
