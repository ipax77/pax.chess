
namespace pax.chess.Validation;

public static partial class MoveValidator
{
    private static readonly int[][] knightDeltas =
    [
        [2, 1],
        [2, -1],
        [-2, 1],
        [-2, -1],
        [1, 2],
        [1, -2],
        [-1, 2],
        [-1, -2]
    ];

    private static List<Square> GetKnightMoves(Square from, BoardPosition pos)
    {
        var piece = pos.Board[from.Index];

        if (!piece.HasValue || piece.Value.Type != PieceType.Knight)
            return [];

        var moves = new List<Square>(8);

        foreach (var delta in knightDeltas)
        {
            int newFile = from.File + delta[0];
            int newRank = from.Rank + delta[1];

            if (newFile < 0 || newFile > 7 ||
                newRank < 0 || newRank > 7)
                continue;

            var target = new Square(newFile, newRank);

            var occupied = pos.Board[target.Index];

            if (!occupied.HasValue || occupied.Value.Color != piece.Value.Color)
                moves.Add(target);
        }

        return moves;
    }
}