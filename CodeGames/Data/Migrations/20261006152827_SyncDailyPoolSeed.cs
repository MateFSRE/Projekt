using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeGames.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncDailyPoolSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 4,
                column: "Explanation",
                value: "len broji znakove. Riječ \"kod\" ima tri znaka, pa je ispis 3.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 5,
                column: "Question",
                value: "Što vraća poziv kvadrat(4)?");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Explanation", "Question", "Title" },
                values: new object[] { "Zaglavlje funkcije u Pythonu mora završiti dvotočkom. Bez nje interpreter prijavi sintaksnu grešku.", "Koji redak ispravno započinje funkciju?", "Dvotočka u funkciji" });

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Explanation", "Question" },
                values: new object[] { "U JavaScriptu su cijeli i decimalni brojevi tipa number. typeof za 10 vraća \"number\".", "Što vraća typeof x?" });

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 19,
                column: "Explanation",
                value: "Funkcija vraća n * n, poziv kvadrat(3) daje 9, a print ispisuje taj rezultat.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 20,
                column: "Explanation",
                value: "n je deklariran s const. Treća linija ga pokušava ponovo dodijeliti, a to JavaScript ne dozvoljava.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 23,
                column: "Explanation",
                value: "Funkcija vraća n * 2. duplo(4) je 8 i console.log ispisuje tu vrijednost.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 25,
                column: "Explanation",
                value: "if pokreće granu kad je uvjet točan. else hvata sve ostalo.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 26,
                column: "Explanation",
                value: "def započinje funkciju. return šalje rezultat n * n pozivatelju.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 30,
                column: "Explanation",
                value: "Lista se prvo stvori. Petlja ide preko nje, a print je uvučen jer pripada tijelu petlje.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "Explanation", "Question" },
                values: new object[] { "bod mora postojati prije uvjeta. print u grani if i print u grani else uvučeni su za četiri razmaka.", "Ispiši prolaz ako je bod barem 10, inače pad." });

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 34,
                column: "Explanation",
                value: "Tijelo funkcije je uvučeno. Poziv dolazi nakon definicije i nakon što broj postoji. Natpis ide prije vrijednosti.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 35,
                column: "Explanation",
                value: "Niz i početni zbroj smiju zamijeniti mjesta. Petlja mora doći nakon oba, a ispis nakon petlje. Uvlake prate if unutar for.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "Explanation", "Title" },
                values: new object[] { "Petlja se vrti dok je i manji ili jednak 2. x raste za trenutni i, zatim se i poveća za 1. Na kraju je x 3, a i 3.", "Brojač u petlji" });

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 37,
                column: "Explanation",
                value: "temp čuva staru vrijednost a. Zatim a preuzima b, a b vraća spremljenu vrijednost iz temp. Na kraju je a 5, a b 2.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 39,
                column: "Explanation",
                value: "bod je 8, pa uvjet bod >= 10 nije istinit. Grana if se preskače. else postavlja ocjena na pad.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 42,
                column: "Explanation",
                value: "Srednji stupac je zatvoren osim donjeg reda. Robot mora sići, proći ispod zida i popeti se do cilja. Zvjezdice su u donjem redu.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 4,
                column: "Explanation",
                value: "len broji znakove. Rijec \"kod\" ima tri znaka, pa je ispis 3.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 5,
                column: "Question",
                value: "Što vraca poziv kvadrat(4)?");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Explanation", "Question", "Title" },
                values: new object[] { "Zaglavlje funkcije u Pythonu mora završiti dvotockom. Bez nje interpreter prijavi sintaksnu grešku.", "Koji redak ispravno zapocinje funkciju?", "Dvotocka u funkciji" });

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Explanation", "Question" },
                values: new object[] { "U JavaScriptu su cijeli i decimalni brojevi tipa number. typeof za 10 vraca \"number\".", "Što vraca typeof x?" });

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 19,
                column: "Explanation",
                value: "Funkcija vraca n * n, poziv kvadrat(3) daje 9, a print ispisuje taj rezultat.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 20,
                column: "Explanation",
                value: "n je deklariran s const. Treca linija ga pokušava ponovo dodijeliti, a to JavaScript ne dozvoljava.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 23,
                column: "Explanation",
                value: "Funkcija vraca n * 2. duplo(4) je 8 i console.log ispisuje tu vrijednost.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 25,
                column: "Explanation",
                value: "if pokrece granu kad je uvjet tocan. else hvata sve ostalo.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 26,
                column: "Explanation",
                value: "def zapocinje funkciju. return šalje rezultat n * n pozivatelju.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 30,
                column: "Explanation",
                value: "Lista se prvo stvori. Petlja ide preko nje, a print je uvucen jer pripada tijelu petlje.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "Explanation", "Question" },
                values: new object[] { "bod mora postojati prije uvjeta. print u grani if i print u grani else uvuceni su za cetiri razmaka.", "Ispiši prolaz ako je bod barem 10, inace pad." });

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 34,
                column: "Explanation",
                value: "Tijelo funkcije je uvuceno. Poziv dolazi nakon definicije i nakon što broj postoji. Natpis ide prije vrijednosti.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 35,
                column: "Explanation",
                value: "Niz i pocetni zbroj smiju zamijeniti mjesta. Petlja mora doci nakon oba, a ispis nakon petlje. Uvlake prate if unutar for.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "Explanation", "Title" },
                values: new object[] { "Petlja se vrti dok je i manji ili jednak 2. x raste za trenutni i, zatim se i poveca za 1. Na kraju je x 3, a i 3.", "Brojac u petlji" });

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 37,
                column: "Explanation",
                value: "temp cuva staru vrijednost a. Zatim a preuzima b, a b vraca spremljenu vrijednost iz temp. Na kraju je a 5, a b 2.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 39,
                column: "Explanation",
                value: "bod je 8, pa uvjet bod >= 10 nije istinit. Grana if se preskace. else postavlja ocjena na pad.");

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 42,
                column: "Explanation",
                value: "Srednji stupac je zatvoren osim donjeg reda. Robot mora sici, proci ispod zida i popeti se do cilja. Zvjezdice su u donjem redu.");
        }
    }
}
