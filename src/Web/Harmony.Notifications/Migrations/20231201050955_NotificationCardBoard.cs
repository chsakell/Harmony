using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmony.Notifications.Migrations
{
    /// <inheritdoc />
    public partial class NotificationCardBoard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BoardId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CardId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BoardId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CardId",
                table: "Notifications");
        }
    }
}
