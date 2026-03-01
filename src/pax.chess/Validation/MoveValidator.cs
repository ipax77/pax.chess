
namespace pax.chess.Validation;

public static partial class MoveValidator
{
    public static MoveState IsValidMove(Move move, BoardPosition pos)
    {
        ArgumentNullException.ThrowIfNull(move);
        ArgumentNullException.ThrowIfNull(pos);

        var piece = pos.Board[move.From.Index];

        if (!piece.HasValue)
            return MoveState.PieceNotFound;

        if (piece.Value.Color != pos.SideToMove)
            return MoveState.WrongColor;

        var validSquares = GetValidSquares(move.From, piece.Value.Type, pos);

        if (!validSquares.Contains(move.To))
            return MoveState.TargetInvalid;

        if (move.MoveType.HasFlag(MoveType.CastlingKingSide) || move.MoveType.HasFlag(MoveType.CastlingQueenSide))
        {
            var requiredRight = (piece.Value.Color, move.MoveType.HasFlag(MoveType.CastlingKingSide)) switch
            {
                (PieceColor.White, true) => CastlingRights.WhiteKingSide,
                (PieceColor.White, false) => CastlingRights.WhiteQueenSide,
                (PieceColor.Black, true) => CastlingRights.BlackKingSide,
                (PieceColor.Black, false) => CastlingRights.BlackQueenSide,
                _ => CastlingRights.None
            };

            if (!pos.CastlingRights.HasFlag(requiredRight))
            {
                return MoveState.CastleNotAllowed;
            }
            if (!IsCastlingPathSafe(move.From, move.To, piece.Value.Color, pos))
                return MoveState.CastlingPathAttacked;
        }

        var newPos = pos.MakeMove(move);
        var kingSquare = piece.Value.Type == PieceType.King
            ? move.To
            : pos.Board.GetKingSquare(piece.Value.Color);

        if (IsSquareAttacked(kingSquare, piece.Value.Color, newPos))
            return MoveState.WouldBeCheck;

        return MoveState.Ok;
    }

    public static IReadOnlyCollection<Move> GetValidMoves(Square from, BoardPosition pos, out MoveState moveState)
    {
        ArgumentNullException.ThrowIfNull(pos);
        var piece = pos.Board[from.Index];

        if (!piece.HasValue)
        {
            moveState = MoveState.PieceNotFound;
            return [];
        }

        if (piece.Value.Color != pos.SideToMove)
        {
            moveState = MoveState.WrongColor;
            return [];
        }

        var validSquares = GetValidSquares(from, piece.Value.Type, pos);

        if (validSquares.Count == 0)
        {
            moveState = MoveState.NoValidMoves;
            return [];
        }

        var kingSquare = piece.Value.Type == PieceType.King
            ? (Square?)null  // will use move.To directly
            : pos.Board.GetKingSquare(piece.Value.Color);

        List<Move> moves = new(validSquares.Count);

        foreach (var square in validSquares)
        {
            MoveType moveType = MoveType.None;
            if (pos.Board[square.Index].HasValue)
            {
                moveType |= MoveType.Capture;
            }
            if (piece.Value.Type == PieceType.Pawn && (square.Rank == 0 || square.Rank == 7))
            {
                moveType |= MoveType.Promotion;
            }
            if (pos.EnPassantTarget == square)
            {
                moveType |= MoveType.EnPassant;
            }
            if (piece.Value.Type == PieceType.King && Math.Abs(from.File - square.File) > 1)
            {
                moveType |= from.File - square.File < 0 ? MoveType.CastlingKingSide : MoveType.CastlingQueenSide;
                if (!IsCastlingPathSafe(from, square, piece.Value.Color, pos))
                    continue;
            }

            var move = new Move(from, square, null, moveType);
            var newPos = pos.MakeMove(move);
            var kingAfterMove = kingSquare ?? square;

            if (!IsSquareAttacked(kingAfterMove, piece.Value.Color, newPos))
                moves.Add(move);
        }

        if (moves.Count == 0)
        {
            moveState = MoveState.NoValidMoves;
            return [];
        }

        moveState = MoveState.Ok;
        return moves;
    }

    public static GameState GetGameState(BoardPosition pos)
    {
        ArgumentNullException.ThrowIfNull(pos);

        var kingEntry = pos.Board.GetKingSquare(pos.SideToMove);
        bool inCheck = IsSquareAttacked(kingEntry, pos.SideToMove, pos);

        foreach (var (square, _) in pos.Board.GetPieces(pos.SideToMove))
        {
            var moves = GetValidMoves(square, pos, out _);
            if (moves.Count > 0)
                return inCheck ? GameState.Check : GameState.Normal;
        }

        return inCheck ? GameState.Checkmate : GameState.Stalemate;
    }

    private static bool IsCastlingPathSafe(Square from, Square to, PieceColor color, BoardPosition pos)
    {
        int direction = to.File > from.File ? 1 : -1;
        var passingSquare = new Square(from.File + direction, from.Rank);

        return !IsSquareAttacked(from, color, pos) &&        // can't castle out of check
               !IsSquareAttacked(passingSquare, color, pos) && // passing square must be safe
               !IsSquareAttacked(to, color, pos);              // destination must be safe
    }

    public static bool IsSquareAttacked(Square target, PieceColor defendingColor, BoardPosition pos)
    {
        ArgumentNullException.ThrowIfNull(pos);
        var enemyColor = defendingColor == PieceColor.White ? PieceColor.Black : PieceColor.White;


        foreach (var (from, attacker) in pos.Board.GetPieces(enemyColor))
        {
            var validSquares = GetValidAttackSquares(from, attacker.Type, pos);
            if (validSquares.Contains(target))
            {
                return true;
            }
        }
        return false;
    }

    private static List<Square> GetValidAttackSquares(Square from, PieceType pieceType, BoardPosition pos)
    {
        return pieceType switch
        {
            PieceType.Pawn => GetPawnAttackMoves(from, pos),
            PieceType.Knight => GetKnightMoves(from, pos),
            PieceType.Bishop => GetBishopMoves(from, pos),
            PieceType.Rook => GetRookMoves(from, pos),
            PieceType.Queen => GetQueenMoves(from, pos),
            PieceType.King => GetKingMoves(from, pos),
            _ => []
        };
    }

    private static List<Square> GetValidSquares(Square from, PieceType pieceType, BoardPosition pos)
    {
        return pieceType switch
        {
            PieceType.Pawn => GetPawnMoves(from, pos),
            PieceType.Knight => GetKnightMoves(from, pos),
            PieceType.Bishop => GetBishopMoves(from, pos),
            PieceType.Rook => GetRookMoves(from, pos),
            PieceType.Queen => GetQueenMoves(from, pos),
            PieceType.King => GetKingMoves(from, pos),
            _ => []
        };
    }

    private static List<Square> GetSlidingMoves(
        Square from,
        BoardPosition pos,
        (int FileDelta, int RankDelta)[] deltas)
    {
        var piece = pos.Board[from.Index];
        if (!piece.HasValue)
            return [];

        var moves = new List<Square>(27);

        foreach (var (fileDelta, rankDelta) in deltas)
        {
            int file = from.File + fileDelta;
            int rank = from.Rank + rankDelta;

            while (file >= 0 && file <= 7 &&
                   rank >= 0 && rank <= 7)
            {
                var target = new Square(file, rank);
                var occupied = pos.Board[target.Index];

                if (!occupied.HasValue)
                {
                    moves.Add(target);
                }
                else
                {
                    if (occupied.Value.Color != piece.Value.Color)
                        moves.Add(target);
                    break;
                }

                file += fileDelta;
                rank += rankDelta;
            }
        }

        return moves;
    }
}