using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing_Software.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCustomerFromJobList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobLists_Customers_CustomerId",
                table: "JobLists");

            migrationBuilder.DropIndex(
                name: "IX_JobLists_CustomerId",
                table: "JobLists");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "JobLists");

            migrationBuilder.AlterColumn<string>(
                name: "JobName",
                table: "JobLists",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "JobLists",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "JobName",
                table: "JobLists",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "JobLists",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "JobLists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_JobLists_CustomerId",
                table: "JobLists",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobLists_Customers_CustomerId",
                table: "JobLists",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
