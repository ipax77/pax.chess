namespace pax.chess.Validation;
public static partial class Validate
{
    private static readonly int[][] BishopDeltas = new int[4][]
    {
        new int[2] { 1, 1 },
        new int[2] { 1, -1 },
        new int[2] { -1, 1 },
        new int[2] { -1, -1 }
    };

    private static List<Position> GetBishopMoves(Piece piece, List<Piece> pieces)
    {
        var moves = new List<Position>();

        for (int i = 0; i < BishopDeltas.Length; i++)
        {
            var pos = new Position(piece.Position.X + BishopDeltas[i][0], piece.Position.Y + BishopDeltas[i][1]);
            while (!pos.OutOfBounds)
            {
                var occupied = pieces.SingleOrDefault(f => f.Position == pos);
                if (occupied != null)
                {
                    if (occupied.IsBlack != piece.IsBlack)
                    {
                        moves.Add(pos);
                    }
                    break;
                }
                moves.Add(pos);
                pos = new Position(pos.X + BishopDeltas[i][0], pos.Y + BishopDeltas[i][1]);
            }
        }
        return moves;
    }
}
