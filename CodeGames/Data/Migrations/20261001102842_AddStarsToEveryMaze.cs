using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeGames.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStarsToEveryMaze : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "ConfigJson", "Explanation" },
                values: new object[] { "{\"width\":4,\"height\":1,\"walls\":[],\"start\":{\"x\":0,\"y\":0,\"dir\":\"right\"},\"goal\":{\"x\":3,\"y\":0},\"stars\":[[1,0]],\"allowedBlocks\":[\"forward\"],\"maxBlocks\":4,\"maxSteps\":8,\"optimalBlocks\":3}", "Tri puta naprijed vode robota od lijevog ruba do cilja. Zvjezdica je na tom putu i donosi dodatne bodove." });

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ConfigJson", "Explanation" },
                values: new object[] { "{\"width\":3,\"height\":3,\"walls\":[],\"start\":{\"x\":0,\"y\":0,\"dir\":\"right\"},\"goal\":{\"x\":2,\"y\":2},\"stars\":[[1,0],[2,1]],\"allowedBlocks\":[\"forward\",\"turnLeft\",\"turnRight\"],\"maxBlocks\":8,\"maxSteps\":20,\"optimalBlocks\":5}", "Idi desno do kraja reda, skreni desno i spusti se do donjeg desnog polja. Obje zvjezdice leže na tom putu." });

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ConfigJson", "Explanation" },
                values: new object[] { "{\"width\":5,\"height\":3,\"walls\":[[2,0],[2,1]],\"start\":{\"x\":0,\"y\":1,\"dir\":\"right\"},\"goal\":{\"x\":4,\"y\":1},\"stars\":[[1,2],[3,2]],\"allowedBlocks\":[\"forward\",\"turnLeft\",\"turnRight\"],\"maxBlocks\":14,\"maxSteps\":30,\"optimalBlocks\":9}", "Srednji stupac je zatvoren osim donjeg reda. Robot mora sići, proći ispod zida i popeti se do cilja. Zvjezdice su u donjem redu." });

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ConfigJson", "Explanation" },
                values: new object[] { "{\"width\":4,\"height\":1,\"walls\":[],\"start\":{\"x\":0,\"y\":0,\"dir\":\"right\"},\"goal\":{\"x\":3,\"y\":0},\"stars\":[[2,0]],\"allowedBlocks\":[\"forward\",\"repeat\"],\"maxBlocks\":2,\"maxSteps\":10,\"optimalBlocks\":2}", "Do cilja trebaju tri koraka naprijed, a smiješ samo dva bloka. Ponovi jedan korak tri puta. Zvjezdica je na sredini puta." });

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "ConfigJson", "Explanation" },
                values: new object[] { "{\"width\":4,\"height\":3,\"walls\":[],\"start\":{\"x\":0,\"y\":0,\"dir\":\"right\"},\"goal\":{\"x\":0,\"y\":2},\"stars\":[[2,0],[1,2]],\"allowedBlocks\":[\"forward\",\"turnLeft\",\"turnRight\",\"repeat\"],\"maxBlocks\":12,\"maxSteps\":30,\"optimalBlocks\":8}", "Tri naprijed, skretanje, dva naprijed, skretanje, tri naprijed. Dvije zvjezdice leže na tom krugu." });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "ConfigJson", "Explanation" },
                values: new object[] { "{\"width\":4,\"height\":1,\"walls\":[],\"start\":{\"x\":0,\"y\":0,\"dir\":\"right\"},\"goal\":{\"x\":3,\"y\":0},\"stars\":[],\"allowedBlocks\":[\"forward\"],\"maxBlocks\":4,\"maxSteps\":8,\"optimalBlocks\":3}", "Tri puta naprijed vode robota od lijevog ruba do cilja." });

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ConfigJson", "Explanation" },
                values: new object[] { "{\"width\":3,\"height\":3,\"walls\":[],\"start\":{\"x\":0,\"y\":0,\"dir\":\"right\"},\"goal\":{\"x\":2,\"y\":2},\"stars\":[],\"allowedBlocks\":[\"forward\",\"turnLeft\",\"turnRight\"],\"maxBlocks\":8,\"maxSteps\":20,\"optimalBlocks\":5}", "Idi desno do kraja reda, skreni desno i spusti se do donjeg desnog polja." });

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ConfigJson", "Explanation" },
                values: new object[] { "{\"width\":5,\"height\":3,\"walls\":[[2,0],[2,1]],\"start\":{\"x\":0,\"y\":1,\"dir\":\"right\"},\"goal\":{\"x\":4,\"y\":1},\"stars\":[],\"allowedBlocks\":[\"forward\",\"turnLeft\",\"turnRight\"],\"maxBlocks\":14,\"maxSteps\":30,\"optimalBlocks\":9}", "Srednji stupac je zatvoren osim donjeg reda. Robot mora sići, proći ispod zida i popeti se do cilja." });

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ConfigJson", "Explanation" },
                values: new object[] { "{\"width\":4,\"height\":1,\"walls\":[],\"start\":{\"x\":0,\"y\":0,\"dir\":\"right\"},\"goal\":{\"x\":3,\"y\":0},\"stars\":[],\"allowedBlocks\":[\"forward\",\"repeat\"],\"maxBlocks\":2,\"maxSteps\":10,\"optimalBlocks\":2}", "Do cilja trebaju tri koraka naprijed, a smiješ samo dva bloka. Ponovi jedan korak tri puta." });

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "ConfigJson", "Explanation" },
                values: new object[] { "{\"width\":4,\"height\":3,\"walls\":[],\"start\":{\"x\":0,\"y\":0,\"dir\":\"right\"},\"goal\":{\"x\":0,\"y\":2},\"stars\":[],\"allowedBlocks\":[\"forward\",\"turnLeft\",\"turnRight\",\"repeat\"],\"maxBlocks\":12,\"maxSteps\":30,\"optimalBlocks\":8}", "Tri naprijed, skretanje, dva naprijed, skretanje, tri naprijed. Ponavljanja skraćuju program." });
        }
    }
}
