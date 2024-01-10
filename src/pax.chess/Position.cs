namespace pax.chess;
public sealed record Position
{
    public byte X { get; set; }
    public byte Y { get; set; }

    public Position()
    {

    }

    public Position(byte x, byte y)
    {
        X = x;
        Y = y;
    }

    public Position(int x, int y)
    {
        X = (byte)x;
        Y = (byte)y;
    }

    public Position(string algebraicPos)
    {
        if (string.IsNullOrEmpty(algebraicPos)
            || algebraicPos.Length != 2)
        {
            ArgumentNullException.ThrowIfNull(algebraicPos);
        }

        byte ax = algebraicPos[0] switch
        {
            'a' => 0,
            'b' => 1,
            'c' => 2,
            'd' => 3,
            'e' => 4,
            'f' => 5,
            'g' => 6,
            'h' => 7,
            _ => throw new ArgumentOutOfRangeException(nameof(algebraicPos))
        };

        if (!int.TryParse(algebraicPos[1].ToString(), out int y))
        {
            throw new ArgumentOutOfRangeException(nameof(algebraicPos));
        }

        X = ax;
        Y = (byte)(y - 1);
    }

    public Position(Position position)
    {
        this.X = position?.X ?? throw new ArgumentNullException(nameof(position));
        this.Y = position.Y;
    }

    public int Index() => (int)Y * 8 + (int)X;

    public string ToAlgebraicNotation()
    {
        var ax = X switch
        {
            0 => 'a',
            1 => 'b',
            2 => 'c',
            3 => 'd',
            4 => 'e',
            5 => 'f',
            6 => 'g',
            7 => 'h',
            _ => ' '
        };
        return $"{ax}{Y + 1}";
    }

    public bool OutOfBounds => X < 0 || X > 7 || Y < 0 || Y > 7;

    public static readonly Position Unknown = new(byte.MaxValue, byte.MaxValue);
}
