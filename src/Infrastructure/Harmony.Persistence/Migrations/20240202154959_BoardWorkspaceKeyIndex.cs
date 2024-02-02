using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmony.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BoardWorkspaceKeyIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Boards_Key",
                table: "Boards");

            migrationBuilder.DropIndex(
                name: "IX_Boards_WorkspaceId",
                table: "Boards");

            migrationBuilder.CreateIndex(
                name: "IX_Boards_WorkspaceId_Key",
                table: "Boards",
                columns: new[] { "WorkspaceId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Boards_WorkspaceId_Key",
                table: "Boards");

            migrationBuilder.CreateIndex(
                name: "IX_Boards_Key",
                table: "Boards",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Boards_WorkspaceId",
                table: "Boards",
                column: "WorkspaceId");
        }
    }
}
