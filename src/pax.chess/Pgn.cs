using System.Text;
using System.Text.RegularExpressions;

namespace pax.chess;
public class Pgn
{
    //    [Event "F/S Return Match"]
    //    [Site "Belgrade, Serbia JUG"]
    //    [Date "1992.11.04"]
    //    [Round "29"]
    //    [White "Fischer, Robert J."]
    //    [Black "Spassky, Boris V."]
    //    [Result "1/2-1/2"]

    //1. e4 e5 2. Nf3 Nc6 3. Bb5 a6 { This opening is called the Ruy Lopez.}
    //4. Ba4 Nf6 5. O-O Be7 6. Re1 b5 7. Bb3 d6 8. c3 O-O 9. h3 Nb8 10. d4 Nbd7
    //11. c4 c6 12. cxb5 axb5 13. Nc3 Bb7 14. Bg5 b4 15. Nb1 h6 16. Bh4 c5 17. dxe5
    //Nxe4 18. Bxe7 Qxe7 19. exd6 Qf6 20. Nbd2 Nxd6 21. Nc4 Nxc4 22. Bxc4 Nb6
    //23. Ne5 Rae8 24. Bxf7+ Rxf7 25. Nxf7 Rxe1+ 26. Qxe1 Kxf7 27. Qe3 Qg5 28. Qxg5
    //hxg5 29. b3 Ke6 30. a3 Kd6 31. axb4 cxb4 32. Ra5 Nd5 33. f3 Bc8 34. Kf2 Bf5
    //35. Ra7 g6 36. Ra6+ Kc5 37. Ke1 Nf4 38. g3 Nxh3 39. Kd2 Kb5 40. Rd6 Kc5 41. Ra6
    //Nf2 42. g4 Bd3 43. Re6 1/2-1/2

    private static Regex infoRx = new Regex(@"\[(.*)""(.*)""\]");

    private static Regex comment1Rx = new Regex(@"(.*);(.*)");
    private static Regex comment2Rx = new Regex(@"(.*)}(.*)");
    private static Regex commentRx = new Regex(@"(\{[^\}]+\})");
    private static Regex moveRx = new Regex(@"(\d+)\.+\s([\w\d\+\-!\?]+)\s?([\w\d\+\-!\?]+)?\s?(\{[^\}]+\})?");

    public static Game MapString(string pgn)
    {
        // var lines = pgn.Split(new String[] {"\\n"}, StringSplitOptions.None);
        var pgnLines = Regex.Split(pgn, @"((\r)+)?(\n)+((\r)+)?").Select(s => s.Trim()).Where(x => !String.IsNullOrEmpty(x)).ToArray();
        return MapStrings(pgnLines);
    }

    public static Game MapStrings(string[] pgnLines)
    {
        Game game = new Game();
        Dictionary<string, string> Infos = new Dictionary<string, string>();
        bool moveSection = false;

        List<MoveHelper> moveHelpers = new List<MoveHelper>();
        MoveHelper moveHelper = new MoveHelper();
        for (int i = 0; i < pgnLines.Length; i++)
        {
            if (!moveSection && pgnLines[i].StartsWith("["))
            {
                var match = infoRx.Match(pgnLines[i]);
                if (match.Success)
                {
                    Infos[match.Groups[1].ToString().Trim()] = match.Groups[2].ToString();
                }
            }
            else
            {
                if (pgnLines[i].StartsWith("1."))
                {
                    moveSection = true;
                    if (Infos.ContainsKey("Variant") && Infos["Variant"] != "Standard")
                    {
                        game.Infos = Infos;
                        return game;
                    }
                }
                if (moveSection)
                {
                    // ; comments
                    if (pgnLines[i].Contains(";"))
                    {
                        pgnLines[i] = pgnLines[i].Split(";")[0];
                    }

                    // {} comments
                    var lines = Regex.Split(pgnLines[i], @"\{[^\{]+\}");

                    for (int l = 0; l < lines.Length; l++)
                    {
                        if (String.IsNullOrEmpty(lines[l]))
                        {
                            continue;
                        }
                        // () annotations
                        var line = Regex.Replace(lines[l], @"\([^\[]+\)", "");

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
                                moveHelper.AddMove(halfmoves[h]);
                                if (moveHelper.IsReady)
                                {
                                    moveHelpers.Add(new MoveHelper(moveHelper));
                                    moveHelper = new MoveHelper();
                                }
                            }
                        }
                    }
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

        if (move.EndsWith("+"))
        {
            move = move.Remove(move.Length - 1, 1);
        }

        if (move.Contains("x"))
        {
            move = move.Replace("x", "");
        }

        if (move == "0-0" || move == "O-O" || move == "o-o")
        {
            return new EngineMove(state.Pieces.First(f => f.Type == PieceType.King && f.IsBlack == isBlack).Position, new Position(6, isBlack ? 7 : 0));
        }
        if (move == "0-0-0" || move == "O-O-O" || move == "o-o-o")
        {
            return new EngineMove(state.Pieces.First(f => f.Type == PieceType.King && f.IsBlack == isBlack).Position, new Position(2, isBlack ? 7 : 0));
        }

        if (move.EndsWith("?!"))
        {
            move = move.Remove(move.Length - 2, 2);
        }
        else if (move.EndsWith("!"))
        {
            move = move.Remove(move.Length - 1, 1);
        }
        else if (move.EndsWith("?"))
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
        if (pgnMove.Contains("="))
        {
            transformation = Map.GetPieceType(move.Last().ToString());
            move = move.Remove(move.Length - 2, 2);
        }

        var potentialPieces = state.Pieces.Where(x => x.IsBlack == isBlack && x.Type == pieceType).ToList();

        char? from = null;
        int destX = 0;
        if (move.Length > 2)
        {
            int ifrom;
            from = move[0];
            if (int.TryParse(from.ToString(), out ifrom))
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

        Position destination = new Position(destX, int.Parse(move[1].ToString()) - 1);
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
            throw new Exception($"pgn move invalid: {pgnMove}");
            // return null;
        }
    }

    public static string MapPieces(State state)
    {

        StringBuilder sb = new StringBuilder();
        //sb.Append($"1. ");
        for (int i = 0; i < state.Moves.Count; i++)
        {
            if (i % 2 == 0)
            {
                sb.Append($"{(int)(i / 2) + 1}. ");
            }
            sb.Append(state.Moves[i].PgnMove);
            sb.Append(' ');
        }
        return sb.ToString();
    }

    public static string MapPiece(Move move, State state)
    {
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

        StringBuilder sb = new StringBuilder();
        if (move.Piece.Type != PieceType.Pawn)
        {
            sb.Append(Map.GetPieceString(move.Piece.Type).ToUpper());
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
            sb.Append(Map.GetPieceString((PieceType)move.Transformation).ToUpper());
        }
        return sb.ToString();
    }

    public record MoveHelper
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
