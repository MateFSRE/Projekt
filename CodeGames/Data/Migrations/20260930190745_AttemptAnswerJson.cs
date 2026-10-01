using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeGames.Data.Migrations
{
    /// <inheritdoc />
    public partial class AttemptAnswerJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Answer",
                table: "Attempts",
                newName: "AnswerJson");

            migrationBuilder.AlterColumn<string>(
                name: "AnswerJson",
                table: "Attempts",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AnswerJson",
                table: "Attempts",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.RenameColumn(
                name: "AnswerJson",
                table: "Attempts",
                newName: "Answer");
        }
    }
}
