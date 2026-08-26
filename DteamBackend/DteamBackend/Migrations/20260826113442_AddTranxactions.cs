using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DteamBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddTranxactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tranxactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TxhHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tranxactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tranxactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tranxactions_TxhHash",
                table: "Tranxactions",
                column: "TxhHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tranxactions_UserId",
                table: "Tranxactions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tranxactions");
        }
    }
}
