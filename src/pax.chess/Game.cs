using pax.chess.Validation;
using System.Collections.Generic;

namespace pax.chess;

public class Game
{
    public string Name { get; set; } = String.Empty;
    public Guid GameGuid { get; private set; }

    public Time Time { get; set; }
    public Result Result { get; set; }
    public Termination Termination { get; set; }
    public Dictionary<string, string> Infos { get; internal set; } = new Dictionary<string, string>();
    public State State { get; internal set; } = new State();
    public State ObserverState { get; internal set; } = new State();
    public Dictionary<Move, List<Variation>> Variations { get; init; } = new Dictionary<Move, List<Variation>>();
    public Dictionary<int, List<Variation>> ReviewVariations { get; init; } = new Dictionary<int, List<Variation>>();
    public string? StartFen { get; private set; }
    public event EventHandler<EventArgs>? ObserverMoved;

    public Game(string? fen = null)
    {
        State = Fen.MapString(fen);
        ObserverState = new(State);
        GameGuid = Guid.NewGuid();
        Time = new Time(TimeSpan.FromMinutes(3), TimeSpan.FromSeconds(2));
        StartFen = fen;
    }

    public Game(Game game)
    {
        if (game == null)
        {
            throw new ArgumentNullException(nameof(game));
        }
        State = new(game.State);
        ObserverState = new(game.ObserverState);
        GameGuid = Guid.NewGuid();
        Time = new Time(TimeSpan.FromMinutes(3), TimeSpan.FromSeconds(2));
    }

    public void LoadFen(string fen)
    {
        State = Fen.MapString(fen);
        ObserverState = Fen.MapString(fen);
        StartFen = fen;
    }

    public void LoadPgn(string pgn)
    {
        var game = Pgn.MapString(pgn);
        Infos = game.Infos;
        State = game.State;
        ObserverState = new(State);
        ObserverMoveTo(0);
    }

    public void LoadPgn(string[] pgnLines)
    {
        var game = Pgn.MapStrings(pgnLines);
        Infos = game.Infos;
        State = game.State;
        ObserverState = new(State);
        ObserverMoveTo(0);
    }

    public void Reset()
    {
        State = Fen.MapString();
        ObserverState = Fen.MapString();
    }

    public MoveState Move(Piece piece, int x, int y, PieceType? transformation = null, bool dry = false)
    {
        if (piece == null)
        {
            throw new ArgumentNullException(nameof(piece));
        }
        EngineMove engineMove = new(new(piece.Position), new Position(x, y), transformation);
        return Move(engineMove, dry);
    }

    public MoveState Move(Move move, bool dry = false)
    {
        if (move == null)
        {
            throw new ArgumentNullException(nameof(move));
        }
        EngineMove engineMove = new(new(move.OldPosition), new(move.NewPosition), move.Transformation);
        return Move(engineMove, dry);
    }

    public MoveState Move(EngineMove move, bool dry = false)
    {
        if (move == null)
        {
            throw new ArgumentNullException(nameof(move));
        }
        if (dry)
        {
            State.ExecuteMove(move);
            return MoveState.Ok;
        }
        var moveState = Validate.TryExecuteMove(move, State);
        if (moveState == MoveState.Ok)
        {
            if (State.Info.IsCheckMate)
            {
                Termination = Termination.Mate;
                Result = State.Info.BlackToMove ? Result.WhiteWin : Result.BlackWin;
            }
            return moveState;
        }
        else
        {
            return moveState;
        }
    }

    public void Revert()
    {
        if (State.Moves.Any())
        {
            State.RevertMove();
        }
    }

    protected virtual void OnObserverMoved(EventArgs e)
    {
        ObserverMoved?.Invoke(this, e);
    }

    public void ObserverMoveForward()
    {
        if (State.Moves.Any())
        {
            if (ObserverState.CurrentMove == null)
            {
                ObserverState.ExecuteMove(State.Moves[0].EngineMove);
            }
            else if (ObserverState.CurrentMove.Variation == null && State.Moves.Count > ObserverState.Moves.Count)
            {
                ObserverState.ExecuteMove(State.Moves[ObserverState.Moves.Count].EngineMove);
            }
            else if (ObserverState.CurrentMove.Variation != null && ObserverState.CurrentMove != ObserverState.CurrentMove.Variation.Moves.Last())
            {
                int pos = ObserverState.CurrentMove.Variation.Moves.IndexOf(ObserverState.CurrentMove);
                if (pos == -1)
                {
                    return;
                }
                var move = ObserverState.CurrentMove.Variation.Moves[pos + 1];

                var moveVariation = ObserverState.CurrentMove.Variation;
                var moveState = Validate.TryExecuteMove(move.EngineMove, ObserverState);
                if (moveState == MoveState.Ok && ObserverState.CurrentMove != null)
                {
                    ObserverState.CurrentMove.Variation = moveVariation;
                }
            }
            OnObserverMoved(EventArgs.Empty);
        }
    }



    public void ObserverMoveBackward()
    {
        if (ObserverState.Moves.Any())
        {
            ObserverState.RevertMove();
            OnObserverMoved(EventArgs.Empty);
        }
    }

    public void ObserverMoveTo(int i)
    {
        Move move = State.Moves[i];
        ObserverMoveTo(move);
    }

    public void ObserverMoveTo(Move move)
    {
        if (move == null)
        {
            throw new ArgumentNullException(nameof(move));
        }
        List<Move> moves;
        if (move.Variation == null)
        {
            moves = State.Moves.Take(move.HalfMoveNumber + 1).ToList();
        }
        else
        {
            moves = move.Variation.StartMove > 1 ? State.Moves.GetRange(0, move.Variation.StartMove) : new List<Move>();

            List<Move> reverseMoves = new();
            int pos = move.Variation.Moves.IndexOf(move);
            for (int i = pos; i >= 0; i--)
            {
                reverseMoves.Add(move.Variation.Moves[i]);
            }

            var variation = move.Variation;
            int rootStartMove = variation.RootStartMove;

            List<Variation> variations = new();
            while (variation.RootVariation != null)
            {
                variations.Add(variation.RootVariation);
                variation = variation.RootVariation;
            }

            for (int i = 0; i < variations.Count; i++)
            {
                variation = variations[i];
                for (int j = rootStartMove; j >= 0; j--)
                {
                    reverseMoves.Add(variation.Moves[j]);
                }
                rootStartMove = variation.RootStartMove;
            }
            moves.AddRange(Enumerable.Reverse(reverseMoves));
        }
        ObserverState.SetObsState(moves, StartFen);
        OnObserverMoved(EventArgs.Empty);
    }

    public MoveState VariationMove(Piece piece, int x, int y, PieceType? transformation)
    {
        if (piece == null)
        {
            throw new ArgumentNullException(nameof(piece));
        }
        EngineMove move = new(piece.Position, new Position(x, y), transformation);
        return VariationMove(move);
    }

    public MoveState VariationMove(EngineMove engineMove)
    {
        int startMoveId = ObserverState.CurrentMove == null ? 0 : ObserverState.CurrentMove.Variation == null ? ObserverState.CurrentMove.HalfMoveNumber + 1 : ObserverState.CurrentMove.Variation.StartMove;
        Move? startMove = State.Moves.FirstOrDefault(f => f.HalfMoveNumber == startMoveId);

        if (startMove == null)
        {
            return Move(engineMove);
        }

        if (!Variations.ContainsKey(startMove))
        {
            Variations[startMove] = new List<Variation>();
        }

        Variation moveVariation;
        if (ObserverState.CurrentMove == null || ObserverState.CurrentMove.Variation == null)
        {
            moveVariation = new Variation(startMoveId);
            Variations[startMove].Add(moveVariation);
        }
        else if (ObserverState.CurrentMove == ObserverState.CurrentMove.Variation.Moves.Last())
        {
            moveVariation = ObserverState.CurrentMove.Variation;
        }
        else
        {
            moveVariation = new Variation(startMoveId, ObserverState.CurrentMove.Variation, ObserverState.CurrentMove.Variation.Moves.IndexOf(ObserverState.CurrentMove));
            ObserverState.CurrentMove.Variation.ChildVariations.Add(moveVariation);
            Variations[startMove].Add(moveVariation);
        }

        var moveState = Validate.TryExecuteMove(engineMove, ObserverState);
        if (moveState == MoveState.Ok && ObserverState.CurrentMove != null)
        {
            ObserverState.CurrentMove.Variation = moveVariation;
            moveVariation.Moves.Add(new(ObserverState.CurrentMove));
        }
        else
        {
             //todo cleanup
        }
        return moveState;
    }

    public void CreateVariation(int startMoveId, EngineMove[] engineMoves, Evaluation? eval)
    {
        if (engineMoves == null)
        {
            throw new ArgumentNullException(nameof(engineMoves));
        }
        var startMove = State.Moves[startMoveId];
        ObserverMoveTo(startMove);
        ObserverMoveBackward();
        for (int i = 0; i < engineMoves.Length; i++)
        {
            var result = VariationMove(engineMoves[i]);
            if (result != MoveState.Ok)
            {
                return;
            }
        }
        Variations[startMove].Last().Evaluation = eval;
        OnObserverMoved(EventArgs.Empty);
    }

    public ICollection<Variation> GetCurrentReviewVariations()
    {
        if (ObserverState.CurrentMove != null && ObserverState.CurrentMove.Variation != null)
        {
            return new List<Variation>();
        }

        int currentMove = ObserverState.CurrentMove == null ? 0 : ObserverState.CurrentMove.HalfMoveNumber;
        
        if (!ReviewVariations.ContainsKey(currentMove))
        {
            return new List<Variation>();
        }
        return ReviewVariations[currentMove];
    }

}
