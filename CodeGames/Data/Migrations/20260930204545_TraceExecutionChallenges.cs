using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CodeGames.Data.Migrations
{
    /// <inheritdoc />
    public partial class TraceExecutionChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Challenges",
                columns: new[] { "Id", "CodeSnippet", "ConfigJson", "CorrectAnswer", "Difficulty", "Explanation", "IsActive", "Language", "Points", "Question", "Title", "Type" },
                values: new object[,]
                {
                    { 36, "x = 0\ni = 1\nwhile i <= 2:\n    x = x + i\n    i = i + 1\nprint(x)", "{\"language\":\"Python\",\"code\":\"x = 0\\ni = 1\\nwhile i \\u003C= 2:\\n    x = x \\u002B i\\n    i = i \\u002B 1\\nprint(x)\",\"variables\":[\"x\",\"i\"],\"steps\":[{\"line\":1,\"expected\":{\"x\":\"0\",\"i\":\"\"}},{\"line\":2,\"expected\":{\"x\":\"0\",\"i\":\"1\"}},{\"line\":3,\"expected\":{\"x\":\"0\",\"i\":\"1\"}},{\"line\":4,\"expected\":{\"x\":\"1\",\"i\":\"1\"}},{\"line\":5,\"expected\":{\"x\":\"1\",\"i\":\"2\"}},{\"line\":3,\"expected\":{\"x\":\"1\",\"i\":\"2\"}},{\"line\":4,\"expected\":{\"x\":\"3\",\"i\":\"2\"}},{\"line\":5,\"expected\":{\"x\":\"3\",\"i\":\"3\"}},{\"line\":3,\"expected\":{\"x\":\"3\",\"i\":\"3\"}},{\"line\":6,\"expected\":{\"x\":\"3\",\"i\":\"3\"}}],\"askAt\":[3,6,7]}", "x=1, i=1; x=3, i=2; x=3, i=3", 3, "Petlja se vrti dok je i manji ili jednak 2. x raste za trenutni i, zatim se i poveća za 1. Na kraju je x 3, a i 3.", true, "Python", 30, "Upiši vrijednosti varijabli na traženim koracima.", "Brojač u petlji", "TraceExecution" },
                    { 37, "a = 2\nb = 5\ntemp = a\na = b\nb = temp", "{\"language\":\"Python\",\"code\":\"a = 2\\nb = 5\\ntemp = a\\na = b\\nb = temp\",\"variables\":[\"a\",\"b\",\"temp\"],\"steps\":[{\"line\":1,\"expected\":{\"a\":\"2\",\"b\":\"\",\"temp\":\"\"}},{\"line\":2,\"expected\":{\"a\":\"2\",\"b\":\"5\",\"temp\":\"\"}},{\"line\":3,\"expected\":{\"a\":\"2\",\"b\":\"5\",\"temp\":\"2\"}},{\"line\":4,\"expected\":{\"a\":\"5\",\"b\":\"5\",\"temp\":\"2\"}},{\"line\":5,\"expected\":{\"a\":\"5\",\"b\":\"2\",\"temp\":\"2\"}}],\"askAt\":[2,3,4]}", "a=2, b=5, temp=2; a=5, b=5, temp=2; a=5, b=2, temp=2", 3, "temp čuva staru vrijednost a. Zatim a preuzima b, a b vraća spremljenu vrijednost iz temp. Na kraju je a 5, a b 2.", true, "Python", 30, "Upiši vrijednosti varijabli na traženim koracima.", "Zamjena dviju varijabli", "TraceExecution" },
                    { 38, "brojevi = [2, 4]\ns = 0\nfor broj in brojevi:\n    s = s + broj\nprint(s)", "{\"language\":\"Python\",\"code\":\"brojevi = [2, 4]\\ns = 0\\nfor broj in brojevi:\\n    s = s \\u002B broj\\nprint(s)\",\"variables\":[\"s\",\"broj\"],\"steps\":[{\"line\":1,\"expected\":{\"s\":\"\",\"broj\":\"\"}},{\"line\":2,\"expected\":{\"s\":\"0\",\"broj\":\"\"}},{\"line\":3,\"expected\":{\"s\":\"0\",\"broj\":\"2\"}},{\"line\":4,\"expected\":{\"s\":\"2\",\"broj\":\"2\"}},{\"line\":3,\"expected\":{\"s\":\"2\",\"broj\":\"4\"}},{\"line\":4,\"expected\":{\"s\":\"6\",\"broj\":\"4\"}},{\"line\":5,\"expected\":{\"s\":\"6\",\"broj\":\"4\"}}],\"askAt\":[2,3,5]}", "s=0, broj=2; s=2, broj=2; s=6, broj=4", 3, "for prvo veže broj na 2, pa s postaje 2. Drugi prolaz veže broj na 4 i s postaje 6.", true, "Python", 30, "Upiši vrijednosti varijabli na traženim koracima.", "Suma liste", "TraceExecution" },
                    { 39, "let bod = 8;\nlet ocjena = \"nista\";\nif (bod >= 10) {\n    ocjena = \"prolaz\";\n} else {\n    ocjena = \"pad\";\n}", "{\"language\":\"JavaScript\",\"code\":\"let bod = 8;\\nlet ocjena = \\u0022nista\\u0022;\\nif (bod \\u003E= 10) {\\n    ocjena = \\u0022prolaz\\u0022;\\n} else {\\n    ocjena = \\u0022pad\\u0022;\\n}\",\"variables\":[\"bod\",\"ocjena\"],\"steps\":[{\"line\":1,\"expected\":{\"bod\":\"8\",\"ocjena\":\"\"}},{\"line\":2,\"expected\":{\"bod\":\"8\",\"ocjena\":\"nista\"}},{\"line\":3,\"expected\":{\"bod\":\"8\",\"ocjena\":\"nista\"}},{\"line\":5,\"expected\":{\"bod\":\"8\",\"ocjena\":\"nista\"}},{\"line\":6,\"expected\":{\"bod\":\"8\",\"ocjena\":\"pad\"}}],\"askAt\":[1,2,4]}", "bod=8, ocjena=nista; bod=8, ocjena=nista; bod=8, ocjena=pad", 3, "bod je 8, pa uvjet bod >= 10 nije istinit. Grana if se preskače. else postavlja ocjena na pad.", true, "JavaScript", 30, "Upiši vrijednosti varijabli na traženim koracima.", "If ili else", "TraceExecution" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 39);
        }
    }
}
