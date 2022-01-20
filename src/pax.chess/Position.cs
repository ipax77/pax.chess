namespace pax.chess;
public sealed record Position
{
    public byte X { get; set; }
    public byte Y { get; set; }
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

    public Position(Position position)
    {
        this.X = position?.X ?? throw new ArgumentNullException(nameof(position));
        this.Y = position.Y;
    }

    public bool OutOfBounds => X < 0 || X > 7 || Y < 0 || Y > 7;
}
