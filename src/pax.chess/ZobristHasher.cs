namespace pax.chess;

public interface IPositionHasher
{
    ulong Compute(BoardPosition position);

    ulong Update(
        ulong previousKey,
        BoardPosition previous,
        Move move,
        BoardPosition nextPos);
}

public sealed class ZobristHasher : IPositionHasher
{
    public ulong Compute(BoardPosition position)
    {
        return Zobrist.Compute(position);
    }

    public ulong Update(
        ulong previousKey,
        BoardPosition previous,
        Move move,
        BoardPosition nextPos)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(move);
        ArgumentNullException.ThrowIfNull(nextPos);

        ulong key = previousKey;
        var movingPiece = previous.Board[move.From.Index];
        if (!movingPiece.HasValue)
        {
            return previousKey;
        }
        var targetPiece = previous.Board[move.To.Index];

        var colorIndex = movingPiece.Value.Color == PieceColor.White ? 0 : 1;
        var pieceIndex = (int)movingPiece.Value.Type;

        key ^= Zobrist.SideToMove;
        key ^= Zobrist.PieceSquare[colorIndex][pieceIndex][move.From.Index];

        if (targetPiece.HasValue)
        {
            var capturedColor = targetPiece.Value.Color == PieceColor.White ? 0 : 1;
            var capturedType = (int)targetPiece.Value.Type;

            key ^= Zobrist.PieceSquare[capturedColor][capturedType][move.To.Index];
        }

        if (move.MoveType == MoveType.EnPassant)
        {
            int captureSquare = movingPiece.Value.Color == PieceColor.White
                ? move.To.Index - 8
                : move.To.Index + 8;

            int capturedColor = movingPiece.Value.Color == PieceColor.White ? 1 : 0;

            key ^= Zobrist.PieceSquare[capturedColor][(int)PieceType.Pawn][captureSquare];
        }

        key ^= Zobrist.Castling[(int)previous.CastlingRights];

        if (previous.EnPassantTarget is not null)
            key ^= Zobrist.EnPassantFile[previous.EnPassantTarget.Value.File];

        int newPieceIndex = move.Promotion.HasValue
            ? (int)move.Promotion.Value
            : (int)movingPiece.Value.Type;
        key ^= Zobrist.PieceSquare[colorIndex][newPieceIndex][move.To.Index];

        key ^= Zobrist.Castling[(int)nextPos.CastlingRights];

        if (move.MoveType == MoveType.CastlingKingSide ||
            move.MoveType == MoveType.CastlingQueenSide)
        {
            bool isWhite = movingPiece.Value.Color == PieceColor.White;
            int rookRank = isWhite ? 0 : 7;

            int rookFromFile = move.MoveType == MoveType.CastlingKingSide ? 7 : 0;
            int rookToFile = move.MoveType == MoveType.CastlingKingSide ? 5 : 3;

            int rookFromIndex = (rookRank << 3) | rookFromFile;
            int rookToIndex = (rookRank << 3) | rookToFile;

            int rookColorIndex = colorIndex;
            int rookPieceIndex = (int)PieceType.Rook;

            // Remove rook from original square
            key ^= Zobrist.PieceSquare[rookColorIndex][rookPieceIndex][rookFromIndex];

            // Add rook to new square
            key ^= Zobrist.PieceSquare[rookColorIndex][rookPieceIndex][rookToIndex];
        }

        return key;
    }
}