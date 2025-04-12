using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuditPilot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AccountConfirmations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "AccountConfirmations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountConfirmations", x => x.Id);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_AccountConfirmations_Email",
                table: "AccountConfirmations",
                column: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountConfirmations");

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
    }
}
