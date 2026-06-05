namespace pax.chess.Validation;

public static partial class PseudoMoveValidator
{
    public static GameState GetGameState(BoardPosition pos)
    {
        ArgumentNullException.ThrowIfNull(pos);

        var sideToMove = pos.SideToMove;
        var kingSquare = pos.Board.GetKingSquare(sideToMove);
        bool inCheck = IsSquareAttacked(kingSquare, sideToMove, pos);
        bool hasLegalMove = HasAnyLegalMove(pos);

        return (inCheck, hasLegalMove) switch
        {
            (true, true) => GameState.Check,
            (false, true) => GameState.Normal,
            (true, false) => GameState.Checkmate,
            (false, false) => GameState.Stalemate
        };
    }

    private static bool HasAnyLegalMove(BoardPosition pos)
    {
        var sideToMove = pos.SideToMove;

        foreach (var (from, piece) in pos.Board.GetPieces(sideToMove))
        {
            if (HasAnyLegalMoveFrom(from, piece, pos))
                return true;
        }

        return false;
    }

    private static bool HasAnyLegalMoveFrom(Square from, Piece piece, BoardPosition pos)
    {
        return piece.Type switch
        {
            PieceType.Pawn => HasAnyLegalPawnMove(from, piece, pos),
            PieceType.Knight => HasAnyLegalKnightMove(from, piece, pos),
            PieceType.Bishop => HasAnyLegalSlidingMove(from, piece, pos, bishopDirections),
            PieceType.Rook => HasAnyLegalSlidingMove(from, piece, pos, rookDirections),
            PieceType.Queen => HasAnyLegalSlidingMove(from, piece, pos, queenDirections),
            PieceType.King => HasAnyLegalKingMove(from, piece, pos),
            _ => false
        };
    }

    private static bool HasAnyLegalPawnMove(Square from, Piece piece, BoardPosition pos)
    {
        int direction = piece.Color == PieceColor.White ? 1 : -1;
        int startRank = piece.Color == PieceColor.White ? 1 : 6;
        int forwardRank = from.Rank + direction;

        if ((uint)forwardRank <= 7)
        {
            var forward = new Square(from.File, forwardRank);

            if (!pos.Board[forward.Index].HasValue)
            {
                if (IsLegalCandidateMove(from, forward, piece, pos))
                    return true;

                int doubleRank = from.Rank + (2 * direction);

                if (from.Rank == startRank && (uint)doubleRank <= 7)
                {
                    var forward2 = new Square(from.File, doubleRank);

                    if (!pos.Board[forward2.Index].HasValue &&
                        IsLegalCandidateMove(from, forward2, piece, pos))
                    {
                        return true;
                    }
                }
            }

            if (HasAnyLegalPawnCapture(from, piece, pos, from.File - 1, forwardRank) ||
                HasAnyLegalPawnCapture(from, piece, pos, from.File + 1, forwardRank))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasAnyLegalPawnCapture(
        Square from,
        Piece piece,
        BoardPosition pos,
        int targetFile,
        int targetRank)
    {
        if ((uint)targetFile > 7)
            return false;

        var target = new Square(targetFile, targetRank);
        var occupant = pos.Board[target.Index];

        if (target != pos.EnPassantTarget &&
            (occupant is not { } targetPiece || targetPiece.Color == piece.Color))
        {
            return false;
        }

        return IsLegalCandidateMove(from, target, piece, pos);
    }

    private static bool HasAnyLegalKnightMove(Square from, Piece piece, BoardPosition pos)
    {
        foreach (int targetIndex in knightTargets[from.Index])
        {
            var targetPiece = pos.Board[targetIndex];

            if (targetPiece is { Color: var color } && color == piece.Color)
                continue;

            if (IsLegalCandidateMove(from, new Square(targetIndex), piece, pos))
                return true;
        }

        return false;
    }

    private static bool HasAnyLegalSlidingMove(
        Square from,
        Piece piece,
        BoardPosition pos,
        ReadOnlySpan<(int FileDelta, int RankDelta)> directions)
    {
        foreach (var (fileDelta, rankDelta) in directions)
        {
            int file = from.File + fileDelta;
            int rank = from.Rank + rankDelta;

            while ((uint)file <= 7 && (uint)rank <= 7)
            {
                var target = new Square(file, rank);
                var targetPiece = pos.Board[target.Index];

                if (targetPiece is { Color: var color } && color == piece.Color)
                    break;

                if (IsLegalCandidateMove(from, target, piece, pos))
                    return true;

                if (targetPiece.HasValue)
                    break;

                file += fileDelta;
                rank += rankDelta;
            }
        }

        return false;
    }

    private static bool HasAnyLegalKingMove(Square from, Piece piece, BoardPosition pos)
    {
        foreach (int targetIndex in kingTargets[from.Index])
        {
            var targetPiece = pos.Board[targetIndex];

            if (targetPiece is { Color: var color } && color == piece.Color)
                continue;

            if (IsLegalCandidateMove(from, new Square(targetIndex), piece, pos))
                return true;
        }

        return HasAnyLegalCastlingMove(from, piece, pos);
    }

    private static bool HasAnyLegalCastlingMove(Square from, Piece piece, BoardPosition pos)
    {
        int homeRank = piece.Color == PieceColor.White ? 0 : 7;

        if (from.File != 4 || from.Rank != homeRank)
            return false;

        var kingSideTarget = new Square(6, homeRank);
        if (HasCastlingRight(piece.Color, kingSide: true, pos) &&
            HasCastlingRook(new Square(7, homeRank), piece.Color, pos) &&
            IsCastlingPathClear(from, kingSideTarget, pos) &&
            IsCastlingPathSafe(from, kingSideTarget, piece.Color, pos) &&
            IsLegalCandidateMove(from, kingSideTarget, piece, pos))
        {
            return true;
        }

        var queenSideTarget = new Square(2, homeRank);
        return HasCastlingRight(piece.Color, kingSide: false, pos) &&
               HasCastlingRook(new Square(0, homeRank), piece.Color, pos) &&
               IsCastlingPathClear(from, queenSideTarget, pos) &&
               IsCastlingPathSafe(from, queenSideTarget, piece.Color, pos) &&
               IsLegalCandidateMove(from, queenSideTarget, piece, pos);
    }

    private static bool HasCastlingRight(PieceColor color, bool kingSide, BoardPosition pos)
    {
        var requiredRight = (color, kingSide) switch
        {
            (PieceColor.White, true) => CastlingRights.WhiteKingSide,
            (PieceColor.White, false) => CastlingRights.WhiteQueenSide,
            (PieceColor.Black, true) => CastlingRights.BlackKingSide,
            (PieceColor.Black, false) => CastlingRights.BlackQueenSide,
            _ => CastlingRights.None
        };

        return pos.CastlingRights.HasFlag(requiredRight);
    }

    private static bool HasCastlingRook(Square square, PieceColor color, BoardPosition pos)
    {
        return pos.Board[square.Index] is { Type: PieceType.Rook, Color: var rookColor } &&
               rookColor == color;
    }

    private static bool IsLegalCandidateMove(
        Square from,
        Square to,
        Piece piece,
        BoardPosition pos)
    {
        var moveType = MoveType.None;

        if (piece.Type == PieceType.King && IsKingCastlingShape(from, to, piece.Color))
        {
            moveType = to.File > from.File
                ? MoveType.CastlingKingSide
                : MoveType.CastlingQueenSide;
        }

        var newPos = pos.MakeMove(new Move(from, to, null, moveType));
        var kingSquare = piece.Type == PieceType.King
            ? to
            : pos.Board.GetKingSquare(piece.Color);

        return !IsSquareAttacked(kingSquare, piece.Color, newPos);
    }
}
