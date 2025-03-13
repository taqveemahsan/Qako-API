using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuditPilot.Data.Migrations
{
    /// <inheritdoc />
    public partial class ClientProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2c5cc807-ed34-4def-ae62-1e8d93f2f53b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3f56c844-8572-451f-ba48-a20fb237cde7");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5823d775-f5bb-4bf5-8eb9-9430193d467f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "89a82971-bf5c-42df-a5cd-e8745a7ed2ca");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "ClientProjects",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "ClientProjects",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0f9e5035-99eb-403a-8627-60ddf0f8e013", null, "Audit Manager", "AUDITMANAGER" },
                    { "2f420aac-909c-48d1-9480-208b774a99c3", null, "Partner", "PARTNER" },
                    { "472e2b64-146d-47b4-b4e4-235db53e2ea2", null, "Tax Manager", "TAXMANAGER" },
                    { "b341e7b1-ad31-4fa7-8384-e3b8c88f44a4", null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0f9e5035-99eb-403a-8627-60ddf0f8e013");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2f420aac-909c-48d1-9480-208b774a99c3");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "472e2b64-146d-47b4-b4e4-235db53e2ea2");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b341e7b1-ad31-4fa7-8384-e3b8c88f44a4");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "ClientProjects");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "ClientProjects");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2c5cc807-ed34-4def-ae62-1e8d93f2f53b", null, "Partner", "PARTNER" },
                    { "3f56c844-8572-451f-ba48-a20fb237cde7", null, "User", "USER" },
                    { "5823d775-f5bb-4bf5-8eb9-9430193d467f", null, "Audit Manager", "AUDITMANAGER" },
                    { "89a82971-bf5c-42df-a5cd-e8745a7ed2ca", null, "Tax Manager", "TAXMANAGER" }
                });
        }
    }
}
