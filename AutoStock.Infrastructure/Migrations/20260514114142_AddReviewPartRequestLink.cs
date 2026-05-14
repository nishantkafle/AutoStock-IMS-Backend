using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoStock.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewPartRequestLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PartRequestId",
                table: "Reviews",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_PartRequestId",
                table: "Reviews",
                column: "PartRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_PartRequests_PartRequestId",
                table: "Reviews",
                column: "PartRequestId",
                principalTable: "PartRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_PartRequests_PartRequestId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_PartRequestId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "PartRequestId",
                table: "Reviews");
        }
    }
}
