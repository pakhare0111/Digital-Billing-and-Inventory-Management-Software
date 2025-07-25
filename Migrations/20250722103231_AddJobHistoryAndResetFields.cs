using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing_Software.Migrations
{
    /// <inheritdoc />
    public partial class AddJobHistoryAndResetFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InvoiceId",
                table: "CustomerJobLists",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvoicedDate",
                table: "CustomerJobLists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInvoiced",
                table: "CustomerJobLists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsResetForNewVisit",
                table: "CustomerJobLists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetDate",
                table: "CustomerJobLists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerJobLists_InvoiceId",
                table: "CustomerJobLists",
                column: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerJobLists_Invoices_InvoiceId",
                table: "CustomerJobLists",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerJobLists_Invoices_InvoiceId",
                table: "CustomerJobLists");

            migrationBuilder.DropIndex(
                name: "IX_CustomerJobLists_InvoiceId",
                table: "CustomerJobLists");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "CustomerJobLists");

            migrationBuilder.DropColumn(
                name: "InvoicedDate",
                table: "CustomerJobLists");

            migrationBuilder.DropColumn(
                name: "IsInvoiced",
                table: "CustomerJobLists");

            migrationBuilder.DropColumn(
                name: "IsResetForNewVisit",
                table: "CustomerJobLists");

            migrationBuilder.DropColumn(
                name: "ResetDate",
                table: "CustomerJobLists");
        }
    }
}
