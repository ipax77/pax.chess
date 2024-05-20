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

        if (move.PieceType == PieceType.King && Math.Abs(move.FromPosition.X - move.ToPosition.X) > 1)
        {
            if (move.FromPosition.X - move.ToPosition.X < 0)
            {
                return "O-O";
            }
            else
            {
                return "O-O-O";
            }
        }

        // Add piece type (except for pawns)
        if (move.PieceType != PieceType.Pawn)
        {
            moveBuilder.Append(GetPieceTypeString(move.PieceType));
        }

        moveBuilder.Append(move.PgnFromNotation);

        //if (!string.IsNullOrEmptymove.IsNotUnique && move.PieceType != PieceType.Pawn)
        //{
        //    var algebraicFrom = move.FromPosition.ToAlgebraicNotation();
        //    if (move.PieceType == PieceType.Knight)
        //    {
        //        moveBuilder.Append(algebraicFrom[1]);
        //    }
        //    else if (move.FromPosition.X == move.ToPosition.X)
        //    {
        //        moveBuilder.Append(algebraicFrom[1]);
        //    }
        //    else
        //    {
        //        moveBuilder.Append(algebraicFrom[0]);
        //    }
        //}

        // Add capture indicator
        if (move.Capture != PieceType.None)
        {
            if (move.PieceType == PieceType.Pawn)
            {
                var algebraicFrom = move.FromPosition.ToAlgebraicNotation();
                moveBuilder.Append(algebraicFrom[0]);
            }
            moveBuilder.Append('x');
        }

        // Add to position
        moveBuilder.Append(move.ToPosition.ToAlgebraicNotation());

        // Add promotion
        if (move.Transformation != PieceType.None)
        {
            moveBuilder.Append('=').Append(GetPieceTypeString(move.Transformation));
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

    public static List<PgnMove> GetPgnMoves(string pgn)
    {
        List<PgnMove> moves = [];

        if (string.IsNullOrEmpty(pgn))
        {
            return moves;
        }

        var pgnLines = PgnLinesRx().Split(pgn).Select(s => s.Trim()).Where(x => !String.IsNullOrEmpty(x)).ToArray();

        if (pgnLines is null || pgnLines.Length == 0)
        {
            return moves;
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

        if (ents.Length == 0)
        {
            return moves;
        }

        Result result = Result.None;
        var lastEnt = ents.Last();

        if (lastEnt.Equals("1-0", StringComparison.Ordinal))
        {
            result = Result.WhiteWin;
            Array.Resize(ref ents,  ents.Length - 1);
        }
        else if (lastEnt.Equals("0-0", StringComparison.Ordinal))
        {
            result = Result.Draw;
            Array.Resize(ref ents, ents.Length - 1);
        }
        else if (lastEnt.Equals("0-1", StringComparison.Ordinal))
        {
            result = Result.BlackWin;
            Array.Resize(ref ents, ents.Length - 1);
        }

        moves.Add(new() { Result = result });

        int moveNumber = 0;
        foreach (var ent in ents)
        {
            var moveInfo = ent;

            if (ent.EndsWith('.') || ent.Length < 2)
            {
                // move number
                moveNumber++;
                continue;
            }

            if (ent.Length == 2)
            {
                // pawn move
                moves.Add(new() { MoveNumber = moveNumber, PieceType = PieceType.Pawn, ToPosition = new(ent), Origin = ent });
                continue;
            }

            bool isCheck = false;
            bool isCheckMate = false;

            if (moveInfo.EndsWith('+'))
            {
                isCheck = true;
                moveInfo = moveInfo[..(ent.Length - 1)];
            }

            if (moveInfo.EndsWith('#'))
            {
                isCheckMate = true;
                moveInfo = moveInfo[..(ent.Length - 1)];
            }

            PieceType transformation = PieceType.None;
            if (moveInfo[^2] == '=')
            {
                transformation = GetPieceType(moveInfo[^1]);
                moveInfo = moveInfo[..(ent.Length - 2)];
            }

            if (moveInfo.Length == 2)
            {
                moves.Add(new() { MoveNumber = moveNumber, PieceType = PieceType.Pawn, ToPosition = new(moveInfo), Transformation = transformation, IsCheck = isCheck, IsCheckMate = isCheckMate, Origin = ent });
            }
            else if (moveInfo.StartsWith('O'))
            {
                var mcount = moveInfo.Split('-', StringSplitOptions.RemoveEmptyEntries).Length - 1;
                if (mcount == 1)
                {
                    moves.Add(new() { MoveNumber = moveNumber, IsCastleKingSide = true, PieceType = PieceType.King, IsCheck = isCheck, IsCheckMate = isCheckMate, Origin = ent });
                }
                else if (mcount == 2)
                {
                    moves.Add(new() { MoveNumber = moveNumber, IsCastleQueenSide = true, PieceType = PieceType.King, IsCheck = isCheck, IsCheckMate = isCheckMate, Origin = ent });
                }
            }
            else if (moveInfo.Length == 3)
            {
                moves.Add(new()
                {
                    MoveNumber = moveNumber,
                    PieceType = GetPieceType(moveInfo[0]),
                    ToPosition = new(moveInfo[1..]),
                    IsCheck = isCheck,
                    IsCheckMate = isCheckMate,
                    Origin = ent,
                    Transformation = transformation
                });
            }
            else if (moveInfo.Length == 4 && moveInfo[1].Equals('x'))
            {
                int fromX = 0;
                PieceType pieceType;
                if (Char.IsLower(moveInfo[0]))
                { 
                    (fromX, _) = GetFromXY(moveInfo[0]);
                    pieceType = PieceType.Pawn;
                }
                else
                {
                    pieceType = GetPieceType(moveInfo[0]);
                }

                moves.Add(new()
                {
                    MoveNumber = moveNumber,
                    PieceType = pieceType,
                    FromX = fromX,
                    ToPosition = new(moveInfo[2..]),
                    IsCapture = true,
                    IsCheck = isCheck,
                    IsCheckMate = isCheckMate,
                    Origin = ent,
                    Transformation = transformation
                });
            }
            else if (moveInfo.Length == 4)
            {
                (var fromX, var fromY) = GetFromXY(moveInfo[1]);
                moves.Add(new()
                {
                    MoveNumber = moveNumber,
                    PieceType = GetPieceType(moveInfo[0]),
                    FromX = fromX,
                    FromY = fromY,
                    ToPosition = new(moveInfo[2..]),
                    IsCheck = isCheck,
                    IsCheckMate = isCheckMate,
                    Origin = ent,
                    Transformation = transformation
                });
            }
            else if (moveInfo.Length == 5 && moveInfo[2].Equals('x'))
            {
                (var fromX, var fromY) = GetFromXY(moveInfo[1]);
                moves.Add(new()
                {
                    MoveNumber = moveNumber,
                    PieceType = GetPieceType(moveInfo[0]),
                    FromX = fromX,
                    FromY = fromY,
                    ToPosition = new(moveInfo[3..]),
                    IsCheck = isCheck,
                    IsCheckMate = isCheckMate,
                    Origin = ent,
                    Transformation = transformation
                });
            }
            else if (moveInfo.Length == 5)
            {
                Position from = new(moveInfo[1..3]);
                moves.Add(new()
                {
                    MoveNumber = moveNumber,
                    PieceType = GetPieceType(moveInfo[0]),
                    FromX = from.X + 1,
                    FromY = from.Y + 1,
                    ToPosition = new(moveInfo[3..]),
                    IsCheck = isCheck,
                    IsCheckMate = isCheckMate,
                    Origin = ent,
                    Transformation = transformation
                });
            }
            else
            {
                Console.Write($"unkonwn: {moveInfo}");
            }
        }

        return moves;
    }

    private static PieceType GetPieceType(char c)
    {
        return c switch
        {
            'N' => PieceType.Knight,
            'B' => PieceType.Bishop,
            'R' => PieceType.Rook,
            'Q' => PieceType.Queen,
            'K' => PieceType.King,
            _ => PieceType.Pawn
        };
    }

    private static string GetPieceTypeString(PieceType pieceType)
    {
        return pieceType switch
        {
            PieceType.Knight => "N",
            PieceType.Bishop => "B",
            PieceType.Rook => "R",
            PieceType.Queen => "Q",
            PieceType.King => "K",
            _ => string.Empty
        };
    }

    private static (int x, int y) GetFromXY(char c)
    {
        if (int.TryParse(c.ToString(), out int y))
        {
            return (0, y);
        }

        var x = c switch
        {
            'a' => 1,
            'b' => 2,
            'c' => 3,
            'd' => 4,
            'e' => 5,
            'f' => 6,
            'g' => 7,
            'h' => 8,
            _ => 0
        };

        return (x, 0);
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
    public int HalfMove { get; init; }
    public int PawnHalfMoveClock { get; init; }
    public PieceType PieceType { get; set; }
    public Position FromPosition { get; init; } = Position.Unknown;
    public Position ToPosition { get; init; } = Position.Unknown;
    public bool EnPassantCapture { get; set; }
    public bool EnPassantPawnMove { get; set; }
    public bool IsCheck { get; set; }
    public bool IsCheckMate { get; set; }
    public PieceType Capture { get; init; }
    public string PgnFromNotation { get; init; } = string.Empty;
    public bool CanCasteQueenSide { get; init; } = true;
    public bool CanCasteKingSide { get; init; } = true;
    public PieceType Transformation { get; init; }
    public Evaluation? Evaluation { get; set; }
    public MoveVariation? Variation { get; set;}
    public BoardMove? BaseMove { get; set; }
}

public record MoveVariation
{
    public int StartMove { get; init; }
    public int RootStartMove { get; init; }
    public List<BoardMove> Moves { get; init; } = [];
    public MoveVariation? RootVariation { get; set; }
    public List<MoveVariation> ChildVariations { get; init; } = [];
    public Evaluation? Evaluation { get; set; }
    public int Pv { get; set; }
    public MoveVariation(int startMove, MoveVariation? rootVariation = null, int rootStartMove = 0)
    {
        StartMove = startMove;
        RootVariation = rootVariation;
        RootStartMove = rootStartMove;
    }
}

public record PgnMove
{
    public int MoveNumber { get; init; }
    public PieceType PieceType { get; init; }
    public int FromX { get; init; }
    public int FromY { get; init; }
    public Position? ToPosition { get; init; } = Position.Unknown;
    public bool IsCapture { get; init; }
    public bool IsCheck { get; init; }
    public bool IsCheckMate { get; init; }
    public bool IsCastleKingSide { get; init; }
    public bool IsCastleQueenSide { get; init; }
    public PieceType Transformation { get; init; }
    public List<string> Comments { get; init; } = new();
    public Result Result { get; set; }
    public string Origin { get; init; } = string.Empty;
}

