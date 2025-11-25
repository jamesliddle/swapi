using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarshipApi.Migrations
{
    /// <inheritdoc />
    public partial class AddStarshipIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Starships",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "Starships",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Manufacturer",
                table: "Starships",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CostInCredits",
                table: "Starships",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Starships_CostInCredits",
                table: "Starships",
                column: "CostInCredits");

            migrationBuilder.CreateIndex(
                name: "IX_Starships_Manufacturer",
                table: "Starships",
                column: "Manufacturer");

            migrationBuilder.CreateIndex(
                name: "IX_Starships_Model",
                table: "Starships",
                column: "Model");

            migrationBuilder.CreateIndex(
                name: "IX_Starships_Name",
                table: "Starships",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Starships_CostInCredits",
                table: "Starships");

            migrationBuilder.DropIndex(
                name: "IX_Starships_Manufacturer",
                table: "Starships");

            migrationBuilder.DropIndex(
                name: "IX_Starships_Model",
                table: "Starships");

            migrationBuilder.DropIndex(
                name: "IX_Starships_Name",
                table: "Starships");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Starships",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "Starships",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Manufacturer",
                table: "Starships",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "CostInCredits",
                table: "Starships",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
