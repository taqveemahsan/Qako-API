using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuditPilot.Data.Migrations
{
    /// <inheritdoc />
    public partial class addDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "53bc211e-2600-4b3b-bc3c-f51597d84c1d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "af1f5a4b-4473-40dd-8514-21a2ec6fd6ac");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "da0a52a2-042f-4840-9d54-15d632a53d48");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f9d0fe2d-56df-49d8-8600-4f963a082966");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredOn",
                table: "UserProjectPermissions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "30e8503d-8b95-412f-a81d-e2c943d3ec9a", null, "Partner", "PARTNER" },
                    { "6809fecc-ff67-41d9-8b85-5e68b507a1ba", null, "TaxManager", "TAXMANAGER" },
                    { "bb34e678-bebe-427c-93fb-74eaa374d91a", null, "User", "USER" },
                    { "d7bf7bef-3806-4e07-8680-62446744c204", null, "AuditManager", "AUDITMANAGER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "30e8503d-8b95-412f-a81d-e2c943d3ec9a");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6809fecc-ff67-41d9-8b85-5e68b507a1ba");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bb34e678-bebe-427c-93fb-74eaa374d91a");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d7bf7bef-3806-4e07-8680-62446744c204");

            migrationBuilder.DropColumn(
                name: "ExpiredOn",
                table: "UserProjectPermissions");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "53bc211e-2600-4b3b-bc3c-f51597d84c1d", null, "Partner", "PARTNER" },
                    { "af1f5a4b-4473-40dd-8514-21a2ec6fd6ac", null, "TaxManager", "TAXMANAGER" },
                    { "da0a52a2-042f-4840-9d54-15d632a53d48", null, "User", "USER" },
                    { "f9d0fe2d-56df-49d8-8600-4f963a082966", null, "AuditManager", "AUDITMANAGER" }
                });
        }
    }
}
