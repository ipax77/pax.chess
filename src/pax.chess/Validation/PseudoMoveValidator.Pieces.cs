namespace pax.chess.Validation;

public static partial class PseudoMoveValidator
{
    private static bool IsPseudoLegalPawnMove(
        Square from,
        Square to,
        Piece piece,
        BoardPosition pos)
    {
        int direction = piece.Color == PieceColor.White ? 1 : -1;
        int startRank = piece.Color == PieceColor.White ? 1 : 6;
        int fileDelta = to.File - from.File;
        int rankDelta = to.Rank - from.Rank;

        if (fileDelta == 0)
        {
            if (rankDelta == direction)
                return !pos.Board[to.Index].HasValue;

            if (rankDelta == 2 * direction && from.Rank == startRank)
            {
                var forward = new Square(from.File, from.Rank + direction);

                return !pos.Board[forward.Index].HasValue &&
                       !pos.Board[to.Index].HasValue;
            }

            return false;
        }

        if (Math.Abs(fileDelta) != 1 || rankDelta != direction)
            return false;

        if (pos.EnPassantTarget == to)
            return true;

        var target = pos.Board[to.Index];

        return target is { } targetPiece && targetPiece.Color != piece.Color;
    }

    private static bool IsPseudoLegalKnightMove(
        Square from,
        Square to,
        Piece piece,
        BoardPosition pos)
    {
        int fileDelta = Math.Abs(to.File - from.File);
        int rankDelta = Math.Abs(to.Rank - from.Rank);

        if (!((fileDelta == 1 && rankDelta == 2) ||
              (fileDelta == 2 && rankDelta == 1)))
        {
            return false;
        }

        var target = pos.Board[to.Index];

        return target is not { } targetPiece || targetPiece.Color != piece.Color;
    }

    private static bool IsPseudoLegalBishopMove(
        Square from,
        Square to,
        Piece piece,
        BoardPosition pos)
    {
        int fileDelta = to.File - from.File;
        int rankDelta = to.Rank - from.Rank;

        if (Math.Abs(fileDelta) != Math.Abs(rankDelta))
            return false;

        return IsClearPathAndValidTarget(from, to, piece.Color, pos);
    }

    private static bool IsPseudoLegalRookMove(
        Square from,
        Square to,
        Piece piece,
        BoardPosition pos)
    {
        if (from.File != to.File && from.Rank != to.Rank)
            return false;

        return IsClearPathAndValidTarget(from, to, piece.Color, pos);
    }

    private static bool IsPseudoLegalQueenMove(
        Square from,
        Square to,
        Piece piece,
        BoardPosition pos)
    {
        int fileDelta = to.File - from.File;
        int rankDelta = to.Rank - from.Rank;

        bool diagonal = Math.Abs(fileDelta) == Math.Abs(rankDelta);
        bool straight = from.File == to.File || from.Rank == to.Rank;

        if (!diagonal && !straight)
            return false;

        return IsClearPathAndValidTarget(from, to, piece.Color, pos);
    }

    private static bool IsPseudoLegalKingMove(
        Square from,
        Square to,
        Piece piece,
        BoardPosition pos)
    {
        int fileDelta = Math.Abs(to.File - from.File);
        int rankDelta = Math.Abs(to.Rank - from.Rank);

        bool normalKingMove = fileDelta <= 1 && rankDelta <= 1 && (fileDelta != 0 || rankDelta != 0);

        if (normalKingMove)
        {
            var target = pos.Board[to.Index];
            return target is not { } targetPiece || targetPiece.Color != piece.Color;
        }

        if (!IsKingCastlingShape(from, to, piece.Color))
            return false;

        return IsCastlingPathClear(from, to, pos);
    }
}
