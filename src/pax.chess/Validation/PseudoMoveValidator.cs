namespace pax.chess.Validation;

public static partial class PseudoMoveValidator
{
    private static readonly (int FileDelta, int RankDelta)[] rookDirections =
    [
        ( 1,  0),
        (-1,  0),
        ( 0,  1),
        ( 0, -1)
    ];

    private static readonly (int FileDelta, int RankDelta)[] bishopDirections =
    [
        ( 1,  1),
        ( 1, -1),
        (-1,  1),
        (-1, -1)
    ];

    private static readonly (int FileDelta, int RankDelta)[] queenDirections =
    [
        ( 1,  0),
        (-1,  0),
        ( 0,  1),
        ( 0, -1),
        ( 1,  1),
        ( 1, -1),
        (-1,  1),
        (-1, -1)
    ];

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

        var kingSquare = piece.Type == PieceType.King
            ? move.To
            : pos.Board.GetKingSquare(piece.Color);

        if (IsSquareAttackedAfterMove(kingSquare, piece.Color, pos, move, piece))
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

        return !IsSquareAttacked(from, color, pos) &&
               !IsSquareAttacked(passingSquare, color, pos) &&
               !IsSquareAttacked(to, color, pos);
    }

    private static bool IsPieceAt(
        Square square,
        PieceType pieceType,
        PieceColor color,
        BoardPosition pos)
    {
        var piece = pos.Board[square.Index];

        return piece is { Type: var type, Color: var pieceColor } &&
               type == pieceType &&
               pieceColor == color;
    }
}
