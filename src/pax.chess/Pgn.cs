using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace pax.chess;
public static class Pgn
{
    private static readonly Regex infoRx = new(@"\[(.*)""(.*)""\]");

    public static Game MapString(string pgn)
    {
        var pgnLines = Regex.Split(pgn, @"((\r)+)?(\n)+((\r)+)?").Select(s => s.Trim()).Where(x => !String.IsNullOrEmpty(x)).ToArray();
        return MapStrings(pgnLines);
    }

    public static Game MapStrings(string[] pgnLines)
    {
        if (pgnLines == null)
        {
            throw new ArgumentNullException(nameof(pgnLines));
        }
        Game game = new();
        bool moveSection = false;
        StringBuilder sb = new();

        for (int i = 0; i < pgnLines.Length; i++)
        {
            if (!moveSection && pgnLines[i].StartsWith("[", StringComparison.Ordinal))
            {
                var info = infoRx.Match(pgnLines[i]);
                if (info.Success)
                {
                    game.Infos[info.Groups[1].ToString().Trim()] = info.Groups[2].ToString();
                }
            }
            else
            {
                if (pgnLines[i].StartsWith("1.", StringComparison.Ordinal))
                {
                    moveSection = true;
                    if (game.Infos.ContainsKey("Variant") && game.Infos["Variant"] != "Standard")
                    {
                        return game;
                    }
                }
                if (moveSection)
                {
                    string pgnline = pgnLines[i];
                    // ; comments
                    if (pgnline.Contains(';', StringComparison.Ordinal))
                    {
                        pgnline = pgnline.Split(";")[0];
                    }
                    sb.Append(pgnline + " ");
                }
            }
        }

        string line = sb.ToString();

        // () variations
        Match annotation = Regex.Match(line, @"(\((?:\[??[^\(]*?\)))");
        Dictionary<string, int> annotations = new();
        do
        {
            while (annotation.Success)
            {
                annotations[annotation.Groups[1].Value] = annotation.Groups[1].Value.Count(c => c == ')');
                annotation = annotation.NextMatch();
            }
            foreach (var ent in annotations.OrderByDescending(o => o.Value))
            {
                line = line.Replace(ent.Key, "", StringComparison.Ordinal);
            }
            annotations.Clear();
            annotation = Regex.Match(line, @"(\((?:\[??[^\(]*?\)))");
        } while (annotation.Success);


        // {} comments
        Match comment = Regex.Match(line, @"(\{(?:\{??[^\{]*?\}))");
        Dictionary<string, int> comments = new();
        do
        {
            while (comment.Success)
            {
                line = line.Replace(comment.Groups[1].Value, "", StringComparison.Ordinal);
                comments[comment.Groups[1].Value] = comment.Groups[1].Value.Count(c => c == '}');
                comment = comment.NextMatch();
            }
            foreach (var ent in comments.OrderByDescending(o => o.Value))
            {
                line = line.Replace(ent.Key, "", StringComparison.Ordinal);
            }
            comments.Clear();
            comment = Regex.Match(line, @"(\{(?:\{??[^\{]*?\}))");
        } while (comment.Success);

        List<MoveHelper> moveHelpers = new();
        MoveHelper moveHelper = new();
        var moves = Regex.Split(line, @"\d+\.\.\.|\d+\.");
        for (int m = 0; m < moves.Length; m++)
        {
            var move = moves[m].Trim();
            if (String.IsNullOrEmpty(move))
            {
                continue;
            }
            var halfmoves = Regex.Split(move, @"\s+");
            for (int h = 0; h < halfmoves.Length; h++)
            {
                if (halfmoves[h] == "∓" || halfmoves[h] == "=" || halfmoves[h] == "±")
                {
                    continue;
                }
                moveHelper.AddMove(halfmoves[h]);
                if (moveHelper.IsReady)
                {
                    moveHelpers.Add(new MoveHelper(moveHelper));
                    moveHelper = new MoveHelper();
                }
            }
        }
        moveHelpers.Add(moveHelper);

        for (int i = 0; i < moveHelpers.Count; i++)
        {
            var whiteMove = GetMove(moveHelpers[i].WhiteMove, false, game.State);
            if (whiteMove != null)
            {
                game.Move(whiteMove);
            }

            var blackMove = GetMove(moveHelpers[i].BlackMove, true, game.State);
            if (blackMove != null)
            {
                game.Move(blackMove);
            }
        }

        return game;
    }

    private static EngineMove? GetMove(string pgnMove, bool isBlack, State state)
    {
        string move = pgnMove.Trim();

        if (String.IsNullOrEmpty(move) || move == "1-0" || move == "0-1" || move == "1/2-1/2" || move == "0.5/0.5")
        {
            return null;
        }



        if (move.Contains('x', StringComparison.Ordinal))
        {
            move = move.Replace("x", "", StringComparison.Ordinal);
        }

        if (move == "0-0" || move == "O-O" || move == "o-o")
        {
            return new EngineMove(state.Pieces.First(f => f.Type == PieceType.King && f.IsBlack == isBlack).Position, new Position(6, isBlack ? 7 : 0));
        }
        if (move == "0-0-0" || move == "O-O-O" || move == "o-o-o")
        {
            return new EngineMove(state.Pieces.First(f => f.Type == PieceType.King && f.IsBlack == isBlack).Position, new Position(2, isBlack ? 7 : 0));
        }

        if (move.EndsWith("?!", StringComparison.Ordinal))
        {
            move = move.Remove(move.Length - 2, 2);
        }
        else if (move.EndsWith("??", StringComparison.Ordinal))
        {
            move = move.Remove(move.Length - 2, 2);
        }
        else if (move.EndsWith("!", StringComparison.Ordinal))
        {
            move = move.Remove(move.Length - 1, 1);
        }
        else if (move.EndsWith("?", StringComparison.Ordinal))
        {
            move = move.Remove(move.Length - 1, 1);
        }

        if (move.EndsWith("+", StringComparison.Ordinal))
        {
            move = move.Remove(move.Length - 1, 1);
        }

        PieceType pieceType = PieceType.Pawn;
        if (Char.IsUpper(move[0]))
        {
            pieceType = Map.GetPieceType(move[0].ToString());
            move = move[1..];
        }

        if (pgnMove.Last() == '#')
        {
            move = move.Remove(move.Length - 1, 1);
        }

        PieceType? transformation = null;
        if (pgnMove.Contains('=', StringComparison.Ordinal))
        {
            transformation = Map.GetPieceType(move.Last().ToString());
            move = move.Remove(move.Length - 2, 2);
        }

        var potentialPieces = state.Pieces.Where(x => x.IsBlack == isBlack && x.Type == pieceType).ToList();

        char? from = null;
        int destX = 0;
        if (move.Length > 2)
        {
            from = move[0];
            if (int.TryParse(from.ToString(), out int ifrom))
            {
                potentialPieces = potentialPieces.Where(x => x.Position.Y == ifrom - 1).ToList();
            }
            else
            {
                potentialPieces = potentialPieces.Where(x => x.Position.X == Map.GetIntColumn((char)from)).ToList();
            }
            move = move[1..];
            destX = Map.GetIntColumn(move[0]);
        }
        else if (pieceType == PieceType.Pawn)
        {
            destX = Map.GetIntColumn(move[0]);
            potentialPieces = potentialPieces.Where(x => x.Position.X >= destX - 1 && x.Position.X <= destX + 1).ToList();
        }
        else
        {
            destX = Map.GetIntColumn(move[0]);
        }

        Position destination = new(destX, int.Parse(move[1].ToString(), CultureInfo.InvariantCulture) - 1);
        Piece? piece = null;
        if (potentialPieces.Count == 1)
        {
            piece = potentialPieces[0];
        }
        else
        {
            for (int i = 0; i < potentialPieces.Count; i++)
            {
                var positions = Validation.Validate.GetMoves(potentialPieces[i], state);
                if (positions.Contains(destination))
                {
                    if (Validation.Validate.WouldBeCheck(potentialPieces[i], destination, transformation, state))
                    {
                        continue;
                    }
                    else
                    {
                        piece = potentialPieces[i];
                        break;
                    }
                }
            }
        }
        if (piece != null)
        {
            return new EngineMove(piece.Position, destination, transformation);
        }
        else
        {
            throw new ArgumentOutOfRangeException($"pgn move invalid: {pgnMove}");
        }
    }

    public static string MapPieces(State state)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }
        StringBuilder sb = new();
        for (int i = 0; i < state.Moves.Count; i++)
        {
            if (i % 2 == 0)
            {
                sb.Append(CultureInfo.InvariantCulture, $"{(int)(i / 2) + 1}. ");
            }
            sb.Append(state.Moves[i].PgnMove);
            sb.Append(' ');
        }
        return sb.ToString();
    }

    public static string MapPiece(Move move, State state)
    {
        if (move == null)
        {
            throw new ArgumentNullException(nameof(move));
        }
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }
        if (move.IsCastle)
        {
            if (move.OldPosition.X < move.NewPosition.X)
            {
                return "O-O";
            }
            else
            {
                return "O-O-O";
            }
        }

        StringBuilder sb = new();
        if (move.Piece.Type != PieceType.Pawn)
        {
            sb.Append(Map.GetPieceString(move.Piece.Type).ToUpperInvariant());
        }
        else if (move.Capture != null)
        {
            sb.Append(Map.GetCharColumn(move.OldPosition.X));
            sb.Append('x');
        }
        if (move.Piece.Type == PieceType.Knight || move.Piece.Type == PieceType.Rook)
        {
            var alternatePiece = state.Pieces.FirstOrDefault(x => x.Type == move.Piece.Type && x.IsBlack == move.Piece.IsBlack && x != move.Piece);
            if (alternatePiece != null)
            {
                var positions = Validation.Validate.GetMoves(alternatePiece, state);
                if (positions.Contains(move.NewPosition))
                {
                    if (alternatePiece.Position.X == move.OldPosition.X)
                    {
                        sb.Append(move.OldPosition.Y + 1);
                    }
                    else
                    {
                        sb.Append(Map.GetCharColumn(move.OldPosition.X));
                    }
                }
            }
        }
        if (move.Capture != null && move.Piece.Type != PieceType.Pawn)
        {
            sb.Append('x');
        }
        sb.Append(Map.GetCharColumn(move.NewPosition.X));
        sb.Append(move.NewPosition.Y + 1);

        if (move.Transformation != null && move.Piece.Type == PieceType.Pawn)
        {
            sb.Append(Map.GetPieceString((PieceType)move.Transformation).ToUpperInvariant());
        }
        return sb.ToString();
    }

    public static string GetPgnMove(EngineMove move, State state)
    {
        if (move == null)
        {
            throw new ArgumentNullException(nameof(move));
        }
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }
        Piece piece = state.Pieces.Single(s => s.Position == move.OldPosition);

        if (piece.Type == PieceType.King && Math.Abs(move.OldPosition.X - move.NewPosition.X) > 1)
        {
            if (move.OldPosition.X < move.NewPosition.X)
            {
                return "O-O";
            }
            else
            {
                return "O-O-O";
            }
        }

        Piece? capture = state.Pieces.FirstOrDefault(f => f.Position == move.NewPosition);

        StringBuilder sb = new();
        if (piece.Type != PieceType.Pawn)
        {
            sb.Append(Map.GetPieceString(piece.Type).ToUpperInvariant());
        }
        else if (capture != null)
        {
            sb.Append(Map.GetCharColumn(move.OldPosition.X));
            sb.Append('x');
        }
        if (piece.Type == PieceType.Knight || piece.Type == PieceType.Rook)
        {
            var alternatePiece = state.Pieces.FirstOrDefault(x => x.Type == piece.Type && x.IsBlack == piece.IsBlack && x != piece);
            if (alternatePiece != null)
            {
                var positions = Validation.Validate.GetMoves(alternatePiece, state);
                if (positions.Contains(move.NewPosition))
                {
                    if (alternatePiece.Position.X == move.OldPosition.X)
                    {
                        sb.Append(move.OldPosition.Y + 1);
                    }
                    else
                    {
                        sb.Append(Map.GetCharColumn(move.OldPosition.X));
                    }
                }
            }
        }
        if (capture != null && piece.Type != PieceType.Pawn)
        {
            sb.Append('x');
        }
        sb.Append(Map.GetCharColumn(move.NewPosition.X));
        sb.Append(move.NewPosition.Y + 1);

        if (move.Transformation != null && piece.Type == PieceType.Pawn)
        {
            sb.Append(Map.GetPieceString((PieceType)move.Transformation).ToUpperInvariant());
        }
        return sb.ToString();
    }

    internal sealed record MoveHelper
    {
        public int MoveNumber { get; set; }
        public string WhiteMove { get; set; } = String.Empty;
        public string BlackMove { get; set; } = String.Empty;
        public string? Comment { get; set; }
        public bool IsReady => !String.IsNullOrEmpty(WhiteMove) && !String.IsNullOrEmpty(BlackMove);
        public bool IsClean => String.IsNullOrEmpty(WhiteMove) && String.IsNullOrEmpty(BlackMove);

        public void AddMove(string move)
        {
            if (String.IsNullOrEmpty(WhiteMove))
            {
                WhiteMove = move;
            }
            else
            {
                BlackMove = move;
            }
        }

        public MoveHelper() { }
        public MoveHelper(MoveHelper helper)
        {
            MoveNumber = helper.MoveNumber;
            WhiteMove = helper.WhiteMove;
            BlackMove = helper.BlackMove;
            Comment = helper.Comment;
        }
    }
}
