
namespace pax.chess.Validation;

public static partial class MoveValidator
{
    private static readonly int[][] bishopDeltas =
    [
        [1, 1],
        [1, -1],
        [-1, 1],
        [-1, -1]
    ];

    private static List<Square> GetBishopMoves(Square from, BoardPosition pos)
    {
        var piece = pos.Board[from.Index];

        if (!piece.HasValue || piece.Value.Type != PieceType.Bishop)
            return [];

        return GetSlidingMoves(from, pos, bishopDeltas);
    }
}