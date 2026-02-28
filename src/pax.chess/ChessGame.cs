
namespace pax.chess;

public sealed class ChessGame(BoardPosition initialPosition, GameMetadata metadata)
{
    public BoardPosition CurrentPosition { get; private set; } = initialPosition ?? throw new ArgumentNullException(nameof(initialPosition));

    public IReadOnlyList<Move> Moves => _moves.AsReadOnly();
    private readonly List<Move> _moves = [];

    public GameMetadata Metadata { get; } = metadata ?? new GameMetadata();

    public GameResult Result { get; private set; } = GameResult.Ongoing;

    public static ChessGame CreateStandard()
    {
        return new ChessGame(BoardPosition.CreateInitial(), new GameMetadata());
    }

    public void ApplyMove(Move move, IMoveValidator? validator = null)
    {
        if (validator != null && !validator.IsLegal(CurrentPosition, move))
            throw new InvalidOperationException("Illegal move.");

        CurrentPosition = CurrentPosition.MakeMove(move);
        _moves.Add(move);
    }
}

public sealed record Move(
    Square From,
    Square To,
    PieceType? Promotion = null,
    MoveType MoveType = MoveType.None
);

public sealed class GameMetadata
{
    public string? Event { get; set; }
    public string? Site { get; set; }
    public DateTime? Date { get; set; }
    public string? Round { get; set; }
    public string? White { get; set; }
    public string? Black { get; set; }
    public string? Annotator { get; set; }

    public Dictionary<string, string> AdditionalTags { get; } = new();
}

public interface IMoveValidator
{
    bool IsLegal(BoardPosition position, Move move);
    IEnumerable<Move> GenerateLegalMoves(BoardPosition position);
}