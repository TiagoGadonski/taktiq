using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCalisthenicsFieldsToExercise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NoEquipmentAlternative",
                table: "Exercises",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "Progressions",
                table: "Exercises",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "Regressions",
                table: "Exercises",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpaceRequired",
                table: "Exercises",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoEquipmentAlternative",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "Progressions",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "Regressions",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "SpaceRequired",
                table: "Exercises");
        }
    }
}
