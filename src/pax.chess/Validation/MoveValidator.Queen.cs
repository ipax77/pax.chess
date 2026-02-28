
namespace pax.chess.Validation;

public static partial class MoveValidator
{
    private static List<Square> GetQueenMoves(Square from, BoardPosition pos)
    {
        var piece = pos.Board[from.Index];

        if (!piece.HasValue || piece.Value.Type != PieceType.Queen)
            return [];

        return GetSlidingMoves(from, pos, kingDeltas);
    }
}