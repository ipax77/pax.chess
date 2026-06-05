namespace pax.chess.Validation;

public static class PseudoMoveValidator
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

        if (IsSquareAttacked(kingSquare, piece.Color, newPos))
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

        return !IsSquareAttacked(from, color, pos) &&
               !IsSquareAttacked(passingSquare, color, pos) &&
               !IsSquareAttacked(to, color, pos);
    }

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
