using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuditPilot.Data.Migrations
{
    /// <inheritdoc />
    public partial class addroles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1c339032-c0bc-435e-9131-a24b51f7a98e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "344cc4e5-8632-42cf-a0d7-c91406393741");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "81e0411d-43bb-48c4-98ed-35dfbe410ecf");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f98043c7-3c59-45a8-a761-a4a1d3ef31aa");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1d05377f-cb73-4ce5-a5f2-b37a117e0ae9", null, "OtherManager", "OTHERMANAGER" },
                    { "5b57a1e1-9365-4c3b-9a63-035446fdca61", null, "AuditManager", "AUDITMANAGER" },
                    { "7970cc28-7801-4272-a31c-8f689bbd865d", null, "CorporateManager", "CORPORATEMANAGER" },
                    { "7d13ae1f-cc16-43d6-8ca6-ffb4f0db8946", null, "User", "USER" },
                    { "bddda9f7-ed42-4b24-965c-50f8d6c3e153", null, "AdvisoryManager", "ADVISORYMANAGER" },
                    { "c50f54ac-7f83-442e-8580-88a7bc8d9d95", null, "BookkeepingManager", "BOOKKEEPINGMANAGER" },
                    { "c9d8b089-cbb1-4f77-8cee-4a7e5a89aca9", null, "TaxManager", "TAXMANAGER" },
                    { "e8639162-34f9-414d-8bcd-b06d0b681a09", null, "Partner", "PARTNER" },
                    { "f1468545-bb5c-437b-b9d4-8d74966703b6", null, "ERPManager", "ERPMANAGER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1d05377f-cb73-4ce5-a5f2-b37a117e0ae9");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5b57a1e1-9365-4c3b-9a63-035446fdca61");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7970cc28-7801-4272-a31c-8f689bbd865d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7d13ae1f-cc16-43d6-8ca6-ffb4f0db8946");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bddda9f7-ed42-4b24-965c-50f8d6c3e153");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c50f54ac-7f83-442e-8580-88a7bc8d9d95");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c9d8b089-cbb1-4f77-8cee-4a7e5a89aca9");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e8639162-34f9-414d-8bcd-b06d0b681a09");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f1468545-bb5c-437b-b9d4-8d74966703b6");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1c339032-c0bc-435e-9131-a24b51f7a98e", null, "AuditManager", "AUDITMANAGER" },
                    { "344cc4e5-8632-42cf-a0d7-c91406393741", null, "User", "USER" },
                    { "81e0411d-43bb-48c4-98ed-35dfbe410ecf", null, "Partner", "PARTNER" },
                    { "f98043c7-3c59-45a8-a761-a4a1d3ef31aa", null, "TaxManager", "TAXMANAGER" }
                });
        }
    }
}
