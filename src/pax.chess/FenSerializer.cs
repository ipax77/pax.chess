
using System.Globalization;
using System.Text;

namespace pax.chess;

public static class FenSerializer
{
    public static BoardPosition Parse(string fen)
    {
        if (string.IsNullOrEmpty(fen))
        {
            fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        }

        var lines = fen.Split("/");
        var infos = lines[7].Split(" ");
        lines[7] = infos[0];
        var board = MapPieces(lines);

        PieceColor startingColor = infos[1] == "b" ? PieceColor.Black : PieceColor.White;
        CastlingRights castlingRights = CastlingRights.None;

        if (infos[2].Contains('K', StringComparison.Ordinal))
        {
            castlingRights |= CastlingRights.WhiteKingSide;
        }
        if (infos[2].Contains('Q', StringComparison.Ordinal))
        {
            castlingRights |= CastlingRights.WhiteQueenSide;
        }
        if (infos[2].Contains('k', StringComparison.Ordinal))
        {
            castlingRights |= CastlingRights.BlackKingSide;
        }
        if (infos[2].Contains('q', StringComparison.Ordinal))
        {
            castlingRights |= CastlingRights.BlackQueenSide;
        }

        Square? enPassantPosition = null;
        if (infos[3] != "-")
        {
            int x = GetColumnIndex(infos[3][0]);
            if (int.TryParse(infos[3][1].ToString(), out int y))
            {
                enPassantPosition = new Square(x, y - 1);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"invalid enpassant info: {infos[3]}");
            }
        }
        int pawnHalfMoveClock;
        if (int.TryParse(infos[4], out int pawnmoves))
        {
            pawnHalfMoveClock = pawnmoves;
        }
        else
        {
            throw new ArgumentOutOfRangeException($"invalid pawn half moves: {infos[4]}");
        }

        int moveNumber;
        if (int.TryParse(infos[5], out int totalmoves))
        {
            moveNumber = totalmoves;
        }
        else
        {
            throw new ArgumentOutOfRangeException($"invalid total moves: {infos[5]}");
        }

        return new BoardPosition(board, startingColor, castlingRights, enPassantPosition, pawnHalfMoveClock, moveNumber);
    }

    private static Board MapPieces(string[] fenLines)
    {
        Board board = new();

        for (int fenRank = 0; fenRank < 8; fenRank++)
        {
            int file = 0;
            string line = fenLines[fenRank];

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (char.IsDigit(c))
                {
                    file += c - '0';
                    continue;
                }

                int rank = 7 - fenRank;

                var square = new Square(file, rank);

                var color = char.IsLower(c)
                    ? PieceColor.Black
                    : PieceColor.White;

                var piece = new Piece(GetPieceType(c), color);

                board[square.Index] = piece;

                file++;
            }
        }

        return board;
    }

    public static string Serialize(BoardPosition pos)
    {
        ArgumentNullException.ThrowIfNull(pos);

        StringBuilder sb = new();
        for (int rank = 7; rank >= 0; rank--)
        {
            int emptyCount = 0;

            for (int file = 0; file < 8; file++)
            {
                var square = new Square(file, rank);
                Piece? piece = pos.Board[square.Index];

                if (piece.HasValue)
                {
                    if (emptyCount > 0)
                    {
                        sb.Append(emptyCount);
                        emptyCount = 0;
                    }

                    string pieceString = GetPieceString(piece.Value.Type);

                    sb.Append(piece.Value.Color == PieceColor.Black
                        ? pieceString
                        : pieceString.ToUpperInvariant());
                }
                else
                {
                    emptyCount++;
                }
            }

            if (emptyCount > 0)
                sb.Append(emptyCount);

            sb.Append('/');
        }
        sb.Length--;
        sb.Append(' ');

        if (pos.SideToMove == PieceColor.Black)
        {
            sb.Append('b');
        }
        else
        {
            sb.Append('w');
        }
        sb.Append(' ');

        if (pos.CastlingRights == CastlingRights.None)
        {
            sb.Append('-');
        }
        else
        {
            if (pos.CastlingRights.HasFlag(CastlingRights.WhiteKingSide)) sb.Append('K');
            if (pos.CastlingRights.HasFlag(CastlingRights.WhiteQueenSide)) sb.Append('Q');
            if (pos.CastlingRights.HasFlag(CastlingRights.BlackKingSide)) sb.Append('k');
            if (pos.CastlingRights.HasFlag(CastlingRights.BlackQueenSide)) sb.Append('q');
        }

        sb.Append(' ');
        if (pos.EnPassantTarget.HasValue)
        {
            char x = GetCharColumn(pos.EnPassantTarget.Value.File);
            var rankChar = (pos.EnPassantTarget.Value.Rank + 1).ToString(CultureInfo.InvariantCulture);
            sb.Append(x);
            sb.Append(rankChar);
        }
        else
        {
            sb.Append('-');
        }
        sb.Append(' ');
        sb.Append(pos.HalfmoveClock);
        sb.Append(' ');
        sb.Append(pos.FullmoveNumber);

        return sb.ToString();
    }

    public static PieceType GetPieceType(char c)
    {
        return c switch
        {
            'P' => PieceType.Pawn,
            'N' => PieceType.Knight,
            'B' => PieceType.Bishop,
            'R' => PieceType.Rook,
            'Q' => PieceType.Queen,
            'K' => PieceType.King,
            'p' => PieceType.Pawn,
            'n' => PieceType.Knight,
            'b' => PieceType.Bishop,
            'r' => PieceType.Rook,
            'q' => PieceType.Queen,
            'k' => PieceType.King,
            _ => throw new ArgumentOutOfRangeException($"invalid piece char {c}")
        };
    }

    private static string GetPieceString(PieceType pieceType)
    {
        return pieceType switch
        {
            PieceType.Pawn => "p",
            PieceType.Knight => "n",
            PieceType.Bishop => "b",
            PieceType.Rook => "r",
            PieceType.Queen => "q",
            PieceType.King => "k",
            _ => throw new ArgumentOutOfRangeException($"invalid piece type {pieceType}")
        };
    }

    private static char GetCharColumn(int x)
    {
        return x switch
        {
            0 => 'a',
            1 => 'b',
            2 => 'c',
            3 => 'd',
            4 => 'e',
            5 => 'f',
            6 => 'g',
            7 => 'h',
            _ => throw new ArgumentOutOfRangeException($"invalid column {x}"),
        };
    }

    public static int GetColumnIndex(char x)
    {
        return x switch
        {
            'a' => 0,
            'b' => 1,
            'c' => 2,
            'd' => 3,
            'e' => 4,
            'f' => 5,
            'g' => 6,
            'h' => 7,
            _ => throw new ArgumentOutOfRangeException($"invalid file char {x}"),
        };
    }
}