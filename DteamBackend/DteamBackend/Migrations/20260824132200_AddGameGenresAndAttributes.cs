using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DteamBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddGameGenresAndAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiscountPercentage",
                table: "Games",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<List<string>>(
                name: "Genres",
                table: "Games",
                type: "text[]",
                nullable: false,
                defaultValue: new List<string>());

            migrationBuilder.AddColumn<List<string>>(
                name: "Platforms",
                table: "Games",
                type: "text[]",
                nullable: false,
                defaultValue: new List<string>());

            migrationBuilder.AddColumn<List<string>>(
                name: "Features",
                table: "Games",
                type: "text[]",
                nullable: false,
                defaultValue: new List<string>());

            migrationBuilder.AddColumn<List<string>>(
                name: "Tags",
                table: "Games",
                type: "text[]",
                nullable: false,
                defaultValue: new List<string>());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Genres",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Platforms",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Features",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Games");
        }
    }
}
