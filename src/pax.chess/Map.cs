using System.Text;

namespace pax.chess;
public static class Map
{
    public static PieceType GetPieceType(string c)
    {
        return c.ToLower() switch
        {
            "p" => PieceType.Pawn,
            "n" => PieceType.Knight,
            "b" => PieceType.Bishop,
            "r" => PieceType.Rook,
            "q" => PieceType.Queen,
            "k" => PieceType.King,
            _ => throw new Exception($"invalid piece char {c}")
        };
    }
    public static string GetPieceString(PieceType pieceType)
    {
        return pieceType switch
        {
            PieceType.Pawn => "p",
            PieceType.Knight => "n",
            PieceType.Bishop => "b",
            PieceType.Rook => "r",
            PieceType.Queen => "q",
            PieceType.King => "k",
            _ => throw new Exception($"invalid piece type {pieceType}")
        };
    }

    public static int GetIntColumn(char x)
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
            _ => throw new Exception($"invalid column {x}"),
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
            _ => throw new Exception($"invalid column {x}"),
        };
    }

    public static EngineMove? GetEngineMove(string? moveString)
    {
        if (String.IsNullOrEmpty(moveString) || moveString.Length < 4)
        {
            return null;
        }
        PieceType? pieceType = null;
        if (moveString.Length > 4)
        {
            pieceType = GetPieceType(moveString.Last().ToString());
        }
        return new EngineMove(GetIntColumn(moveString[0]), int.Parse(moveString[1].ToString()) - 1, GetIntColumn(moveString[2]), int.Parse(moveString[3].ToString()) - 1, pieceType);
    }

    public static EngineMove GetValidEngineMove(string moveString)
    {
        PieceType? pieceType = null;
        if (moveString.Length > 4)
        {
            pieceType = GetPieceType(moveString.Last().ToString());
        }
        return new EngineMove(GetIntColumn(moveString[0]), int.Parse(moveString[1].ToString()) - 1, GetIntColumn(moveString[2]), int.Parse(moveString[3].ToString()) - 1, pieceType);
    }

    public static string GetEngineMoveString(EngineMove move)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(GetCharColumn(move.OldPosition.X));
        sb.Append(move.OldPosition.Y + 1);
        sb.Append(GetCharColumn(move.NewPosition.X));
        sb.Append(move.NewPosition.Y + 1);
        if (move.Transformation != null)
        {
            sb.Append(GetPieceString((PieceType)move.Transformation).ToUpper());
        }
        return sb.ToString();
    }

    public static string GetEngineMoveString(Move move)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(GetCharColumn(move.OldPosition.X));
        sb.Append(move.OldPosition.Y + 1);
        sb.Append(GetCharColumn(move.NewPosition.X));
        sb.Append(move.NewPosition.Y + 1);
        if (move.Transformation != null)
        {
            sb.Append(GetPieceString((PieceType)move.Transformation).ToUpper());
        }
        return sb.ToString();
    }
}
