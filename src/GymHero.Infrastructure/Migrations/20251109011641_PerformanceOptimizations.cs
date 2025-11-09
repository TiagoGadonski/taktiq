using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PerformanceOptimizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Friendships",
                table: "Friendships");

            migrationBuilder.RenameIndex(
                name: "IX_ChallengeProgresses_ParticipantId",
                table: "ChallengeProgresses",
                newName: "IX_ChallengeProgress_ParticipantId");

            migrationBuilder.RenameIndex(
                name: "IX_ChallengeProgresses_ChallengeId",
                table: "ChallengeProgresses",
                newName: "IX_ChallengeProgress_ChallengeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Friendships",
                table: "Friendships",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlans_OwnerActive",
                table: "WorkoutPlans",
                columns: new[] { "OwnerId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressMetrics_OwnerDate",
                table: "ProgressMetrics",
                columns: new[] { "OwnerId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserReadCreated",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_AddresseeStatus",
                table: "Friendships",
                columns: new[] { "AddresseeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_RequesterAddressee",
                table: "Friendships",
                columns: new[] { "RequesterId", "AddresseeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_RequesterId",
                table: "Friendships",
                column: "RequesterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkoutPlans_OwnerActive",
                table: "WorkoutPlans");

            migrationBuilder.DropIndex(
                name: "IX_ProgressMetrics_OwnerDate",
                table: "ProgressMetrics");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserReadCreated",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Friendships",
                table: "Friendships");

            migrationBuilder.DropIndex(
                name: "IX_Friendships_AddresseeStatus",
                table: "Friendships");

            migrationBuilder.DropIndex(
                name: "IX_Friendships_RequesterAddressee",
                table: "Friendships");

            migrationBuilder.DropIndex(
                name: "IX_Friendships_RequesterId",
                table: "Friendships");

            migrationBuilder.RenameIndex(
                name: "IX_ChallengeProgress_ParticipantId",
                table: "ChallengeProgresses",
                newName: "IX_ChallengeProgresses_ParticipantId");

            migrationBuilder.RenameIndex(
                name: "IX_ChallengeProgress_ChallengeId",
                table: "ChallengeProgresses",
                newName: "IX_ChallengeProgresses_ChallengeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Friendships",
                table: "Friendships",
                columns: new[] { "RequesterId", "AddresseeId" });
        }
    }
}
