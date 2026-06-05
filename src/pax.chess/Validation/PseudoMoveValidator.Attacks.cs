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

    private static bool IsSquareAttacked(
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
