using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockService.Migrations
{
    /// <inheritdoc />
    public partial class AddStockOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockOperations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockOperations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockOperations_OperationId",
                table: "StockOperations",
                column: "OperationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockOperations");
        }
    }
}
