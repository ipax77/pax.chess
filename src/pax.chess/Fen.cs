using System.Text;
using System.Globalization;

namespace pax.chess;
public static class Fen
{
    internal static State MapString(string? fen = null)
    {
        // rnbqkb1r/pppp1ppp/8/4p3/4nP2/3P4/PPP1N1PP/RNBQKB1R b KQkq - 0 4
        if (String.IsNullOrEmpty(fen))
        {
            fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        }
        State state = new();
        var lines = fen.Split("/");
        var infos = lines[7].Split(" ");
        lines[7] = infos[0];
        state.Pieces = MapPieces(lines);

        state.Info.BlackToMove = infos[1] == "b";

        if (!infos[2].Contains('K', StringComparison.Ordinal))
        {
            state.Info.WhiteCanCastleKingSide = false;
        }
        if (!infos[2].Contains('Q', StringComparison.Ordinal))
        {
            state.Info.WhiteCanCastleQueenSide = false;
        }
        if (!infos[2].Contains('k', StringComparison.Ordinal))
        {
            state.Info.BlackCanCastleKingSide = false;
        }
        if (!infos[2].Contains('q', StringComparison.Ordinal))
        {
            state.Info.BlackCanCastleQueenSide = false;
        }

        if (infos[3] != "-")
        {
            int x = Map.GetIntColumn(infos[2][0]);
            if (int.TryParse(infos[2][1].ToString(), out int y))
            {
                state.Info.EnPassantPosition = new Position(x, y - 1);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"invalid enpassant info: {infos[2]}");
            }
        }

        if (int.TryParse(infos[4], out int pawnmoves))
        {
            state.Info.PawnHalfMoveClock = pawnmoves;
        }
        else
        {
            throw new ArgumentOutOfRangeException($"invalid pawn half moves: {infos[4]}");
        }

        state.Info.IsCheck = state.IsCheck();
        if (state.Info.IsCheck)
        {
            state.Info.IsCheckMate = Validation.Validate.IsCheckMate(state);
        }

        return state;
    }

    private static List<Piece> MapPieces(string[] fenLines)
    {
        List<Piece> pieces = new();
        for (int y = 0; y < 8; y++)
        {
            int x = 0;
            for (int i = 0; i < fenLines[y].Length; i++)
            {
                string? interest = null;
                char c = fenLines[y][i];
                if (int.TryParse(new string(c, 1), out int ci))
                {
                    x += ci - 1;
                }
                else
                {
                    interest = c.ToString();
                }
                if (!String.IsNullOrEmpty(interest))
                {
                    pieces.Add(new Piece(Map.GetPieceType(interest), Char.IsLower(interest[0]), x, 7 - y));
                }
                x += 1;
            }
        }
        return pieces;
    }

    /// <summary>
    /// Converts given state to FEN string
    /// </summary>
    /// <returns>FEN string</returns>
    public static string MapList(State state)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }
        StringBuilder sb = new();
        for (int y = 0; y < 8; y++)
        {
            int c = 0;
            for (int x = 0; x < 8; x++)
            {
                Piece? piece = state.Pieces.FirstOrDefault(f => f.Position.X == x && f.Position.Y == 7 - y);
                if (piece != null)
                {
                    if (c > 0)
                    {
                        sb.Append(c);
                    }
                    string pieceString = Map.GetPieceString(piece.Type);
                    sb.Append(!piece.IsBlack ? pieceString.ToUpper(CultureInfo.InvariantCulture) : pieceString);
                    c = 0;
                }
                else
                {
                    c++;
                }
            }
            if (c > 0)
            {
                sb.Append(c);
            }
            sb.Append('/');
        }
        sb.Length--;
        sb.Append(' ');

        if (state.Info.BlackToMove)
        {
            sb.Append('b');
        }
        else
        {
            sb.Append('w');
        }
        sb.Append(' ');

        if
        (
            !state.Info.WhiteCanCastleKingSide
         && !state.Info.WhiteCanCastleQueenSide
         && !state.Info.BlackCanCastleKingSide
         && !state.Info.BlackCanCastleQueenSide

        )
        {
            sb.Append('-');
        }
        else
        {
            if (state.Info.WhiteCanCastleKingSide)
            {
                sb.Append('K');
            }
            if (state.Info.WhiteCanCastleQueenSide)
            {
                sb.Append('Q');
            }
            if (state.Info.BlackCanCastleKingSide)
            {
                sb.Append('k');
            }
            if (state.Info.BlackCanCastleQueenSide)
            {
                sb.Append('q');
            }
        }

        sb.Append(' ');
        if (state.Info.EnPassantPosition != null)
        {
            char x = Map.GetCharColumn(state.Info.EnPassantPosition.X);
            var y = state.Info.EnPassantPosition.Y.ToString(CultureInfo.InvariantCulture);
            sb.Append(x);
            sb.Append(y);
        }
        else
        {
            sb.Append('-');
        }
        sb.Append(' ');
        sb.Append(state.Info.PawnHalfMoveClock);
        sb.Append(' ');
        sb.Append(state.Moves.Count / 2);

        return sb.ToString();
    }


}
