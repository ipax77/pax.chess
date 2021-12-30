namespace pax.chess.Validation;

public partial class Validate
{
    private static readonly int[][] KnightDeltas = new int[8][]
    {
        new int[2] {2, 1},
        new int[2] {2, -1},
        new int[2] {-2, 1},
        new int[2] {-2, -1},
        new int[2] {1, 2},
        new int[2] {1, -2},
        new int[2] {-1, 2},
        new int[2] {-1, -2}
    };

    private static List<Position> GetKnightMoves(Piece piece, List<Piece> pieces)
    {
        var moves = new List<Position>();

        for (int i = 0; i < KnightDeltas.Length; i++)
        {
            var pos = new Position(piece.Position.X + KnightDeltas[i][0], piece.Position.Y + KnightDeltas[i][1]);
            if (!pos.OutOfBounds)
            {
                var occupied = pieces.SingleOrDefault(f => f.Position == pos);
                if (occupied != null)
                {
                    if (occupied.IsBlack != piece.IsBlack)
                    {
                        moves.Add(pos);
                    }
                }
                else
                {
                    moves.Add(pos);
                }
            }
        }
        return moves;
    }
}
