using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telenor.Api.TestExecutionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConnectTenants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConnectTenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientKey = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SharedSecret = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    BaseUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    DisplayUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ProductType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    InstalledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectTenants", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConnectTenants_ClientKey",
                table: "ConnectTenants",
                column: "ClientKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConnectTenants");
        }
    }
}
