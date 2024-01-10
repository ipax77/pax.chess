using System.Text;
using System.Text.RegularExpressions;

namespace pax.chess;

public static partial class PgnParser
{
    public static string MovesToPgn(List<BoardMove> moves)
    {
        StringBuilder pgnBuilder = new StringBuilder();

        for (int i = 0; i < moves.Count; i++)
        {
            // Add move number
            if (i % 2 == 0)
            {
                pgnBuilder.Append((i / 2) + 1).Append(". ");
            }

            // Add move
            pgnBuilder.Append(MoveToPgn(moves[i]));

            // Add space between moves
            pgnBuilder.Append(' ');
        }

        return pgnBuilder.ToString().Trim();
    }

    public static string MoveToPgn(BoardMove move)
    {
        StringBuilder moveBuilder = new StringBuilder();

        // Add piece type (except for pawns)
        if (move.PieceType != PieceType.Pawn)
        {
            moveBuilder.Append(move.PieceType.ToString()[0]);
        }

        // Add from position
        moveBuilder.Append(move.FromPosition.ToAlgebraicNotation());

        // Add capture indicator
        if (move.Capture != PieceType.None)
        {
            moveBuilder.Append('x');
        }

        // Add to position
        moveBuilder.Append(move.ToPosition.ToAlgebraicNotation());

        // Add promotion
        if (move.Transformation != PieceType.None)
        {
            moveBuilder.Append('=').Append(move.Transformation.ToString()[0]);
        }

        // Add check or checkmate indicator
        if (move.IsCheckMate)
        {
            moveBuilder.Append('#');
        }
        else if (move.IsCheck)
        {
            moveBuilder.Append('+');
        }

        return moveBuilder.ToString();
    }

    private static readonly char[] separator = new[] { ' ', '\n', '\r' };

    public static List<Position> GetMoves(string pgn)
    {
        List<Position> poss = [];
        
        if (string.IsNullOrEmpty(pgn))
        {
            return poss;
        }

        var pgnLines = PgnLinesRx().Split(pgn).Select(s => s.Trim()).Where(x => !String.IsNullOrEmpty(x)).ToArray();

        if (pgnLines is null || pgnLines.Length == 0)
        {
            return poss;
        }

        StringBuilder sb = new();

        foreach (var line in pgnLines)
        {
            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                // annotation
                continue;
            }
            sb.Append(line + ' ');
        }

        var linePgn = CommentRx().Replace(sb.ToString(), "");

        var ents = linePgn.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var ent in ents)
        {
            if (ent.EndsWith('.'))
            {
                // move number
                continue;
            }

            if (ent.Length == 2)
            {
                // pawn move
                poss.Add(new("ent"));
            }
        }

        return poss;
    }

    [GeneratedRegex(@"((\r)+)?(\n)+((\r)+)?")]
    private static partial Regex PgnLinesRx();
    [GeneratedRegex(@"^\d+\.$")]
    private static partial Regex MoveNrRx();
    [GeneratedRegex(@"[+#=!?]+$")]
    private static partial Regex MoveStringObstacles();
    [GeneratedRegex(@"\{.*?\}")]
    private static partial Regex CommentRx();
}

public record BoardMove
{
    public int HalfMove {  get; init; }
    public int PawnHalfMoveClock { get; init; }
    public PieceType PieceType { get; init; }
    public Position FromPosition {  get; init; } = Position.Zero;
    public Position ToPosition { get; init; } = Position.Zero;
    public bool EnPassantCapture { get; init; }
    public bool EnPassantPawnMove { get; init; }
    public bool IsCheck { get; init; }
    public bool IsCheckMate { get; init; }
    public PieceType Capture {  get; init; }
    public bool IsNotUnique { get; init; }
    public bool CanCasteQueenSide { get; init; }
    public bool CanCasteKingSide { get; init; }
    public PieceType Transformation { get; init; }
}

public record BoardEngineMove : BoardMove
{
    public Evaluation? Evaluation { get; set; }
}

public record PgnMove : BoardEngineMove
{
    public IList<string> Comments { get; set; } = new List<string>();
}