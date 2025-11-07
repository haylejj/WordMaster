using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordMaster.Infrastructure.EfCore.Migrations;

/// <inheritdoc />
public partial class _02_update_word_confg : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "TurkishWord",
            table: "Words",
            type: "nvarchar(60)",
            maxLength: 60,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(30)",
            oldMaxLength: 30);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "TurkishWord",
            table: "Words",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(60)",
            oldMaxLength: 60);
    }
}
