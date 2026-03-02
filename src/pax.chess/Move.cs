namespace pax.chess;

public sealed record Move(
    Square From,
    Square To,
    PieceType? Promotion = null,
    MoveType MoveType = MoveType.None
);
