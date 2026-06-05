namespace pax.chess.Validation;

public static partial class PseudoMoveValidator
{
    private static readonly (int FileDelta, int RankDelta)[] knightAttackDirections =
    [
        ( 2,  1), ( 2, -1),
        (-2,  1), (-2, -1),
        ( 1,  2), ( 1, -2),
        (-1,  2), (-1, -2)
    ];

    private static readonly (int FileDelta, int RankDelta)[] kingAttackDirections =
    [
        ( 0,  1), ( 0, -1),
        ( 1,  0), (-1,  0),
        ( 1,  1), ( 1, -1),
        (-1,  1), (-1, -1)
    ];

    private static readonly int[][] knightTargets = CreateTargets(knightAttackDirections);
    private static readonly int[][] kingTargets = CreateTargets(kingAttackDirections);

    public static bool IsSquareAttacked(
        Square square,
        PieceColor defenderColor,
        BoardPosition pos)
    {
        ArgumentNullException.ThrowIfNull(pos);

        return IsSquareAttackedCore(square, defenderColor, pos);
    }

    private static bool IsSquareAttackedCore(
        Square square,
        PieceColor defenderColor,
        BoardPosition pos)
    {
        var attackerColor = defenderColor == PieceColor.White
            ? PieceColor.Black
            : PieceColor.White;

        return IsAttackedByPawn(square, attackerColor, pos) ||
               IsAttackedByKnight(square, attackerColor, pos) ||
               IsAttackedByKing(square, attackerColor, pos) ||
               IsAttackedBySlidingPiece(square, attackerColor, pos);
    }

    private static bool IsSquareAttackedAfterMove(
        Square square,
        PieceColor defenderColor,
        BoardPosition pos,
        Square moveFrom,
        Square moveTo,
        Piece movingPiece)
    {
        var attackerColor = defenderColor == PieceColor.White
            ? PieceColor.Black
            : PieceColor.White;

        return IsAttackedByPawnAfterMove(square, attackerColor, pos, moveFrom, moveTo, movingPiece) ||
               IsAttackedByKnightAfterMove(square, attackerColor, pos, moveFrom, moveTo, movingPiece) ||
               IsAttackedByKingAfterMove(square, attackerColor, pos, moveFrom, moveTo, movingPiece) ||
               IsAttackedBySlidingPieceAfterMove(square, attackerColor, pos, moveFrom, moveTo, movingPiece);
    }

    private static Piece? GetPieceAfterMove(
        BoardPosition pos,
        Square moveFrom,
        Square moveTo,
        Piece movingPiece,
        Square square)
    {
        if (square == moveFrom)
            return null;

        if (IsEnPassantCapture(pos, moveFrom, moveTo, movingPiece))
        {
            var capturedSquare = new Square(moveTo.File, moveFrom.Rank);

            if (square == capturedSquare)
                return null;
        }

        if (TryGetCastlingRookSquares(moveFrom, moveTo, movingPiece, out var rookFrom, out var rookTo))
        {
            if (square == rookFrom)
                return null;

            if (square == rookTo)
                return new Piece(PieceType.Rook, movingPiece.Color);
        }

        if (square == moveTo)
            return movingPiece;

        return pos.Board[square.Index];
    }

    private static bool IsAttackedByPawn(
        Square square,
        PieceColor attackerColor,
        BoardPosition pos)
    {
        int pawnRankDelta = attackerColor == PieceColor.White ? -1 : 1;
        int sourceRank = square.Rank + pawnRankDelta;

        if ((uint)sourceRank > 7)
            return false;

        int leftSourceFile = square.File - 1;
        if ((uint)leftSourceFile <= 7 &&
            IsPieceAt(new Square(leftSourceFile, sourceRank), PieceType.Pawn, attackerColor, pos))
        {
            return true;
        }

        int rightSourceFile = square.File + 1;
        return (uint)rightSourceFile <= 7 &&
               IsPieceAt(new Square(rightSourceFile, sourceRank), PieceType.Pawn, attackerColor, pos);
    }

    private static bool IsAttackedByPawnAfterMove(
        Square square,
        PieceColor attackerColor,
        BoardPosition pos,
        Square moveFrom,
        Square moveTo,
        Piece movingPiece)
    {
        int pawnRankDelta = attackerColor == PieceColor.White ? -1 : 1;
        int sourceRank = square.Rank + pawnRankDelta;

        if ((uint)sourceRank > 7)
            return false;

        int leftSourceFile = square.File - 1;
        if ((uint)leftSourceFile <= 7 &&
            IsPieceAtAfterMove(new Square(leftSourceFile, sourceRank), PieceType.Pawn, attackerColor, pos, moveFrom, moveTo, movingPiece))
        {
            return true;
        }

        int rightSourceFile = square.File + 1;
        return (uint)rightSourceFile <= 7 &&
               IsPieceAtAfterMove(new Square(rightSourceFile, sourceRank), PieceType.Pawn, attackerColor, pos, moveFrom, moveTo, movingPiece);
    }

    private static bool IsAttackedByKnight(
        Square square,
        PieceColor attackerColor,
        BoardPosition pos)
    {
        foreach (int attackerIndex in knightTargets[square.Index])
        {
            var piece = pos.Board[attackerIndex];

            if (piece is { Type: PieceType.Knight, Color: var color } &&
                color == attackerColor)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsAttackedByKnightAfterMove(
        Square square,
        PieceColor attackerColor,
        BoardPosition pos,
        Square moveFrom,
        Square moveTo,
        Piece movingPiece)
    {
        foreach (int attackerIndex in knightTargets[square.Index])
        {
            var piece = GetPieceAfterMove(pos, moveFrom, moveTo, movingPiece, new Square(attackerIndex));

            if (piece is { Type: PieceType.Knight, Color: var color } &&
                color == attackerColor)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsAttackedByKing(
        Square square,
        PieceColor attackerColor,
        BoardPosition pos)
    {
        foreach (int attackerIndex in kingTargets[square.Index])
        {
            var piece = pos.Board[attackerIndex];

            if (piece is { Type: PieceType.King, Color: var color } &&
                color == attackerColor)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsAttackedByKingAfterMove(
        Square square,
        PieceColor attackerColor,
        BoardPosition pos,
        Square moveFrom,
        Square moveTo,
        Piece movingPiece)
    {
        foreach (int attackerIndex in kingTargets[square.Index])
        {
            var piece = GetPieceAfterMove(pos, moveFrom, moveTo, movingPiece, new Square(attackerIndex));

            if (piece is { Type: PieceType.King, Color: var color } &&
                color == attackerColor)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsAttackedBySlidingPiece(
        Square square,
        PieceColor attackerColor,
        BoardPosition pos)
    {
        return IsAttackedAlongDirections(
                   square,
                   attackerColor,
                   pos,
                   rookDirections,
                   PieceType.Rook,
                   PieceType.Queen) ||
               IsAttackedAlongDirections(
                   square,
                   attackerColor,
                   pos,
                   bishopDirections,
                   PieceType.Bishop,
                   PieceType.Queen);
    }

    private static bool IsAttackedBySlidingPieceAfterMove(
        Square square,
        PieceColor attackerColor,
        BoardPosition pos,
        Square moveFrom,
        Square moveTo,
        Piece movingPiece)
    {
        return IsAttackedAlongDirectionsAfterMove(
                   square,
                   attackerColor,
                   pos,
                   moveFrom,
                   moveTo,
                   movingPiece,
                   rookDirections,
                   PieceType.Rook,
                   PieceType.Queen) ||
               IsAttackedAlongDirectionsAfterMove(
                   square,
                   attackerColor,
                   pos,
                   moveFrom,
                   moveTo,
                   movingPiece,
                   bishopDirections,
                   PieceType.Bishop,
                   PieceType.Queen);
    }

    private static bool IsAttackedAlongDirections(
        Square square,
        PieceColor attackerColor,
        BoardPosition pos,
        ReadOnlySpan<(int FileDelta, int RankDelta)> directions,
        PieceType pieceTypeA,
        PieceType pieceTypeB)
    {
        foreach (var (fileDelta, rankDelta) in directions)
        {
            int file = square.File + fileDelta;
            int rank = square.Rank + rankDelta;

            while ((uint)file <= 7 && (uint)rank <= 7)
            {
                var candidate = new Square(file, rank);
                var piece = pos.Board[candidate.Index];

                if (piece.HasValue)
                {
                    if (piece.Value.Color == attackerColor &&
                        (piece.Value.Type == pieceTypeA ||
                         piece.Value.Type == pieceTypeB))
                    {
                        return true;
                    }

                    break;
                }

                file += fileDelta;
                rank += rankDelta;
            }
        }

        return false;
    }

    private static bool IsAttackedAlongDirectionsAfterMove(
        Square square,
        PieceColor attackerColor,
        BoardPosition pos,
        Square moveFrom,
        Square moveTo,
        Piece movingPiece,
        ReadOnlySpan<(int FileDelta, int RankDelta)> directions,
        PieceType pieceTypeA,
        PieceType pieceTypeB)
    {
        foreach (var (fileDelta, rankDelta) in directions)
        {
            int file = square.File + fileDelta;
            int rank = square.Rank + rankDelta;

            while ((uint)file <= 7 && (uint)rank <= 7)
            {
                var candidate = new Square(file, rank);
                var piece = GetPieceAfterMove(pos, moveFrom, moveTo, movingPiece, candidate);

                if (piece.HasValue)
                {
                    if (piece.Value.Color == attackerColor &&
                        (piece.Value.Type == pieceTypeA ||
                         piece.Value.Type == pieceTypeB))
                    {
                        return true;
                    }

                    break;
                }

                file += fileDelta;
                rank += rankDelta;
            }
        }

        return false;
    }

    private static bool IsPieceAtAfterMove(
        Square square,
        PieceType pieceType,
        PieceColor color,
        BoardPosition pos,
        Square moveFrom,
        Square moveTo,
        Piece movingPiece)
    {
        var piece = GetPieceAfterMove(pos, moveFrom, moveTo, movingPiece, square);

        return piece is { Type: var type, Color: var pieceColor } &&
               type == pieceType &&
               pieceColor == color;
    }

    private static bool IsEnPassantCapture(
        BoardPosition pos,
        Square moveFrom,
        Square moveTo,
        Piece movingPiece)
    {
        return movingPiece.Type == PieceType.Pawn &&
               moveTo == pos.EnPassantTarget &&
               moveFrom.File != moveTo.File &&
               !pos.Board[moveTo.Index].HasValue;
    }

    private static bool TryGetCastlingRookSquares(
        Square moveFrom,
        Square moveTo,
        Piece movingPiece,
        out Square rookFrom,
        out Square rookTo)
    {
        rookFrom = default;
        rookTo = default;

        if (movingPiece.Type != PieceType.King || Math.Abs(moveTo.File - moveFrom.File) != 2)
            return false;

        bool kingSide = moveTo.File > moveFrom.File;
        int rank = moveFrom.Rank;

        rookFrom = kingSide
            ? new Square(7, rank)
            : new Square(0, rank);

        rookTo = kingSide
            ? new Square(5, rank)
            : new Square(3, rank);

        return true;
    }

    private static int[][] CreateTargets((int FileDelta, int RankDelta)[] directions)
    {
        var targets = new int[64][];

        for (int index = 0; index < 64; index++)
        {
            var square = new Square(index);
            var squareTargets = new List<int>(directions.Length);

            foreach (var (fileDelta, rankDelta) in directions)
            {
                int file = square.File + fileDelta;
                int rank = square.Rank + rankDelta;

                if ((uint)file <= 7 && (uint)rank <= 7)
                    squareTargets.Add(new Square(file, rank).Index);
            }

            targets[index] = [.. squareTargets];
        }

        return targets;
    }
}
