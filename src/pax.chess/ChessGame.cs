
using pax.chess.Validation;

namespace pax.chess;

public sealed class ChessGame
{
    public BoardPosition InitialPosition { get; private set; } = BoardPosition.CreateInitial();
    public BoardPosition CurrentPosition { get; private set; }
    public BoardPosition? PreviousPosition { get; private set; }

    public IReadOnlyList<MoveInfo> Moves => _moves.AsReadOnly();

    public GameMetadata Metadata { get; private set; }
    public ChessClock? Clock { get; private set; }
    public GameConclusion? Conclusion { get; private set; }
    public GameResult? Result => Conclusion?.Result
        ?? GameOutcomeEvaluator.Evaluate(CurrentPosition, _moves, _repetition)?.Result;
    public EventHandler? OnMoveApplied { get; set; }

    private IPositionHasher? positionHasher;
    private readonly Dictionary<ulong, int> _repetition = [];
    private ulong _currentKey;
    private readonly List<MoveInfo> _moves = [];

    public ChessGame()
    {
        CurrentPosition = BoardPosition.CreateInitial();
        Metadata = new();
    }

    public ChessGame(BoardPosition initialPosition, GameMetadata metadata, ChessClock? clock = null)
    {
        InitialPosition = initialPosition;
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

    public void Evaluate()
    {
        var evaluated = GameOutcomeEvaluator.Evaluate(CurrentPosition, _moves, _repetition);

        if (evaluated is not null)
        {
            Conclusion = new GameConclusion(
                evaluated.Termination,
                evaluated.Result
            );
            Clock?.Pause();
        }
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
    /// Attempts to apply a move in a user-facing (UI) context.
    /// 
    /// This method performs full validation before execution. If the move is invalid,
    /// the corresponding <see cref="MoveState"/> is returned and no changes are made.
    /// 
    /// When the move is valid:
    /// - The move is executed.
    /// - The position is evaluated.
    /// - The resulting <see cref="MoveState"/> is returned.
    /// 
    /// Intended for interactive use where validation feedback and evaluation
    /// updates are required.
    /// </summary>
    /// <param name="move">The move to validate and apply.</param>
    /// <returns>
    /// The validation result. <see cref="MoveState.Ok"/> if the move was successfully applied;
    /// otherwise, the specific validation failure.
    /// </returns>
    public MoveState TryApplyMove(Move move)
    {
        EnsureNotTerminated();
        var state = MoveValidator.IsValidMove(move, CurrentPosition);
        if (state != MoveState.Ok)
        {
            return state;
        }
        ApplyMove(move);
        Evaluate();
        return state;
    }

    /// <summary>
    /// Applies a move without performing validation.
    /// 
    /// This method assumes the move is already known to be legal and executes it
    /// with minimal overhead. It is optimized for performance-critical paths
    /// (e.g., UCI engine move execution).
    /// 
    /// Responsibilities:
    /// - Updates the game clock (if present)
    /// - Detects timeout and determines the resulting game conclusion
    /// - Updates repetition tracking and position hash (if enabled)
    /// - Advances <see cref="CurrentPosition"/>
    /// 
    /// No validation or evaluation is performed.
    /// </summary>
    /// <param name="move">A pre-validated move to execute.</param>
    public void ApplyMove(Move move)
    {
        var color = CurrentPosition.SideToMove;
        TimeSpan? remaining = null;

        if (Clock != null)
        {
            Clock.ApplyMove(color);
            remaining = color == PieceColor.White ? Clock.WhiteTime : Clock.BlackTime;

            if (Clock.HasTimedOut(color))
            {
                var opponent = color == PieceColor.White
                    ? PieceColor.Black
                    : PieceColor.White;

                var result = MoveValidator.IsWinnable(CurrentPosition, opponent)
                    ? (color == PieceColor.White ? GameResult.BlackWin : GameResult.WhiteWin)
                    : GameResult.Draw;

                Conclusion = new GameConclusion(
                    GameTermination.Timeout,
                    result,
                    color
                );
                return;
            }
        }

        var next = CurrentPosition.MakeMove(move);
        _moves.Add(new MoveInfo(move, remaining));

        if (next.Board.GetPieces(PieceColor.White).Count == 1 && next.Board.GetPieces(PieceColor.Black).Count == 1)
        {
            UpdatePosition(next);
            Conclusion = new(GameTermination.NoMaterial, GameResult.Draw);
            return;
        }

        if (positionHasher is not null)
        {
            _currentKey = positionHasher.Update(_currentKey, CurrentPosition, move, next);
            _repetition.TryGetValue(_currentKey, out var count);
            _repetition[_currentKey] = count + 1;
        }
        UpdatePosition(next);
        OnMoveApplied?.Invoke(this, EventArgs.Empty);
    }

    private void UpdatePosition(BoardPosition pos)
    {
        PreviousPosition = CurrentPosition.Clone();
        CurrentPosition = pos;
    }

    public void Resign(PieceColor color)
    {
        EnsureNotTerminated();

        var result = color == PieceColor.White
            ? GameResult.BlackWin
            : GameResult.WhiteWin;

        Conclusion = new GameConclusion(
            GameTermination.Resignation,
            result,
            color
        );
    }

    private void EnsureNotTerminated()
    {
        if (Conclusion is not null)
            throw new InvalidOperationException("Game is already terminated.");
    }

    public void AcceptDraw(PieceColor color)
    {
        EnsureNotTerminated();

        Conclusion = new GameConclusion(
            GameTermination.DrawByAgreement,
            GameResult.Draw,
            color
        );
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

public sealed record GameConclusion(
    GameTermination Termination,
    GameResult Result,
    PieceColor? AffectedPlayer = null
);