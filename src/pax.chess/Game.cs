using pax.chess.Validation;
using System.Text;

namespace pax.chess;

public class Game
{
    public string Name { get; set; } = String.Empty;
    public Guid Guid { get; private set; }

    public Time Time { get; set; }
    public Result Result { get; set; }
    public Termination Termination { get; set; }
    public Dictionary<string, string> Infos { get; internal set; } = new Dictionary<string, string>();
    public State State { get; internal set; } = new State();
    public State ObserverState { get; internal set; } = new State();
    public Dictionary<Move, List<Variation>> Variations { get; init; } = new Dictionary<Move, List<Variation>>();
    public string? StartFen { get; private set; }

    public Game(string? fen = null)
    {
        State = Fen.MapString(fen);
        ObserverState = new(State);
        Guid = Guid.NewGuid();
        Time = new Time(TimeSpan.FromMinutes(3), TimeSpan.FromSeconds(2));
        StartFen = fen;
    }

    public Game(Game game)
    {
        State = new(game.State);
        ObserverState = new(game.ObserverState);
        Guid = Guid.NewGuid();
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
        EngineMove engineMove = new EngineMove(new(piece.Position), new Position(x, y), transformation);
        return Move(engineMove, dry);
    }

    public MoveState Move(Move move, bool dry = false)
    {
        EngineMove engineMove = new EngineMove(new(move.OldPosition), new(move.NewPosition), move.Transformation);
        return Move(engineMove, dry);
    }

    public MoveState Move(EngineMove move, bool dry = false)
    {
        if (dry)
        {
            State.ExecuteMove(move);
            return MoveState.Ok;
        }
        var moveState = Validate.TryExecuteMove(move, State, move.Transformation);
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

    public string PgnMove(Move move, State state)
    {
        if (move.IsCastle)
        {
            if (move.NewPosition.X == 2)
            {
                return "O-O-O";
            }
            else
            {
                return "O-O";
            }
        }
        StringBuilder sb = new StringBuilder();
        if (move.Piece.Type != PieceType.Pawn)
        {
            sb.Append(Map.GetPieceString(move.Piece.Type).ToUpper());
        }
        else if (move.Capture != null)
        {
            sb.Append(Map.GetCharColumn(move.OldPosition.X));
            sb.Append('x');
        }
        if (move.Piece.Type == PieceType.Knight || move.Piece.Type == PieceType.Rook)
        {
            var alternatePiece = state.Pieces.FirstOrDefault(x => x.Type == move.Piece.Type && x.IsBlack == move.Piece.IsBlack && x != move.Piece);
            if (alternatePiece != null)
            {
                var positions = Validation.Validate.GetMoves(alternatePiece, state);
                if (positions.Contains(move.NewPosition))
                {

                    if (alternatePiece.Position.X == move.OldPosition.X)
                    {
                        sb.Append(move.OldPosition.Y + 1);
                    }
                    else if (alternatePiece.Position.Y == move.OldPosition.Y)
                    {
                        sb.Append(Map.GetCharColumn(move.OldPosition.X));
                    }
                    else
                    {
                        sb.Append(Map.GetCharColumn(move.OldPosition.X));
                    }
                }
            }
        }
        if (move.Capture != null && move.Piece.Type != PieceType.Pawn)
        {
            sb.Append('x');
        }
        sb.Append(Map.GetCharColumn(move.NewPosition.X));
        sb.Append(move.NewPosition.Y + 1);

        if (move.Transformation != null && move.Piece.Type == PieceType.Pawn)
        {
            sb.Append(Map.GetPieceString((PieceType)move.Transformation).ToUpper());
        }
        return sb.ToString();
    }

    public void Revert()
    {
        if (State.Moves.Any())
        {
            State.RevertMove();
        }
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
                var moveState = Validate.TryExecuteMove(move.EngineMove, ObserverState, move.Transformation);
                if (moveState == MoveState.Ok && ObserverState.CurrentMove != null)
                {
                    ObserverState.CurrentMove.Variation = moveVariation;
                }
            }
        }
    }

    public void ObserverMoveBackward()
    {
        if (ObserverState.Moves.Any())
        {
            ObserverState.RevertMove();
        }
    }

    public void ObserverMoveTo(int i)
    {
        Move move = State.Moves[i];
        ObserverMoveTo(move);
    }

    public void ObserverMoveTo(Move move)
    {
        List<Move> moves;
        if (move.Variation == null)
        {
            moves = State.Moves.Take(move.HalfMoveNumber + 1).ToList();
        }
        else
        {
            moves = move.Variation.StartMove > 1 ? State.Moves.GetRange(0, move.Variation.StartMove) : new List<Move>();

            List<Move> reverseMoves = new List<Move>();
            int pos = move.Variation.Moves.IndexOf(move);
            for (int i = pos; i >= 0; i--)
            {
                reverseMoves.Add(move.Variation.Moves[i]);
            }

            var variation = move.Variation;
            int rootStartMove = variation.RootStartMove;

            List<Variation> variations = new List<Variation>();
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
    }

    public MoveState VariationMove(Piece piece, int x, int y, PieceType? transformation)
    {
        EngineMove move = new EngineMove(piece.Position, new Position(x, y), transformation);
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

        var moveState = Validate.TryExecuteMove(engineMove, ObserverState, engineMove.Transformation);
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

    public void CreateVariation(int startMoveId, List<EngineMove> engineMoves, Evaluation? eval)
    {
        var startMove = State.Moves[startMoveId];
        ObserverMoveTo(startMove);
        ObserverMoveBackward();
        for (int i = 0; i < engineMoves.Count; i++)
        {
            VariationMove(engineMoves[i]);
        }
        Variations[startMove].Last().Evaluation = eval;
    }

}
