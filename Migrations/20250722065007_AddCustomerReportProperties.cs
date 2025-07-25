using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing_Software.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerReportProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PercentageOfTotal",
                table: "Customers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Rank",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PercentageOfTotal",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Rank",
                table: "Customers");
        }
    }
}
