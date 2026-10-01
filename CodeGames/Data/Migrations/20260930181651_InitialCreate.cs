using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CodeGames.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Challenges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Difficulty = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CodeSnippet = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Question = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenges", x => x.Id);
                    table.CheckConstraint("CK_Challenge_Difficulty", "[Difficulty] >= 1 AND [Difficulty] <= 3");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Xp = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    CurrentStreak = table.Column<int>(type: "int", nullable: false),
                    BestStreak = table.Column<int>(type: "int", nullable: false),
                    LastActiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChallengeId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeOptions_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ChallengeId = table.Column<int>(type: "int", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    PointsAwarded = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attempts_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Attempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Challenges",
                columns: new[] { "Id", "CodeSnippet", "CorrectAnswer", "Difficulty", "Explanation", "IsActive", "Language", "Points", "Question", "Title", "Type" },
                values: new object[,]
                {
                    { 1, "x = 2\ny = 3\nprint(x + y)", "5", 1, "Varijable x i y imaju vrijednosti 2 i 3. print ispisuje njihov zbroj, a to je 5.", true, "Python", 10, "Što program ispisuje?", "Zbroj dva broja", "PredictOutput" },
                    { 2, "ime = \"Ana\"\nprint(ime * 2)", "AnaAna", 2, "Operator * uz tekst ponavlja taj tekst onoliko puta koliki je broj. \"Ana\" * 2 daje AnaAna.", true, "Python", 20, "Što program ispisuje?", "Ponavljanje teksta", "PredictOutput" },
                    { 3, "brojevi = [1, 2, 3]\nprint(brojevi[0] + brojevi[-1])", "4", 3, "Indeks 0 je prvi element (1), a indeks -1 je zadnji (3). Njihov zbroj je 4.", true, "Python", 30, "Što program ispisuje?", "Prvi i zadnji element", "PredictOutput" },
                    { 4, "print(len(\"kod\"))", "3", 1, "len broji znakove. Riječ \"kod\" ima tri znaka, pa je ispis 3.", true, "Python", 10, "Što se ispisuje?", "Duljina teksta", "MultipleChoice" },
                    { 5, "def kvadrat(n):\n    return n * n", "16", 2, "Funkcija množi broj samim sobom. 4 * 4 je 16.", true, "Python", 20, "Što vraća poziv kvadrat(4)?", "Kvadrat broja", "MultipleChoice" },
                    { 6, "a = [1, 2]\nb = a\nb.append(3)", "[1, 2, 3]", 3, "b = a ne kopira listu. Oba imena pokazuju na istu listu, pa append mijenja i a.", true, "Python", 30, "Što lista a sadrži nakon append?", "Ista lista pod dva imena", "MultipleChoice" },
                    { 7, "x = 10\nif x = 10:\n    print(\"da\")", "if x == 10:", 1, "Jedan znak = je dodjela i ne smije stajati u uvjetu. Usporedba jednakosti u Pythonu piše se s ==.", true, "Python", 10, "Koji redak ispravlja grešku u uvjetu?", "Usporedba u uvjetu", "FixBug" },
                    { 8, "brojevi = [1, 2, 3]\nprint(brojevi[3])", "print(brojevi[2])", 2, "Lista ima indekse 0, 1 i 2. Indeks 3 ne postoji. Zadnji element je na indeksu 2.", true, "Python", 20, "Koji redak ispisuje zadnji element bez greške?", "Indeks izvan liste", "FixBug" },
                    { 9, "def zbroj(a, b)\n    return a + b", "def zbroj(a, b):", 3, "Zaglavlje funkcije u Pythonu mora završiti dvotočkom. Bez nje interpreter prijavi sintaksnu grešku.", true, "Python", 30, "Koji redak ispravno započinje funkciju?", "Dvotočka u funkciji", "FixBug" },
                    { 10, "let a = 2;\nlet b = 3;\nconsole.log(a + b);", "5", 1, "a i b su brojevi. Operator + ih zbraja, a console.log ispisuje 5.", true, "JavaScript", 10, "Što program ispisuje?", "Zbroj u konzoli", "PredictOutput" },
                    { 11, "console.log(\"5\" + 1);", "51", 2, "Kad je jedan operand tekst, + ne zbraja nego spaja znakove. Broj 1 postaje znak \"1\", pa je rezultat 51.", true, "JavaScript", 20, "Što program ispisuje?", "Spajanje teksta i broja", "PredictOutput" },
                    { 12, "let x = 10;", "number", 1, "U JavaScriptu su cijeli i decimalni brojevi tipa number. typeof za 10 vraća \"number\".", true, "JavaScript", 10, "Što vraća typeof x?", "Tip broja", "MultipleChoice" },
                    { 13, "let a = [1, 2];\nlet b = a;\nb.push(3);", "[1, 2, 3]", 3, "b = a ne stvara novi niz. push dodaje 3 u isti niz, pa se promjena vidi i kroz a.", true, "JavaScript", 30, "Što niz a sadrži nakon push?", "Isti niz pod dva imena", "MultipleChoice" },
                    { 14, "let ime = \"Ana\";\nconsole.log(\"Zdravo \" + Ime);", "console.log(\"Zdravo \" + ime);", 2, "JavaScript razlikuje velika i mala slova. Varijabla se zove ime, a Ime nije definirana.", true, "JavaScript", 20, "Koji redak ispravlja grešku u imenu varijable?", "Velika i mala slova", "FixBug" },
                    { 15, "const n = 5;\nn = 6;\nconsole.log(n);", "let n = 5;", 3, "const se ne smije ponovno dodijeliti. let drži broj koji se kasnije može promijeniti.", true, "JavaScript", 30, "Koja izmjena dozvoljava novu vrijednost n?", "Ponovna dodjela const", "FixBug" }
                });

            migrationBuilder.InsertData(
                table: "ChallengeOptions",
                columns: new[] { "Id", "ChallengeId", "IsCorrect", "Text" },
                values: new object[,]
                {
                    { 1, 4, true, "3" },
                    { 2, 4, false, "2" },
                    { 3, 4, false, "4" },
                    { 4, 4, false, "kod" },
                    { 5, 5, false, "8" },
                    { 6, 5, true, "16" },
                    { 7, 5, false, "4" },
                    { 8, 5, false, "44" },
                    { 9, 6, false, "[1, 2]" },
                    { 10, 6, true, "[1, 2, 3]" },
                    { 11, 6, false, "[3]" },
                    { 12, 6, false, "[1, 2, 3, 3]" },
                    { 13, 7, true, "if x == 10:" },
                    { 14, 7, false, "if x = 10:" },
                    { 15, 7, false, "if x === 10:" },
                    { 16, 7, false, "if (x = 10):" },
                    { 17, 8, true, "print(brojevi[2])" },
                    { 18, 8, false, "print(brojevi[3])" },
                    { 19, 8, false, "print(brojevi(2))" },
                    { 20, 8, false, "print(brojevi.2)" },
                    { 21, 9, true, "def zbroj(a, b):" },
                    { 22, 9, false, "def zbroj(a, b)" },
                    { 23, 9, false, "function zbroj(a, b):" },
                    { 24, 9, false, "def zbroj(a b):" },
                    { 25, 12, true, "number" },
                    { 26, 12, false, "string" },
                    { 27, 12, false, "int" },
                    { 28, 12, false, "integer" },
                    { 29, 13, false, "[1, 2]" },
                    { 30, 13, true, "[1, 2, 3]" },
                    { 31, 13, false, "[3]" },
                    { 32, 13, false, "undefined" },
                    { 33, 14, true, "console.log(\"Zdravo \" + ime);" },
                    { 34, 14, false, "console.log(\"Zdravo \" + Ime);" },
                    { 35, 14, false, "console.log(\"Zdravo \" + name);" },
                    { 36, 14, false, "print(\"Zdravo \" + ime);" },
                    { 37, 15, true, "let n = 5;" },
                    { 38, 15, false, "const n = 5;" },
                    { 39, 15, false, "var const n = 5;" },
                    { 40, 15, false, "int n = 5;" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attempts_ChallengeId",
                table: "Attempts",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_Attempts_UserId",
                table: "Attempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeOptions_ChallengeId",
                table: "ChallengeOptions",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Xp",
                table: "Users",
                column: "Xp",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attempts");

            migrationBuilder.DropTable(
                name: "ChallengeOptions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Challenges");
        }
    }
}
