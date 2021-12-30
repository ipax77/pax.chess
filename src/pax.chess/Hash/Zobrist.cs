namespace pax.chess;
public static class Zobrist
{
    private static Random random = new Random(1070372);
    private static HashSet<uint> randomNumbers = new HashSet<uint>();
    private static List<uint> Numbers = new List<uint>();
    private static uint[][] PieceNumbers = new uint[14][];
    private static uint Start;

    public static void Init()
    {
        Start = GetUniqueRandomNumber();

        // 768 pieces black/white
        // 17 enpassant pos
        // 1 blacktomove
        // 1 castling

        // blacktomove
        Numbers.Add(GetUniqueRandomNumber());
        Numbers.Add(GetUniqueRandomNumber());


        // castling
        Numbers.Add(GetUniqueRandomNumber());
        Numbers.Add(GetUniqueRandomNumber());
        Numbers.Add(GetUniqueRandomNumber());
        Numbers.Add(GetUniqueRandomNumber());

        // enpassant
        Numbers.Add(GetUniqueRandomNumber());
        Numbers.Add(GetUniqueRandomNumber());
        Numbers.Add(GetUniqueRandomNumber());
        Numbers.Add(GetUniqueRandomNumber());
        Numbers.Add(GetUniqueRandomNumber());
        Numbers.Add(GetUniqueRandomNumber());
        Numbers.Add(GetUniqueRandomNumber());
        Numbers.Add(GetUniqueRandomNumber());

        for (int i = 0; i < 14; i++)
        {
            PieceNumbers[i] = new uint[64];
            for (int j = 0; j < 64; j++)
            {
                PieceNumbers[i][j] = GetUniqueRandomNumber();
            }
        }
    }

    public static uint GetHashCode(State state)
    {
        if (Start == 0)
        {
            Init();
        }

        List<uint> numbers = new List<uint>();

        numbers.Add(state.Info.BlackToMove ? Numbers[1] : Numbers[0]);
        if (state.Info.WhiteCanCastleKingSide)
        {
            numbers.Add(Numbers[2]);
        }
        if (state.Info.WhiteCanCastleQueenSide)
        {
            numbers.Add(Numbers[3]);
        }
        if (state.Info.BlackCanCastleKingSide)
        {
            numbers.Add(Numbers[4]);
        }
        if (state.Info.BlackCanCastleQueenSide)
        {
            numbers.Add(Numbers[5]);
        }

        if (state.Info.EnPassantPosition != null)
        {
            numbers.Add(Numbers[5 + state.Info.EnPassantPosition.X]);
        }

        int square = 0;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                numbers.Add(PieceNumber(state.Pieces.FirstOrDefault(f => f.Position.X == x && f.Position.Y == y), square));
                square++;
            }
        }

        return ZobHash(numbers);
    }

    private static uint PieceNumber(Piece? piece, int square)
    {
        int pieceNum = 0;
        if (piece != null)
        {
            pieceNum = piece.IsBlack ? (int)piece.Type + 6 : (int)piece.Type;
        }
        return PieceNumbers[pieceNum][square];
    }

    private static uint GetUniqueRandomNumber()
    {
        uint i = (uint)random.Next();
        while (randomNumbers.Contains(i))
        {
            i = (uint)random.Next();
        }
        randomNumbers.Add(i);
        return i;
    }

    private static uint ZobHash(List<uint> numbers)
    {
        var hash = Start;
        var i = 1;
        foreach (var c in numbers)
            hash ^= RotateLeft(c, i++);
        return hash;
    }

    private static uint RotateLeft(uint value, int count)
    {
        var r = count % 32;
        return (value << r) | (value >> (32 - r));
    }
}
