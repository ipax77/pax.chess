using pax.chess.Validation;

namespace pax.chess.Extensions;

public static class Uci
{
    public static Move? CreateMove(string notation, BoardPosition pos)
    {
        if (string.IsNullOrWhiteSpace(notation))
            return null;
        ArgumentNullException.ThrowIfNull(pos);

#pragma warning disable CA1308 // Normalize strings to uppercase
        notation = notation.Trim().ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase

        // UCI must be 4 or 5 characters
        if (notation.Length != 4 && notation.Length != 5)
            return null;

        // From square
        int fromFile = FenSerializer.GetColumnIndex(notation[0]);
        int fromRank = notation[1] - '1';

        // To square
        int toFile = FenSerializer.GetColumnIndex(notation[2]);
        int toRank = notation[3] - '1';

        if (fromFile < 0 || toFile < 0 || fromRank < 0 || toRank < 0)
            return null;

        PieceType? promotion = null;

        if (notation.Length == 5)
        {
            promotion = FenSerializer.GetPieceType(notation[4]);
        }

        var startSquare = new Square(fromFile, fromRank);
        var targetSquare = new Square(toFile, toRank);

        MoveType moveType = MoveType.None;
        var piece = pos.Board[startSquare.Index];

        if (piece?.Type == PieceType.King)
        {
            int fileDelta = targetSquare.File - startSquare.File;

            if (Math.Abs(fileDelta) > 1)
            {
                if (fileDelta > 0)
                    moveType = MoveType.CastlingKingSide;
                else
                    moveType = MoveType.CastlingQueenSide;
            }
        }
        return new Move(startSquare, targetSquare, promotion, moveType);
    }

    public static string GetUci(Move move)
    {
        if (move == null) return string.Empty;

        string from = GetSquareNotation(move.From);
        string to = GetSquareNotation(move.To);
        string promotion = "";

        if (move.Promotion != null)
        {
            // Promotion must be lowercase (q, r, b, n)
            promotion = FenSerializer.GetPieceString(move.Promotion.Value);
        }

        return $"{from}{to}{promotion}";
    }

    private static string GetSquareNotation(Square sq)
    {
        char file = (char)('a' + sq.File);
        char rank = (char)('1' + sq.Rank);
        return $"{file}{rank}";
    }
}