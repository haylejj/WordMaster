using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordMaster.Infrastructure.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class _04_add_allowed_ip_address : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllowedIpAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedIpAddresses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllowedIpAddresses_IpAddress",
                table: "AllowedIpAddresses",
                column: "IpAddress",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllowedIpAddresses");
        }
    }
}
