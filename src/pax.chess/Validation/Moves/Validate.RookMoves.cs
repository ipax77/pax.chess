namespace pax.chess.Validation;
public static partial class Validate
{
    private static readonly int[][] RookDeltas = new int[4][]
    {
        new int[2] { 0, 1 },
        new int[2] { 0, -1 },
        new int[2] { 1, 0 },
        new int[2] { -1, 0 }
    };

    private static List<Position> GetRookMoves(Piece piece, List<Piece> pieces)
    {
        var moves = new List<Position>();

        for (int i = 0; i < RookDeltas.Length; i++)
        {
            var pos = new Position(piece.Position.X + RookDeltas[i][0], piece.Position.Y + RookDeltas[i][1]);
            while (!pos.OutOfBounds)
            {
                var occupied = pieces.FirstOrDefault(f => f.Position == pos);
                if (occupied != null)
                {
                    if (occupied.IsBlack != piece.IsBlack)
                    {
                        moves.Add(pos);
                    }
                    break;
                }
                moves.Add(pos);
                pos = new Position(pos.X + RookDeltas[i][0], pos.Y + RookDeltas[i][1]);
            }
        }
        return moves;
    }
}
