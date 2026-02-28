
namespace pax.chess.Validation;

public static partial class MoveValidator
{
    private static readonly int[][] rookDeltas =
    [
        [0, 1],
        [0, -1],
        [1, 0],
        [-1, 0]
    ];

    private static List<Square> GetRookMoves(Square from, BoardPosition pos)
    {
        var piece = pos.Board[from.Index];

        if (!piece.HasValue || piece.Value.Type != PieceType.Rook)
            return [];

        return GetSlidingMoves(from, pos, rookDeltas);
    }
}