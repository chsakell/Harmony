using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmony.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CardListStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Cards");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Cards",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "BoardLists",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "BoardLists");

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Cards",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
