using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuditPilot.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserFandLName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "53b302f6-05fd-461e-aaaf-ae3046ee2e6c");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "70643b31-5fa1-4cf4-814e-8ae1c3132f2e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7eddeac4-35ba-4313-8e3a-884897d8188c");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b6dc3c17-ac8f-49d6-abe6-712d3a29c853");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "53b302f6-05fd-461e-aaaf-ae3046ee2e6c", null, "Tax Manager", "TAXMANAGER" },
                    { "70643b31-5fa1-4cf4-814e-8ae1c3132f2e", null, "User", "USER" },
                    { "7eddeac4-35ba-4313-8e3a-884897d8188c", null, "Audit Manager", "AUDITMANAGER" },
                    { "b6dc3c17-ac8f-49d6-abe6-712d3a29c853", null, "Partner", "PARTNER" }
                });
        }
    }
}
