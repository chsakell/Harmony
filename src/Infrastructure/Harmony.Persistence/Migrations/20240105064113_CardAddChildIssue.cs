using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmony.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CardAddChildIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentCardId",
                table: "Cards",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_ParentCardId",
                table: "Cards",
                column: "ParentCardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Cards_ParentCardId",
                table: "Cards",
                column: "ParentCardId",
                principalTable: "Cards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Cards_ParentCardId",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_Cards_ParentCardId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "ParentCardId",
                table: "Cards");
        }
    }
}
