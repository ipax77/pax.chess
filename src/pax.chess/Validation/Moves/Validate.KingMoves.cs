namespace pax.chess.Validation;
public static partial class Validate
{
    private static readonly int[][] KingDeltas =
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

    private static List<Position> GetPossibleKingMoves(Piece piece, ChessBoard chessBoard)
    {
        var moves = new List<Position>();

        foreach (var delta in KingDeltas)
        {
            int deltaX = delta[0];
            int deltaY = delta[1];

            var pos = new Position(piece.Position.X + deltaX, piece.Position.Y + deltaY);
            if (!pos.OutOfBounds)
            {
                var targetPiece = chessBoard.GetPieceAt(pos);

                if (targetPiece == null || targetPiece.IsBlack != piece.IsBlack)
                {
                    moves.Add(pos);
                }
            }
        }

        AddCastlingMoves(piece, chessBoard, moves);

        return moves;
    }

    private static void AddCastlingMoves(Piece king, ChessBoard chessBoard, List<Position> moves)
    {
        if (CanCastleKingSide(king, chessBoard))
        {
            moves.Add(GetKingSideCastlingTarget(king));
        }

        if (CanCastleQueenSide(king, chessBoard))
        {
            moves.Add(GetQueenSideCastlingTarget(king));
        }
    }

    private static bool CanCastleKingSide(Piece king, ChessBoard chessBoard)
    {
        if (king.IsBlack && chessBoard.BlackCanCastleKingSide)
        {
            return !ArePiecesBetween(new Position(5, 7), new Position(7, 7), chessBoard);
        }

        if (!king.IsBlack && chessBoard.WhiteCanCastleKingSide)
        {
            return !ArePiecesBetween(new Position(5, 0), new Position(7, 0), chessBoard);
        }

        return false;
    }

    private static bool CanCastleQueenSide(Piece king, ChessBoard chessBoard)
    {
        if (king.IsBlack && chessBoard.BlackCanCastleQueenSide)
        {
            return !ArePiecesBetween(new Position(1, 7), new Position(3, 7), chessBoard);
        }

        if (!king.IsBlack && chessBoard.WhiteCanCastleQueenSide)
        {
            return !ArePiecesBetween(new Position(1, 0), new Position(3, 0), chessBoard);
        }

        return false;
    }

    private static bool ArePiecesBetween(Position start, Position end, ChessBoard chessBoard)
    {
        int startX = Math.Min(start.X, end.X);
        int endX = Math.Max(start.X, end.X);

        for (int x = startX + 1; x < endX; x++)
        {
            if (chessBoard.GetPieceAt(new Position(x, start.Y)) != null)
            {
                return true;
            }
        }

        return false;
    }

    private static Position GetKingSideCastlingTarget(Piece king)
    {
        return king.IsBlack ? new Position(6, 7) : new Position(6, 0);
    }

    private static Position GetQueenSideCastlingTarget(Piece king)
    {
        return king.IsBlack ? new Position(2, 7) : new Position(2, 0);
    }
}
