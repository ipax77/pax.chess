namespace pax.chess;

public static class Zobrist
{
    public static readonly ulong[][][] PieceSquare = InitializePieceSquare();
    public static readonly ulong[] Castling = InitializeCastling();
    public static readonly ulong[] EnPassantFile = InitializeEnPassantFile();
    public static readonly ulong SideToMove = InitializeSideToMove();

    public static ulong Compute(BoardPosition pos)
    {
        ArgumentNullException.ThrowIfNull(pos);
        ulong hash = 0;

        foreach (var (sq, piece) in pos.Board.GetPieces(PieceColor.White))
            hash ^= PieceSquare[0][(int)piece.Type][sq.Index];

        foreach (var (sq, piece) in pos.Board.GetPieces(PieceColor.Black))
            hash ^= PieceSquare[1][(int)piece.Type][sq.Index];

        hash ^= Castling[(int)pos.CastlingRights];

        if (pos.EnPassantTarget is not null)
            hash ^= EnPassantFile[pos.EnPassantTarget.Value.File];

        if (pos.SideToMove == PieceColor.Black)
            hash ^= SideToMove;

        return hash;
    }

    private static ulong[][][] InitializePieceSquare()
    {
        var rng = new Random(1070372);
        var pieceSquare = new ulong[2][][];

        ulong Next()
        {
            var buffer = new byte[8];
#pragma warning disable CA5394 // Do not use insecure randomness
            rng.NextBytes(buffer);
#pragma warning restore CA5394 // Do not use insecure randomness
            return BitConverter.ToUInt64(buffer);
        }

        for (int color = 0; color < 2; color++)
        {
            pieceSquare[color] = new ulong[6][];
            for (int piece = 0; piece < 6; piece++)
            {
                pieceSquare[color][piece] = new ulong[64];
                for (int square = 0; square < 64; square++)
                    pieceSquare[color][piece][square] = Next();
            }
        }

        return pieceSquare;
    }

    private static ulong[] InitializeCastling()
    {
        var rng = new Random(1070372);
        var castling = new ulong[16];

        ulong Next()
        {
            var buffer = new byte[8];
#pragma warning disable CA5394 // Do not use insecure randomness
            rng.NextBytes(buffer);
#pragma warning restore CA5394 // Do not use insecure randomness
            return BitConverter.ToUInt64(buffer);
        }

        for (int i = 0; i < 16; i++)
            castling[i] = Next();

        return castling;
    }

    private static ulong[] InitializeEnPassantFile()
    {
        var rng = new Random(1070372);
        var enPassantFile = new ulong[8];

        ulong Next()
        {
            var buffer = new byte[8];
#pragma warning disable CA5394 // Do not use insecure randomness
            rng.NextBytes(buffer);
#pragma warning restore CA5394 // Do not use insecure randomness
            return BitConverter.ToUInt64(buffer);
        }

        for (int i = 0; i < 8; i++)
            enPassantFile[i] = Next();

        return enPassantFile;
    }

    private static ulong InitializeSideToMove()
    {
        var rng = new Random(1070372);

        ulong Next()
        {
            var buffer = new byte[8];
#pragma warning disable CA5394 // Do not use insecure randomness
            rng.NextBytes(buffer);
#pragma warning restore CA5394 // Do not use insecure randomness
            return BitConverter.ToUInt64(buffer);
        }

        return Next();
    }
}