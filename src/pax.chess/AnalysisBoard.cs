using pax.chess.Validation;

namespace pax.chess;

/// <summary>
/// Represents a node in the move tree of an <see cref="AnalysisBoard"/>.
/// </summary>
public sealed class MoveNode
{
    /// <summary>
    /// The position after the move was applied.
    /// </summary>
    public BoardPosition Position { get; }

    /// <summary>
    /// The move that led to this position. Null for the root node.
    /// </summary>
    public Move? Move { get; }

    /// <summary>
    /// The parent node in the move tree. Null for the root node.
    /// </summary>
    public MoveNode? Parent { get; }

    /// <summary>
    /// A list of moves that can be played from this position.
    /// The first variation is typically the main line.
    /// </summary>
    public IReadOnlyList<MoveNode> Variations => _variations.AsReadOnly();
    private readonly List<MoveNode> _variations = [];

    /// <summary>
    /// Optional comment for this move.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Additional move information.
    /// </summary>
    public MoveInfo? MoveInfo { get; }

    /// <summary>
    /// The game state at this position (e.g., Check, Checkmate).
    /// </summary>
    public GameState GameState { get; }

    /// <summary>
    /// Creates a new move node.
    /// </summary>
    internal MoveNode(BoardPosition position, Move? move = null, MoveNode? parent = null, MoveInfo? moveInfo = null)
    {
        Position = position;
        Move = move;
        Parent = parent;
        MoveInfo = moveInfo;
        GameState = MoveValidator.GetGameState(position);
    }

    internal MoveNode AddVariation(Move move, MoveInfo? moveInfo = null)
    {
        var san = PgnSerializer.ToSan(move, Position);
        var nextPosition = Position.MakeMove(move);
        moveInfo = moveInfo == null ? new(move, null, san) : moveInfo with { San = san };
        var node = new MoveNode(nextPosition, move, this, moveInfo);
        _variations.Add(node);
        return node;
    }

    internal bool RemoveVariation(MoveNode node)
    {
        return _variations.Remove(node);
    }
}

/// <summary>
/// A chess board for analysis that supports move variations (branching) 
/// and navigation through the move tree.
/// </summary>
public sealed class AnalysisBoard
{
    /// <summary>
    /// The starting node of the move tree.
    /// </summary>
    public MoveNode Root { get; }

    /// <summary>
    /// The node the analysis is currently focused on.
    /// </summary>
    public MoveNode CurrentNode { get; private set; }

    /// <summary>
    /// The current board position.
    /// </summary>
    public BoardPosition CurrentPosition => CurrentNode.Position;

    private readonly ChessGame? _game;

    /// <summary>
    /// Initializes a new analysis board with the standard starting position.
    /// </summary>
    public AnalysisBoard()
    {
        Root = new MoveNode(BoardPosition.CreateInitial());
        CurrentNode = Root;
    }

    /// <summary>
    /// Initializes a new instance of the AnalysisBoard class using the specified chess game.
    /// </summary>
    /// <remarks>The constructor applies all moves from the provided game to set up the initial state of the
    /// analysis board. If any move is invalid, an exception is thrown.</remarks>
    /// <param name="game">The chess game to analyze. This parameter cannot be null.</param>
    /// <exception cref="InvalidOperationException">Thrown if an invalid move is detected during the initialization of the analysis board.</exception>
    public AnalysisBoard(ChessGame game)
    {
        ArgumentNullException.ThrowIfNull(game);
        _game = game;
        Root = new MoveNode(game.InitialPosition);
        CurrentNode = Root;
        foreach (var move in game.Moves)
        {
            var state = TryApplyMove(move.Move, new(move.Move, move.TimeRemaining));
            if (state != MoveState.Ok)
                throw new InvalidOperationException($"Invalid move in game: {move} at position {CurrentPosition}");
        }
    }

    /// <summary>
    /// Initializes a new analysis board with a specific starting position.
    /// </summary>
    public AnalysisBoard(BoardPosition initialPosition)
    {
        ArgumentNullException.ThrowIfNull(initialPosition);
        Root = new MoveNode(initialPosition);
        CurrentNode = Root;
    }

    /// <summary>
    /// Attempts to apply a move at the current position.
    /// If the move already exists as a variation, it switches to it.
    /// Otherwise, a new variation is created and switched to.
    /// </summary>
    /// <param name="move">The move to apply.</param>
    /// <param name="moveInfo">Optional move info.</param>
    /// <returns>The result of the move validation.</returns>
    public MoveState TryApplyMove(Move move, MoveInfo? moveInfo = null)
    {
        ArgumentNullException.ThrowIfNull(move);
        var state = MoveValidator.IsValidMove(move, CurrentNode.Position);
        if (state != MoveState.Ok)
            return state;

        var existing = CurrentNode.Variations.FirstOrDefault(v => v.Move == move);
        if (existing != null)
        {
            CurrentNode = existing;
        }
        else
        {
            CurrentNode = CurrentNode.AddVariation(move, moveInfo);
        }

        return MoveState.Ok;
    }

    /// <summary>
    /// Deletes a variation (and all its sub-variations) from the tree.
    /// If the current node is part of the deleted variation, the current node 
    /// is moved to the parent of the deleted variation.
    /// </summary>
    /// <param name="node">The node to delete. Root cannot be deleted.</param>
    /// <returns>True if the node was deleted; otherwise, false.</returns>
    public bool DeleteVariation(MoveNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (node == Root || node.Parent == null)
            return false;

        // Check if CurrentNode is node or a descendant of node
        var curr = CurrentNode;
        bool currentIsAffected = false;
        while (curr != null)
        {
            if (curr == node)
            {
                currentIsAffected = true;
                break;
            }
            curr = curr.Parent;
        }

        if (currentIsAffected)
        {
            CurrentNode = node.Parent;
        }

        return node.Parent.RemoveVariation(node);
    }

    /// <summary>
    /// Deletes the current variation (and all its sub-variations).
    /// The current node is moved to the parent node.
    /// </summary>
    /// <returns>True if the variation was deleted; otherwise, false (e.g., if at Root).</returns>
    public bool DeleteCurrentVariation()
    {
        if (CurrentNode == Root)
            return false;

        return DeleteVariation(CurrentNode);
    }

    /// <summary>
    /// Moves the analysis forward to the specified variation.
    /// </summary>
    /// <param name="variationIndex">The index of the variation to follow (0 for main line).</param>
    /// <returns>True if navigation was successful; otherwise, false.</returns>
    public bool MoveForward(int variationIndex = 0)
    {
        if (variationIndex < 0 || variationIndex >= CurrentNode.Variations.Count)
            return false;

        CurrentNode = CurrentNode.Variations[variationIndex];
        return true;
    }

    /// <summary>
    /// Moves the analysis back to the parent position.
    /// </summary>
    /// <returns>True if navigation was successful; otherwise, false.</returns>
    public bool MoveBackward()
    {
        if (CurrentNode.Parent == null)
            return false;

        CurrentNode = CurrentNode.Parent;
        return true;
    }

    /// <summary>
    /// Resets the analysis to the root position.
    /// </summary>
    public void GoToRoot() => CurrentNode = Root;

    /// <summary>
    /// Jumps the analysis to a specific node in the tree.
    /// </summary>
    public void GoToNode(MoveNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        CurrentNode = node;
    }

    /// <summary>
    /// Gets the sequence of move nodes from the root to the current node.
    /// </summary>
    public IReadOnlyList<MoveNode> GetPath()
    {
        var path = new List<MoveNode>();
        var curr = CurrentNode;
        while (curr != Root)
        {
            path.Add(curr);
            curr = curr.Parent!;
        }
        path.Reverse();
        return path;
    }
}
