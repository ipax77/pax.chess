
namespace pax.chess.Validation;

public static partial class MoveValidator
{
    public static IReadOnlyCollection<Move> GetValidMoves(Square from, BoardPosition pos)
    {
        ArgumentNullException.ThrowIfNull(pos);
        var piece = pos.Board[from.Index];

        if (!piece.HasValue)
        {
            return [];
        }

        var validSquares = piece.Value.Type switch
        {
            PieceType.Pawn => GetPawnMoves(from, pos),
            PieceType.Knight => GetKnightMoves(from, pos),
            PieceType.Bishop => GetBishopMoves(from, pos),
            PieceType.Rook => GetRookMoves(from, pos),
            PieceType.Queen => GetQueenMoves(from, pos),
            PieceType.King => GetKingMoves(from, pos),
            _ => []
        };

        if (validSquares.Count == 0)
        {
            return [];
        }

        List<Move> moves = [];
        // check

        return moves;
    }

    private static List<Square> GetSlidingMoves(
        Square from,
        BoardPosition pos,
        int[][] deltas)
    {
        var piece = pos.Board[from.Index];
        if (!piece.HasValue)
            return [];

        var moves = new List<Square>();

        foreach (var delta in deltas)
        {
            int file = from.File + delta[0];
            int rank = from.Rank + delta[1];

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

                file += delta[0];
                rank += delta[1];
            }
        }

        return moves;
    }
}