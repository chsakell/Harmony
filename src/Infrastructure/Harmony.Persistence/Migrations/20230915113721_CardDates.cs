using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmony.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CardDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Cards",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderDate",
                table: "Cards",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "ReminderDate",
                table: "Cards");
        }
    }
}
