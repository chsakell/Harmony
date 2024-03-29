using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmony.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Retrospective : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Retrospectives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BoardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SprintId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    MaxVotesPerUser = table.Column<int>(type: "int", nullable: false),
                    HideVoteCount = table.Column<bool>(type: "bit", nullable: false),
                    HideCardsInitially = table.Column<bool>(type: "bit", nullable: false),
                    DisableVotingInitially = table.Column<bool>(type: "bit", nullable: false),
                    ShowCardsAuthor = table.Column<bool>(type: "bit", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Retrospectives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Retrospectives_Boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Retrospectives_Sprints_SprintId",
                        column: x => x.SprintId,
                        principalTable: "Sprints",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Retrospectives_BoardId",
                table: "Retrospectives",
                column: "BoardId");

            migrationBuilder.CreateIndex(
                name: "IX_Retrospectives_SprintId",
                table: "Retrospectives",
                column: "SprintId",
                unique: true,
                filter: "[SprintId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Retrospectives");
        }
    }
}
