using System.Globalization;
using System.Text.RegularExpressions;

namespace pax.chess;

/// <summary>
/// Mapping between database and API objects
/// </summary>
public static class DbMap
{
    private static readonly Regex moveRx = new(@"(\w\d\w\d[QRNB]?)");

    /// <summary>
    /// Converts database object to API object
    /// </summary>
    public static Game GetGame(DbGame dbGame, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(dbGame);

        Game game = new();
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
                throw new MoveException($"failed executing db enginemove: {move}");
            }
        }

        game.Result = dbGame.Result;
        game.Termination = dbGame.Termination;


        if (dbGame.MoveEvaluations.Count != 0)
        {
            foreach (var eval in dbGame.MoveEvaluations)
            {
                var variation = GetVariation(game, eval.StartMove, eval.EngineMoves);
                variation.Pv = eval.Pv;
                variation.Evaluation = new((int)(eval.Score * 100.0m), 0, false);
                variation.Evaluation.MoveQuality = eval.MoveQuality;
                if (variation.Pv == 1)
                {
                    game.State.Moves[variation.StartMove].Evaluation = variation.Evaluation;
                }

                if (!game.ReviewVariations.TryGetValue(variation.StartMove, out List<Variation>? value))
                {
                    value = new List<Variation>();
                    game.ReviewVariations[variation.StartMove] = value;
                }

                value.Add(variation);
            }

            foreach (var ent in game.ReviewVariations)
            {
                game.ReviewVariations[ent.Key] = ent.Value.OrderBy(o => o.Pv).ToList();
            }
        }
        game.Variations.Clear();
        game.State.Moves.ForEach(f => f.Variation = null);

        if (dbGame.Variations.Count != 0)
        {
            foreach (var dbVariation in dbGame.Variations.Where(x => x.EngineMoves.Length != 0))
            {
                var variation = GetVariation(game, dbVariation.StartMove, dbVariation.EngineMoves);
                if (dbVariation.Evaluation != null)
                {
                    variation.Evaluation = new Evaluation(dbVariation.Evaluation.Score, dbVariation.Evaluation.Mate, false);
                }
                // todo subvariations
            }
        }

        SetGameInfo(game, dbGame);
        game.Result = dbGame.Result;
        game.Termination = dbGame.Termination;

        if (game.State.Moves.Count != 0)
        {
            game.ObserverMoveTo(game.State.Moves.First());
            game.ObserverMoveBackward();
        }
        return game;
    }

    private static Variation GetVariation(Game game, int startMoveId, string engineMoves)
    {
        ArgumentNullException.ThrowIfNull(game);

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

    /// <summary>
    /// Converts database move string to Game object
    /// </summary>
    public static Game GetGame(string engineMoves)
    {
        Game game = new();

        var moves = GetEngineMoves(engineMoves);
        foreach (var move in moves)
        {
            var state = game.Move(move);
            if (state != MoveState.Ok)
            {
                throw new MoveException($"failed executing db enginemove: {move}");
            }
        }
        return game;
    }

    private static List<EngineMove> GetEngineMoves(string engineMoves)
    {
        List<EngineMove> moves = new();
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
                throw new MoveMapException($"failed mapping db enginemove: {m.Groups[1].Value}");
            }
            m = m.NextMatch();
        }
        return moves;
    }

    /// <summary>
    /// Converts API object to database object
    /// </summary>
    public static DbGame GetGame(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        DbGame dbGame = new();
        SetGameInfo(dbGame, game);
        dbGame.HalfMoves = game.State.Moves.Count;
        dbGame.EngineMoves = String.Concat(game.State.Moves.Select(s => Map.GetEngineMoveString(s)));
        dbGame.Termination = game.Termination;
        dbGame.Result = game.Result;
        return dbGame;
    }

    private static void SetGameInfo(DbGame dbGame, Game game)
    {
        if (game.Infos.Count != 0)
        {
            if (game.Infos.TryGetValue("Event", out string? value))
            {
                dbGame.Event = value;
            }
            if (game.Infos.TryGetValue("Link", out string? lvalue))
            {
                dbGame.Site = lvalue;
            }
            else if (game.Infos.TryGetValue("Site", out string? svalue))
            {
                dbGame.Site = svalue;
            }
            else
            {
                dbGame.Site = game.GameGuid.ToString();
            }
            if (game.Infos.TryGetValue("UTCDate", out string? tvalue))
            {
                dbGame.UTCDate = DateOnly.ParseExact(tvalue, "yyyy.MM.dd", CultureInfo.InvariantCulture);
            }
            if (game.Infos.TryGetValue("UTCTime", out string? dvalue))
            {
                dbGame.UTCTime = TimeOnly.ParseExact(dvalue, @"HH\:mm\:ss", CultureInfo.InvariantCulture);
            }
            if (game.Infos.TryGetValue("White", out string? wvalue))
            {
                dbGame.White = wvalue;
            }
            if (game.Infos.TryGetValue("Black", out string? bvalue))
            {
                dbGame.Black = bvalue;
            }
            if (game.Infos.ContainsKey("WhiteElo"))
            {
                if (short.TryParse(game.Infos["WhiteElo"], out short elo))
                {
                    dbGame.WhiteElo = elo;
                }
            }
            if (game.Infos.ContainsKey("BlackElo"))
            {
                if (short.TryParse(game.Infos["BlackElo"], out short elo))
                {
                    dbGame.BlackElo = elo;
                }
            }
            if (game.Infos.TryGetValue("Variant", out string? vvalue))
            {
                dbGame.Variant = vvalue switch
                {
                    "Standard" => Variant.Standard,
                    "Chess960" => Variant.Chess960,
                    _ => Variant.Unknown
                };
            }
            if (game.Infos.TryGetValue("ECO", out string? evalue))
            {
                dbGame.ECO = evalue;
            }
            if (game.Infos.TryGetValue("Opening", out string? ovalue))
            {
                dbGame.Opening = ovalue;
            }
            if (game.Infos.TryGetValue("Annotator", out string? avalue))
            {
                dbGame.Annotator = avalue;
            }
            if (game.Infos.TryGetValue("TimeControl", out string? tcvalue))
            {
                dbGame.TimeControl = tcvalue;
            }
            if (game.Infos.TryGetValue("Result", out string? rvalue))
            {
                dbGame.Result = rvalue switch
                {
                    "0-1" => Result.BlackWin,
                    "1-0" => Result.WhiteWin,
                    "1/2-1/2" => Result.Draw,
                    _ => Result.None
                };
            }
            if (game.Infos.TryGetValue("Termination", out string? tervalue))
            {
                dbGame.Termination = tervalue switch
                {
                    "Normal" => Termination.Agreed,
                    "Time forfeit" => Termination.Time,
                    _ => Termination.None
                };
            }
        }
    }

    private static void SetGameInfo(Game game, DbGame dbGame)
    {
        if (dbGame.Event != null)
        {
            game.Infos["Event"] = dbGame.Event;
        }
        if (dbGame.Site != null)
        {
            game.Infos["Site"] = dbGame.Site;
        }
        game.Infos["UTCDate"] = dbGame.UTCDate.ToString("yyyy.MM.dd", CultureInfo.InvariantCulture);
        game.Infos["UTCTime"] = dbGame.UTCTime.ToString(@"HH\:mm\:ss", CultureInfo.InvariantCulture);
        if (dbGame.White != null)
        {
            game.Infos["White"] = dbGame.White;
        }
        if (dbGame.Black != null)
        {
            game.Infos["Black"] = dbGame.Black;
        }
        game.Infos["WhiteElo"] = dbGame.WhiteElo.ToString(CultureInfo.InvariantCulture);
        game.Infos["BlackElo"] = dbGame.BlackElo.ToString(CultureInfo.InvariantCulture);
        if (dbGame.TimeControl != null)
        {
            game.Infos["TimeControl"] = dbGame.TimeControl;
        }
    }

    private static List<DbVariation> GetVariations(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        List<DbVariation> variations = new();
        foreach (var ent in game.Variations)
        {
            foreach (var variation in ent.Value.Where(x => x.RootVariation == null && x.Moves.Count != 0))
            {
                DbEvaluation? dbEvaluation = null;
                if (variation.Evaluation != null)
                {
                    dbEvaluation = new DbEvaluation()
                    {
                        Score = (short)variation.Evaluation.Score,
                        Mate = (sbyte)variation.Evaluation.Mate
                    };
                }
                DbVariation dbVariation = new()
                {
                    StartMove = variation.StartMove,
                    EngineMoves = String.Concat(variation.Moves.Select(s => s.EngineMove.ToString())),
                    Evaluation = dbEvaluation
                };

                foreach (var subvariation in ent.Value.Where(x => x.RootVariation == variation && x.Moves.Count != 0))
                {
                    // todo recursive subsub search
                    dbVariation.SubVariations.Add(new DbSubVariation()
                    {
                        RootStartMove = subvariation.RootStartMove,
                        EngineMovesWithSubs = String.Concat(subvariation.Moves.Select(s => s.EngineMove.ToString())),
                        RootVariation = dbVariation
                    });
                }
                variations.Add(dbVariation);
            }
        }
        return variations;
    }

    /// <summary>
    /// Updates existing database object with given game
    /// </summary>
    public static void UpdateDbGame(DbGame dbGame, Game game)
    {
        ArgumentNullException.ThrowIfNull(dbGame);
        ArgumentNullException.ThrowIfNull(game);
        SetGameInfo(dbGame, game);
        dbGame.HalfMoves = game.State.Moves.Count;
        dbGame.EngineMoves = String.Concat(game.State.Moves.Select(s => Map.GetEngineMoveString(s)));
        GetVariations(game).ForEach(f => dbGame.Variations.Add(f));
        GetMoveEvaluations(game).ForEach(f => dbGame.MoveEvaluations.Add(f));
    }

    private static List<DbMoveEvaluation> GetMoveEvaluations(Game game)
    {
        if (game.ReviewVariations.Count == 0)
        {
            return new List<DbMoveEvaluation>();
        }

        List<DbMoveEvaluation> evals = new();
        foreach (var ent in game.ReviewVariations)
        {
            foreach (var variation in ent.Value)
            {

                var eval = new DbMoveEvaluation()
                {
                    StartMove = variation.StartMove,
                    Pv = variation.Pv,
                    EngineMoves = String.Concat(variation.Moves.Select(s => s.EngineMove.ToString())),
                    Score = variation.Evaluation == null ? 0 : (decimal)variation.Evaluation.ChartScore(),
                };
                if (eval.Pv == 1)
                {
                    var move = game.State.Moves[variation.StartMove];
                    eval.MoveQuality = move.Evaluation == null ? MoveQuality.Unknown : move.Evaluation.MoveQuality;
                }
                evals.Add(eval);
            }
        }
        return evals;
    }
}
