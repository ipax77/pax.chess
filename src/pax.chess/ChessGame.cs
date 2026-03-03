
using pax.chess.Validation;

namespace pax.chess;

public sealed class ChessGame
{
    public BoardPosition CurrentPosition { get; private set; }

    public IReadOnlyList<MoveInfo> Moves => _moves.AsReadOnly();
    private readonly List<MoveInfo> _moves = [];

    public GameMetadata Metadata { get; private set; }
    public ChessClock? Clock { get; private set; }
    public GameTermination Termination { get; private set; } = GameTermination.None;
    public PieceColor? Resignee { get; private set; }  // who resigned
    public GameResult Result => Termination switch
{
    GameTermination.Resignation => Resignee == PieceColor.White
        ? GameResult.BlackWin
        : GameResult.WhiteWin,
        GameTermination.DrawAccepted => GameResult.Draw,
        _ => GameOutcomeEvaluator.Evaluate(CurrentPosition, _moves, _repetition)
};
    private IPositionHasher? positionHasher;
    private readonly Dictionary<ulong, int> _repetition = [];
    private ulong _currentKey;

    public ChessGame()
    {
        CurrentPosition = BoardPosition.CreateInitial();
        Metadata = new();
    }

    public ChessGame(BoardPosition initialPosition, GameMetadata metadata, ChessClock? clock = null)
    {
        CurrentPosition = initialPosition;
        Metadata = metadata;
        Clock = clock;
    }

    public void SetMetadata(GameMetadata metadata)
    {
        Metadata = metadata;
    }

    public void SetClock(ChessClock clock)
    {
        Clock = clock;
    }

    public void ActivatePositionHashing(IPositionHasher? hasher = null)
    {
        if (positionHasher is not null)
            throw new InvalidOperationException("Hashing is already active.");

        positionHasher = hasher ?? new ZobristHasher();
        _currentKey = positionHasher.Compute(CurrentPosition);
        _repetition[_currentKey] = 1;
    }

    /// <summary>
    /// Validate Move and execute only when valid.
    /// </summary>
    /// <param name="move"></param>
    /// <returns></returns>
    public MoveState TryApplyMove(Move move)
    {
        var state = MoveValidator.IsValidMove(move, CurrentPosition);
        if (state != MoveState.Ok)
        {
            return state;
        }
        ApplyMove(move);
        return state;
    }

    /// <summary>
    /// Execute move without validation
    /// </summary>
    /// <param name="move"></param>
    public void ApplyMove(Move move)
    {
        var color = CurrentPosition.SideToMove;
        TimeSpan? remaining = null;

        if (Clock != null)
        {
            Clock.ApplyMove(color);
            remaining = color == PieceColor.White ? Clock.WhiteTime : Clock.BlackTime;
        }

        var next = CurrentPosition.MakeMove(move);
        _moves.Add(new MoveInfo(move, remaining));

        if (positionHasher is not null)
        {
            _currentKey = positionHasher.Update(_currentKey, CurrentPosition, move, next);
            _repetition.TryGetValue(_currentKey, out var count);
            _repetition[_currentKey] = count + 1;
        }
        CurrentPosition = next;
    }

    public void Resign(PieceColor color)
    {
        if (Termination != GameTermination.None)
            throw new InvalidOperationException("Game is already terminated.");

        Resignee = color;
        Termination = GameTermination.Resignation;
    }

    public void AcceptDraw()
    {
        if (Termination != GameTermination.None)
            throw new InvalidOperationException("Game is already terminated.");

        Termination = GameTermination.DrawAccepted;
    }

    public int? GetCurrentRepetitions()
    {
        if (positionHasher is null)
        {
            return null;
        }

        if (_repetition.TryGetValue(_currentKey, out var count))
        {
            return count;
        }
        return 0;
    }

    public ulong? CurrentHash => positionHasher is null ? null : _currentKey;
}
