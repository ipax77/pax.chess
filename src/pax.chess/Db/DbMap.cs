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
        var moves = GetEngineMoves(dbGame.EngineMoves);
        foreach (var move in moves)
        {
            var state = game.Move(move);
            if (state != MoveState.Ok)
            {
                throw new Exception($"failed executing db enginemove: {move}");
            }
        }

        game.Result = dbGame.Result;
        game.Termination = dbGame.Termination;

        if (dbGame.Variations != null)
        {
            foreach (var dbVariation in dbGame.Variations)
            {
                var variation = GetVariation(game, dbVariation.StartMove, dbVariation.EngineMoves);
                if (dbVariation.Evaluation != null)
                {
                    variation.Evaluation = new Evaluation(dbVariation.Evaluation.Score, dbVariation.Evaluation.Mate, dbVariation.Evaluation.IsBlack);
                }
                // todo subvariations
            }
        }
        if (game.State.Moves.Any())
        {
            game.ObserverMoveTo(game.State.Moves.First());
            game.ObserverMoveBackward();
        }
        return game;
    }

    public static Variation GetVariation(Game game, int startMoveId, string engineMoves)
    {
        var moves = GetEngineMoves(engineMoves);
        
        var startMove = game.State.Moves[startMoveId];
        game.ObserverMoveTo(startMove);
        game.ObserverMoveBackward();
        for (int i = 0; i < moves.Count; i++)
        {
            game.VariationMove(moves[i]);
        }
        return game.Variations[startMove].Last();
    }

    public static Game GetGame(string engineMoves)
    {
        Game game = new Game();

        var moves = GetEngineMoves(engineMoves);
        foreach (var move in moves)
        {
            var state = game.Move(move);
            if (state != MoveState.Ok)
            {
                throw new Exception($"failed executing db enginemove: {move}");
            }
        }
        return game;
    }

    public static List<EngineMove> GetEngineMoves(string engineMoves)
    {
        List<EngineMove> moves = new List<EngineMove>();
        Match m = moveRx.Match(engineMoves);
        while (m.Success)
        {
            EngineMove? move = Map.GetEngineMove(m.Groups[1].Value);
            if (move != null)
            {
                moves.Add(move);
            }
            else
            {
                throw new Exception($"failed mapping db enginemove: {m.Groups[1].Value}");
            }
            m = m.NextMatch();
        }
        return moves;
    }

    public static DbGame GetGame(Game game)
    {
        DbGame dbGame = new DbGame();
        SetGameInfo(dbGame, game);
        dbGame.HalfMoves = game.State.Moves.Count;
        dbGame.EngineMoves = String.Concat(game.State.Moves.Select(s => Map.GetEngineMoveString(s)));
        return dbGame;
    }

    public static void SetGameInfo(DbGame dbGame, Game game)
    {
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
        }
    }

    public static List<DbVariation> GetVariations(Game game)
    {
        List<DbVariation> variations = new List<DbVariation>();
        foreach (var ent in game.Variations)
        {
            foreach (var variation in ent.Value.Where(x => x.RootVariation == null))
            {
                DbEvaluation? dbEvaluation = null;
                if (variation.Evaluation != null)
                {
                    dbEvaluation = new DbEvaluation()
                    {
                        Score = (short)variation.Evaluation.Score,
                        Mate = (sbyte)variation.Evaluation.Mate,
                        IsBlack = variation.Evaluation.IsBlack,
                    };
                }
                DbVariation dbVariation = new DbVariation()
                {
                    StartMove = variation.StartMove,
                    EngineMoves = String.Concat(variation.Moves.Select(s => s.ToString())),
                    Evaluation = dbEvaluation
                };

                foreach (var subvariation in ent.Value.Where(x => x.RootVariation == variation))
                {
                    // todo recursive subsub search
                    dbVariation.SubVariations.Add(new DbSubVariation()
                    {
                        RootStartMove = subvariation.RootStartMove,
                        EngineMovesWithSubs = String.Concat(subvariation.Moves.Select(s => s.ToString())),
                        RootVariation = dbVariation
                    });
                }
                variations.Add(dbVariation);
            }
        }
        return variations;
    }
}
