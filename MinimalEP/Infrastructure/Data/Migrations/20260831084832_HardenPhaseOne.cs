using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinimalEP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class HardenPhaseOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workloads_EmployeeId",
                table: "Workloads");

            migrationBuilder.AddColumn<Guid>(
                name: "FamilyId",
                table: "RefreshTokens",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "RefreshTokens",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.Sql("UPDATE RefreshTokens SET FamilyId = Id");

            migrationBuilder.CreateIndex(
                name: "IX_Workloads_EmployeeId",
                table: "Workloads",
                column: "EmployeeId",
                unique: true,
                filter: "[Stop] IS NULL AND [Deleted] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_FamilyId",
                table: "RefreshTokens",
                column: "FamilyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workloads_EmployeeId",
                table: "Workloads");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_FamilyId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "FamilyId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "RefreshTokens");

            migrationBuilder.CreateIndex(
                name: "IX_Workloads_EmployeeId",
                table: "Workloads",
                column: "EmployeeId");
        }
    }
}
