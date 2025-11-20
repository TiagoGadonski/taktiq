using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymHero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoMetadataToMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DurationSeconds",
                table: "Medias",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "Medias",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "Medias",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationSeconds",
                table: "Medias");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "Medias");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "Medias");
        }
    }
}
