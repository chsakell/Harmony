using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmony.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CardListNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_BoardLists_BoardListId",
                table: "Cards");

            migrationBuilder.AlterColumn<Guid>(
                name: "BoardListId",
                table: "Cards",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_BoardLists_BoardListId",
                table: "Cards",
                column: "BoardListId",
                principalTable: "BoardLists",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_BoardLists_BoardListId",
                table: "Cards");

            migrationBuilder.AlterColumn<Guid>(
                name: "BoardListId",
                table: "Cards",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_BoardLists_BoardListId",
                table: "Cards",
                column: "BoardListId",
                principalTable: "BoardLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
