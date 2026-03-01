namespace pax.chess.Validation;

public static partial class MoveValidator
{
    private static List<Square> GetPawnMoves(Square from, BoardPosition pos)
    {
        var piece = pos.Board[from.Index];

        if (!piece.HasValue || piece.Value.Type != PieceType.Pawn)
            return [];

        var moves = new List<Square>(4);

        int direction = piece.Value.Color == PieceColor.White ? 1 : -1;
        int startRank = piece.Value.Color == PieceColor.White ? 1 : 6;
        int promotionRank = piece.Value.Color == PieceColor.White ? 7 : 0;

        // --- Forward 1 ---
        int forwardRank = from.Rank + direction;

        if (forwardRank >= 0 && forwardRank <= 7)
        {
            var forward = new Square(from.File, forwardRank);

            if (!pos.Board[forward.Index].HasValue)
            {
                moves.Add(forward);

                // --- Forward 2 (from start) ---
                if (from.Rank == startRank)
                {
                    int doubleRank = from.Rank + (2 * direction);
                    var forward2 = new Square(from.File, doubleRank);

                    if (!pos.Board[forward2.Index].HasValue)
                        moves.Add(forward2);
                }
            }
        }
        moves.AddRange(GetPawnAttackMoves(from, pos, piece));
        return moves;
    }

    private static List<Square> GetPawnAttackMoves(Square from, BoardPosition pos, Piece? piece = null)
    {
        if (piece == null)
        {
            piece = pos.Board[from.Index];

            if (!piece.HasValue || piece.Value.Type != PieceType.Pawn)
                return [];
        }
        var moves = new List<Square>(2);
        int direction = piece.Value.Color == PieceColor.White ? 1 : -1;

        foreach (int fileDelta in new[] { -1, 1 })
        {
            int newFile = from.File + fileDelta;
            int newRank = from.Rank + direction;

            if (newFile < 0 || newFile > 7 ||
                newRank < 0 || newRank > 7)
                continue;

            var target = new Square(newFile, newRank);

            // En passant
            if (pos.EnPassantTarget.HasValue &&
                pos.EnPassantTarget.Value.Equals(target))
            {
                moves.Add(target);
                continue;
            }

            var occupant = pos.Board[target.Index];

            if (occupant.HasValue &&
                occupant.Value.Color != piece.Value.Color)
            {
                moves.Add(target);
            }
        }
        return moves;
    }
}