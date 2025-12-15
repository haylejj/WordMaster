using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordMaster.Infrastructure.Migrations;

/// <inheritdoc />
public partial class UpdatePracticeTracking : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsLastAnswerCorrectInFolderPractice",
            table: "Words");

        migrationBuilder.CreateTable(
            name: "PracticeHistories",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                WordId = table.Column<long>(type: "bigint", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                PracticeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PracticeHistories", x => x.Id);
                table.ForeignKey(
                    name: "FK_PracticeHistories_AspNetUsers_AppUserId",
                    column: x => x.AppUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_PracticeHistories_Words_WordId",
                    column: x => x.WordId,
                    principalTable: "Words",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PracticeHistories_AppUserId",
            table: "PracticeHistories",
            column: "AppUserId");

        migrationBuilder.CreateIndex(
            name: "IX_PracticeHistories_WordId",
            table: "PracticeHistories",
            column: "WordId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PracticeHistories");

        migrationBuilder.AddColumn<bool>(
            name: "IsLastAnswerCorrectInFolderPractice",
            table: "Words",
            type: "bit",
            nullable: true);
    }
}
