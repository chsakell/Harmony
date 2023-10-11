using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmony.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CardActivityType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "CardActivities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "CardActivities");
        }
    }
}
