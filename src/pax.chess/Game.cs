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

    public List<Position> ValidPositions(Piece piece)
    {
        if (!Validate.IsMyTurn(piece, State))
        {
            return new List<Position>();
        }
        return Validate.GetMoves(piece, State);
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

    public void PvMoveForward()
    {
        if (ObserverState != null && State.Moves.Count >= ObserverMove + 1)
        {
            ObserverState.ExecuteMove(State.Moves[ObserverMove].EngineMove);
            ObserverMove++;
        }
    }

    public void PvMoveBackward()
    {
        if (ObserverState != null && ObserverMove > 0)
        {
            ObserverMove--;
            ObserverState.RevertMove();
        }
    }
}
