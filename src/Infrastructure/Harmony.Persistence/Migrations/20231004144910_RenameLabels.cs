using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmony.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameLabels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardLabels_BoardBoardLabels_BoardLabelId",
                table: "CardLabels");

            migrationBuilder.DropTable(
                name: "BoardBoardLabels");

            migrationBuilder.RenameColumn(
                name: "BoardLabelId",
                table: "CardLabels",
                newName: "LabelId");

            migrationBuilder.RenameIndex(
                name: "IX_CardLabels_BoardLabelId",
                table: "CardLabels",
                newName: "IX_CardLabels_LabelId");

            migrationBuilder.CreateTable(
                name: "BoardLabels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Colour = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BoardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "date", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardLabels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoardLabels_Boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoardLabels_BoardId",
                table: "BoardLabels",
                column: "BoardId");

            migrationBuilder.AddForeignKey(
                name: "FK_CardLabels_BoardLabels_LabelId",
                table: "CardLabels",
                column: "LabelId",
                principalTable: "BoardLabels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardLabels_BoardLabels_LabelId",
                table: "CardLabels");

            migrationBuilder.DropTable(
                name: "BoardLabels");

            migrationBuilder.RenameColumn(
                name: "LabelId",
                table: "CardLabels",
                newName: "BoardLabelId");

            migrationBuilder.RenameIndex(
                name: "IX_CardLabels_LabelId",
                table: "CardLabels",
                newName: "IX_CardLabels_BoardLabelId");

            migrationBuilder.CreateTable(
                name: "BoardBoardLabels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Colour = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateCreated = table.Column<DateTime>(type: "date", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "date", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardBoardLabels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoardBoardLabels_Boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoardBoardLabels_BoardId",
                table: "BoardBoardLabels",
                column: "BoardId");

            migrationBuilder.AddForeignKey(
                name: "FK_CardLabels_BoardBoardLabels_BoardLabelId",
                table: "CardLabels",
                column: "BoardLabelId",
                principalTable: "BoardBoardLabels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
