using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmony.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameLabelsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoardLabels_Boards_BoardId",
                table: "BoardLabels");

            migrationBuilder.DropForeignKey(
                name: "FK_CardLabels_BoardLabels_LabelId",
                table: "CardLabels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BoardLabels",
                table: "BoardLabels");

            migrationBuilder.RenameTable(
                name: "BoardLabels",
                newName: "Labels");

            migrationBuilder.RenameIndex(
                name: "IX_BoardLabels_BoardId",
                table: "Labels",
                newName: "IX_Labels_BoardId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Labels",
                table: "Labels",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CardLabels_Labels_LabelId",
                table: "CardLabels",
                column: "LabelId",
                principalTable: "Labels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Labels_Boards_BoardId",
                table: "Labels",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardLabels_Labels_LabelId",
                table: "CardLabels");

            migrationBuilder.DropForeignKey(
                name: "FK_Labels_Boards_BoardId",
                table: "Labels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Labels",
                table: "Labels");

            migrationBuilder.RenameTable(
                name: "Labels",
                newName: "BoardLabels");

            migrationBuilder.RenameIndex(
                name: "IX_Labels_BoardId",
                table: "BoardLabels",
                newName: "IX_BoardLabels_BoardId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BoardLabels",
                table: "BoardLabels",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BoardLabels_Boards_BoardId",
                table: "BoardLabels",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CardLabels_BoardLabels_LabelId",
                table: "CardLabels",
                column: "LabelId",
                principalTable: "BoardLabels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
