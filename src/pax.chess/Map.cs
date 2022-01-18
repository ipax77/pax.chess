using System.Text;
using System.Globalization;

namespace pax.chess;
public static class Map
{
    internal static PieceType GetPieceType(string c)
    {
        return c.ToUpperInvariant() switch
        {
            "P" => PieceType.Pawn,
            "N" => PieceType.Knight,
            "B" => PieceType.Bishop,
            "R" => PieceType.Rook,
            "Q" => PieceType.Queen,
            "K" => PieceType.King,
            _ => throw new ArgumentOutOfRangeException($"invalid piece char {c}")
        };
    }
    internal static string GetPieceString(PieceType pieceType)
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

    internal static int GetIntColumn(char x)
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
            _ => throw new ArgumentOutOfRangeException($"invalid column {x}"),
        };
    }

    public static Char GetCharColumn(int x)
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

    public static EngineMove? GetEngineMove(string? moveString)
    {
        if (String.IsNullOrEmpty(moveString) || moveString.Length < 4 || moveString == "0000")
        {
            return null;
        }
        PieceType? pieceType = null;
        if (moveString.Length > 4)
        {
            pieceType = GetPieceType(moveString.Last().ToString());
        }
        return new EngineMove(GetIntColumn(moveString[0]), int.Parse(moveString[1].ToString(), CultureInfo.InvariantCulture) - 1, GetIntColumn(moveString[2]), int.Parse(moveString[3].ToString(), CultureInfo.InvariantCulture) - 1, pieceType);
    }

    public static EngineMove GetValidEngineMove(string moveString)
    {
        if (moveString == null)
        {
            throw new ArgumentNullException(nameof(moveString));
        }
        PieceType? pieceType = null;
        if (moveString.Length > 4)
        {
            pieceType = GetPieceType(moveString.Last().ToString());
        }
        return new EngineMove(GetIntColumn(moveString[0]), int.Parse(moveString[1].ToString(), CultureInfo.InvariantCulture) - 1, GetIntColumn(moveString[2]), int.Parse(moveString[3].ToString(), CultureInfo.InvariantCulture) - 1, pieceType);
    }

    public static string GetEngineMoveString(EngineMove move)
    {
        if (move == null)
        {
            throw new ArgumentNullException(nameof(move));
        }
        StringBuilder sb = new();
        sb.Append(GetCharColumn(move.OldPosition.X));
        sb.Append(move.OldPosition.Y + 1);
        sb.Append(GetCharColumn(move.NewPosition.X));
        sb.Append(move.NewPosition.Y + 1);
        if (move.Transformation != null)
        {
            sb.Append(GetPieceString((PieceType)move.Transformation).ToUpperInvariant());
        }
        return sb.ToString();
    }

    public static string GetEngineMoveString(Move move)
    {
        if (move == null)
        {
            throw new ArgumentNullException(nameof(move));
        }
        StringBuilder sb = new();
        sb.Append(GetCharColumn(move.OldPosition.X));
        sb.Append(move.OldPosition.Y + 1);
        sb.Append(GetCharColumn(move.NewPosition.X));
        sb.Append(move.NewPosition.Y + 1);
        if (move.Transformation != null)
        {
            sb.Append(GetPieceString((PieceType)move.Transformation).ToUpperInvariant());
        }
        return sb.ToString();
    }
}
