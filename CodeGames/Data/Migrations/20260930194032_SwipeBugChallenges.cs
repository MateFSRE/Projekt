using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CodeGames.Data.Migrations
{
    /// <inheritdoc />
    public partial class SwipeBugChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfigJson",
                table: "Challenges",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConfigJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConfigJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConfigJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 4,
                column: "ConfigJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 5,
                column: "ConfigJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 6,
                column: "ConfigJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 7,
                column: "ConfigJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 8,
                column: "ConfigJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 9,
                column: "ConfigJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 10,
                column: "ConfigJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 11,
                column: "ConfigJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 12,
                column: "ConfigJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 13,
                column: "ConfigJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 14,
                column: "ConfigJson",
                value: null);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 15,
                column: "ConfigJson",
                value: null);

            migrationBuilder.InsertData(
                table: "Challenges",
                columns: new[] { "Id", "CodeSnippet", "ConfigJson", "CorrectAnswer", "Difficulty", "Explanation", "IsActive", "Language", "Points", "Question", "Title", "Type" },
                values: new object[,]
                {
                    { 16, "broj = 4\nif broj = 4:\n    print(\"da\")\nelse:\n    print(\"ne\")", "{\"code\":\"broj = 4\\nif broj = 4:\\n    print(\\u0022da\\u0022)\\nelse:\\n    print(\\u0022ne\\u0022)\",\"language\":\"Python\",\"hasBug\":true,\"bugLine\":2}", "true", 1, "U uvjetu stoji jedan znak =, a to je dodjela. Usporedba u Pythonu piše se s ==.", true, "Python", 10, "Ima li ovaj kod bug?", "Dodjela umjesto usporedbe", "SwipeBug" },
                    { 17, "brojevi = [1, 2, 3]\nzbroj = 0\nfor broj in brojevi:\n    zbroj = zbroj + broj\nprint(brojevi[3])", "{\"code\":\"brojevi = [1, 2, 3]\\nzbroj = 0\\nfor broj in brojevi:\\n    zbroj = zbroj \\u002B broj\\nprint(brojevi[3])\",\"language\":\"Python\",\"hasBug\":true,\"bugLine\":5}", "true", 1, "Lista ima indekse 0, 1 i 2. Indeks 3 ne postoji, pa zadnja linija puca.", true, "Python", 10, "Ima li ovaj kod bug?", "Indeks izvan liste", "SwipeBug" },
                    { 18, "bodovi = [2, 4, 6]\nzbroj = 0\nfor bod in bodovi:\n    zbroj = zbroj + bod\nprint(zbroj)", "{\"code\":\"bodovi = [2, 4, 6]\\nzbroj = 0\\nfor bod in bodovi:\\n    zbroj = zbroj \\u002B bod\\nprint(zbroj)\",\"language\":\"Python\",\"hasBug\":false,\"bugLine\":null}", "false", 1, "Petlja prolazi kroz svaki broj i zbraja ga. Indeksi su ispravni i nema sintaksne greške.", true, "Python", 10, "Ima li ovaj kod bug?", "Zbroj bodova", "SwipeBug" },
                    { 19, "def kvadrat(n):\n    return n * n\nrezultat = kvadrat(3)\nprint(rezultat)", "{\"code\":\"def kvadrat(n):\\n    return n * n\\nrezultat = kvadrat(3)\\nprint(rezultat)\",\"language\":\"Python\",\"hasBug\":false,\"bugLine\":null}", "false", 1, "Funkcija vraća n * n, poziv kvadrat(3) daje 9, a print ispisuje taj rezultat.", true, "Python", 10, "Ima li ovaj kod bug?", "Kvadrat broja", "SwipeBug" },
                    { 20, "const n = 5;\nlet poruka = \"start\";\nn = n + 1;\nporuka = poruka + n;\nconsole.log(poruka);", "{\"code\":\"const n = 5;\\nlet poruka = \\u0022start\\u0022;\\nn = n \\u002B 1;\\nporuka = poruka \\u002B n;\\nconsole.log(poruka);\",\"language\":\"JavaScript\",\"hasBug\":true,\"bugLine\":3}", "true", 1, "n je deklariran s const. Treća linija ga pokušava ponovo dodijeliti, a to JavaScript ne dozvoljava.", true, "JavaScript", 10, "Ima li ovaj kod bug?", "Ponovna dodjela const", "SwipeBug" },
                    { 21, "let bod = 10;\nif (bod = 10) {\n    console.log(\"prolaz\");\n} else {\n    console.log(\"pad\");\n}", "{\"code\":\"let bod = 10;\\nif (bod = 10) {\\n    console.log(\\u0022prolaz\\u0022);\\n} else {\\n    console.log(\\u0022pad\\u0022);\\n}\",\"language\":\"JavaScript\",\"hasBug\":true,\"bugLine\":2}", "true", 1, "U uvjetu je dodjela =, ne usporedba. Uvjet treba biti bod === 10.", true, "JavaScript", 10, "Ima li ovaj kod bug?", "Dodjela u uvjetu", "SwipeBug" },
                    { 22, "let a = 2;\nlet b = 3;\nlet zbroj = a + b;\nconsole.log(zbroj);", "{\"code\":\"let a = 2;\\nlet b = 3;\\nlet zbroj = a \\u002B b;\\nconsole.log(zbroj);\",\"language\":\"JavaScript\",\"hasBug\":false,\"bugLine\":null}", "false", 1, "a i b su brojevi, + ih zbraja, a console.log ispisuje zbroj. Nema greške.", true, "JavaScript", 10, "Ima li ovaj kod bug?", "Zbroj dva broja", "SwipeBug" },
                    { 23, "function duplo(n) {\n    return n * 2;\n}\nlet x = duplo(4);\nconsole.log(x);", "{\"code\":\"function duplo(n) {\\n    return n * 2;\\n}\\nlet x = duplo(4);\\nconsole.log(x);\",\"language\":\"JavaScript\",\"hasBug\":false,\"bugLine\":null}", "false", 1, "Funkcija vraća n * 2. duplo(4) je 8 i console.log ispisuje tu vrijednost.", true, "JavaScript", 10, "Ima li ovaj kod bug?", "Dupliranje broja", "SwipeBug" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DropColumn(
                name: "ConfigJson",
                table: "Challenges");
        }
    }
}
