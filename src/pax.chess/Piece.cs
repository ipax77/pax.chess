namespace pax.chess;
public record Piece
{
    public PieceType Type { get; set; }
    public bool IsBlack { get; init; }
    public Position Position { get; set; }
    public Piece(PieceType type, bool isBlack, int x, int y)
    {
        Type = type;
        IsBlack = isBlack;
        Position = new Position(x, y);
    }

    public Piece(Piece piece)
    {
        ArgumentNullException.ThrowIfNull(piece);
        Type = piece.Type;
        IsBlack = piece.IsBlack;
        Position = new Position(piece.Position.X, piece.Position.Y);
    }

    public string Notation()
    {
        return Type switch
        {
            PieceType.Pawn => "P",
            PieceType.Queen => "Q",
            PieceType.King => "K",
            PieceType.Rook => "R",
            PieceType.Bishop => "B",
            PieceType.Knight => "N",
            _ => string.Empty
        };
    }

    public static Piece Unknown => new(PieceType.None, false, 255, 255);
}
