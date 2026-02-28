
namespace pax.chess.Validation;

public static partial class MoveValidator
{
    private static readonly int[][] kingDeltas =
    [
        [0, 1],
        [0, -1],
        [1, 0],
        [-1, 0],
        [1, 1],
        [1, -1],
        [-1, 1],
        [-1, -1]
    ];

    private static List<Square> GetKingMoves(Square from, BoardPosition pos)
    {
        var piece = pos.Board[from.Index];

        if (!piece.HasValue || piece.Value.Type != PieceType.King)
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

        if (piece.Value.Color == PieceColor.Black)
        {
            if (pos.CastlingRights.HasFlag(CastlingRights.BlackKingSide))
            {
                var p1 = pos.Board[new Square(5, 7).Index];
                var p2 = pos.Board[new Square(6, 7).Index];
                if (!p1.HasValue && !p2.HasValue)
                    moves.Add(new Square(6, 7));
                
            }
            if (pos.CastlingRights.HasFlag(CastlingRights.BlackQueenSide))
            {
                var p1 = pos.Board[new Square(1, 7).Index];
                var p2 = pos.Board[new Square(2, 7).Index];
                var p3 = pos.Board[new Square(3, 7).Index];
                if (!p1.HasValue && !p2.HasValue && !p3.HasValue)
                    moves.Add(new Square(2, 7));                    
            }
        }
        else
        {
            if (pos.CastlingRights.HasFlag(CastlingRights.WhiteKingSide))
            {
                var p1 = pos.Board[new Square(5, 0).Index];
                var p2 = pos.Board[new Square(6, 0).Index];
                if (!p1.HasValue && !p2.HasValue)
                    moves.Add(new Square(6, 0));
                
            }
            if (pos.CastlingRights.HasFlag(CastlingRights.WhiteQueenSide))
            {
                var p1 = pos.Board[new Square(1, 0).Index];
                var p2 = pos.Board[new Square(2, 0).Index];
                var p3 = pos.Board[new Square(3, 0).Index];
                if (!p1.HasValue && !p2.HasValue && !p3.HasValue)
                    moves.Add(new Square(2, 0));                    
            }
        }
        return moves;
    }
}