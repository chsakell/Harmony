using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmony.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkspaceUniqueName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_Name",
                table: "Workspaces",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workspaces_Name",
                table: "Workspaces");
        }
    }
}
