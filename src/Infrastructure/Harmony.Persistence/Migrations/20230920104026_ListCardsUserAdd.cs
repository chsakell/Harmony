using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmony.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ListCardsUserAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Cards",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "BoardLists",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_UserId",
                table: "Cards",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BoardLists_UserId",
                table: "BoardLists",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BoardLists_Users_UserId",
                table: "BoardLists",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Users_UserId",
                table: "Cards",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoardLists_Users_UserId",
                table: "BoardLists");

            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Users_UserId",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_Cards_UserId",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_BoardLists_UserId",
                table: "BoardLists");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "BoardLists");
        }
    }
}
