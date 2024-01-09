namespace pax.chess.Validation;

public static partial class Validate
{
    private static readonly int[][] KnightDeltas =
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

    private static List<Position> GetPossibleKnightMoves(Piece piece, ChessBoard chessBoard)
    {
        var moves = new List<Position>();

        foreach (var delta in KnightDeltas)
        {
            int deltaX = delta[0];
            int deltaY = delta[1];

            var pos = new Position(piece.Position.X + deltaX, piece.Position.Y + deltaY);

            if (!pos.OutOfBounds)
            {
                var occupied = chessBoard.GetPieceAt(pos);

                if (occupied == null || occupied.IsBlack != piece.IsBlack)
                {
                    moves.Add(pos);
                }
            }
        }

        return moves;
    }
}
