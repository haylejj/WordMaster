using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordMaster.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OptimizePracticeHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PracticeHistories_Words_WordId",
                table: "PracticeHistories");

            migrationBuilder.DropIndex(
                name: "IX_PracticeHistories_WordId",
                table: "PracticeHistories");

            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "PracticeHistories");

            migrationBuilder.DropColumn(
                name: "WordId",
                table: "PracticeHistories");

            migrationBuilder.AddColumn<int>(
                name: "CorrectCount",
                table: "PracticeHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WordCount",
                table: "PracticeHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WrongCount",
                table: "PracticeHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrectCount",
                table: "PracticeHistories");

            migrationBuilder.DropColumn(
                name: "WordCount",
                table: "PracticeHistories");

            migrationBuilder.DropColumn(
                name: "WrongCount",
                table: "PracticeHistories");

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "PracticeHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "WordId",
                table: "PracticeHistories",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_PracticeHistories_WordId",
                table: "PracticeHistories",
                column: "WordId");

            migrationBuilder.AddForeignKey(
                name: "FK_PracticeHistories_Words_WordId",
                table: "PracticeHistories",
                column: "WordId",
                principalTable: "Words",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
