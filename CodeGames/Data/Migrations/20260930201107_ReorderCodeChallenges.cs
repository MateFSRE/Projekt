using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CodeGames.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReorderCodeChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Challenges",
                columns: new[] { "Id", "CodeSnippet", "ConfigJson", "CorrectAnswer", "Difficulty", "Explanation", "IsActive", "Language", "Points", "Question", "Title", "Type" },
                values: new object[,]
                {
                    { 30, "brojevi = [1, 2, 3]\nfor broj in brojevi:\n    print(broj)", "{\"language\":\"Python\",\"goal\":\"Ispi\\u0161i brojeve od 1 do 3.\",\"lines\":[{\"id\":\"1\",\"text\":\"brojevi = [1, 2, 3]\",\"indent\":0},{\"id\":\"2\",\"text\":\"for broj in brojevi:\",\"indent\":0},{\"id\":\"3\",\"text\":\"print(broj)\",\"indent\":4}],\"correctOrders\":[[\"1\",\"2\",\"3\"]]}", "1,2,3", 1, "Lista se prvo stvori. Petlja ide preko nje, a print je uvučen jer pripada tijelu petlje.", true, "Python", 10, "Ispiši brojeve od 1 do 3.", "Brojevi od 1 do 3", "ReorderCode" },
                    { 31, "const a = 2;\nconst b = 3;\nconst zbroj = a + b;\nconsole.log(zbroj);", "{\"language\":\"JavaScript\",\"goal\":\"Ispi\\u0161i zbroj dva broja.\",\"lines\":[{\"id\":\"1\",\"text\":\"const a = 2;\",\"indent\":0},{\"id\":\"2\",\"text\":\"const b = 3;\",\"indent\":0},{\"id\":\"3\",\"text\":\"const zbroj = a \\u002B b;\",\"indent\":0},{\"id\":\"4\",\"text\":\"console.log(zbroj);\",\"indent\":0}],\"correctOrders\":[[\"1\",\"2\",\"3\",\"4\"],[\"2\",\"1\",\"3\",\"4\"]]}", "1,2,3,4", 1, "a i b smiju zamijeniti mjesta. Zbroj dolazi tek kad oba postoje, a ispis je zadnji.", true, "JavaScript", 10, "Ispiši zbroj dva broja.", "Zbroj dva broja", "ReorderCode" },
                    { 32, "bod = 10\nif bod >= 10:\n    print(\"prolaz\")\nelse:\n    print(\"pad\")", "{\"language\":\"Python\",\"goal\":\"Ispi\\u0161i prolaz ako je bod barem 10, ina\\u010De pad.\",\"lines\":[{\"id\":\"1\",\"text\":\"bod = 10\",\"indent\":0},{\"id\":\"2\",\"text\":\"if bod \\u003E= 10:\",\"indent\":0},{\"id\":\"3\",\"text\":\"print(\\u0022prolaz\\u0022)\",\"indent\":4},{\"id\":\"4\",\"text\":\"else:\",\"indent\":0},{\"id\":\"5\",\"text\":\"print(\\u0022pad\\u0022)\",\"indent\":4}],\"correctOrders\":[[\"1\",\"2\",\"3\",\"4\",\"5\"]]}", "1,2,3,4,5", 2, "bod mora postojati prije uvjeta. print u grani if i print u grani else uvučeni su za četiri razmaka.", true, "Python", 20, "Ispiši prolaz ako je bod barem 10, inače pad.", "Prolaz ili pad", "ReorderCode" },
                    { 33, "const brojevi = [1, 2, 3];\nfor (const broj of brojevi) {\n    console.log(broj);\n}\nconst ukupno = brojevi.length;\nconsole.log(ukupno);", "{\"language\":\"JavaScript\",\"goal\":\"Ispi\\u0161i svaki broj iz niza i koliko ih ima.\",\"lines\":[{\"id\":\"1\",\"text\":\"const brojevi = [1, 2, 3];\",\"indent\":0},{\"id\":\"2\",\"text\":\"for (const broj of brojevi) {\",\"indent\":0},{\"id\":\"3\",\"text\":\"console.log(broj);\",\"indent\":4},{\"id\":\"4\",\"text\":\"}\",\"indent\":0},{\"id\":\"5\",\"text\":\"const ukupno = brojevi.length;\",\"indent\":0},{\"id\":\"6\",\"text\":\"console.log(ukupno);\",\"indent\":0}],\"correctOrders\":[[\"1\",\"2\",\"3\",\"4\",\"5\",\"6\"],[\"1\",\"5\",\"2\",\"3\",\"4\",\"6\"]]}", "1,2,3,4,5,6", 2, "Duljina niza smije se uzeti prije petlje ili poslije nje. Ispis ukupno mora ostati na kraju.", true, "JavaScript", 20, "Ispiši svaki broj iz niza i koliko ih ima.", "Brojevi i duljina", "ReorderCode" },
                    { 34, "def kvadrat(n):\n    rezultat = n * n\n    return rezultat\nbroj = 4\nispis = kvadrat(broj)\nprint(\"Rezultat:\")\nprint(ispis)", "{\"language\":\"Python\",\"goal\":\"Definiraj kvadrat i ispi\\u0161i rezultat za broj 4.\",\"lines\":[{\"id\":\"1\",\"text\":\"def kvadrat(n):\",\"indent\":0},{\"id\":\"2\",\"text\":\"rezultat = n * n\",\"indent\":4},{\"id\":\"3\",\"text\":\"return rezultat\",\"indent\":4},{\"id\":\"4\",\"text\":\"broj = 4\",\"indent\":0},{\"id\":\"5\",\"text\":\"ispis = kvadrat(broj)\",\"indent\":0},{\"id\":\"6\",\"text\":\"print(\\u0022Rezultat:\\u0022)\",\"indent\":0},{\"id\":\"7\",\"text\":\"print(ispis)\",\"indent\":0}],\"correctOrders\":[[\"1\",\"2\",\"3\",\"4\",\"5\",\"6\",\"7\"]]}", "1,2,3,4,5,6,7", 3, "Tijelo funkcije je uvučeno. Poziv dolazi nakon definicije i nakon što broj postoji. Natpis ide prije vrijednosti.", true, "Python", 30, "Definiraj kvadrat i ispiši rezultat za broj 4.", "Kvadrat broja", "ReorderCode" },
                    { 35, "const brojevi = [1, 2, 3, 4, 5];\nlet zbroj = 0;\nfor (const broj of brojevi) {\n    if (broj % 2 === 0) {\n        zbroj = zbroj + broj;\n    }\n}\nconsole.log(zbroj);", "{\"language\":\"JavaScript\",\"goal\":\"Zbroji parne brojeve i ispi\\u0161i zbroj.\",\"lines\":[{\"id\":\"1\",\"text\":\"const brojevi = [1, 2, 3, 4, 5];\",\"indent\":0},{\"id\":\"2\",\"text\":\"let zbroj = 0;\",\"indent\":0},{\"id\":\"3\",\"text\":\"for (const broj of brojevi) {\",\"indent\":0},{\"id\":\"4\",\"text\":\"if (broj % 2 === 0) {\",\"indent\":4},{\"id\":\"5\",\"text\":\"zbroj = zbroj \\u002B broj;\",\"indent\":8},{\"id\":\"6\",\"text\":\"}\",\"indent\":4},{\"id\":\"7\",\"text\":\"}\",\"indent\":0},{\"id\":\"8\",\"text\":\"console.log(zbroj);\",\"indent\":0}],\"correctOrders\":[[\"1\",\"2\",\"3\",\"4\",\"5\",\"6\",\"7\",\"8\"],[\"2\",\"1\",\"3\",\"4\",\"5\",\"6\",\"7\",\"8\"]]}", "1,2,3,4,5,6,7,8", 3, "Niz i početni zbroj smiju zamijeniti mjesta. Petlja mora doći nakon oba, a ispis nakon petlje. Uvlake prate if unutar for.", true, "JavaScript", 30, "Zbroji parne brojeve i ispiši zbroj.", "Zbroj parnih", "ReorderCode" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 35);
        }
    }
}
