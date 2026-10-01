using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeGames.Data.Migrations
{
    /// <inheritdoc />
    public partial class AttemptScoreFraction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ScoreFraction",
                table: "Attempts",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.Sql("UPDATE [Attempts] SET [ScoreFraction] = 1 WHERE [IsCorrect] = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScoreFraction",
                table: "Attempts");
        }
    }
}
