using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuditPilot.Data.Migrations
{
    /// <inheritdoc />
    public partial class folderstructuretable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "FolderStructures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FolderName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ParentFolderId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    GoogleDriveFolderId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderStructures", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FolderStructures");

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
                    { "30e8503d-8b95-412f-a81d-e2c943d3ec9a", null, "Partner", "PARTNER" },
                    { "6809fecc-ff67-41d9-8b85-5e68b507a1ba", null, "TaxManager", "TAXMANAGER" },
                    { "bb34e678-bebe-427c-93fb-74eaa374d91a", null, "User", "USER" },
                    { "d7bf7bef-3806-4e07-8680-62446744c204", null, "AuditManager", "AUDITMANAGER" }
                });
        }
    }
}
