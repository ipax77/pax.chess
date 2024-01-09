namespace pax.chess.Validation;

public static partial class Validate
{
    public static MoveState ValidateBoardMove(ChessBoard chessBoard,
                                         Position from,
                                         Position to,
                                         PieceType? transformation = null)
    {
        ArgumentNullException.ThrowIfNull(chessBoard);
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        if (from.OutOfBounds || to.OutOfBounds)
        {
            return MoveState.OutOfBounds;
        }

        var pieceTomove = chessBoard.GetPieceAt(from);

        if (pieceTomove == null)
        {
            return MoveState.PieceNotFound;
        }

        if (chessBoard.BlackToMove != pieceTomove.IsBlack)
        {
            return MoveState.WrongColor;
        }

        var possibleMoves = GetMoves(pieceTomove, chessBoard);

        if (possibleMoves.Count == 0)
        {
            return MoveState.TargetInvalid;
        }

        if (!possibleMoves.Contains(to))
        {
            return MoveState.TargetInvalid;
        }

        return MoveState.Ok;
    }

    private static List<Position> GetMoves(Piece piece, ChessBoard chessBoard)
    {
        return piece.Type switch
        {
            PieceType.Pawn => GetPossiblePawnMoves(piece, chessBoard),
            PieceType.Knight => GetPossibleKnightMoves(piece, chessBoard),
            PieceType.Bishop => GetPossibleBishopMoves(piece, chessBoard),
            PieceType.Rook => GetPossibleRookMoves(piece, chessBoard),
            PieceType.Queen => GetPossibleQueenMoves(piece, chessBoard),
            PieceType.King => GetPossibleKingMoves(piece, chessBoard),
            _ => throw new ArgumentOutOfRangeException($"unknown piece type {piece.Type}")
        };
    }

    public static bool IsCheck(ChessBoard chessBoard)
    {
        ArgumentNullException.ThrowIfNull(chessBoard);

        var king = chessBoard.Pieces
            .OfType<Piece>()
            .FirstOrDefault(x => x.Type == PieceType.King && x.IsBlack == chessBoard.BlackToMove);

        ArgumentNullException.ThrowIfNull(king);

        var possibleCheckers = chessBoard.Pieces.OfType<Piece>()
                            .Where(x => x != null && x.IsBlack != king.IsBlack)
                            .ToList();

        return possibleCheckers.Any(possibleChecker =>
        {
            var possibleMoves = GetMoves(possibleChecker, chessBoard);
            return possibleMoves.Contains(king.Position);
        });
    }

    public static bool IsCheckMate(ChessBoard chessBoard)
    {
        if (!IsCheck(chessBoard))
        {
            return false;
        }

        var pieces = chessBoard.Pieces.OfType<Piece>().Where(x => x != null && x.IsBlack == chessBoard.BlackToMove).ToList();
        var king = pieces.First(x => x.Type == PieceType.King);
        var kingMoves = Validate.GetPossibleKingMoves(king, chessBoard);

        if (kingMoves.Any(move => !WouldBeCheck(chessBoard, king.Position, move)))
        {
            return false;
        }

        foreach (var piece in pieces.Where(p => p.Type != PieceType.King))
        {
            var possibleMoves = GetMoves(piece, chessBoard);
            if (possibleMoves.Any(move => !WouldBeCheck(chessBoard, piece.Position, move)))
            {
                return false;
            }
        }

        return true;
    }

    public static bool WouldBeCheck(ChessBoard chessBoard,
                                    Position from,
                                    Position to,
                                    PieceType? transformation = null)
    {
        ArgumentNullException.ThrowIfNull(chessBoard);
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        if (from.OutOfBounds)
        {
            throw new ArgumentOutOfRangeException(nameof(from));
        }

        if (to.OutOfBounds)
        {
            throw new ArgumentOutOfRangeException(nameof(to));
        }

        var pieceToMove = chessBoard.GetPieceAt(from);

        ArgumentNullException.ThrowIfNull(pieceToMove);

        var pieces = chessBoard.Pieces.ToArray();
        var capture = pieces[to.Index()];

        pieces[from.Index()] = null;
        pieces[to.Index()] = pieceToMove with { Position = to };

        // promotion
        if (pieceToMove.Type == PieceType.Pawn
            && (pieceToMove.IsBlack && to.Y == 0 || !pieceToMove.IsBlack && to.Y == 7))
        {
            ArgumentNullException.ThrowIfNull(transformation);
            pieceToMove.Type = transformation.Value;
        }
        // enpassant
        else if (pieceToMove.Type == PieceType.Pawn
            && chessBoard.EnPassantPosition is not null
            && from.X != to.X && capture is null)
        {
            pieces[new Position(to.X, chessBoard.BlackToMove ? to.Y + 1 : to.Y - 1).Index()] = null;
        }

        var newBoard = chessBoard with { Pieces = pieces };

        var king = pieces
            .OfType<Piece>()
            .FirstOrDefault(x => x.Type == PieceType.King && x.IsBlack == chessBoard.BlackToMove);

        ArgumentNullException.ThrowIfNull(king);

        var possibleCheckers = pieces.OfType<Piece>()
                                    .Where(x => x != null && x.IsBlack != king.IsBlack)
                                    .ToList();

        if (IsCastleMove(pieceToMove.Type, from, to))
        {
            return CheckCastleWouldBeCheck(from, to, possibleCheckers, newBoard);
        }

        return possibleCheckers.Any(possibleChecker =>
        {
            var possibleMoves = GetMoves(possibleChecker, newBoard);
            return possibleMoves.Contains(king.Position);
        });
    }

    private static bool IsCastleMove(PieceType pieceType, Position from, Position to)
    {
        return pieceType == PieceType.King && Math.Abs(from.X - to.X) > 1;
    }

    private static bool CheckCastleWouldBeCheck(Position from, Position to, List<Piece> possibleCheckers, ChessBoard chessBoard)
    {
        Position intermediateSquare;

        if (to.X > from.X) // King-side castling
        {
            intermediateSquare = new Position((byte)5, from.Y);
        }
        else // Queen-side castling
        {
            intermediateSquare = new Position((byte)3, from.Y);
        }

        // Check if any of the squares is under attack
        return possibleCheckers.Any(possibleChecker =>
        {
            var possibleMoves = GetMoves(possibleChecker, chessBoard);
            return possibleMoves.Contains(from)
                || possibleMoves.Contains(intermediateSquare)
                || possibleMoves.Contains(to);
        });
    }
}
