namespace pax.chess.Validation;
public static partial class Validate
{
    private static readonly int[][] KingDeltas = new int[8][]
    {
        new int[2] { 0, 1 },
        new int[2] { 0, -1 },
        new int[2] { 1, 0 },
        new int[2] { -1, 0 },
        new int[2] { 1, 1 },
        new int[2] { 1, -1 },
        new int[2] { -1, 1 },
        new int[2] { -1, -1 }
    };

    private static List<Position> GetKingMoves(Piece piece, State state)
    {
        var moves = new List<Position>();

        for (int i = 0; i < KingDeltas.Length; i++)
        {
            var pos = new Position(piece.Position.X + KingDeltas[i][0], piece.Position.Y + KingDeltas[i][1]);
            if (!pos.OutOfBounds)
            {
                var occupied = state.Pieces.SingleOrDefault(f => f.Position == pos);
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
        if (piece.IsBlack ? piece.Position == new Position(4, 7) : piece.Position == new Position(4, 0))
        {
            if (piece.IsBlack)
            {
                if (state.Info.BlackCanCastleKingSide)
                {
                    if (!state.Pieces.Where(x =>
                        x.Position == new Position(5, 7)
                     || x.Position == new Position(6, 7))
                        .Any())
                    {
                        moves.Add(new Position(6, 7));
                    }
                }
                if (state.Info.BlackCanCastleQueenSide)
                {
                    if (!state.Pieces.Where(x =>
                        x.Position == new Position(1, 7)
                     || x.Position == new Position(2, 7)
                     || x.Position == new Position(3, 7))
                        .Any())
                    {
                        moves.Add(new Position(2, 7));
                    }
                }
            }
            else
            {
                if (state.Info.WhiteCanCastleKingSide)
                {
                    if (!state.Pieces.Where(x =>
                        x.Position == new Position(5, 0)
                     || x.Position == new Position(6, 0))
                        .Any())
                    {
                        moves.Add(new Position(6, 0));
                    }
                }
                if (state.Info.WhiteCanCastleQueenSide)
                {
                    if (!state.Pieces.Where(x =>
                        x.Position == new Position(1, 0)
                     || x.Position == new Position(2, 0)
                     || x.Position == new Position(3, 0))
                        .Any())
                    {
                        moves.Add(new Position(2, 0));
                    }
                }
            }
        }
        return moves;
    }
}
