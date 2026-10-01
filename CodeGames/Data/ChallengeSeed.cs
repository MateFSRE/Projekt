using System.Text.Json;
using CodeGames.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeGames.Data;

// 15 zadataka starih tipova, 8 SwipeBug, 6 FillBlanks, 6 ReorderCode, 4 TraceExecution i 6 RobotMaze.
public static class ChallengeSeed
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Challenge>().HasData(
            Challenge(1, ChallengeType.PredictOutput, "Python", 1, "Zbroj dva broja",
                "x = 2\ny = 3\nprint(x + y)",
                "Što program ispisuje?",
                "5",
                "Varijable x i y imaju vrijednosti 2 i 3. print ispisuje njihov zbroj, a to je 5."),
            Challenge(2, ChallengeType.PredictOutput, "Python", 2, "Ponavljanje teksta",
                "ime = \"Ana\"\nprint(ime * 2)",
                "Što program ispisuje?",
                "AnaAna",
                "Operator * uz tekst ponavlja taj tekst onoliko puta koliki je broj. \"Ana\" * 2 daje AnaAna."),
            Challenge(3, ChallengeType.PredictOutput, "Python", 3, "Prvi i zadnji element",
                "brojevi = [1, 2, 3]\nprint(brojevi[0] + brojevi[-1])",
                "Što program ispisuje?",
                "4",
                "Indeks 0 je prvi element (1), a indeks -1 je zadnji (3). Njihov zbroj je 4."),
            Challenge(4, ChallengeType.MultipleChoice, "Python", 1, "Duljina teksta",
                "print(len(\"kod\"))",
                "Što se ispisuje?",
                "3",
                "len broji znakove. Riječ \"kod\" ima tri znaka, pa je ispis 3."),
            Challenge(5, ChallengeType.MultipleChoice, "Python", 2, "Kvadrat broja",
                "def kvadrat(n):\n    return n * n",
                "Što vraća poziv kvadrat(4)?",
                "16",
                "Funkcija množi broj samim sobom. 4 * 4 je 16."),
            Challenge(6, ChallengeType.MultipleChoice, "Python", 3, "Ista lista pod dva imena",
                "a = [1, 2]\nb = a\nb.append(3)",
                "Što lista a sadrži nakon append?",
                "[1, 2, 3]",
                "b = a ne kopira listu. Oba imena pokazuju na istu listu, pa append mijenja i a."),
            Challenge(7, ChallengeType.FixBug, "Python", 1, "Usporedba u uvjetu",
                "x = 10\nif x = 10:\n    print(\"da\")",
                "Koji redak ispravlja grešku u uvjetu?",
                "if x == 10:",
                "Jedan znak = je dodjela i ne smije stajati u uvjetu. Usporedba jednakosti u Pythonu piše se s ==."),
            Challenge(8, ChallengeType.FixBug, "Python", 2, "Indeks izvan liste",
                "brojevi = [1, 2, 3]\nprint(brojevi[3])",
                "Koji redak ispisuje zadnji element bez greške?",
                "print(brojevi[2])",
                "Lista ima indekse 0, 1 i 2. Indeks 3 ne postoji. Zadnji element je na indeksu 2."),
            Challenge(9, ChallengeType.FixBug, "Python", 3, "Dvotočka u funkciji",
                "def zbroj(a, b)\n    return a + b",
                "Koji redak ispravno započinje funkciju?",
                "def zbroj(a, b):",
                "Zaglavlje funkcije u Pythonu mora završiti dvotočkom. Bez nje interpreter prijavi sintaksnu grešku."),
            Challenge(10, ChallengeType.PredictOutput, "JavaScript", 1, "Zbroj u konzoli",
                "let a = 2;\nlet b = 3;\nconsole.log(a + b);",
                "Što program ispisuje?",
                "5",
                "a i b su brojevi. Operator + ih zbraja, a console.log ispisuje 5."),
            Challenge(11, ChallengeType.PredictOutput, "JavaScript", 2, "Spajanje teksta i broja",
                "console.log(\"5\" + 1);",
                "Što program ispisuje?",
                "51",
                "Kad je jedan operand tekst, + ne zbraja nego spaja znakove. Broj 1 postaje znak \"1\", pa je rezultat 51."),
            Challenge(12, ChallengeType.MultipleChoice, "JavaScript", 1, "Tip broja",
                "let x = 10;",
                "Što vraća typeof x?",
                "number",
                "U JavaScriptu su cijeli i decimalni brojevi tipa number. typeof za 10 vraća \"number\"."),
            Challenge(13, ChallengeType.MultipleChoice, "JavaScript", 3, "Isti niz pod dva imena",
                "let a = [1, 2];\nlet b = a;\nb.push(3);",
                "Što niz a sadrži nakon push?",
                "[1, 2, 3]",
                "b = a ne stvara novi niz. push dodaje 3 u isti niz, pa se promjena vidi i kroz a."),
            Challenge(14, ChallengeType.FixBug, "JavaScript", 2, "Velika i mala slova",
                "let ime = \"Ana\";\nconsole.log(\"Zdravo \" + Ime);",
                "Koji redak ispravlja grešku u imenu varijable?",
                "console.log(\"Zdravo \" + ime);",
                "JavaScript razlikuje velika i mala slova. Varijabla se zove ime, a Ime nije definirana."),
            Challenge(15, ChallengeType.FixBug, "JavaScript", 3, "Ponovna dodjela const",
                "const n = 5;\nn = 6;\nconsole.log(n);",
                "Koja izmjena dozvoljava novu vrijednost n?",
                "let n = 5;",
                "const se ne smije ponovno dodijeliti. let drži broj koji se kasnije može promijeniti."));

        modelBuilder.Entity<Challenge>().HasData(SwipeChallenges.ToArray());
        modelBuilder.Entity<Challenge>().HasData(FillChallenges.ToArray());
        modelBuilder.Entity<Challenge>().HasData(ReorderChallenges.ToArray());
        modelBuilder.Entity<Challenge>().HasData(TraceChallenges.ToArray());
        modelBuilder.Entity<Challenge>().HasData(MazeChallenges.ToArray());

        modelBuilder.Entity<ChallengeOption>().HasData(
            Option(1, 4, "3", true),
            Option(2, 4, "2", false),
            Option(3, 4, "4", false),
            Option(4, 4, "kod", false),
            Option(5, 5, "8", false),
            Option(6, 5, "16", true),
            Option(7, 5, "4", false),
            Option(8, 5, "44", false),
            Option(9, 6, "[1, 2]", false),
            Option(10, 6, "[1, 2, 3]", true),
            Option(11, 6, "[3]", false),
            Option(12, 6, "[1, 2, 3, 3]", false),
            Option(13, 7, "if x == 10:", true),
            Option(14, 7, "if x = 10:", false),
            Option(15, 7, "if x === 10:", false),
            Option(16, 7, "if (x = 10):", false),
            Option(17, 8, "print(brojevi[2])", true),
            Option(18, 8, "print(brojevi[3])", false),
            Option(19, 8, "print(brojevi(2))", false),
            Option(20, 8, "print(brojevi.2)", false),
            Option(21, 9, "def zbroj(a, b):", true),
            Option(22, 9, "def zbroj(a, b)", false),
            Option(23, 9, "function zbroj(a, b):", false),
            Option(24, 9, "def zbroj(a b):", false),
            Option(25, 12, "number", true),
            Option(26, 12, "string", false),
            Option(27, 12, "int", false),
            Option(28, 12, "integer", false),
            Option(29, 13, "[1, 2]", false),
            Option(30, 13, "[1, 2, 3]", true),
            Option(31, 13, "[3]", false),
            Option(32, 13, "undefined", false),
            Option(33, 14, "console.log(\"Zdravo \" + ime);", true),
            Option(34, 14, "console.log(\"Zdravo \" + Ime);", false),
            Option(35, 14, "console.log(\"Zdravo \" + name);", false),
            Option(36, 14, "print(\"Zdravo \" + ime);", false),
            Option(37, 15, "let n = 5;", true),
            Option(38, 15, "const n = 5;", false),
            Option(39, 15, "var const n = 5;", false),
            Option(40, 15, "int n = 5;", false));
    }

    public static IReadOnlyList<Challenge> SwipeChallenges { get; } =
    [
        Swipe(16, "Python", "Dodjela umjesto usporedbe",
            "broj = 4\nif broj = 4:\n    print(\"da\")\nelse:\n    print(\"ne\")",
            true, 2,
            "U uvjetu stoji jedan znak =, a to je dodjela. Usporedba u Pythonu piše se s ==."),
        Swipe(17, "Python", "Indeks izvan liste",
            "brojevi = [1, 2, 3]\nzbroj = 0\nfor broj in brojevi:\n    zbroj = zbroj + broj\nprint(brojevi[3])",
            true, 5,
            "Lista ima indekse 0, 1 i 2. Indeks 3 ne postoji, pa zadnja linija puca."),
        Swipe(18, "Python", "Zbroj bodova",
            "bodovi = [2, 4, 6]\nzbroj = 0\nfor bod in bodovi:\n    zbroj = zbroj + bod\nprint(zbroj)",
            false, null,
            "Petlja prolazi kroz svaki broj i zbraja ga. Indeksi su ispravni i nema sintaksne greške."),
        Swipe(19, "Python", "Kvadrat broja",
            "def kvadrat(n):\n    return n * n\nrezultat = kvadrat(3)\nprint(rezultat)",
            false, null,
            "Funkcija vraća n * n, poziv kvadrat(3) daje 9, a print ispisuje taj rezultat."),
        Swipe(20, "JavaScript", "Ponovna dodjela const",
            "const n = 5;\nlet poruka = \"start\";\nn = n + 1;\nporuka = poruka + n;\nconsole.log(poruka);",
            true, 3,
            "n je deklariran s const. Treća linija ga pokušava ponovo dodijeliti, a to JavaScript ne dozvoljava."),
        Swipe(21, "JavaScript", "Dodjela u uvjetu",
            "let bod = 10;\nif (bod = 10) {\n    console.log(\"prolaz\");\n} else {\n    console.log(\"pad\");\n}",
            true, 2,
            "U uvjetu je dodjela =, ne usporedba. Uvjet treba biti bod === 10."),
        Swipe(22, "JavaScript", "Zbroj dva broja",
            "let a = 2;\nlet b = 3;\nlet zbroj = a + b;\nconsole.log(zbroj);",
            false, null,
            "a i b su brojevi, + ih zbraja, a console.log ispisuje zbroj. Nema greške."),
        Swipe(23, "JavaScript", "Dupliranje broja",
            "function duplo(n) {\n    return n * 2;\n}\nlet x = duplo(4);\nconsole.log(x);",
            false, null,
            "Funkcija vraća n * 2. duplo(4) je 8 i console.log ispisuje tu vrijednost.")
    ];

    public static IReadOnlyList<Challenge> FillChallenges { get; } =
    [
        Fill(24, "Python", "Petlja kroz listu",
            "brojevi = [1, 2, 3]\n{{1}} broj {{2}} brojevi:\n    print(broj)",
            [("1", "for"), ("2", "in")],
            ["for", "in", "while", "if", "def"],
            "for prolazi kroz listu, a in veže svaki element na ime broj."),
        Fill(25, "Python", "Uvjet prolaza",
            "bod = 10\n{{1}} bod >= 10:\n    print(\"prolaz\")\n{{2}}:\n    print(\"pad\")",
            [("1", "if"), ("2", "else")],
            ["if", "else", "elif", "for", "return"],
            "if pokreće granu kad je uvjet točan. else hvata sve ostalo."),
        Fill(26, "Python", "Funkcija kvadrata",
            "{{1}} kvadrat(n):\n    {{2}} n * n",
            [("1", "def"), ("2", "return")],
            ["def", "return", "function", "print", "if"],
            "def započinje funkciju. return šalje rezultat n * n pozivatelju."),
        Fill(27, "Python", "Dodavanje u listu",
            "imena = [\"Ana\", \"Leo\"]\nimena.{{1}}(\"Mia\")\nprint(imena[{{2}}])",
            [("1", "append"), ("2", "0")],
            ["append", "0", "push", "1", "pop"],
            "append dodaje na kraj liste. Prvi element i dalje je na indeksu 0."),
        Fill(28, "JavaScript", "Petlja po nizu",
            "const brojevi = [1, 2, 3];\n{{1}} (const broj {{2}} brojevi) {\n    console.log(broj);\n}",
            [("1", "for"), ("2", "of")],
            ["for", "of", "while", "in", "if"],
            "for ... of prolazi kroz vrijednosti niza. of daje element, ne indeks."),
        Fill(29, "JavaScript", "Funkcija i niz",
            "{{1}} zbroj(a, b) {\n    {{2}} a + b;\n}\nconst brojevi = [1, 2];\nbrojevi.{{3}}(3);",
            [("1", "function"), ("2", "return"), ("3", "push")],
            ["function", "return", "push", "def", "append", "for"],
            "function i return definiraju zbroj. push dodaje 3 na kraj niza.")
    ];

    public static IReadOnlyList<Challenge> ReorderChallenges { get; } =
    [
        Reorder(30, "Python", 1, "Brojevi od 1 do 3",
            "Ispiši brojeve od 1 do 3.",
            [
                ("1", "brojevi = [1, 2, 3]", 0),
                ("2", "for broj in brojevi:", 0),
                ("3", "print(broj)", 4)
            ],
            [
                ["1", "2", "3"]
            ],
            "Lista se prvo stvori. Petlja ide preko nje, a print je uvučen jer pripada tijelu petlje."),
        Reorder(31, "JavaScript", 1, "Zbroj dva broja",
            "Ispiši zbroj dva broja.",
            [
                ("1", "const a = 2;", 0),
                ("2", "const b = 3;", 0),
                ("3", "const zbroj = a + b;", 0),
                ("4", "console.log(zbroj);", 0)
            ],
            [
                ["1", "2", "3", "4"],
                ["2", "1", "3", "4"]
            ],
            "a i b smiju zamijeniti mjesta. Zbroj dolazi tek kad oba postoje, a ispis je zadnji."),
        Reorder(32, "Python", 2, "Prolaz ili pad",
            "Ispiši prolaz ako je bod barem 10, inače pad.",
            [
                ("1", "bod = 10", 0),
                ("2", "if bod >= 10:", 0),
                ("3", "print(\"prolaz\")", 4),
                ("4", "else:", 0),
                ("5", "print(\"pad\")", 4)
            ],
            [
                ["1", "2", "3", "4", "5"]
            ],
            "bod mora postojati prije uvjeta. print u grani if i print u grani else uvučeni su za četiri razmaka."),
        Reorder(33, "JavaScript", 2, "Brojevi i duljina",
            "Ispiši svaki broj iz niza i koliko ih ima.",
            [
                ("1", "const brojevi = [1, 2, 3];", 0),
                ("2", "for (const broj of brojevi) {", 0),
                ("3", "console.log(broj);", 4),
                ("4", "}", 0),
                ("5", "const ukupno = brojevi.length;", 0),
                ("6", "console.log(ukupno);", 0)
            ],
            [
                ["1", "2", "3", "4", "5", "6"],
                ["1", "5", "2", "3", "4", "6"]
            ],
            "Duljina niza smije se uzeti prije petlje ili poslije nje. Ispis ukupno mora ostati na kraju."),
        Reorder(34, "Python", 3, "Kvadrat broja",
            "Definiraj kvadrat i ispiši rezultat za broj 4.",
            [
                ("1", "def kvadrat(n):", 0),
                ("2", "rezultat = n * n", 4),
                ("3", "return rezultat", 4),
                ("4", "broj = 4", 0),
                ("5", "ispis = kvadrat(broj)", 0),
                ("6", "print(\"Rezultat:\")", 0),
                ("7", "print(ispis)", 0)
            ],
            [
                ["1", "2", "3", "4", "5", "6", "7"]
            ],
            "Tijelo funkcije je uvučeno. Poziv dolazi nakon definicije i nakon što broj postoji. Natpis ide prije vrijednosti."),
        Reorder(35, "JavaScript", 3, "Zbroj parnih",
            "Zbroji parne brojeve i ispiši zbroj.",
            [
                ("1", "const brojevi = [1, 2, 3, 4, 5];", 0),
                ("2", "let zbroj = 0;", 0),
                ("3", "for (const broj of brojevi) {", 0),
                ("4", "if (broj % 2 === 0) {", 4),
                ("5", "zbroj = zbroj + broj;", 8),
                ("6", "}", 4),
                ("7", "}", 0),
                ("8", "console.log(zbroj);", 0)
            ],
            [
                ["1", "2", "3", "4", "5", "6", "7", "8"],
                ["2", "1", "3", "4", "5", "6", "7", "8"]
            ],
            "Niz i početni zbroj smiju zamijeniti mjesta. Petlja mora doći nakon oba, a ispis nakon petlje. Uvlake prate if unutar for.")
    ];

    public static IReadOnlyList<Challenge> TraceChallenges { get; } =
    [
        Trace(36, "Python", "Brojač u petlji",
            "x = 0\ni = 1\nwhile i <= 2:\n    x = x + i\n    i = i + 1\nprint(x)",
            ["x", "i"],
            [
                Step(1, ("x", "0"), ("i", "")),
                Step(2, ("x", "0"), ("i", "1")),
                Step(3, ("x", "0"), ("i", "1")),
                Step(4, ("x", "1"), ("i", "1")),
                Step(5, ("x", "1"), ("i", "2")),
                Step(3, ("x", "1"), ("i", "2")),
                Step(4, ("x", "3"), ("i", "2")),
                Step(5, ("x", "3"), ("i", "3")),
                Step(3, ("x", "3"), ("i", "3")),
                Step(6, ("x", "3"), ("i", "3"))
            ],
            [3, 6, 7],
            "Petlja se vrti dok je i manji ili jednak 2. x raste za trenutni i, zatim se i poveća za 1. Na kraju je x 3, a i 3."),
        Trace(37, "Python", "Zamjena dviju varijabli",
            "a = 2\nb = 5\ntemp = a\na = b\nb = temp",
            ["a", "b", "temp"],
            [
                Step(1, ("a", "2"), ("b", ""), ("temp", "")),
                Step(2, ("a", "2"), ("b", "5"), ("temp", "")),
                Step(3, ("a", "2"), ("b", "5"), ("temp", "2")),
                Step(4, ("a", "5"), ("b", "5"), ("temp", "2")),
                Step(5, ("a", "5"), ("b", "2"), ("temp", "2"))
            ],
            [2, 3, 4],
            "temp čuva staru vrijednost a. Zatim a preuzima b, a b vraća spremljenu vrijednost iz temp. Na kraju je a 5, a b 2."),
        Trace(38, "Python", "Suma liste",
            "brojevi = [2, 4]\ns = 0\nfor broj in brojevi:\n    s = s + broj\nprint(s)",
            ["s", "broj"],
            [
                Step(1, ("s", ""), ("broj", "")),
                Step(2, ("s", "0"), ("broj", "")),
                Step(3, ("s", "0"), ("broj", "2")),
                Step(4, ("s", "2"), ("broj", "2")),
                Step(3, ("s", "2"), ("broj", "4")),
                Step(4, ("s", "6"), ("broj", "4")),
                Step(5, ("s", "6"), ("broj", "4"))
            ],
            [2, 3, 5],
            "for prvo veže broj na 2, pa s postaje 2. Drugi prolaz veže broj na 4 i s postaje 6."),
        Trace(39, "JavaScript", "If ili else",
            "let bod = 8;\nlet ocjena = \"nista\";\nif (bod >= 10) {\n    ocjena = \"prolaz\";\n} else {\n    ocjena = \"pad\";\n}",
            ["bod", "ocjena"],
            [
                Step(1, ("bod", "8"), ("ocjena", "")),
                Step(2, ("bod", "8"), ("ocjena", "nista")),
                Step(3, ("bod", "8"), ("ocjena", "nista")),
                Step(5, ("bod", "8"), ("ocjena", "nista")),
                Step(6, ("bod", "8"), ("ocjena", "pad"))
            ],
            [1, 2, 4],
            "bod je 8, pa uvjet bod >= 10 nije istinit. Grana if se preskače. else postavlja ocjena na pad.")
    ];

    public static IReadOnlyList<Challenge> MazeChallenges { get; } =
    [
        Maze(40, 1, "Ravna linija",
            "Tri puta naprijed vode robota od lijevog ruba do cilja. Zvjezdica je na tom putu i donosi dodatne bodove.",
            4, 1, [], 0, 0, "right", 3, 0, [(1, 0)],
            ["forward"], 4, 8, 3),
        Maze(41, 1, "Skretanje",
            "Idi desno do kraja reda, skreni desno i spusti se do donjeg desnog polja. Obje zvjezdice leže na tom putu.",
            3, 3, [], 0, 0, "right", 2, 2, [(1, 0), (2, 1)],
            ["forward", "turnLeft", "turnRight"], 8, 20, 5),
        Maze(42, 2, "Zaobilaženje zida",
            "Srednji stupac je zatvoren osim donjeg reda. Robot mora sići, proći ispod zida i popeti se do cilja. Zvjezdice su u donjem redu.",
            5, 3, [(2, 0), (2, 1)], 0, 1, "right", 4, 1, [(1, 2), (3, 2)],
            ["forward", "turnLeft", "turnRight"], 14, 30, 9),
        Maze(43, 2, "Prva petlja",
            "Do cilja trebaju tri koraka naprijed, a smiješ samo dva bloka. Ponovi jedan korak tri puta. Zvjezdica je na sredini puta.",
            4, 1, [], 0, 0, "right", 3, 0, [(2, 0)],
            ["forward", "repeat"], 2, 10, 2),
        Maze(44, 3, "Petlja sa skretanjem",
            "Tri naprijed, skretanje, dva naprijed, skretanje, tri naprijed. Dvije zvjezdice leže na tom krugu.",
            4, 3, [], 0, 0, "right", 0, 2, [(2, 0), (1, 2)],
            ["forward", "turnLeft", "turnRight", "repeat"], 12, 30, 8),
        Maze(45, 3, "Zvjezdice na putu",
            "Zid tjera robota u donji red. Obje zvjezdice leže na tom putu, a petlja drži program kratkim.",
            5, 3, [(2, 0), (2, 1)], 0, 1, "right", 4, 1, [(1, 2), (3, 2)],
            ["forward", "turnLeft", "turnRight", "repeat"], 12, 25, 7)
    ];

    private static (int Line, (string Name, string Value)[] Expected) Step(
        int line,
        params (string Name, string Value)[] expected)
    {
        return (line, expected);
    }

    private static Challenge Challenge(
        int id,
        ChallengeType type,
        string language,
        int difficulty,
        string title,
        string codeSnippet,
        string question,
        string correctAnswer,
        string explanation)
    {
        return new Challenge
        {
            Id = id,
            Type = type,
            Language = language,
            Difficulty = difficulty,
            Title = title,
            CodeSnippet = codeSnippet,
            Question = question,
            CorrectAnswer = correctAnswer,
            Explanation = explanation,
            Points = difficulty * 10,
            IsActive = true,
            Options = null!,
            Attempts = null!
        };
    }

    private static Challenge Swipe(
        int id,
        string language,
        string title,
        string code,
        bool hasBug,
        int? bugLine,
        string explanation)
    {
        var config = JsonSerializer.Serialize(
            new SwipeConfig(code, language, hasBug, bugLine),
            SwipeConfig.JsonOptions);

        return new Challenge
        {
            Id = id,
            Type = ChallengeType.SwipeBug,
            Language = language,
            Difficulty = 1,
            Title = title,
            CodeSnippet = code,
            Question = "Ima li ovaj kod bug?",
            CorrectAnswer = hasBug ? "true" : "false",
            Explanation = explanation,
            ConfigJson = config,
            Points = 10,
            IsActive = true,
            Options = null!,
            Attempts = null!
        };
    }

    private sealed record SwipeConfig(string Code, string Language, bool HasBug, int? BugLine)
    {
        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private static Challenge Fill(
        int id,
        string language,
        string title,
        string template,
        (string Id, string Answer)[] blanks,
        string[] wordBank,
        string explanation)
    {
        var config = JsonSerializer.Serialize(
            new FillConfig(
                language,
                template,
                blanks.Select(blank => new FillBlank(blank.Id, blank.Answer)).ToArray(),
                wordBank),
            SwipeConfig.JsonOptions);

        return new Challenge
        {
            Id = id,
            Type = ChallengeType.FillBlanks,
            Language = language,
            Difficulty = 2,
            Title = title,
            CodeSnippet = template,
            Question = "Dopuni rupe.",
            CorrectAnswer = string.Join(", ", blanks.Select(blank => blank.Answer)),
            Explanation = explanation,
            ConfigJson = config,
            Points = 20,
            IsActive = true,
            Options = null!,
            Attempts = null!
        };
    }

    private sealed record FillConfig(string Language, string Template, FillBlank[] Blanks, string[] WordBank);

    private sealed record FillBlank(string Id, string Answer);

    private static Challenge Reorder(
        int id,
        string language,
        int difficulty,
        string title,
        string goal,
        (string Id, string Text, int Indent)[] lines,
        string[][] correctOrders,
        string explanation)
    {
        var config = JsonSerializer.Serialize(
            new ReorderConfig(
                language,
                goal,
                lines.Select(line => new ReorderLine(line.Id, line.Text, line.Indent)).ToArray(),
                correctOrders),
            SwipeConfig.JsonOptions);

        var snippet = string.Join(
            "\n",
            correctOrders[0].Select(lineId =>
            {
                var line = lines.First(item => item.Id == lineId);
                return new string(' ', line.Indent) + line.Text;
            }));

        return new Challenge
        {
            Id = id,
            Type = ChallengeType.ReorderCode,
            Language = language,
            Difficulty = difficulty,
            Title = title,
            CodeSnippet = snippet,
            Question = goal,
            CorrectAnswer = string.Join(",", correctOrders[0]),
            Explanation = explanation,
            ConfigJson = config,
            Points = difficulty * 10,
            IsActive = true,
            Options = null!,
            Attempts = null!
        };
    }

    private sealed record ReorderConfig(
        string Language,
        string Goal,
        ReorderLine[] Lines,
        string[][] CorrectOrders);

    private sealed record ReorderLine(string Id, string Text, int Indent);

    private static Challenge Trace(
        int id,
        string language,
        string title,
        string code,
        string[] variables,
        (int Line, (string Name, string Value)[] Expected)[] steps,
        int[] askAt,
        string explanation)
    {
        var config = JsonSerializer.Serialize(
            new TraceConfig(
                language,
                code,
                variables,
                steps.Select(step => new TraceStep(
                    step.Line,
                    step.Expected.ToDictionary(pair => pair.Name, pair => pair.Value))).ToArray(),
                askAt),
            SwipeConfig.JsonOptions);

        var answer = string.Join("; ", askAt.Select(index =>
        {
            var step = steps[index];
            var pairs = step.Expected.Select(pair => $"{pair.Name}={pair.Value}");
            return string.Join(", ", pairs);
        }));

        return new Challenge
        {
            Id = id,
            Type = ChallengeType.TraceExecution,
            Language = language,
            Difficulty = 3,
            Title = title,
            CodeSnippet = code,
            Question = "Upiši vrijednosti varijabli na traženim koracima.",
            CorrectAnswer = answer,
            Explanation = explanation,
            ConfigJson = config,
            Points = 30,
            IsActive = true,
            Options = null!,
            Attempts = null!
        };
    }

    private sealed record TraceConfig(
        string Language,
        string Code,
        string[] Variables,
        TraceStep[] Steps,
        int[] AskAt);

    private sealed record TraceStep(int Line, Dictionary<string, string> Expected);

    private static Challenge Maze(
        int id,
        int difficulty,
        string title,
        string explanation,
        int width,
        int height,
        (int X, int Y)[] walls,
        int startX,
        int startY,
        string direction,
        int goalX,
        int goalY,
        (int X, int Y)[] stars,
        string[] allowedBlocks,
        int maxBlocks,
        int maxSteps,
        int optimalBlocks)
    {
        var config = JsonSerializer.Serialize(
            new MazeConfig(
                width,
                height,
                walls.Select(wall => new[] { wall.X, wall.Y }).ToArray(),
                new MazeStart(startX, startY, direction),
                new MazeGoal(goalX, goalY),
                stars.Select(star => new[] { star.X, star.Y }).ToArray(),
                allowedBlocks,
                maxBlocks,
                maxSteps,
                optimalBlocks),
            SwipeConfig.JsonOptions);

        return new Challenge
        {
            Id = id,
            Type = ChallengeType.RobotMaze,
            Language = "Blokovi",
            Difficulty = difficulty,
            Title = title,
            CodeSnippet = $"{width}x{height}",
            Question = "Dovedi robota do cilja.",
            CorrectAnswer = "simulacija",
            Explanation = explanation,
            ConfigJson = config,
            Points = difficulty * 10,
            IsActive = true,
            Options = null!,
            Attempts = null!
        };
    }

    private sealed record MazeConfig(
        int Width,
        int Height,
        int[][] Walls,
        MazeStart Start,
        MazeGoal Goal,
        int[][] Stars,
        string[] AllowedBlocks,
        int MaxBlocks,
        int MaxSteps,
        int OptimalBlocks);

    private sealed record MazeStart(int X, int Y, string Dir);

    private sealed record MazeGoal(int X, int Y);

    private static ChallengeOption Option(int id, int challengeId, string text, bool isCorrect)
    {
        return new ChallengeOption
        {
            Id = id,
            ChallengeId = challengeId,
            Text = text,
            IsCorrect = isCorrect,
            Challenge = null!
        };
    }
}
