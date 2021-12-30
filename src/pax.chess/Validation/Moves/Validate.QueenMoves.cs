namespace pax.chess.Validation;
public partial class Validate
{
    private static List<Position> GetQueenMoves(Piece piece, List<Piece> pieces)
    {
        var moves = new List<Position>();

        for (int i = 0; i < KingDeltas.Length; i++)
        {
            var pos = new Position(piece.Position.X + KingDeltas[i][0], piece.Position.Y + KingDeltas[i][1]);
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
                pos = new Position(pos.X + KingDeltas[i][0], pos.Y + KingDeltas[i][1]);
            }
        }
        return moves;
    }
}
