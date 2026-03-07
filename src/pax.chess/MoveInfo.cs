namespace pax.chess;

public sealed record MoveInfo(
    Move Move,
    TimeSpan? TimeRemaining = null,
    string? San = null
);
