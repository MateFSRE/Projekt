using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CodeGames.Data.Migrations
{
    /// <inheritdoc />
    public partial class RobotMazeChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Challenges",
                columns: new[] { "Id", "CodeSnippet", "ConfigJson", "CorrectAnswer", "Difficulty", "Explanation", "IsActive", "Language", "Points", "Question", "Title", "Type" },
                values: new object[,]
                {
                    { 40, "4x1", "{\"width\":4,\"height\":1,\"walls\":[],\"start\":{\"x\":0,\"y\":0,\"dir\":\"right\"},\"goal\":{\"x\":3,\"y\":0},\"stars\":[],\"allowedBlocks\":[\"forward\"],\"maxBlocks\":4,\"maxSteps\":8,\"optimalBlocks\":3}", "simulacija", 1, "Tri puta naprijed vode robota od lijevog ruba do cilja.", true, "Blokovi", 10, "Dovedi robota do cilja.", "Ravna linija", "RobotMaze" },
                    { 41, "3x3", "{\"width\":3,\"height\":3,\"walls\":[],\"start\":{\"x\":0,\"y\":0,\"dir\":\"right\"},\"goal\":{\"x\":2,\"y\":2},\"stars\":[],\"allowedBlocks\":[\"forward\",\"turnLeft\",\"turnRight\"],\"maxBlocks\":8,\"maxSteps\":20,\"optimalBlocks\":5}", "simulacija", 1, "Idi desno do kraja reda, skreni desno i spusti se do donjeg desnog polja.", true, "Blokovi", 10, "Dovedi robota do cilja.", "Skretanje", "RobotMaze" },
                    { 42, "5x3", "{\"width\":5,\"height\":3,\"walls\":[[2,0],[2,1]],\"start\":{\"x\":0,\"y\":1,\"dir\":\"right\"},\"goal\":{\"x\":4,\"y\":1},\"stars\":[],\"allowedBlocks\":[\"forward\",\"turnLeft\",\"turnRight\"],\"maxBlocks\":14,\"maxSteps\":30,\"optimalBlocks\":9}", "simulacija", 2, "Srednji stupac je zatvoren osim donjeg reda. Robot mora sići, proći ispod zida i popeti se do cilja.", true, "Blokovi", 20, "Dovedi robota do cilja.", "Zaobilaženje zida", "RobotMaze" },
                    { 43, "4x1", "{\"width\":4,\"height\":1,\"walls\":[],\"start\":{\"x\":0,\"y\":0,\"dir\":\"right\"},\"goal\":{\"x\":3,\"y\":0},\"stars\":[],\"allowedBlocks\":[\"forward\",\"repeat\"],\"maxBlocks\":2,\"maxSteps\":10,\"optimalBlocks\":2}", "simulacija", 2, "Do cilja trebaju tri koraka naprijed, a smiješ samo dva bloka. Ponovi jedan korak tri puta.", true, "Blokovi", 20, "Dovedi robota do cilja.", "Prva petlja", "RobotMaze" },
                    { 44, "4x3", "{\"width\":4,\"height\":3,\"walls\":[],\"start\":{\"x\":0,\"y\":0,\"dir\":\"right\"},\"goal\":{\"x\":0,\"y\":2},\"stars\":[],\"allowedBlocks\":[\"forward\",\"turnLeft\",\"turnRight\",\"repeat\"],\"maxBlocks\":12,\"maxSteps\":30,\"optimalBlocks\":8}", "simulacija", 3, "Tri naprijed, skretanje, dva naprijed, skretanje, tri naprijed. Ponavljanja skraćuju program.", true, "Blokovi", 30, "Dovedi robota do cilja.", "Petlja sa skretanjem", "RobotMaze" },
                    { 45, "5x3", "{\"width\":5,\"height\":3,\"walls\":[[2,0],[2,1]],\"start\":{\"x\":0,\"y\":1,\"dir\":\"right\"},\"goal\":{\"x\":4,\"y\":1},\"stars\":[[1,2],[3,2]],\"allowedBlocks\":[\"forward\",\"turnLeft\",\"turnRight\",\"repeat\"],\"maxBlocks\":12,\"maxSteps\":25,\"optimalBlocks\":7}", "simulacija", 3, "Zid tjera robota u donji red. Obje zvjezdice leže na tom putu, a petlja drži program kratkim.", true, "Blokovi", 30, "Dovedi robota do cilja.", "Zvjezdice na putu", "RobotMaze" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 45);
        }
    }
}
