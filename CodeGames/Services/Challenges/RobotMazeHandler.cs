using System.Text.Json;
using System.Text.Json.Serialization;
using CodeGames.Exceptions;
using CodeGames.Models;

namespace CodeGames.Services.Challenges;

public class RobotMazeHandler : IChallengeHandler
{
    public const int MaxRepeatTimes = 10;
    public const int MaxDepth = 4;
    public const int PointsPerStar = 2;

    private static readonly string[] Commands = ["forward", "turnLeft", "turnRight", "repeat"];
    private static readonly string[] Directions = ["up", "right", "down", "left"];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ChallengeType Type => ChallengeType.RobotMaze;

    public object BuildPublicView(Challenge challenge)
    {
        var config = ReadConfig(challenge);
        return new
        {
            width = config.Width,
            height = config.Height,
            walls = config.Walls,
            start = config.Start,
            goal = config.Goal,
            stars = config.Stars,
            allowedBlocks = config.AllowedBlocks,
            maxBlocks = config.MaxBlocks,
            maxSteps = config.MaxSteps,
            optimalBlocks = config.OptimalBlocks
        };
    }

    public EvaluationResult Evaluate(Challenge challenge, JsonElement answer)
    {
        var config = ReadConfig(challenge);
        var blocks = 0;
        var program = ReadProgram(answer, config.AllowedBlocks, config.MaxBlocks, ref blocks, depth: 1);
        var simulation = Simulate(config, program);
        var reachedGoal = simulation.Outcome == "reachedGoal";

        return new EvaluationResult
        {
            IsCorrect = reachedGoal,
            ScoreFraction = Score(reachedGoal, simulation.StarsCollected, config.Stars.Count, blocks, config.OptimalBlocks),
            BonusPoints = reachedGoal ? simulation.StarsCollected * PointsPerStar : 0,
            Details = new
            {
                outcome = simulation.Outcome,
                path = simulation.Path.Select(pose => new { x = pose.X, y = pose.Y, dir = pose.Dir }).ToList(),
                starsCollected = simulation.StarsCollected,
                starsTotal = config.Stars.Count,
                blocksUsed = blocks
            },
            RevealedSolution = new
            {
                optimalBlocks = config.OptimalBlocks
            }
        };
    }

    public void ValidateConfig(Challenge challenge)
    {
        ReadConfig(challenge);
    }

    // Cilj nosi pune bodove ako je program dovoljno kratak, inače 0.8.
    // Zvjezdice se ne miješaju u taj udio: svaka dodaje BonusPoints.
    // Promašaj koji je pokupio zvjezdicu ostavlja udio iznad nule, ali ispod 1.
    private static double Score(bool reachedGoal, int starsCollected, int starsTotal, int blocksUsed, int optimalBlocks)
    {
        if (!reachedGoal)
        {
            if (starsTotal == 0 || starsCollected <= 0)
            {
                return 0d;
            }

            return Math.Min(0.99d, (double)starsCollected / starsTotal);
        }

        return blocksUsed <= optimalBlocks ? 1d : 0.8d;
    }

    private static Simulation Simulate(MazeConfig config, List<Command> program)
    {
        if (config.Start is null || config.Goal is null)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Početak treba x, y i smjer.");
        }

        var start = config.Start;
        var walls = config.Walls.Select(wall => (wall[0], wall[1])).ToHashSet();
        var stars = config.Stars.Select(star => (star[0], star[1])).ToHashSet();
        var pose = new Pose(start.X, start.Y, start.Dir);
        var path = new List<Pose> { pose };
        var collected = 0;
        var steps = 0;
        var outcome = Run(program, config, walls, stars, ref pose, path, ref collected, ref steps);
        if (outcome == "running")
        {
            outcome = "didNotReachGoal";
        }

        return new Simulation(outcome, path, collected);
    }

    private static string Run(
        List<Command> program,
        MazeConfig config,
        HashSet<(int X, int Y)> walls,
        HashSet<(int X, int Y)> stars,
        ref Pose pose,
        List<Pose> path,
        ref int collected,
        ref int steps)
    {
        foreach (var command in program)
        {
            if (command.Type == "repeat")
            {
                for (var time = 0; time < command.Times; time++)
                {
                    var nested = Run(command.Body, config, walls, stars, ref pose, path, ref collected, ref steps);
                    if (nested != "running")
                    {
                        return nested;
                    }
                }

                continue;
            }

            if (steps >= config.MaxSteps)
            {
                return "outOfSteps";
            }

            steps++;
            if (command.Type == "turnLeft")
            {
                pose = pose with { Dir = Turn(pose.Dir, -1) };
                path.Add(pose);
                continue;
            }

            if (command.Type == "turnRight")
            {
                pose = pose with { Dir = Turn(pose.Dir, 1) };
                path.Add(pose);
                continue;
            }

            var next = Ahead(pose);
            if (next.X < 0 || next.Y < 0 || next.X >= config.Width || next.Y >= config.Height || walls.Contains((next.X, next.Y)))
            {
                return "hitWall";
            }

            pose = next;
            path.Add(pose);
            if (stars.Remove((pose.X, pose.Y)))
            {
                collected++;
            }

            if (config.Goal is not null && pose.X == config.Goal.X && pose.Y == config.Goal.Y)
            {
                return "reachedGoal";
            }
        }

        return "running";
    }

    private static Pose Ahead(Pose pose)
    {
        var (dx, dy) = pose.Dir switch
        {
            "up" => (0, -1),
            "right" => (1, 0),
            "down" => (0, 1),
            "left" => (-1, 0),
            _ => (0, 0)
        };

        return pose with { X = pose.X + dx, Y = pose.Y + dy };
    }

    private static string Turn(string direction, int turn)
    {
        var index = Array.IndexOf(Directions, direction);
        var next = (index + turn + Directions.Length) % Directions.Length;
        return Directions[next];
    }

    private static List<Command> ReadProgram(
        JsonElement answer,
        IReadOnlyCollection<string> allowed,
        int maxBlocks,
        ref int blocks,
        int depth)
    {
        if (answer.ValueKind != JsonValueKind.Array)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Program mora biti niz blokova.");
        }

        if (depth > MaxDepth)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Program je preduboko ugniježđen.");
        }

        var program = new List<Command>();
        foreach (var item in answer.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty("type", out var typeProperty))
            {
                throw new AppException(StatusCodes.Status400BadRequest, "Svaki blok treba type.");
            }

            var type = typeProperty.GetString() ?? string.Empty;
            if (!allowed.Contains(type))
            {
                throw new AppException(StatusCodes.Status400BadRequest, "Blok nije dopušten na ovoj razini.");
            }

            blocks++;
            if (blocks > maxBlocks)
            {
                throw new AppException(StatusCodes.Status400BadRequest, "Program ima previše blokova.");
            }

            if (type == "repeat")
            {
                if (!item.TryGetProperty("times", out var timesProperty) || timesProperty.ValueKind != JsonValueKind.Number || !timesProperty.TryGetInt32(out var times))
                {
                    throw new AppException(StatusCodes.Status400BadRequest, "Ponavljanje treba cijeli broj puta.");
                }

                if (times is < 1 or > MaxRepeatTimes)
                {
                    throw new AppException(StatusCodes.Status400BadRequest, $"Ponavljanje smije biti od 1 do {MaxRepeatTimes}.");
                }

                if (!item.TryGetProperty("body", out var bodyProperty))
                {
                    throw new AppException(StatusCodes.Status400BadRequest, "Ponavljanje treba tijelo.");
                }

                program.Add(new Command(type, times, ReadProgram(bodyProperty, allowed, maxBlocks, ref blocks, depth + 1)));
                continue;
            }

            program.Add(new Command(type, 0, []));
        }

        return program;
    }

    private static MazeConfig ReadConfig(Challenge challenge)
    {
        if (string.IsNullOrWhiteSpace(challenge.ConfigJson))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Robot u labirintu treba ConfigJson.");
        }

        MazeConfig? config;
        try
        {
            config = JsonSerializer.Deserialize<MazeConfig>(challenge.ConfigJson, JsonOptions);
        }
        catch (JsonException)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "ConfigJson za labirint nije ispravan.");
        }

        if (config is null)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "ConfigJson za labirint nije ispravan.");
        }

        if (config.Width is < 1 or > 8 || config.Height is < 1 or > 8)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Mreža smije biti od 1 do 8 polja po strani.");
        }

        if (config.Start is null || config.Goal is null || !Directions.Contains(config.Start.Dir))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Početak treba x, y i smjer.");
        }

        if (!Inside(config, config.Start.X, config.Start.Y) || !Inside(config, config.Goal.X, config.Goal.Y))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Početak i cilj moraju biti na mreži.");
        }

        if (config.Start.X == config.Goal.X && config.Start.Y == config.Goal.Y)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Početak i cilj ne smiju biti isto polje.");
        }

        config.Walls ??= [];
        config.Stars ??= [];
        config.AllowedBlocks ??= [];

        var walls = new HashSet<(int X, int Y)>();
        foreach (var wall in config.Walls)
        {
            if (wall is null || wall.Length != 2 || !Inside(config, wall[0], wall[1]) || !walls.Add((wall[0], wall[1])))
            {
                throw new AppException(StatusCodes.Status400BadRequest, "Zidovi moraju biti jedinstvena polja na mreži.");
            }
        }

        if (walls.Contains((config.Start.X, config.Start.Y)) || walls.Contains((config.Goal.X, config.Goal.Y)))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Početak i cilj ne smiju biti zid.");
        }

        var stars = new HashSet<(int X, int Y)>();
        foreach (var star in config.Stars)
        {
            if (star is null || star.Length != 2 || !Inside(config, star[0], star[1]) || walls.Contains((star[0], star[1])) || !stars.Add((star[0], star[1])))
            {
                throw new AppException(StatusCodes.Status400BadRequest, "Zvjezdice moraju biti slobodna jedinstvena polja.");
            }

            if (star[0] == config.Start.X && star[1] == config.Start.Y)
            {
                throw new AppException(StatusCodes.Status400BadRequest, "Zvjezdica ne smije stajati na početku.");
            }
        }

        if (stars.Count == 0)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Labirint treba barem jednu zvjezdicu.");
        }

        if (config.AllowedBlocks.Count == 0
            || config.AllowedBlocks.Any(block => !Commands.Contains(block))
            || config.AllowedBlocks.Distinct(StringComparer.Ordinal).Count() != config.AllowedBlocks.Count)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Dopušteni blokovi nisu ispravni.");
        }

        if (config.MaxBlocks is < 1 or > 30 || config.MaxSteps is < 1 or > 200 || config.OptimalBlocks is < 1 || config.OptimalBlocks > config.MaxBlocks)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Ograničenja blokova i koraka nisu ispravna.");
        }

        return config;
    }

    private static bool Inside(MazeConfig config, int x, int y)
    {
        return x >= 0 && y >= 0 && x < config.Width && y < config.Height;
    }

    private sealed record Command(string Type, int Times, List<Command> Body);

    private sealed record Pose(int X, int Y, string Dir);

    private sealed record Simulation(string Outcome, List<Pose> Path, int StarsCollected);

    private sealed class MazeConfig
    {
        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("walls")]
        public List<int[]> Walls { get; set; } = [];

        [JsonPropertyName("start")]
        public MazeStart? Start { get; set; }

        [JsonPropertyName("goal")]
        public MazeGoal? Goal { get; set; }

        [JsonPropertyName("stars")]
        public List<int[]> Stars { get; set; } = [];

        [JsonPropertyName("allowedBlocks")]
        public List<string> AllowedBlocks { get; set; } = [];

        [JsonPropertyName("maxBlocks")]
        public int MaxBlocks { get; set; }

        [JsonPropertyName("maxSteps")]
        public int MaxSteps { get; set; }

        [JsonPropertyName("optimalBlocks")]
        public int OptimalBlocks { get; set; }
    }

    private sealed class MazeStart
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("dir")]
        public string Dir { get; set; } = string.Empty;
    }

    private sealed class MazeGoal
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }
    }
}
