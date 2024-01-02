using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmony.Notifications.Migrations
{
    /// <inheritdoc />
    public partial class NotificationWorkspaceId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Notifications");
        }
    }
}
