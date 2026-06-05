namespace pax.chess.Validation;

public static class PseudoMoveValidator
{
    public static MoveState IsValidMove(Move move, BoardPosition pos)
    {
        ArgumentNullException.ThrowIfNull(move);
        ArgumentNullException.ThrowIfNull(pos);

        var maybePiece = pos.Board[move.From.Index];

        if (maybePiece is not { } piece)
            return MoveState.PieceNotFound;

        if (piece.Color != pos.SideToMove)
            return MoveState.WrongColor;

        if (!IsPseudoLegalMove(move.From, move.To, piece, pos))
            return MoveState.TargetInvalid;

        bool kingCastlingShape = piece.Type == PieceType.King && IsKingCastlingShape(move.From, move.To, piece.Color);
        bool castlingMove = move.MoveType.HasFlag(MoveType.CastlingKingSide) ||
                            move.MoveType.HasFlag(MoveType.CastlingQueenSide);

        if (kingCastlingShape)
        {
            bool kingSide = castlingMove
                ? move.MoveType.HasFlag(MoveType.CastlingKingSide)
                : move.To.File > move.From.File;

            var requiredRight = (piece.Color, kingSide) switch
            {
                (PieceColor.White, true) => CastlingRights.WhiteKingSide,
                (PieceColor.White, false) => CastlingRights.WhiteQueenSide,
                (PieceColor.Black, true) => CastlingRights.BlackKingSide,
                (PieceColor.Black, false) => CastlingRights.BlackQueenSide,
                _ => CastlingRights.None
            };

            if (!pos.CastlingRights.HasFlag(requiredRight))
                return castlingMove ? MoveState.CastleNotAllowed : MoveState.TargetInvalid;

            if (castlingMove && !IsCastlingPathSafe(move.From, move.To, piece.Color, pos))
                return MoveState.CastlingPathAttacked;
        }
        else if (castlingMove)
        {
            return MoveState.TargetInvalid;
        }

        var newPos = pos.MakeMove(move);

        var kingSquare = piece.Type == PieceType.King
            ? move.To
            : pos.Board.GetKingSquare(piece.Color);

        if (MoveValidator.IsSquareAttacked(kingSquare, piece.Color, newPos))
            return MoveState.WouldBeCheck;

        return MoveState.Ok;
    }

    private static bool IsPseudoLegalMove(
        Square from,
        Square to,
        Piece piece,
        BoardPosition pos)
    {
        return piece.Type switch
        {
            PieceType.Pawn => IsPseudoLegalPawnMove(from, to, piece, pos),
            PieceType.Knight => IsPseudoLegalKnightMove(from, to, piece, pos),
            PieceType.Bishop => IsPseudoLegalBishopMove(from, to, piece, pos),
            PieceType.Rook => IsPseudoLegalRookMove(from, to, piece, pos),
            PieceType.Queen => IsPseudoLegalQueenMove(from, to, piece, pos),
            PieceType.King => IsPseudoLegalKingMove(from, to, piece, pos),
            _ => false
        };
    }

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

    private static bool IsClearPathAndValidTarget(
        Square from,
        Square to,
        PieceColor movingColor,
        BoardPosition pos)
    {
        int fileDelta = Math.Sign(to.File - from.File);
        int rankDelta = Math.Sign(to.Rank - from.Rank);

        int file = from.File + fileDelta;
        int rank = from.Rank + rankDelta;

        while (file != to.File || rank != to.Rank)
        {
            var square = new Square(file, rank);

            if (pos.Board[square.Index].HasValue)
                return false;

            file += fileDelta;
            rank += rankDelta;
        }

        var target = pos.Board[to.Index];

        return target is not { } targetPiece || targetPiece.Color != movingColor;
    }

    private static bool IsKingCastlingShape(Square from, Square to, PieceColor color)
    {
        int homeRank = color == PieceColor.White ? 0 : 7;

        return from.File == 4 &&
               from.Rank == homeRank &&
               to.Rank == homeRank &&
               (to.File == 6 || to.File == 2);
    }

    private static bool IsCastlingPathClear(Square from, Square to, BoardPosition pos)
    {
        int direction = to.File > from.File ? 1 : -1;
        int file = from.File + direction;

        while (file != to.File + direction)
        {
            var square = new Square(file, from.Rank);

            if (pos.Board[square.Index].HasValue)
                return false;

            file += direction;
        }

        if (to.File == 2)
        {
            var queenSideRookNeighbor = new Square(1, from.Rank);
            return !pos.Board[queenSideRookNeighbor.Index].HasValue;
        }

        return true;
    }

    private static bool IsCastlingPathSafe(Square from, Square to, PieceColor color, BoardPosition pos)
    {
        int direction = to.File > from.File ? 1 : -1;
        var passingSquare = new Square(from.File + direction, from.Rank);

        return !MoveValidator.IsSquareAttacked(from, color, pos) &&
               !MoveValidator.IsSquareAttacked(passingSquare, color, pos) &&
               !MoveValidator.IsSquareAttacked(to, color, pos);
    }
}
