using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoStock.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseInvoiceItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceItem_Parts_PartId",
                table: "PurchaseInvoiceItem");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceItem_PurchaseInvoices_PurchaseInvoiceId",
                table: "PurchaseInvoiceItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PurchaseInvoiceItem",
                table: "PurchaseInvoiceItem");

            migrationBuilder.RenameTable(
                name: "PurchaseInvoiceItem",
                newName: "PurchaseInvoiceItems");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseInvoiceItem_PurchaseInvoiceId",
                table: "PurchaseInvoiceItems",
                newName: "IX_PurchaseInvoiceItems_PurchaseInvoiceId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseInvoiceItem_PartId",
                table: "PurchaseInvoiceItems",
                newName: "IX_PurchaseInvoiceItems_PartId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PurchaseInvoiceItems",
                table: "PurchaseInvoiceItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceItems_Parts_PartId",
                table: "PurchaseInvoiceItems",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceItems_PurchaseInvoices_PurchaseInvoiceId",
                table: "PurchaseInvoiceItems",
                column: "PurchaseInvoiceId",
                principalTable: "PurchaseInvoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceItems_Parts_PartId",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceItems_PurchaseInvoices_PurchaseInvoiceId",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PurchaseInvoiceItems",
                table: "PurchaseInvoiceItems");

            migrationBuilder.RenameTable(
                name: "PurchaseInvoiceItems",
                newName: "PurchaseInvoiceItem");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseInvoiceItems_PurchaseInvoiceId",
                table: "PurchaseInvoiceItem",
                newName: "IX_PurchaseInvoiceItem_PurchaseInvoiceId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseInvoiceItems_PartId",
                table: "PurchaseInvoiceItem",
                newName: "IX_PurchaseInvoiceItem_PartId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PurchaseInvoiceItem",
                table: "PurchaseInvoiceItem",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceItem_Parts_PartId",
                table: "PurchaseInvoiceItem",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceItem_PurchaseInvoices_PurchaseInvoiceId",
                table: "PurchaseInvoiceItem",
                column: "PurchaseInvoiceId",
                principalTable: "PurchaseInvoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
