
using pax.chess.Validation;

namespace pax.chess;

public sealed class ChessGame
{
    public BoardPosition CurrentPosition { get; private set; }

    public IReadOnlyList<MoveInfo> Moves => _moves.AsReadOnly();
    private readonly List<MoveInfo> _moves = [];

    public GameMetadata Metadata { get; private set; }
    public ChessClock? Clock { get; private set; }

    public GameResult Result => GameOutcomeEvaluator.Evaluate(CurrentPosition, _moves, _repetition);
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

    public void ActivatePositionHashing(IPositionHasher? positionHasher = null)
    {
        if (positionHasher is null)
        {
            this.positionHasher = new ZobristHasher();
        }
        else
        {
            this.positionHasher = positionHasher;
        }
        _currentKey = this.positionHasher.Compute(CurrentPosition);
        _repetition.Add(_currentKey, 1);
    }

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

    public int GetCurrentRepetitions()
    {
        if (_repetition.TryGetValue(_currentKey, out var count))
        {
            return count;
        }
        return 0;
    }

    public ulong CurrentHash => _currentKey;
}
