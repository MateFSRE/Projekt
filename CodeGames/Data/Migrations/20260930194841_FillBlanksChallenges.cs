using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CodeGames.Data.Migrations
{
    /// <inheritdoc />
    public partial class FillBlanksChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Challenges",
                columns: new[] { "Id", "CodeSnippet", "ConfigJson", "CorrectAnswer", "Difficulty", "Explanation", "IsActive", "Language", "Points", "Question", "Title", "Type" },
                values: new object[,]
                {
                    { 24, "brojevi = [1, 2, 3]\n{{1}} broj {{2}} brojevi:\n    print(broj)", "{\"language\":\"Python\",\"template\":\"brojevi = [1, 2, 3]\\n{{1}} broj {{2}} brojevi:\\n    print(broj)\",\"blanks\":[{\"id\":\"1\",\"answer\":\"for\"},{\"id\":\"2\",\"answer\":\"in\"}],\"wordBank\":[\"for\",\"in\",\"while\",\"if\",\"def\"]}", "for, in", 2, "for prolazi kroz listu, a in veže svaki element na ime broj.", true, "Python", 20, "Dopuni rupe.", "Petlja kroz listu", "FillBlanks" },
                    { 25, "bod = 10\n{{1}} bod >= 10:\n    print(\"prolaz\")\n{{2}}:\n    print(\"pad\")", "{\"language\":\"Python\",\"template\":\"bod = 10\\n{{1}} bod \\u003E= 10:\\n    print(\\u0022prolaz\\u0022)\\n{{2}}:\\n    print(\\u0022pad\\u0022)\",\"blanks\":[{\"id\":\"1\",\"answer\":\"if\"},{\"id\":\"2\",\"answer\":\"else\"}],\"wordBank\":[\"if\",\"else\",\"elif\",\"for\",\"return\"]}", "if, else", 2, "if pokreće granu kad je uvjet točan. else hvata sve ostalo.", true, "Python", 20, "Dopuni rupe.", "Uvjet prolaza", "FillBlanks" },
                    { 26, "{{1}} kvadrat(n):\n    {{2}} n * n", "{\"language\":\"Python\",\"template\":\"{{1}} kvadrat(n):\\n    {{2}} n * n\",\"blanks\":[{\"id\":\"1\",\"answer\":\"def\"},{\"id\":\"2\",\"answer\":\"return\"}],\"wordBank\":[\"def\",\"return\",\"function\",\"print\",\"if\"]}", "def, return", 2, "def započinje funkciju. return šalje rezultat n * n pozivatelju.", true, "Python", 20, "Dopuni rupe.", "Funkcija kvadrata", "FillBlanks" },
                    { 27, "imena = [\"Ana\", \"Leo\"]\nimena.{{1}}(\"Mia\")\nprint(imena[{{2}}])", "{\"language\":\"Python\",\"template\":\"imena = [\\u0022Ana\\u0022, \\u0022Leo\\u0022]\\nimena.{{1}}(\\u0022Mia\\u0022)\\nprint(imena[{{2}}])\",\"blanks\":[{\"id\":\"1\",\"answer\":\"append\"},{\"id\":\"2\",\"answer\":\"0\"}],\"wordBank\":[\"append\",\"0\",\"push\",\"1\",\"pop\"]}", "append, 0", 2, "append dodaje na kraj liste. Prvi element i dalje je na indeksu 0.", true, "Python", 20, "Dopuni rupe.", "Dodavanje u listu", "FillBlanks" },
                    { 28, "const brojevi = [1, 2, 3];\n{{1}} (const broj {{2}} brojevi) {\n    console.log(broj);\n}", "{\"language\":\"JavaScript\",\"template\":\"const brojevi = [1, 2, 3];\\n{{1}} (const broj {{2}} brojevi) {\\n    console.log(broj);\\n}\",\"blanks\":[{\"id\":\"1\",\"answer\":\"for\"},{\"id\":\"2\",\"answer\":\"of\"}],\"wordBank\":[\"for\",\"of\",\"while\",\"in\",\"if\"]}", "for, of", 2, "for ... of prolazi kroz vrijednosti niza. of daje element, ne indeks.", true, "JavaScript", 20, "Dopuni rupe.", "Petlja po nizu", "FillBlanks" },
                    { 29, "{{1}} zbroj(a, b) {\n    {{2}} a + b;\n}\nconst brojevi = [1, 2];\nbrojevi.{{3}}(3);", "{\"language\":\"JavaScript\",\"template\":\"{{1}} zbroj(a, b) {\\n    {{2}} a \\u002B b;\\n}\\nconst brojevi = [1, 2];\\nbrojevi.{{3}}(3);\",\"blanks\":[{\"id\":\"1\",\"answer\":\"function\"},{\"id\":\"2\",\"answer\":\"return\"},{\"id\":\"3\",\"answer\":\"push\"}],\"wordBank\":[\"function\",\"return\",\"push\",\"def\",\"append\",\"for\"]}", "function, return, push", 2, "function i return definiraju zbroj. push dodaje 3 na kraj niza.", true, "JavaScript", 20, "Dopuni rupe.", "Funkcija i niz", "FillBlanks" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 29);
        }
    }
}
