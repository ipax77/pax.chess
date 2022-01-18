using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Generic;

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
        if (dbGame == null)
        {
            throw new ArgumentNullException(nameof(dbGame));
        }

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


        if (dbGame.MoveEvaluations.Any())
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

                if (!game.ReviewVariations.ContainsKey(variation.StartMove))
                {
                    game.ReviewVariations[variation.StartMove] = new List<Variation>();
                }
                game.ReviewVariations[variation.StartMove].Add(variation);
            }

            foreach (var ent in game.ReviewVariations)
            {
                game.ReviewVariations[ent.Key] = ent.Value.OrderBy(o => o.Pv).ToList();
            }
        }
        game.Variations.Clear();
        game.State.Moves.ForEach(f => f.Variation = null);

        if (dbGame.Variations.Any())
        {
            foreach (var dbVariation in dbGame.Variations)
            {
                var variation = GetVariation(game, dbVariation.StartMove, dbVariation.EngineMoves);
                if (dbVariation.Evaluation != null)
                {
                    variation.Evaluation = new Evaluation(dbVariation.Evaluation.Score, dbVariation.Evaluation.Mate, false);
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

    private static Variation GetVariation(Game game, int startMoveId, string engineMoves)
    {
        if (game == null)
        {
            throw new ArgumentNullException(nameof(game));
        }

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
        if (game == null)
        {
            throw new ArgumentNullException(nameof(game));
        }
        DbGame dbGame = new();
        SetGameInfo(dbGame, game);
        dbGame.HalfMoves = game.State.Moves.Count;
        dbGame.EngineMoves = String.Concat(game.State.Moves.Select(s => Map.GetEngineMoveString(s)));
        return dbGame;
    }

    private static void SetGameInfo(DbGame dbGame, Game game)
    {
        if (game.Infos.Any())
        {
            if (game.Infos.ContainsKey("Event"))
            {
                dbGame.Event = game.Infos["Event"];
            }
            if (game.Infos.ContainsKey("Link"))
            {
                dbGame.Site = game.Infos["Link"];
            }
            else if (game.Infos.ContainsKey("Site"))
            {
                dbGame.Site = game.Infos["Site"];
            }
            else
            {
                dbGame.Site = game.GameGuid.ToString();
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

    private static List<DbVariation> GetVariations(Game game)
    {
        if (game == null)
        {
            throw new ArgumentNullException(nameof(game));
        }
        List<DbVariation> variations = new();
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
                        Mate = (sbyte)variation.Evaluation.Mate
                    };
                }
                DbVariation dbVariation = new()
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
        if (dbGame == null)
        {
            throw new ArgumentNullException(nameof(dbGame));
        }
        if (game == null)
        {
            throw new ArgumentNullException(nameof(game));
        }
        SetGameInfo(dbGame, game);
        dbGame.HalfMoves = game.State.Moves.Count;
        dbGame.EngineMoves = String.Concat(game.State.Moves.Select(s => Map.GetEngineMoveString(s)));
        GetVariations(game).ForEach(f => dbGame.Variations.Add(f));
        GetMoveEvaluations(game).ForEach(f => dbGame.MoveEvaluations.Add(f));
    }

    private static List<DbMoveEvaluation> GetMoveEvaluations(Game game)
    {
        if (!game.ReviewVariations.Any())
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
