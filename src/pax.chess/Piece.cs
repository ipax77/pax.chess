﻿namespace pax.chess;
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
}
