
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using pax.chess.Validation;

namespace pax.chess;

public static partial class PgnSerializer
{
    public static ChessGame Parse(string pgn, IPositionHasher? positionHasher = null)
    {
        BoardPosition pos = BoardPosition.CreateInitial();
        GameMetadata metadata = new();
        ChessGame game = new(pos, metadata);

        game.ActivatePositionHashing(positionHasher);

        var pgnLines = LineRegex().Split(pgn)
            .Select(s => s.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();

        bool moveSection = false;

        StringBuilder sb = new();
        foreach (var line in pgnLines)
        {
            if (line.StartsWith("1.", StringComparison.Ordinal))
            {
                moveSection = true;
            }
            if (moveSection)
            {
                if (line.Contains(';', StringComparison.Ordinal))
                {
                    var pgnline = line.Split(";")[0];
                    sb.Append(pgnline);
                }
                else
                {
                    sb.Append(line + " ");
                }
            }
        }
        var cleanPgn = sb.ToString();

        // () variations
        Match annotation = VariationRegex().Match(cleanPgn);
        Dictionary<string, int> annotations = [];
        do
        {
            while (annotation.Success)
            {
                annotations[annotation.Groups[1].Value] = annotation.Groups[1].Value.Count(c => c == ')');
                annotation = annotation.NextMatch();
            }
            foreach (var ent in annotations.OrderByDescending(o => o.Value))
            {
                cleanPgn = cleanPgn.Replace(ent.Key, "", StringComparison.Ordinal);
            }
            annotations.Clear();
            annotation = AnnotationRegex().Match(cleanPgn);
        } while (annotation.Success);

        // {} comments
        Match comment = CommentRegex().Match(cleanPgn);
        Dictionary<string, int> comments = [];
        do
        {
            while (comment.Success)
            {
                cleanPgn = cleanPgn.Replace(comment.Groups[1].Value, "", StringComparison.Ordinal);
                comments[comment.Groups[1].Value] = comment.Groups[1].Value.Count(c => c == '}');
                comment = comment.NextMatch();
            }
            foreach (var ent in comments.OrderByDescending(o => o.Value))
            {
                cleanPgn = cleanPgn.Replace(ent.Key, "", StringComparison.Ordinal);
            }
            comments.Clear();
            comment = CommentRegex().Match(cleanPgn);
        } while (comment.Success);


        List<MoveHelper> moveHelpers = [];
        MoveHelper moveHelper = new();
        var moves = MoveRegex().Split(cleanPgn);
        foreach (var move in moves)
        {
            if (string.IsNullOrEmpty(move))
            {
                continue;
            }
            var halfMoves = SpaceRegex().Split(move);
            foreach (var halfMove in halfMoves)
            {
                if (halfMove == "∓" || halfMove == "=" || halfMove == "±")
                {
                    continue;
                }
                moveHelper.AddMove(halfMove);
                if (moveHelper.IsReady)
                {
                    moveHelpers.Add(new MoveHelper(moveHelper));
                    moveHelper = new();
                }
            }
        }

        if (!moveHelper.IsClean)
        {
            moveHelpers.Add(new MoveHelper(moveHelper));
        }

        foreach (var ent in moveHelpers)
        {
            var whiteMove = GetMove(ent.WhiteMove, false, game.CurrentPosition);
            if (whiteMove != null)
            {
                var san = ToSan(whiteMove, game.CurrentPosition);
                game.ApplyMove(whiteMove, san);
            }

            var blackMove = GetMove(ent.BlackMove, true, game.CurrentPosition);
            if (blackMove != null)
            {
                var san = ToSan(blackMove, game.CurrentPosition);
                game.ApplyMove(blackMove, san);
            }
        }
        game.Evaluate();
        return game;
    }

    private static Move? GetMove(string pgnMove, bool isBlack, BoardPosition pos)
    {
        string move = pgnMove.Trim();
        var color = isBlack ? PieceColor.Black : PieceColor.White;

        if (string.IsNullOrEmpty(move) || move == "1-0" || move == "0-1" || move == "1/2-1/2" || move == "0.5/0.5" || move == "*")
        {
            return null;
        }

        if (move.Contains('x', StringComparison.Ordinal))
        {
            move = move.Replace("x", "", StringComparison.Ordinal);
        }
        if (move == "0-0" || move == "O-O" || move == "o-o")
        {
            var kingSquare = pos.Board.GetKingSquare(color);
            return new Move(kingSquare, new Square(kingSquare.File + 2, kingSquare.Rank), null, MoveType.CastlingKingSide);
        }
        if (move == "0-0-0" || move == "O-O-O" || move == "o-o-o")
        {
            var kingSquare = pos.Board.GetKingSquare(color);
            return new Move(kingSquare, new Square(kingSquare.File - 2, kingSquare.Rank), null, MoveType.CastlingQueenSide);
        }

        if (move.EndsWith("?!", StringComparison.Ordinal))
        {
            move = move[..^2];
        }
        else if (move.EndsWith("??", StringComparison.Ordinal))
        {
            move = move[..^2];
        }
        else if (move.EndsWith('!'))
        {
            move = move[..^1];
        }
        else if (move.EndsWith('?'))
        {
            move = move[..^1];
        }

        if (move.EndsWith('+'))
        {
            move = move[..^1];
        }

        PieceType pieceType = PieceType.Pawn;
        if (char.IsUpper(move[0]))
        {
            pieceType = FenSerializer.GetPieceType(move[0]);
            move = move[1..];
        }

        if (pgnMove.Last() == '#')
        {
            move = move[..^1];
        }

        PieceType? transformation = null;
        if (pgnMove.Contains('=', StringComparison.Ordinal))
        {
            transformation = FenSerializer.GetPieceType(move.Last());
            move = move[..^2];
        }

        var potentialPieces = pos.Board.GetPieces(color).Where(x => x.Piece.Type == pieceType);

        char? from = null;
        int destX = 0;
        if (move.Length > 2)
        {
            from = move[0];
            if (int.TryParse(from.ToString(), out int ifrom))
            {
                potentialPieces = [.. potentialPieces.Where(x => x.Square.Rank == ifrom - 1)];
            }
            else
            {
                potentialPieces = [.. potentialPieces.Where(x => x.Square.File == FenSerializer.GetColumnIndex((char)from))];
            }
            move = move[1..];
            destX = FenSerializer.GetColumnIndex(move[0]);
        }
        else if (pieceType == PieceType.Pawn)
        {
            destX = FenSerializer.GetColumnIndex(move[0]);
            potentialPieces = [.. potentialPieces.Where(x => x.Square.File >= destX - 1 && x.Square.File <= destX + 1)];
        }
        else
        {
            destX = FenSerializer.GetColumnIndex(move[0]);
        }

        Square destination = new(destX, int.Parse(move[1].ToString(), CultureInfo.InvariantCulture) - 1);
        Square? fromSquare = null;
        if (potentialPieces.Count() == 1)
        {
            fromSquare = potentialPieces.First().Square;
        }
        else
        {
            foreach (var pPiece in potentialPieces)
            {
                MoveState moveState = MoveState.Ok;
                var validSquares = MoveValidator.GetValidMoves(pPiece.Square, pos, out moveState).Select(s => s.To).ToList();
                if (validSquares.Contains(destination))
                {
                    fromSquare = pPiece.Square;
                    break;
                }
            }
        }
        if (fromSquare.HasValue)
        {
            MoveType moveType = MoveType.None;
            if (pos.Board[destination.Index].HasValue)
                moveType |= MoveType.Capture;
            
            if (transformation.HasValue)
                moveType |= MoveType.Promotion;

            if (pieceType == PieceType.Pawn && destination == pos.EnPassantTarget)
                moveType |= MoveType.EnPassant;

            return new Move(fromSquare.Value, destination, transformation, moveType);
        }
        else
        {
            throw new ArgumentOutOfRangeException($"pgn move invalid: {pgnMove}");
        }
    }


    public static string Serialize(ChessGame game)
    {
        ArgumentNullException.ThrowIfNull(game);

        StringBuilder sb = new();

        // 1. Tags
        sb.AppendLine(CultureInfo.InvariantCulture, $"[Event \"{game.Metadata.Event ?? "?"}\"]");
        sb.AppendLine(CultureInfo.InvariantCulture, $"[Site \"{game.Metadata.Site ?? "?"}\"]");
        sb.AppendLine(CultureInfo.InvariantCulture, $"[Date \"{game.Metadata.Date?.ToString("yyyy.MM.dd", CultureInfo.InvariantCulture) ?? "????.??.??"}\"]");
        sb.AppendLine(CultureInfo.InvariantCulture, $"[Round \"{game.Metadata.Round ?? "?"}\"]");
        sb.AppendLine(CultureInfo.InvariantCulture, $"[White \"{game.Metadata.White ?? "?"}\"]");
        sb.AppendLine(CultureInfo.InvariantCulture, $"[Black \"{game.Metadata.Black ?? "?"}\"]");
        sb.AppendLine(CultureInfo.InvariantCulture, $"[Result \"{ResultToString(game.Result)}\"]");

        foreach (var tag in game.Metadata.AdditionalTags)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"[{tag.Key} \"{tag.Value}\"]");
        }

        sb.AppendLine();

        // 2. Moves
        var currentPos = BoardPosition.CreateInitial(); // Assuming we start from initial position for PGN
        int moveCount = 0;

        for (int i = 0; i < game.Moves.Count; i++)
        {
            var moveInfo = game.Moves[i];
            var move = moveInfo.Move;
            if (i % 2 == 0)
            {
                moveCount++;
                sb.Append(CultureInfo.InvariantCulture, $"{moveCount}. ");
            }

            sb.Append(ToSan(move, currentPos));
            
            if (moveInfo.TimeRemaining.HasValue)
            {
                sb.Append(CultureInfo.InvariantCulture, $" {{ [%clk {moveInfo.TimeRemaining.Value:h\\:mm\\:ss}] }}");
            }

            sb.Append(' ');

            currentPos = currentPos.MakeMove(move);
        }

        sb.Append(ResultToString(game.Result));

        return sb.ToString();
    }

    private static string ResultToString(GameResult? result)
    {
        return result switch
        {
            GameResult.WhiteWin => "1-0",
            GameResult.BlackWin => "0-1",
            GameResult.Draw => "1/2-1/2",
            _ => "*"
        };
    }

    public static string ToSan(Move move, BoardPosition pos)
    {
        ArgumentNullException.ThrowIfNull(move);
        ArgumentNullException.ThrowIfNull(pos);
        var piece = pos.Board[move.From.Index] ?? throw new InvalidOperationException("No piece at from square");

        if (move.MoveType.HasFlag(MoveType.CastlingKingSide))
            return "O-O" + GetCheckSuffix(move, pos);
        if (move.MoveType.HasFlag(MoveType.CastlingQueenSide))
            return "O-O-O" + GetCheckSuffix(move, pos);

        StringBuilder sb = new();
        if (piece.Type != PieceType.Pawn)
        {
            sb.Append(GetPieceChar(piece.Type));

            // Disambiguation
            var ambassadors = pos.Board.GetPieces(piece.Color)
                .Where(p => p.Piece.Type == piece.Type && p.Square != move.From);

            bool needsFile = false;
            bool needsRank = false;
            bool ambiguous = false;

            foreach (var amb in ambassadors)
            {
                var moves = MoveValidator.GetValidMoves(amb.Square, pos, out _);
                if (moves.Any(m => m.To == move.To))
                {
                    ambiguous = true;
                    if (amb.Square.File == move.From.File) needsRank = true;
                    else needsFile = true;
                }
            }

            if (ambiguous)
            {
                if (needsFile) sb.Append((char)('a' + move.From.File));
                if (needsRank) sb.Append(move.From.Rank + 1);
            }
        }

        if (pos.Board[move.To.Index].HasValue || move.MoveType.HasFlag(MoveType.EnPassant))
        {
            if (piece.Type == PieceType.Pawn)
            {
                sb.Append((char)('a' + move.From.File));
            }
            sb.Append('x');
        }

        sb.Append((char)('a' + move.To.File));
        sb.Append(move.To.Rank + 1);

        if (move.Promotion.HasValue)
        {
            sb.Append('=');
            sb.Append(GetPieceChar(move.Promotion.Value));
        }

        sb.Append(GetCheckSuffix(move, pos));

        return sb.ToString();
    }

    private static string GetCheckSuffix(Move move, BoardPosition pos)
    {
        var nextPos = pos.MakeMove(move);
        var state = MoveValidator.GetGameState(nextPos);
        return state switch
        {
            GameState.Checkmate => "#",
            GameState.Check => "+",
            _ => ""
        };
    }

    private static char GetPieceChar(PieceType type)
    {
        return type switch
        {
            PieceType.Knight => 'N',
            PieceType.Bishop => 'B',
            PieceType.Rook => 'R',
            PieceType.Queen => 'Q',
            PieceType.King => 'K',
            _ => ' '
        };
    }


    [GeneratedRegex(@"((\r)+)?(\n)+((\r)+)?")]
    private static partial Regex LineRegex();
    [GeneratedRegex(@"\[(.*)""(.*)""\]")]
    private static partial Regex InfoRegex();
    [GeneratedRegex(@"\d+\.\.\.|\d+\.")]
    private static partial Regex MoveRegex();
    [GeneratedRegex(@"(\((?:\[??[^\(]*?\)))")]
    private static partial Regex VariationRegex();
    [GeneratedRegex(@"(\((?:\[??[^\(]*?\)))")]
    private static partial Regex AnnotationRegex();
    [GeneratedRegex(@"(\{(?:\{??[^\{]*?\}))")]
    private static partial Regex CommentRegex();
    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRegex();
}

internal sealed class MoveHelper
{
    public int MoveNumber { get; set; }
    public string WhiteMove { get; set; } = string.Empty;
    public string BlackMove { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public bool IsReady => !string.IsNullOrEmpty(WhiteMove) && !string.IsNullOrEmpty(BlackMove);
    public bool IsClean => string.IsNullOrEmpty(WhiteMove) && string.IsNullOrEmpty(BlackMove);

    public MoveHelper() { }
    public MoveHelper(MoveHelper helper)
    {
        MoveNumber = helper.MoveNumber;
        WhiteMove = helper.WhiteMove;
        BlackMove = helper.BlackMove;
        Comment = helper.Comment;
    }

    public void AddMove(string move)
    {
        if (string.IsNullOrEmpty(WhiteMove))
        {
            WhiteMove = move;
        }
        else
        {
            BlackMove = move;
        }
    }
}