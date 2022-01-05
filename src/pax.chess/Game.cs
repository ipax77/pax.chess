using pax.chess.Validation;
using System.Text;

namespace pax.chess;

public class Game
{
    public string Name { get; set; } = String.Empty;
    public Guid Guid { get; private set; }

    public int ObserverMove { get; private set; }
    public Time Time { get; set; }
    public Result Result { get; set; }
    public Termination Termination { get; set; }
    public Dictionary<string, string> Infos { get; internal set; } = new Dictionary<string, string>();
    public State State { get; internal set; } = new State();
    public State ObserverState { get; internal set; } = new State();
    public Variation? CurrentVariation { get; private set; }

    public Game(string? fen = null)
    {
        State = Fen.MapString(fen);
        ObserverState = new(State);
        Guid = Guid.NewGuid();
        Time = new Time(TimeSpan.FromMinutes(3), TimeSpan.FromSeconds(2));
    }

    public Game(Game game)
    {
        State = new(game.State);
        ObserverState = new(game.ObserverState);
        ObserverMove = game.ObserverMove;
        Guid = Guid.NewGuid();
        Time = new Time(TimeSpan.FromMinutes(3), TimeSpan.FromSeconds(2));
    }

    public void LoadFen(string fen)
    {
        State = Fen.MapString(fen);
        ObserverState = Fen.MapString(fen);
        ObserverMove = 0;
    }

    public void LoadPgn(string pgn)
    {
        var game = Pgn.MapString(pgn);
        Infos = game.Infos;
        State = game.State;
        ObserverState = new(State);
        ObserverMove = ObserverState.Moves.Count;
    }

    public void LoadPgn(string[] pgnLines)
    {
        var game = Pgn.MapStrings(pgnLines);
        Infos = game.Infos;
        State = game.State;
        ObserverState = new(State);
    }

    public void Reset()
    {
        State = Fen.MapString();
        ObserverState = Fen.MapString();
        ObserverMove = 0;
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
        if (State.Moves.Count >= ObserverMove + 1)
        {
            ObserverState.ExecuteMove(State.Moves[ObserverMove].EngineMove);
            ObserverMove++;
        }
    }

    public void ObserverMoveBackward()
    {
        if (ObserverMove > 0)
        {
            ObserverMove--;
            ObserverState.RevertMove();
        }
    }

    public void ObserverMoveTo(int move)
    {
        if (ObserverMove > move)
        {
            while (ObserverMove > move)
            {
                ObserverMoveBackward();
            }
        }
        else if (ObserverMove < move)
        {
            while (ObserverMove < move)
            {
                ObserverMoveForward();
            }
        }
    }

    public void ObserverMoveTo(Move move)
    {
        ObserverState.Moves.ForEach(f => f.Variation = null);

        if (move.Variation == null)
        {
            ObserverMoveTo(move.HalfMoveNumber);
        }
        else
        {
            ObserverMoveTo(0);

            List<Variation> variations = new List<Variation>() { move.Variation };
            Move startMove = move.Variation.StartMove;
            
            while (startMove.Variation != null)
            {
                variations.Add(startMove.Variation);
                startMove = startMove.Variation.StartMove;
            }

            for (int i = 0; i < startMove.HalfMoveNumber; i++)
            {
                ObserverState.ExecuteMove(State.Moves[i].EngineMove);
            }

            for (int i = variations.Count - 1; i >= 0; i--)
            {
                for (int j = 0; i < variations[i].Moves.Count; j++)
                {
                    var obsMove = ObserverState.ExecuteMove(variations[i].Moves[j].EngineMove);
                    obsMove.Variation = variations[i];
                }
            }
        }
    }

    public MoveState VariationMove(Piece piece, int x, int y, PieceType? transformation)
    {
        if (!State.Moves.Any())
        {
            return Move(piece, x, y, transformation);
        }

        Move startMove;
        if (ObserverState.CurrentMove == null)
        {
            startMove = State.Moves[0];
        }
        else if (ObserverState.CurrentMove.Variation == null)
        {
            startMove = State.Moves.First(f => f.HalfMoveNumber == ObserverState.CurrentMove.HalfMoveNumber);
        }
        else if (ObserverState.CurrentMove.Variation.Moves.Last() != ObserverState.CurrentMove)
        {
            startMove = ObserverState.CurrentMove.Variation.Moves.First(f => f.HalfMoveNumber == ObserverState.CurrentMove.HalfMoveNumber);
        }
        else
        {
            startMove = ObserverState.CurrentMove.Variation.StartMove;
        }

        EngineMove move = new EngineMove(piece.Position, new Position(x, y), transformation);
        var moveState = Validate.TryExecuteMove(move, ObserverState, move.Transformation);
        if (moveState == MoveState.Ok && ObserverState.CurrentMove != null)
        {
            if (ObserverState.Info.IsCheckMate)
            {
                Termination = Termination.Mate;
                Result = ObserverState.Info.BlackToMove ? Result.WhiteWin : Result.BlackWin;
            }

            if (startMove.Variation == null)
            {
                startMove.Variation = new Variation(startMove, ObserverState.CurrentMove);
                startMove.Variations.Add(startMove.Variation);
                ObserverState.CurrentMove.Variation = startMove.Variation;
            }
            else
            {
                startMove.Variation.Moves.Add(ObserverState.CurrentMove);
                ObserverState.CurrentMove.Variation = startMove.Variation;
            }
        }
        return moveState;
    }


}
