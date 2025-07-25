using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing_Software.Migrations
{
    /// <inheritdoc />
    public partial class AddJobCompletionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedDate",
                table: "JobLists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "JobLists",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedDate",
                table: "JobLists");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "JobLists");
        }
    }
}
