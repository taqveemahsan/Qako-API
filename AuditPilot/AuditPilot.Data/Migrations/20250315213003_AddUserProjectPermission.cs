using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuditPilot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProjectPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "UserProjectPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HasAccess = table.Column<bool>(type: "bit", nullable: false),
                    AssignedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProjectPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProjectPermissions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProjectPermissions_ClientProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "ClientProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_UserProjectPermissions_ProjectId",
                table: "UserProjectPermissions",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProjectPermissions_UserId",
                table: "UserProjectPermissions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserProjectPermissions");

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
    }
}
