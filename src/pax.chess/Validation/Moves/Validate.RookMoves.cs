namespace pax.chess.Validation;
public static partial class Validate
{
    private static readonly int[][] RookDeltas =
    [
        [0, 1],
        [0, -1],
        [1, 0],
        [-1, 0]
    ];

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

    private static List<Position> GetPossibleRookMoves(Piece piece, ChessBoard chessBoard)
    {
        var moves = new List<Position>();

        foreach (var delta in RookDeltas)
        {
            int deltaX = delta[0];
            int deltaY = delta[1];

            var pos = new Position(piece.Position.X + deltaX, piece.Position.Y + deltaY);

            while (!pos.OutOfBounds)
            {
                var occupied = chessBoard.GetPieceAt(pos);

                if (occupied != null)
                {
                    if (occupied.IsBlack != piece.IsBlack)
                    {
                        moves.Add(pos);
                    }
                    break;
                }

                moves.Add(pos);
                pos = new Position(pos.X + deltaX, pos.Y + deltaY);
            }
        }

        return moves;
    }
}