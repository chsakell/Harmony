using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmony.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChecklistUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "CheckLists",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CheckLists_UserId",
                table: "CheckLists",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckLists_Users_UserId",
                table: "CheckLists",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckLists_Users_UserId",
                table: "CheckLists");

            migrationBuilder.DropIndex(
                name: "IX_CheckLists_UserId",
                table: "CheckLists");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CheckLists");
        }
    }
}
