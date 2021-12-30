namespace pax.chess.Validation;
public partial class Validate
{
    private static List<Position> GetPawnMoves(Piece piece, State state)
    {
        int delta = piece.IsBlack ? -1 : 1;

        var moves = new List<Position>();

        var pos1 = new Position(piece.Position.X, piece.Position.Y + delta);
        var occupied = state.Pieces.FirstOrDefault(f => f.Position == pos1);
        if (occupied == null)
        {
            moves.Add(pos1);

            // start pos?
            if (piece.IsBlack ? piece.Position.Y == 6 : piece.Position.Y == 1)
            {
                var pos2 = new Position(piece.Position.X, piece.Position.Y + (2 * delta));
                occupied = state.Pieces.FirstOrDefault(f => f.Position == pos2);
                if (occupied == null)
                {
                    moves.Add(new Position(piece.Position.X, piece.Position.Y + (2 * delta)));
                }
            }
        }

        var cap1 = new Position(piece.Position.X + 1, piece.Position.Y + delta);
        if (!cap1.OutOfBounds)
        {
            if (state.Info.EnPassantPosition == cap1)
            {
                moves.Add(cap1);
            }
            else
            {
                var enemy = state.Pieces.SingleOrDefault(s => s.Position == cap1 && s.IsBlack != piece.IsBlack);
                if (enemy != null)
                {
                    moves.Add(cap1);
                }
            }
        }

        var cap2 = new Position(piece.Position.X - 1, piece.Position.Y + delta);
        if (!cap2.OutOfBounds)
        {
            if (state.Info.EnPassantPosition == cap2)
            {
                moves.Add(cap2);
            }
            else
            {
                var enemy = state.Pieces.SingleOrDefault(s => s.Position == cap2 && s.IsBlack != piece.IsBlack);
                if (enemy != null)
                {
                    moves.Add(cap2);
                }
            }
        }

        return moves;
    }
}
