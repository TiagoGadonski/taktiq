using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExcludedExercisesToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExcludedExercises",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExcludedExercises",
                table: "Users");
        }
    }
}
