using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinimalEP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ScaleReadQueries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workloads_CustomerId",
                table: "Workloads");

            migrationBuilder.CreateIndex(
                name: "IX_Workloads_CustomerId_Id",
                table: "Workloads",
                columns: new[] { "CustomerId", "Id" },
                filter: "[Deleted] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Workloads_EmployeeId_Id",
                table: "Workloads",
                columns: new[] { "EmployeeId", "Id" },
                filter: "[Deleted] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workloads_CustomerId_Id",
                table: "Workloads");

            migrationBuilder.DropIndex(
                name: "IX_Workloads_EmployeeId_Id",
                table: "Workloads");

            migrationBuilder.CreateIndex(
                name: "IX_Workloads_CustomerId",
                table: "Workloads",
                column: "CustomerId");
        }
    }
}
