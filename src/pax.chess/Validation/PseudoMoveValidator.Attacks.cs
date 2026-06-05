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

    private static bool IsSquareAttackedAfterMove(
        Square square,
        PieceColor defenderColor,
        BoardPosition pos,
        Move move,
        Piece movingPiece)
    {
        var attackerColor = defenderColor == PieceColor.White
            ? PieceColor.Black
            : PieceColor.White;

        return IsAttackedByPawnAfterMove(square, attackerColor, pos, move, movingPiece) ||
               IsAttackedByKnightAfterMove(square, attackerColor, pos, move, movingPiece) ||
               IsAttackedByKingAfterMove(square, attackerColor, pos, move, movingPiece) ||
               IsAttackedBySlidingPieceAfterMove(square, attackerColor, pos, move, movingPiece);
    }

    private static Piece? GetPieceAfterMove(
        BoardPosition pos,
        Move move,
        Piece movingPiece,
        Square square)
    {
        if (square == move.From)
            return null;

        if (IsEnPassantCapture(pos, move, movingPiece))
        {
            var capturedSquare = new Square(move.To.File, move.From.Rank);

            if (square == capturedSquare)
                return null;
        }

        if (TryGetCastlingRookSquares(move, movingPiece, out var rookFrom, out var rookTo))
        {
            if (square == rookFrom)
                return null;

            if (square == rookTo)
                return new Piece(PieceType.Rook, movingPiece.Color);
        }

        if (square == move.To)
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
        Move move,
        Piece movingPiece)
    {
        int pawnRankDelta = attackerColor == PieceColor.White ? -1 : 1;
        int sourceRank = square.Rank + pawnRankDelta;

        if ((uint)sourceRank > 7)
            return false;

        int leftSourceFile = square.File - 1;
        if ((uint)leftSourceFile <= 7 &&
            IsPieceAtAfterMove(new Square(leftSourceFile, sourceRank), PieceType.Pawn, attackerColor, pos, move, movingPiece))
        {
            return true;
        }

        int rightSourceFile = square.File + 1;
        return (uint)rightSourceFile <= 7 &&
               IsPieceAtAfterMove(new Square(rightSourceFile, sourceRank), PieceType.Pawn, attackerColor, pos, move, movingPiece);
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
        Move move,
        Piece movingPiece)
    {
        foreach (int attackerIndex in knightTargets[square.Index])
        {
            var piece = GetPieceAfterMove(pos, move, movingPiece, new Square(attackerIndex));

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
        Move move,
        Piece movingPiece)
    {
        foreach (int attackerIndex in kingTargets[square.Index])
        {
            var piece = GetPieceAfterMove(pos, move, movingPiece, new Square(attackerIndex));

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
        Move move,
        Piece movingPiece)
    {
        return IsAttackedAlongDirectionsAfterMove(
                   square,
                   attackerColor,
                   pos,
                   move,
                   movingPiece,
                   rookDirections,
                   PieceType.Rook,
                   PieceType.Queen) ||
               IsAttackedAlongDirectionsAfterMove(
                   square,
                   attackerColor,
                   pos,
                   move,
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
        Move move,
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
                var piece = GetPieceAfterMove(pos, move, movingPiece, candidate);

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
        Move move,
        Piece movingPiece)
    {
        var piece = GetPieceAfterMove(pos, move, movingPiece, square);

        return piece is { Type: var type, Color: var pieceColor } &&
               type == pieceType &&
               pieceColor == color;
    }

    private static bool IsEnPassantCapture(
        BoardPosition pos,
        Move move,
        Piece movingPiece)
    {
        return movingPiece.Type == PieceType.Pawn &&
               move.To == pos.EnPassantTarget &&
               move.From.File != move.To.File &&
               !pos.Board[move.To.Index].HasValue;
    }

    private static bool TryGetCastlingRookSquares(
        Move move,
        Piece movingPiece,
        out Square rookFrom,
        out Square rookTo)
    {
        rookFrom = default;
        rookTo = default;

        if (movingPiece.Type != PieceType.King || Math.Abs(move.To.File - move.From.File) != 2)
            return false;

        bool kingSide = move.To.File > move.From.File;
        int rank = move.From.Rank;

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
