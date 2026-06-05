using pax.chess.Validation;

namespace pax.chess;

public sealed class ChessGame
{
    public BoardPosition InitialPosition { get; }
    public BoardPosition CurrentPosition { get; private set; }
    public BoardPosition? PreviousPosition { get; private set; }

    public IReadOnlyList<MoveInfo> Moves => _moves.AsReadOnly();

    public GameMetadata Metadata { get; }
    public ChessClock? Clock { get; }
    public GameConclusion? Conclusion { get; private set; }
    public GameResult? Result => Conclusion?.Result
        ?? GameOutcomeEvaluator.Evaluate(CurrentPosition, _repetition)?.Result;
    public EventHandler? OnMoveApplied { get; set; }

    public ulong? CurrentHash => _currentKey;
    public int? GetCurrentRepetitions() =>
        _repetition.TryGetValue(_currentKey, out var count) ? count : 0;

    private readonly ChessGameOptions _options;
    private readonly IPositionHasher _hasher;
    private readonly Dictionary<ulong, int> _repetition = [];
    private ulong _currentKey;
    private readonly List<MoveInfo> _moves = [];

    public ChessGame() : this(BoardPosition.CreateInitial(), ChessGameOptions.Default) { }

    public ChessGame(ChessGameOptions options) : this(BoardPosition.CreateInitial(), options) { }

    public ChessGame(BoardPosition initialPosition, ChessGameOptions? options = null)
    {
        _options = options ?? ChessGameOptions.Default;
        _hasher = _options.Hasher ?? new ZobristHasher();

        InitialPosition = initialPosition;
        CurrentPosition = initialPosition;
        Metadata = _options.Metadata;
        Clock = _options.Clock;

        _currentKey = _hasher.Compute(CurrentPosition);
        _repetition[_currentKey] = 1;
    }

    public void Evaluate()
    {
        var evaluated = GameOutcomeEvaluator.Evaluate(CurrentPosition, _repetition);

        if (evaluated is not null)
        {
            Conclusion = new GameConclusion(
                evaluated.Termination,
                evaluated.Result
            );
            Clock?.Pause();
        }
    }

    /// <summary>
    /// Attempts to apply a move, validating it first unless <see cref="ChessGameOptions.SkipValidation"/> is set.
    ///
    /// If validation fails, the corresponding <see cref="MoveState"/> is returned and no changes are made.
    /// If validation passes (or is skipped), the move is executed.
    /// Unless <see cref="ChessGameOptions.SkipEvaluation"/> is set, the position is evaluated afterwards.
    /// </summary>
    /// <param name="move">The move to apply.</param>
    /// <param name="san">Optional SAN string for the move.</param>
    /// <returns>
    /// <see cref="MoveState.Ok"/> if the move was applied successfully;
    /// otherwise the specific validation failure.
    /// </returns>
    public MoveState ApplyMove(Move move, string? san = null)
    {
        EnsureNotTerminated();

        if (!_options.SkipValidation)
        {
            var state = PseudoMoveValidator.IsValidMove(move, CurrentPosition);
            if (state != MoveState.Ok)
                return state;
        }

        ExecuteMove(move, san);

        if (!_options.SkipEvaluation)
            Evaluate();

        return MoveState.Ok;
    }

    private void ExecuteMove(Move move, string? san)
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
        _moves.Add(new MoveInfo(move, remaining, san));

        if (next.Board.GetPieces(PieceColor.White).Count == 1 && next.Board.GetPieces(PieceColor.Black).Count == 1)
        {
            UpdatePosition(next);
            Conclusion = new(GameTermination.NoMaterial, GameResult.Draw);
            return;
        }

        _currentKey = _hasher.Update(_currentKey, CurrentPosition, move, next);
        _repetition.TryGetValue(_currentKey, out var count);
        _repetition[_currentKey] = count + 1;

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

        Conclusion = new GameConclusion(
            GameTermination.Resignation,
            color == PieceColor.White ? GameResult.BlackWin : GameResult.WhiteWin,
            color
        );
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

    private void EnsureNotTerminated()
    {
        if (Conclusion is not null)
            throw new InvalidOperationException("Game is already terminated.");
    }
}