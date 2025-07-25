using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing_Software.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerJobList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerJobLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    JobListId = table.Column<int>(type: "int", nullable: false),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerJobLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerJobLists_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerJobLists_JobLists_JobListId",
                        column: x => x.JobListId,
                        principalTable: "JobLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerJobLists_CustomerId",
                table: "CustomerJobLists",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerJobLists_JobListId",
                table: "CustomerJobLists",
                column: "JobListId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerJobLists");
        }
    }
}
