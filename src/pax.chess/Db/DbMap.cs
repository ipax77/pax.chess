using System.Globalization;
using System.Text.RegularExpressions;

namespace pax.chess;

public static class DbMap
{
    private static Regex moveRx = new Regex(@"(\w\d\w\d[QRNB]?)");

    public static Game GetGame(DbGame dbGame, string? name = null)
    {
        Game game = new Game();
        if (name != null)
        {
            game.Name = name;
        }
        Match m = moveRx.Match(dbGame.EngineMoves);
        while (m.Success)
        {
            EngineMove? move = Map.GetEngineMove(m.Groups[1].Value);
            if (move != null)
            {
                game.Move(move);
            }
            else
            {
                throw new Exception($"failed mapping db enginemove: {m.Groups[1].Value}");
            }
            m = m.NextMatch();
        }

        game.Result = dbGame.Result;
        game.Termination = dbGame.Termination;
        return game;
    }

    public static Game GetGame(string engineMoves)
    {
        Game game = new Game();

        Match m = moveRx.Match(engineMoves);
        while (m.Success)
        {
            EngineMove? move = Map.GetEngineMove(m.Groups[1].Value);
            if (move != null)
            {
                game.Move(move);
            }
            else
            {
                throw new Exception($"failed mapping db enginemove: {m.Groups[1].Value}");
            }
            m = m.NextMatch();
        }
        return game;
    }

    public static DbGame GetGame(Game game)
    {
        DbGame dbGame = new DbGame();

        if (game.Infos.Any())
        {
            if (game.Infos.ContainsKey("Event"))
            {
                dbGame.Event = game.Infos["Event"];
            }
            if (game.Infos.ContainsKey("Site"))
            {
                dbGame.Site = game.Infos["Site"];
            }
            else
            {
                dbGame.Site = game.Guid.ToString();
            }
            if (game.Infos.ContainsKey("UTCDate"))
            {
                dbGame.UTCDate = DateOnly.ParseExact(game.Infos["UTCDate"], "yyyy.MM.dd", CultureInfo.InvariantCulture);
            }
            if (game.Infos.ContainsKey("UTCTime"))
            {
                dbGame.UTCTime = TimeOnly.ParseExact(game.Infos["UTCTime"], @"HH\:mm\:ss", CultureInfo.InvariantCulture);
            }
            if (game.Infos.ContainsKey("White"))
            {
                dbGame.White = game.Infos["White"];
            }
            if (game.Infos.ContainsKey("Black"))
            {
                dbGame.Black = game.Infos["Black"];
            }
            if (game.Infos.ContainsKey("WhiteElo"))
            {
                short elo;
                if (short.TryParse(game.Infos["WhiteElo"], out elo))
                {
                    dbGame.WhiteElo = elo;
                }
            }
            if (game.Infos.ContainsKey("BlackElo"))
            {
                short elo;
                if (short.TryParse(game.Infos["BlackElo"], out elo))
                {
                    dbGame.BlackElo = elo;
                }
            }
            if (game.Infos.ContainsKey("Variant"))
            {
                dbGame.Variant = game.Infos["Variant"] switch
                {
                    "Standard" => Variant.Standard,
                    "Chess960" => Variant.Chess960,
                    _ => Variant.Unknown
                };
            }
            if (game.Infos.ContainsKey("ECO"))
            {
                dbGame.ECO = game.Infos["ECO"];
            }
            if (game.Infos.ContainsKey("Opening"))
            {
                dbGame.Opening = game.Infos["Opening"];
            }
            if (game.Infos.ContainsKey("Annotator"))
            {
                dbGame.Annotator = game.Infos["Annotator"];
            }
            if (game.Infos.ContainsKey("TimeControl"))
            {
                dbGame.TimeControl = game.Infos["TimeControl"];
            }
            if (game.Infos.ContainsKey("Result"))
            {
                dbGame.Result = game.Infos["Result"] switch
                {
                    "0-1" => Result.BlackWin,
                    "1-0" => Result.WhiteWin,
                    "1/2-1/2" => Result.Draw,
                    _ => Result.None
                };
            }
            if (game.Infos.ContainsKey("Termination"))
            {
                dbGame.Termination = game.Infos["Termination"] switch
                {
                    "Normal" => Termination.Agreed,
                    "Time forfeit" => Termination.Time,
                    _ => Termination.None
                };
            }
            dbGame.HalfMoves = game.State.Moves.Count;
            dbGame.EngineMoves = String.Concat(game.State.Moves.Select(s => Map.GetEngineMoveString(s)));
        }

        return dbGame;
    }
}
