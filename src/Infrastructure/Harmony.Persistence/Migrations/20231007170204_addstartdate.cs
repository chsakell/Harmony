using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmony.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addstartdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Cards",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Cards");
        }
    }
}
