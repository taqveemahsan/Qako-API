using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuditPilot.Data.Migrations
{
    /// <inheritdoc />
    public partial class clinet_fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "31fab9fc-b96d-411d-b2db-57e51eebc032");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7d0807c1-a0f9-4863-826b-bdb00f35c3e8");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8605dc83-6d39-49fe-843b-0a85761b081e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f1f428f7-1302-4ff8-9cb0-070e331e321c");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Clients");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "31fab9fc-b96d-411d-b2db-57e51eebc032", null, "Partner", "PARTNER" },
                    { "7d0807c1-a0f9-4863-826b-bdb00f35c3e8", null, "Tax Manager", "TAXMANAGER" },
                    { "8605dc83-6d39-49fe-843b-0a85761b081e", null, "Audit Manager", "AUDITMANAGER" },
                    { "f1f428f7-1302-4ff8-9cb0-070e331e321c", null, "User", "USER" }
                });
        }
    }
}
