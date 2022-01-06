using pax.chess.Validation;

namespace pax.chess;
public record State
{
    public StateInfo Info { get; internal set; } = new StateInfo();
    public List<Piece> Pieces { get; internal set; } = new List<Piece>();
    public List<Move> Moves { get; internal set; } = new List<Move>();
    public Move? CurrentMove { get; internal set; }

    public State() { }
    public State(State state)
    {
        Info = new(state.Info);
        Pieces = new List<Piece>(state.Pieces.Select(s => new Piece(s)));
        Moves = new List<Move>(state.Moves.Select(s => new Move(s)));
    }

    internal void SetObsState(List<Move> moves, string? fen)
    {
        var state = Fen.MapString(fen);
        Info.Set(state.Info);
        Moves.Clear();
        Pieces = new List<Piece>(state.Pieces);
        for (int i = 0; i < moves.Count; i++)
        {
            ExecuteMove(moves[i].EngineMove, moves[i].Variation);
        }
        CurrentMove = Moves.LastOrDefault();
    }

    internal Move ExecuteMove(EngineMove engineMove, Variation? variation = null)
    {
        Piece? pieceToMove = Pieces.SingleOrDefault(f => f.Position == engineMove.OldPosition);
        if (pieceToMove == null)
        {
            throw new Exception($"No piece found at position {engineMove.OldPosition}");
        }

        Move move = new Move(pieceToMove, engineMove.NewPosition, Moves.Count, engineMove.Transformation);
        move.StateInfo = new(Info);
        move.PgnMove = Pgn.MapPiece(move, this);

        Info.BlackToMove = !Info.BlackToMove;

        if (move.IsCastle)
        {
            bool KingSide = move.OldPosition.X < move.NewPosition.X;
            if (KingSide)
            {
                var rook = move.Piece.IsBlack ?
                      Pieces.First(f => f.Position.X == 7 && f.Position.Y == 7)
                    : Pieces.First(f => f.Position.X == 7 && f.Position.Y == 0);
                rook.Position = move.Piece.IsBlack ?
                      new Position(5, 7)
                    : new Position(5, 0);
            }
            else
            {
                var rook = move.Piece.IsBlack ?
                      Pieces.First(f => f.Position.X == 0 && f.Position.Y == 7)
                    : Pieces.First(f => f.Position.X == 0 && f.Position.Y == 0);
                rook.Position = move.Piece.IsBlack ?
                      new Position(3, 7)
                    : new Position(3, 0);
            }
        }
        else
        {
            move.Capture = Pieces.SingleOrDefault(f => f.Position == move.NewPosition);
            if (move.Capture != null)
            {
                Pieces.Remove(move.Capture);
            }
            else if (move.Piece.Type == PieceType.Pawn && move.OldPosition.X != move.NewPosition.X && Info.EnPassantPosition != null)
            {
                var delta = move.Piece.IsBlack ? 1 : -1;
                move.Capture = Pieces.First(f => f.Position == new Position(Info.EnPassantPosition.X, Info.EnPassantPosition.Y + delta));
                Pieces.Remove(move.Capture);
            }
            if (move.Transformation != null)
            {
                move.Piece.Type = (PieceType)move.Transformation;
            }
        }
        move.Piece.Position = move.NewPosition;

        if (move.Piece.Type == PieceType.Pawn || (move.Capture != null && move.Capture.Type == PieceType.Pawn))
        {
            Info.PawnHalfMoveClock = 0;
        }
        else
        {
            Info.PawnHalfMoveClock++;
        }
        if (move.Piece.Type == PieceType.Pawn && Math.Abs(move.OldPosition.Y - move.NewPosition.Y) > 1)
        {
            var delta = move.Piece.IsBlack ? -1 : 1;
            Info.EnPassantPosition = new Position(move.OldPosition.X, move.OldPosition.Y + delta);
        }
        else
        {
            Info.EnPassantPosition = null;
        }

        if (move.Piece.IsBlack && (Info.BlackCanCastleKingSide || Info.BlackCanCastleQueenSide))
        {
            if (move.Piece.Type == PieceType.King)
            {
                Info.BlackCanCastleKingSide = false;
                Info.BlackCanCastleQueenSide = false;
            }
            else if (move.Piece.Type == PieceType.Rook)
            {
                if (move.OldPosition.Y == 7)
                {
                    if (move.OldPosition.X == 0)
                    {
                        Info.BlackCanCastleQueenSide = false;
                    }
                    else if (move.OldPosition.X == 7)
                    {
                        Info.BlackCanCastleKingSide = false;
                    }
                }
            }
        }
        else if (!move.Piece.IsBlack && (Info.WhiteCanCastleKingSide || Info.WhiteCanCastleQueenSide))
        {
            if (move.Piece.Type == PieceType.King)
            {
                Info.WhiteCanCastleKingSide = false;
                Info.WhiteCanCastleQueenSide = false;
            }
            else if (move.Piece.Type == PieceType.Rook)
            {
                if (move.OldPosition.Y == 0)
                {
                    if (move.OldPosition.X == 0)
                    {
                        Info.WhiteCanCastleQueenSide = false;
                    }
                    else if (move.OldPosition.X == 7)
                    {
                        Info.WhiteCanCastleKingSide = false;
                    }
                }
            }
        }
        move.Variation = variation;
        CurrentMove = move;
        Moves.Add(move);
        return move;
    }

    public void RevertMove()
    {
        var move = Moves.Last();
        Piece piece = Pieces.Single(s => s.Position == move.NewPosition);

        if (move.IsCastle)
        {
            bool KingSide = move.OldPosition.X < move.NewPosition.X;
            if (KingSide)
            {
                var rook = move.Piece.IsBlack ?
                      Pieces.First(f => f.Position.X == 5 && f.Position.Y == 7)
                    : Pieces.First(f => f.Position.X == 5 && f.Position.Y == 0);
                rook.Position = move.Piece.IsBlack ?
                      new Position(7, 7)
                    : new Position(7, 0);
            }
            else
            {
                var rook = move.Piece.IsBlack ?
                      Pieces.First(f => f.Position.X == 3 && f.Position.Y == 7)
                    : Pieces.First(f => f.Position.X == 3 && f.Position.Y == 0);
                rook.Position = move.Piece.IsBlack ?
                      new Position(0, 7)
                    : new Position(0, 0);
            }
        }
        else
        {
            if (move.Capture != null)
            {
                Pieces.Add(move.Capture);
            }
            if (move.Transformation != null)
            {
                move.Piece.Type = PieceType.Pawn;
            }
        }
        
        piece.Position = move.OldPosition;
        // move.Piece.Position = move.OldPosition;
        
        Info.Set(move.StateInfo);
        Moves.RemoveAt(Moves.Count - 1);
        if (Moves.Any())
        {
            CurrentMove = Moves.Last();
        } else
        {
            CurrentMove = null;
        }
    }

    public bool IsCheck(Piece? king = null)
    {
        if (king == null)
        {
            king = Pieces.Single(s => s.Type == PieceType.King && s.IsBlack == Info.BlackToMove);
        }
        var possibleAttacers = Pieces.Where(x => x.IsBlack != king.IsBlack).ToArray();
        for (int i = 0; i < possibleAttacers.Length; i++)
        {
            var moves = Validate.GetMoves(possibleAttacers[i], this);
            if (moves.Contains(king.Position))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsCurrentMove(Move? move)
    {
        if (CurrentMove == null || move == null)
        {
            return false;
        }
        if (CurrentMove.Variation == null && move.Variation == null)
        {
            return CurrentMove.HalfMoveNumber == move.HalfMoveNumber;
        }
        return CurrentMove.Variation == move.Variation && CurrentMove.HalfMoveNumber == move.HalfMoveNumber;
    }

    public List<Position> ValidPositions(Piece piece)
    {
        if (!Validate.IsMyTurn(piece, this))
        {
            return new List<Position>();
        }
        return Validate.GetMoves(piece, this);
    }
}
