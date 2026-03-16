using pax.chess.Extensions;
using System.Text;

namespace pax.chess.Analyze;

public sealed class AnalysisBoard
{
    public MoveNode Root { get; private set; }

    public MoveNode CurrentNode { get; private set; }

    public BoardPosition CurrentPosition { get; private set; }

    private readonly BoardPosition _initialPosition;

    public ChessGame ChessGame { get; private set; }

    public AnalysisBoard(ChessGame game)
    {
        ArgumentNullException.ThrowIfNull(game);
        ChessGame = game;
        _initialPosition = game.InitialPosition;

        Root = BuildMainLine(game);
        CurrentNode = Root;
        CurrentPosition = _initialPosition;
    }

    private MoveNode BuildMainLine(ChessGame game)
    {
        var root = new MoveNode
        {
            CachedPosition = _initialPosition
        };

        var current = root;
        var position = _initialPosition;

        foreach (var info in game.Moves)
        {
            var san = info.San ?? PgnSerializer.ToSan(info.Move, position);
            position = position.MakeMove(info.Move);

            var child = new MoveNode
            {
                Move = info.Move,
                Parent = current,
                San = san,
                CachedPosition = position
            };

            current.Children.Add(child);

            current = child;
        }

        return root;
    }

    public void MoveForward()
    {
        var next = CurrentNode.MainLine;

        if (next == null)
            return;

        CurrentNode = next;
        CurrentPosition = next.CachedPosition!;
    }

    public void MoveBackward()
    {
        if (CurrentNode.Parent == null)
            return;

        CurrentNode = CurrentNode.Parent;
        CurrentPosition = CurrentNode.CachedPosition ?? _initialPosition;
    }

    public void MoveToVariation(int index)
    {
        if (index < 0 || index >= CurrentNode.Children.Count)
            return;

        var node = CurrentNode.Children[index];

        CurrentNode = node;
        CurrentPosition = node.CachedPosition!;
    }

    public void MoveToNode(MoveNode node)
    {
        CurrentNode = node;
        CurrentPosition = CurrentNode.CachedPosition!;
    }

    public void AddVariation(Move move)
    {
        var san = PgnSerializer.ToSan(move, CurrentPosition);
        var newPosition = CurrentPosition.MakeMove(move);

        var newNode = new MoveNode
        {
            Move = move,
            Parent = CurrentNode,
            San = san,
            CachedPosition = newPosition
        };

        CurrentNode.Children.Add(newNode);

        CurrentNode = newNode;
        CurrentPosition = newPosition;
    }

    public void AddVariation(MoveNode startNode, IPvInfo pv)
    {
        ArgumentNullException.ThrowIfNull(startNode);
        ArgumentNullException.ThrowIfNull(pv);

        var position = startNode.CachedPosition ?? RebuildPosition(startNode);

        var current = startNode;

        for (int i = 0; i < pv.Moves.Count; i++)
        {
            var pvMove = pv.Moves[i];
            var move = Uci.CreateMove(pvMove, position);
            ArgumentNullException.ThrowIfNull(move);

            var san = PgnSerializer.ToSan(move, position);
            position = position.MakeMove(move);

            var existing = current.Children
                .FirstOrDefault(c => c.Move != null && c.Move.Equals(move));

            if (existing != null)
            {
                current = existing;
                continue;
            }

            MoveNode newNode;

            if (i == 0)
            {
                newNode = new MoveNode
                {
                    Move = move,
                    Parent = current,
                    San = san,
                    CachedPosition = position,
                    Evaluation = pv.Score,
                    Depth = pv.Depth,
                };
            }
            else
            {
                newNode = new MoveNode
                {
                    Move = move,
                    Parent = current,
                    San = san,
                    CachedPosition = position,
                };
            }

            current.Children.Add(newNode);

            current = newNode;
        }
    }

    private BoardPosition RebuildPosition(MoveNode node)
    {
        var stack = new Stack<Move>();

        while (node.Move != null)
        {
            stack.Push(node.Move);
            node = node.Parent!;
        }

        var pos = _initialPosition;

        while (stack.Count > 0)
            pos = pos.MakeMove(stack.Pop());

        return pos;
    }

    public string GetCurrentEngineMoves()
    {
        if (CurrentNode is null) return string.Empty; 
        var node = CurrentNode;
        var stack = new Stack<Move>();

        while (node.Move != null)
        {
            stack.Push(node.Move);
            node = node.Parent!;
        }
        
        StringBuilder sb = new();
        while (stack.Count > 0)
        {
            var move = stack.Pop();
            var engineMove = Uci.GetUci(move);
            sb.Append(engineMove + " ");
        }
        sb.Length--;
        return sb.ToString();
    }

    //private static MoveNode ReplaceNode(MoveNode oldNode, MoveNode newNode)
    //{
    //    var parent = oldNode.Parent;

    //    if (parent == null)
    //        return newNode;

    //    var index = parent.Children.IndexOf(oldNode);

    //    var newChildren = parent.Children.SetItem(index, newNode);

    //    var newParent = parent with
    //    {
    //        Children = newChildren
    //    };

    //    // fix parent pointer of the new node
    //    newNode = newNode with { Parent = newParent };

    //    return ReplaceNode(parent, newParent);
    //}
}

public sealed class MoveNode
{
    public Move? Move { get; init; }
    public MoveNode? Parent { get; internal set; }
#pragma warning disable CA1002 // Do not expose generic lists
#pragma warning disable CA2227 // Collection properties should be read only
    public List<MoveNode> Children { get; set; } = [];
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CA1002 // Do not expose generic lists

    public string San { get; init; } = string.Empty;
    public string? Note { get; set; }

    public BoardPosition? CachedPosition { get; init; }

    public int? Evaluation { get; set; }
    public int? Depth { get; set; }

    public MoveNode? MainLine => Children.Count > 0 ? Children[0] : null;

    public IEnumerable<MoveNode> Variations => Children.Skip(1);

    public bool IsRoot => Move is null;
}

public interface IPvInfo
{
    int Depth { get; init; }
    int HashFull { get; init; }
    int Mate { get; init; }
    IReadOnlyList<string> Moves { get; init; }
    int MultiPv { get; init; }
    int Nodes { get; init; }
    int Nps { get; init; }
    int Score { get; init; }
    int SelDepth { get; init; }
    int TbHits { get; init; }
    int Time { get; init; }
}

//public sealed record Move(
//    Square From,
//    Square To,
//    PieceType? Promotion = null,
//    MoveType MoveType = MoveType.None
//);

public sealed record PvInfo : IPvInfo
{
    public int MultiPv { get; init; }
    public int Depth { get; init; }
    public int SelDepth { get; init; }
    public int Score { get; init; }
    public int Mate { get; init; }
    public int Nodes { get; init; }
    public int Nps { get; init; }
    public int HashFull { get; init; }
    public int TbHits { get; init; }
    public int Time { get; init; }
    public IReadOnlyList<string> Moves { get; init; } = [];
}
