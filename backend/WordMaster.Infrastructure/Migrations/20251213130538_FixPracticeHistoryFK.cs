using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordMaster.Infrastructure.Migrations;

/// <inheritdoc />
public partial class FixPracticeHistoryFK : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_PracticeHistories_AspNetUsers_AppUserId",
            table: "PracticeHistories");

        migrationBuilder.DropIndex(
            name: "IX_PracticeHistories_AppUserId",
            table: "PracticeHistories");

        migrationBuilder.DropColumn(
            name: "AppUserId",
            table: "PracticeHistories");

        migrationBuilder.CreateIndex(
            name: "IX_PracticeHistories_UserId",
            table: "PracticeHistories",
            column: "UserId");

        migrationBuilder.AddForeignKey(
            name: "FK_PracticeHistories_AspNetUsers_UserId",
            table: "PracticeHistories",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_PracticeHistories_AspNetUsers_UserId",
            table: "PracticeHistories");

        migrationBuilder.DropIndex(
            name: "IX_PracticeHistories_UserId",
            table: "PracticeHistories");

        migrationBuilder.AddColumn<Guid>(
            name: "AppUserId",
            table: "PracticeHistories",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_PracticeHistories_AppUserId",
            table: "PracticeHistories",
            column: "AppUserId");

        migrationBuilder.AddForeignKey(
            name: "FK_PracticeHistories_AspNetUsers_AppUserId",
            table: "PracticeHistories",
            column: "AppUserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id");
    }
}
