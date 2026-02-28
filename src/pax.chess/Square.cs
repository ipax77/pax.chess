
namespace pax.chess;

public readonly record struct Square
{
    // The ONLY field. default(Square) = 0 (a1)
    private readonly int _index;
    public int Index => _index;

    // Direct Constructor
    public Square(int index)
    {
        if (index is < 0 or >= 64) throw new ArgumentOutOfRangeException(nameof(index));
        _index = index;
    }

    // Coordinate Constructor
    public Square(int file, int rank)
    {
        // No multiplication needed, just bitwise OR
        _index = (rank << 3) | file;
    }

    public int File => _index & 7;   // index % 8
    public int Rank => _index >> 3;  // index / 8

    public static Square FromIndex(int index) => new Square(index);

    public override string ToString() => $"{(char)('a' + File)}{Rank + 1}";
}