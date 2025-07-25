using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing_Software.Migrations
{
    /// <inheritdoc />
    public partial class UpdateJobListModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobLists_Invoices_InvoiceId",
                table: "JobLists");

            migrationBuilder.DropIndex(
                name: "IX_JobLists_InvoiceId",
                table: "JobLists");

            migrationBuilder.DropColumn(
                name: "CompletedDate",
                table: "JobLists");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "JobLists");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "JobLists");

            migrationBuilder.DropColumn(
                name: "IsSelected",
                table: "JobLists");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "JobLists");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "JobLists");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "JobLists");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedDate",
                table: "JobLists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceId",
                table: "JobLists",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "JobLists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSelected",
                table: "JobLists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "JobLists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "JobLists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "JobLists",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobLists_InvoiceId",
                table: "JobLists",
                column: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobLists_Invoices_InvoiceId",
                table: "JobLists",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
