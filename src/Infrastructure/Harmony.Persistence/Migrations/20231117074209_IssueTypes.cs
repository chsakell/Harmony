using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmony.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IssueTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IssueTypeId",
                table: "Cards",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IssueTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BoardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueTypes_Boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_IssueTypeId",
                table: "Cards",
                column: "IssueTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueTypes_BoardId",
                table: "IssueTypes",
                column: "BoardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_IssueTypes_IssueTypeId",
                table: "Cards",
                column: "IssueTypeId",
                principalTable: "IssueTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_IssueTypes_IssueTypeId",
                table: "Cards");

            migrationBuilder.DropTable(
                name: "IssueTypes");

            migrationBuilder.DropIndex(
                name: "IX_Cards_IssueTypeId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "IssueTypeId",
                table: "Cards");
        }
    }
}
