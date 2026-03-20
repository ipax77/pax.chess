namespace pax.chess;

public sealed class ChessGameOptions
{
    public static readonly ChessGameOptions Default = new();
    public static readonly ChessGameOptions Engine = new() { SkipValidation = true, SkipEvaluation = true };

    public bool SkipValidation { get; init; }
    public bool SkipEvaluation { get; init; }
    public IPositionHasher? Hasher { get; init; } = new ZobristHasher();
    public GameMetadata Metadata { get; init; } = new();
    public ChessClock? Clock { get; init; }
}