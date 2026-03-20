namespace pax.chess;

public sealed record GameConclusion(
    GameTermination Termination,
    GameResult Result,
    PieceColor? AffectedPlayer = null
);