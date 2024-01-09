namespace pax.chess.Validation;
public static partial class Validate
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

    private static List<Position> GetPossiblePawnMoves(Piece piece, ChessBoard chessBoard)
    {
        int delta = piece.IsBlack ? -1 : 1;
        var moves = new List<Position>();

        AddForwardMove(piece, delta, moves, chessBoard);

        if (IsStartingPosition(piece))
        {
            AddDoubleForwardMove(piece, delta, moves, chessBoard);
        }

        AddCaptureMove(piece, delta, 1, moves, chessBoard);
        AddCaptureMove(piece, delta, -1, moves, chessBoard);

        return moves;
    }

    private static void AddForwardMove(Piece piece, int delta, List<Position> moves, ChessBoard chessBoard)
    {
        var pos = new Position(piece.Position.X, piece.Position.Y + delta);

        if (!pos.OutOfBounds && chessBoard.GetPieceAt(pos) == null)
        {
            moves.Add(pos);
        }
    }

    private static void AddDoubleForwardMove(Piece piece, int delta, List<Position> moves, ChessBoard chessBoard)
    {
        var pos = new Position(piece.Position.X, piece.Position.Y + (2 * delta));

        if (!pos.OutOfBounds && chessBoard.GetPieceAt(pos) == null)
        {
            moves.Add(pos);
        }
    }

    private static void AddCaptureMove(Piece piece, int delta, int offset, List<Position> moves, ChessBoard chessBoard)
    {
        var capPos = new Position(piece.Position.X + offset, piece.Position.Y + delta);

        if (!capPos.OutOfBounds)
        {
            if (IsEnPassantCapture(capPos, chessBoard) || IsNormalCapture(capPos, piece, chessBoard))
            {
                moves.Add(capPos);
            }
        }
    }

    private static bool IsStartingPosition(Piece piece)
    {
        return (piece.IsBlack && piece.Position.Y == 6) || (!piece.IsBlack && piece.Position.Y == 1);
    }

    private static bool IsEnPassantCapture(Position capPos, ChessBoard chessBoard)
    {
        return chessBoard.EnPassantPosition == capPos;
    }

    private static bool IsNormalCapture(Position capPos, Piece piece, ChessBoard chessBoard)
    {
        var enemy = chessBoard.GetPieceAt(capPos);

        return enemy != null && enemy.IsBlack != piece.IsBlack;
    }
}
