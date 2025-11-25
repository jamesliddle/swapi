using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarshipApi.Migrations
{
    /// <inheritdoc />
    public partial class AddFilmTitlesColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilmTitles",
                table: "Starships",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilmTitles",
                table: "Starships");
        }
    }
}
