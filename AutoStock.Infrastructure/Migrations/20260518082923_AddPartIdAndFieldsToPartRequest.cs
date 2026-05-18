using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoStock.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPartIdAndFieldsToPartRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PartId",
                table: "PartRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "PartRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "PartRequests",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_PartRequests_PartId",
                table: "PartRequests",
                column: "PartId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartRequests_Parts_PartId",
                table: "PartRequests",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartRequests_Parts_PartId",
                table: "PartRequests");

            migrationBuilder.DropIndex(
                name: "IX_PartRequests_PartId",
                table: "PartRequests");

            migrationBuilder.DropColumn(
                name: "PartId",
                table: "PartRequests");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "PartRequests");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "PartRequests");
        }
    }
}
