using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WordMaster.Infrastructure.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class _05_add_Seed_data_allowedIpAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AllowedIpAddresses",
                columns: new[] { "Id", "CreatedAt", "Description", "IpAddress", "IsActive" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Localhost IPv4 - Local Development", "127.0.0.1", true },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Localhost IPv6 - Local Development", "::1", true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AllowedIpAddresses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AllowedIpAddresses",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
