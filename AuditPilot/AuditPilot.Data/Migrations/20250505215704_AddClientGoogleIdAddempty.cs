using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuditPilot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClientGoogleIdAddempty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1b137d39-c01d-4bf3-8ffe-902940e2d9a5");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6ab02755-20bd-47ac-901e-88336f48b4ab");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6d69b799-be3c-4e34-a007-68dfa68145b5");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "75e2e6f2-23cf-4a6a-8606-63257b5e09e9");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "826b7cd9-598c-4b4d-8310-c8d1afd60fd1");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8a2cef3e-2d42-490e-a7f0-c62b9fb94639");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a2c5aa26-a8eb-4137-bd6a-3593d4469235");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d95cfb96-1d1d-4530-b947-8ceecb87ea14");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f47bcef2-5d52-4f8d-918a-f79f26a5460e");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0eb57ccc-1bc6-4024-80de-8c3b57bf079b", null, "AdvisoryManager", "ADVISORYMANAGER" },
                    { "2f6f2780-1c0c-4792-994b-c21f6ca34048", null, "User", "USER" },
                    { "47667b89-e385-4595-a026-a0d5b546f45c", null, "TaxManager", "TAXMANAGER" },
                    { "64da3ebf-43be-4b3c-83b6-4fb1a9d0540d", null, "CorporateManager", "CORPORATEMANAGER" },
                    { "8cc3db23-6692-4219-8ee4-9ab29f08eb98", null, "BookkeepingManager", "BOOKKEEPINGMANAGER" },
                    { "b1e208fb-fa66-48c5-bcbd-7adfa8330633", null, "AuditManager", "AUDITMANAGER" },
                    { "b35fb289-60f6-4ef1-a39c-ac285da12240", null, "ERPManager", "ERPMANAGER" },
                    { "c020f8ab-afb7-4105-83b6-81fbe02fe7b7", null, "Partner", "PARTNER" },
                    { "e7bbc027-47ea-41c2-8ced-51d5fe71f320", null, "OtherManager", "OTHERMANAGER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0eb57ccc-1bc6-4024-80de-8c3b57bf079b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2f6f2780-1c0c-4792-994b-c21f6ca34048");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "47667b89-e385-4595-a026-a0d5b546f45c");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "64da3ebf-43be-4b3c-83b6-4fb1a9d0540d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8cc3db23-6692-4219-8ee4-9ab29f08eb98");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b1e208fb-fa66-48c5-bcbd-7adfa8330633");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b35fb289-60f6-4ef1-a39c-ac285da12240");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c020f8ab-afb7-4105-83b6-81fbe02fe7b7");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e7bbc027-47ea-41c2-8ced-51d5fe71f320");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1b137d39-c01d-4bf3-8ffe-902940e2d9a5", null, "AdvisoryManager", "ADVISORYMANAGER" },
                    { "6ab02755-20bd-47ac-901e-88336f48b4ab", null, "BookkeepingManager", "BOOKKEEPINGMANAGER" },
                    { "6d69b799-be3c-4e34-a007-68dfa68145b5", null, "Partner", "PARTNER" },
                    { "75e2e6f2-23cf-4a6a-8606-63257b5e09e9", null, "AuditManager", "AUDITMANAGER" },
                    { "826b7cd9-598c-4b4d-8310-c8d1afd60fd1", null, "TaxManager", "TAXMANAGER" },
                    { "8a2cef3e-2d42-490e-a7f0-c62b9fb94639", null, "ERPManager", "ERPMANAGER" },
                    { "a2c5aa26-a8eb-4137-bd6a-3593d4469235", null, "OtherManager", "OTHERMANAGER" },
                    { "d95cfb96-1d1d-4530-b947-8ceecb87ea14", null, "User", "USER" },
                    { "f47bcef2-5d52-4f8d-918a-f79f26a5460e", null, "CorporateManager", "CORPORATEMANAGER" }
                });
        }
    }
}
