using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuditPilot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClientGoogleId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "138de465-d866-4232-8050-cd81d8811659");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1eb66f4c-0b97-4a3c-aa67-e060505d7b14");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "23b9bc42-50af-4ab9-aae0-d1a19de6ba08");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "51eefc44-27fd-4445-8676-24164ced866e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8c093595-c3e3-45db-bf54-dd2cbf9585d7");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ad0c0b53-cac5-4906-894b-54b6e85af872");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3d56f94-0186-4b65-a9f1-9a9d0d13d75b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "da2258f2-5441-4874-911e-85180e3f1a26");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e36cc4f7-330c-4dc8-b473-0a9abc5c30ea");

            migrationBuilder.AddColumn<string>(
                name: "GoogleDriveId",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "GoogleDriveId",
                table: "Clients");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "138de465-d866-4232-8050-cd81d8811659", null, "Partner", "PARTNER" },
                    { "1eb66f4c-0b97-4a3c-aa67-e060505d7b14", null, "CorporateManager", "CORPORATEMANAGER" },
                    { "23b9bc42-50af-4ab9-aae0-d1a19de6ba08", null, "AuditManager", "AUDITMANAGER" },
                    { "51eefc44-27fd-4445-8676-24164ced866e", null, "ERPManager", "ERPMANAGER" },
                    { "8c093595-c3e3-45db-bf54-dd2cbf9585d7", null, "AdvisoryManager", "ADVISORYMANAGER" },
                    { "ad0c0b53-cac5-4906-894b-54b6e85af872", null, "TaxManager", "TAXMANAGER" },
                    { "c3d56f94-0186-4b65-a9f1-9a9d0d13d75b", null, "OtherManager", "OTHERMANAGER" },
                    { "da2258f2-5441-4874-911e-85180e3f1a26", null, "User", "USER" },
                    { "e36cc4f7-330c-4dc8-b473-0a9abc5c30ea", null, "BookkeepingManager", "BOOKKEEPINGMANAGER" }
                });
        }
    }
}
