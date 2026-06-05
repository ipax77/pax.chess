namespace pax.chess.Validation;

public static partial class PseudoMoveValidator
{
    public static IReadOnlyCollection<Move> GetValidMoves(Square from, BoardPosition pos, out MoveState moveState)
    {
        ArgumentNullException.ThrowIfNull(pos);

        var maybePiece = pos.Board[from.Index];

        if (maybePiece is not { } piece)
        {
            moveState = MoveState.PieceNotFound;
            return [];
        }

        if (piece.Color != pos.SideToMove)
        {
            moveState = MoveState.WrongColor;
            return [];
        }

        var moves = new List<Move>(GetMoveCapacity(piece.Type));
        AddLegalMovesFrom(from, piece, pos, moves);

        if (moves.Count == 0)
        {
            moveState = MoveState.NoValidMoves;
            return [];
        }

        moveState = MoveState.Ok;
        return moves;
    }

    public static bool IsWinnable(BoardPosition pos, PieceColor color)
    {
        ArgumentNullException.ThrowIfNull(pos);

        var pieces = pos.Board.GetPieces(color);
        return pieces.Count switch
        {
            <= 1 => false,
            2 => pieces.Any(p => p.Piece.Type is not (PieceType.Bishop or PieceType.Knight)),
            _ => true
        };
    }

    private static int GetMoveCapacity(PieceType pieceType)
    {
        return pieceType switch
        {
            PieceType.Pawn => 4,
            PieceType.Knight => 8,
            PieceType.Bishop => 13,
            PieceType.Rook => 14,
            PieceType.Queen => 27,
            PieceType.King => 8,
            _ => 0
        };
    }

    private static void AddLegalMovesFrom(
        Square from,
        Piece piece,
        BoardPosition pos,
        List<Move> moves)
    {
        switch (piece.Type)
        {
            case PieceType.Pawn:
                AddLegalPawnMoves(from, piece, pos, moves);
                break;
            case PieceType.Knight:
                AddLegalKnightMoves(from, piece, pos, moves);
                break;
            case PieceType.Bishop:
                AddLegalSlidingMoves(from, piece, pos, bishopDirections, moves);
                break;
            case PieceType.Rook:
                AddLegalSlidingMoves(from, piece, pos, rookDirections, moves);
                break;
            case PieceType.Queen:
                AddLegalSlidingMoves(from, piece, pos, queenDirections, moves);
                break;
            case PieceType.King:
                AddLegalKingMoves(from, piece, pos, moves);
                break;
        }
    }

    private static void AddLegalPawnMoves(
        Square from,
        Piece piece,
        BoardPosition pos,
        List<Move> moves)
    {
        int direction = piece.Color == PieceColor.White ? 1 : -1;
        int startRank = piece.Color == PieceColor.White ? 1 : 6;
        int forwardRank = from.Rank + direction;

        if ((uint)forwardRank > 7)
            return;

        var forward = new Square(from.File, forwardRank);

        if (!pos.Board[forward.Index].HasValue)
        {
            AddLegalCandidateMove(from, forward, piece, pos, moves);

            int doubleRank = from.Rank + (2 * direction);
            if (from.Rank == startRank && (uint)doubleRank <= 7)
            {
                var forward2 = new Square(from.File, doubleRank);

                if (!pos.Board[forward2.Index].HasValue)
                    AddLegalCandidateMove(from, forward2, piece, pos, moves);
            }
        }

        AddLegalPawnCapture(from, piece, pos, from.File - 1, forwardRank, moves);
        AddLegalPawnCapture(from, piece, pos, from.File + 1, forwardRank, moves);
    }

    private static void AddLegalPawnCapture(
        Square from,
        Piece piece,
        BoardPosition pos,
        int targetFile,
        int targetRank,
        List<Move> moves)
    {
        if ((uint)targetFile > 7)
            return;

        var target = new Square(targetFile, targetRank);
        var occupant = pos.Board[target.Index];

        if (target != pos.EnPassantTarget &&
            (occupant is not { } targetPiece || targetPiece.Color == piece.Color))
        {
            return;
        }

        AddLegalCandidateMove(from, target, piece, pos, moves);
    }

    private static void AddLegalKnightMoves(
        Square from,
        Piece piece,
        BoardPosition pos,
        List<Move> moves)
    {
        foreach (int targetIndex in knightTargets[from.Index])
        {
            var targetPiece = pos.Board[targetIndex];

            if (targetPiece is { Color: var color } && color == piece.Color)
                continue;

            AddLegalCandidateMove(from, new Square(targetIndex), piece, pos, moves);
        }
    }

    private static void AddLegalSlidingMoves(
        Square from,
        Piece piece,
        BoardPosition pos,
        ReadOnlySpan<(int FileDelta, int RankDelta)> directions,
        List<Move> moves)
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

                AddLegalCandidateMove(from, target, piece, pos, moves);

                if (targetPiece.HasValue)
                    break;

                file += fileDelta;
                rank += rankDelta;
            }
        }
    }

    private static void AddLegalKingMoves(
        Square from,
        Piece piece,
        BoardPosition pos,
        List<Move> moves)
    {
        foreach (int targetIndex in kingTargets[from.Index])
        {
            var targetPiece = pos.Board[targetIndex];

            if (targetPiece is { Color: var color } && color == piece.Color)
                continue;

            AddLegalCandidateMove(from, new Square(targetIndex), piece, pos, moves);
        }

        AddLegalCastlingMoves(from, piece, pos, moves);
    }

    private static void AddLegalCastlingMoves(
        Square from,
        Piece piece,
        BoardPosition pos,
        List<Move> moves)
    {
        int homeRank = piece.Color == PieceColor.White ? 0 : 7;

        if (from.File != 4 || from.Rank != homeRank)
            return;

        AddLegalCastlingMove(from, new Square(6, homeRank), piece, kingSide: true, pos, moves);
        AddLegalCastlingMove(from, new Square(2, homeRank), piece, kingSide: false, pos, moves);
    }

    private static void AddLegalCastlingMove(
        Square from,
        Square to,
        Piece piece,
        bool kingSide,
        BoardPosition pos,
        List<Move> moves)
    {
        var rookSquare = new Square(kingSide ? 7 : 0, from.Rank);

        if (!HasCastlingRight(piece.Color, kingSide, pos) ||
            !HasCastlingRook(rookSquare, piece.Color, pos) ||
            !IsCastlingPathClear(from, to, pos) ||
            !IsCastlingPathSafe(from, to, piece.Color, pos))
        {
            return;
        }

        AddLegalCandidateMove(from, to, piece, pos, moves);
    }

    private static void AddLegalCandidateMove(
        Square from,
        Square to,
        Piece piece,
        BoardPosition pos,
        List<Move> moves)
    {
        if (TryCreateLegalMove(from, to, piece, pos, out var move))
            moves.Add(move);
    }

    private static bool TryCreateLegalMove(
        Square from,
        Square to,
        Piece piece,
        BoardPosition pos,
        out Move move)
    {
        var kingSquare = piece.Type == PieceType.King
            ? to
            : pos.Board.GetKingSquare(piece.Color);

        if (IsSquareAttackedAfterMove(kingSquare, piece.Color, pos, from, to, piece))
        {
            move = null!;
            return false;
        }

        move = new Move(from, to, null, GetMoveType(from, to, piece, pos));
        return true;
    }

    private static MoveType GetMoveType(
        Square from,
        Square to,
        Piece piece,
        BoardPosition pos)
    {
        var moveType = MoveType.None;

        if (pos.Board[to.Index].HasValue)
            moveType |= MoveType.Capture;

        if (piece.Type == PieceType.Pawn)
        {
            if (to.Rank is 0 or 7)
                moveType |= MoveType.Promotion;

            if (to == pos.EnPassantTarget)
                moveType |= MoveType.EnPassant;
        }

        if (piece.Type == PieceType.King && IsKingCastlingShape(from, to, piece.Color))
        {
            moveType |= to.File > from.File
                ? MoveType.CastlingKingSide
                : MoveType.CastlingQueenSide;
        }

        return moveType;
    }
}
