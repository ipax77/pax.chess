using pax.chess.Validation;

namespace pax.chess;

public readonly record struct MoveResult(
    MoveState State,
    Move? Move = null,
    string? San = null,
    string? Error = null)
{
    public bool IsOk => State == MoveState.Ok;
}

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

        var state = ValidateMove(move);
        if (state != MoveState.Ok)
            return state;

        ApplyValidatedMove(move, san);

        return MoveState.Ok;
    }

    /// <summary>
    /// Plays a move from UCI coordinate notation, such as <c>e2e4</c> or <c>e7e8q</c>.
    /// </summary>
    /// <param name="notation">The UCI move notation to parse and apply.</param>
    /// <returns>The move state, parsed move, SAN text, and optional error text.</returns>
    public MoveResult Play(string notation)
    {
        EnsureNotTerminated();

        var result = CreateMoveResult(notation);
        if (!result.IsOk || result.Move is null)
            return result;

        ApplyValidatedMove(result.Move, result.San);
        return result;
    }

    /// <summary>
    /// Serializes the current board position to FEN.
    /// </summary>
    /// <returns>The FEN representation of the current position.</returns>
    public string ToFen() => FenSerializer.Serialize(CurrentPosition);

    /// <summary>
    /// Serializes the game moves and metadata to PGN.
    /// </summary>
    /// <returns>The PGN representation of the current game.</returns>
    public string ToPgn() => PgnSerializer.Serialize(this);

    private MoveState ValidateMove(Move move)
    {
        return _options.SkipValidation
            ? MoveState.Ok
            : PseudoMoveValidator.IsValidMove(move, CurrentPosition);
    }

    private MoveResult CreateMoveResult(string notation)
    {
        if (!TryCreateUciMove(notation, CurrentPosition, out var move) || move is null)
        {
            return new MoveResult(
                MoveState.TargetInvalid,
                Error: "Move notation must be UCI coordinate notation, such as e2e4 or e7e8q.");
        }

        var state = ValidateMove(move);
        if (state != MoveState.Ok)
            return new MoveResult(state, move, Error: $"Move is not legal: {state}.");

        var san = PgnSerializer.ToSan(move, CurrentPosition);
        return new MoveResult(MoveState.Ok, move, san);
    }

    private void ApplyValidatedMove(Move move, string? san)
    {
        ExecuteMove(move, san);

        if (!_options.SkipEvaluation)
            Evaluate();
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

                var result = PseudoMoveValidator.IsWinnable(CurrentPosition, opponent)
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

    private static bool TryCreateUciMove(string? notation, BoardPosition position, out Move? move)
    {
        move = null;

        if (string.IsNullOrWhiteSpace(notation))
            return false;

        ReadOnlySpan<char> value = notation.AsSpan().Trim();
        if (value.Length is not (4 or 5))
            return false;

        int fromFile = GetFile(value[0]);
        int fromRank = GetRank(value[1]);
        int toFile = GetFile(value[2]);
        int toRank = GetRank(value[3]);

        if (fromFile < 0 || fromRank < 0 || toFile < 0 || toRank < 0)
            return false;

        PieceType? promotion = null;
        if (value.Length == 5 && !TryGetPromotion(value[4], out promotion))
            return false;

        var from = new Square(fromFile, fromRank);
        var to = new Square(toFile, toRank);
        var moveType = GetMoveType(from, to, promotion, position);

        move = new Move(from, to, promotion, moveType);
        return true;

        static int GetFile(char value)
        {
            char lower = ToLowerAscii(value);
            return lower is >= 'a' and <= 'h' ? lower - 'a' : -1;
        }

        static int GetRank(char value) => value is >= '1' and <= '8' ? value - '1' : -1;

        static bool TryGetPromotion(char value, out PieceType? promotion)
        {
            promotion = ToLowerAscii(value) switch
            {
                'q' => PieceType.Queen,
                'r' => PieceType.Rook,
                'b' => PieceType.Bishop,
                'n' => PieceType.Knight,
                _ => null
            };

            return promotion.HasValue;
        }

        static char ToLowerAscii(char value)
            => value is >= 'A' and <= 'Z' ? (char)(value + ('a' - 'A')) : value;
    }

    private static MoveType GetMoveType(Square from, Square to, PieceType? promotion, BoardPosition position)
    {
        var piece = position.Board[from.Index];
        var target = position.Board[to.Index];
        var moveType = MoveType.None;

        if (target.HasValue)
            moveType |= MoveType.Capture;

        if (promotion.HasValue)
            moveType |= MoveType.Promotion;

        if (piece?.Type == PieceType.King)
        {
            int fileDelta = to.File - from.File;
            if (Math.Abs(fileDelta) > 1)
                moveType |= fileDelta > 0 ? MoveType.CastlingKingSide : MoveType.CastlingQueenSide;
        }

        if (piece?.Type == PieceType.Pawn &&
            position.EnPassantTarget == to &&
            from.File != to.File &&
            !target.HasValue)
        {
            moveType |= MoveType.EnPassant | MoveType.Capture;
        }

        return moveType;
    }
}
