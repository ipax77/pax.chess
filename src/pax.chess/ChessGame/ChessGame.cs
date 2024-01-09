using pax.chess.Validation;

namespace pax.chess;

public class ChessGame
{
    public ChessGame()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Unique ChessGame Id
    /// </summary>
    public Guid Id { get; } 


}

public record ChessBoard
{
    public Piece?[] Pieces { get; set; } = new Piece[64];

    public bool BlackToMove { get; private set; }
    public bool WhiteCanCastleKingSide { get; private set; } = true;
    public bool WhiteCanCastleQueenSide { get; private set; } = true;
    public bool BlackCanCastleKingSide { get; private set; } = true;
    public bool BlackCanCastleQueenSide { get; private set; } = true;
    public Position? EnPassantPosition { get; private set; }
    public int PawnHalfMoveClock { get; private set; }
    public bool IsCheck { get; private set; }
    public bool IsCheckMate { get; private set; }
    public int HalfMove { get; private set; }

    public ChessBoard(string fen)
    {
        SetFen(fen);
        IsCheck = Validate.IsCheck(this);
        IsCheckMate = Validate.IsCheckMate(this);
    }

    public ChessBoard()
    {
        for (int x = 0; x < 8; x++)
        {
            Pieces[1 * 8 + x] = new Piece(PieceType.Pawn, isBlack: false, x, 1);
            Pieces[6 * 8 + x] = new Piece(PieceType.Pawn, isBlack: true, x, 6);
        }
        SetupSidePieces(isBlack: false, y: 0);
        SetupSidePieces(isBlack: true, y: 7);
    }
    private void SetupSidePieces(bool isBlack, int y)
    {
        Pieces[y * 8 + 0] = new Piece(PieceType.Rook, isBlack, 0, y);
        Pieces[y * 8 + 1] = new Piece(PieceType.Knight, isBlack, 1, y);
        Pieces[y * 8 + 2] = new Piece(PieceType.Bishop, isBlack, 2, y);
        Pieces[y * 8 + 3] = new Piece(PieceType.Queen, isBlack, 3, y);
        Pieces[y * 8 + 4] = new Piece(PieceType.King, isBlack, 4, y);
        Pieces[y * 8 + 5] = new Piece(PieceType.Bishop, isBlack, 5, y);
        Pieces[y * 8 + 6] = new Piece(PieceType.Knight, isBlack, 6, y);
        Pieces[y * 8 + 7] = new Piece(PieceType.Rook, isBlack, 7, y);
    }

    private void SetFen(string fen)
    {
        ArgumentNullException.ThrowIfNull(fen);

        var fenInfos = fen.Split('/', StringSplitOptions.RemoveEmptyEntries);
        ArgumentOutOfRangeException.ThrowIfLessThan(fenInfos.Length, 8);

        var gameInfos = fenInfos[7].Split(' ', StringSplitOptions.RemoveEmptyEntries);

        fenInfos[7] = gameInfos[0];

        BlackToMove = gameInfos[1] == "b";

        if (!gameInfos[2].Contains('K', StringComparison.Ordinal))
        {
            WhiteCanCastleKingSide = false;
        }
        if (!gameInfos[2].Contains('Q', StringComparison.Ordinal))
        {
            WhiteCanCastleQueenSide = false;
        }
        if (!gameInfos[2].Contains('k', StringComparison.Ordinal))
        {
            BlackCanCastleKingSide = false;
        }
        if (!gameInfos[2].Contains('q', StringComparison.Ordinal))
        {
            BlackCanCastleQueenSide = false;
        }

        if (gameInfos[3] != "-")
        {
            int x = Map.GetIntColumn(gameInfos[3][0]);
            if (int.TryParse(gameInfos[3][1].ToString(), out int y))
            {
                EnPassantPosition = new Position(x, y - 1);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"invalid enpassant info: {gameInfos[3]}");
            }
        }

        if (int.TryParse(gameInfos[4], out int pawnmoves))
        {
            PawnHalfMoveClock = pawnmoves;
        }
        else
        {
            throw new ArgumentOutOfRangeException($"invalid pawn half moves: {gameInfos[4]}");
        }

        for (int y = 0; y < fenInfos.Length; y++)
        {
            int x = 0;
            for (int i = 0; i < fenInfos[y].Length; i++)
            {
                string? interest = null;
                char c = fenInfos[y][i];
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
                    Piece piece = new(Map.GetPieceType(interest), Char.IsLower(interest[0]), x, 7 - y);
                    Pieces[piece.Position.Index()] = piece;
                }
                x += 1;
            }
        }
    }


    public MoveState Move(Position from, Position to, bool skipValidation = false)
    {
        if (!skipValidation)
        {
            var moveState = Validate.ValidateBoardMove(this, from, to);

            if (moveState != MoveState.Ok)
            {
                return moveState;
            }

            var wouldBeCheck = Validate.WouldBeCheck(this, from, to);
            if (wouldBeCheck)
            {
                return MoveState.WouldBeCheck;
            }
        }

        var pieceToMove = GetPieceAt(from);

        ArgumentNullException.ThrowIfNull(pieceToMove);

        SetCasteInfo(pieceToMove);

        var capture = GetPieceAt(to);

        SetHalfMoveClock(pieceToMove, capture);
        HandleEnPassant(pieceToMove, capture, to);

        Pieces[from.Index()] = null;
        Pieces[to.Index()] = pieceToMove;
        pieceToMove.Position = to;

        IsCheck = Validate.IsCheck(this);
        IsCheckMate = Validate.IsCheckMate(this);

        BlackToMove = !BlackToMove;
        HalfMove++;

        return MoveState.Ok;
    }

    private void HandleEnPassant(Piece pieceToMove, Piece? capture, Position to)
    {
        if (pieceToMove.Type == PieceType.Pawn && Math.Abs(pieceToMove.Position.Y - to.Y) > 1)
        {
            EnPassantPosition = GetEnPassantTargetPosition(BlackToMove, to);
            return;
        }

        if (pieceToMove.Type == PieceType.Pawn && EnPassantPosition is not null && to.Y != pieceToMove.Position.Y && capture is null)
        {
            Pieces[new Position(EnPassantPosition.X, BlackToMove ? EnPassantPosition.Y + 1 : EnPassantPosition.Y - 1).Index()] = null;
        }

        EnPassantPosition = null;
    }

    private static Position GetEnPassantTargetPosition(bool isBlack, Position to)
    {
        return isBlack ? new Position(to.X, to.Y + 1) : new Position(to.X, to.Y - 1);
    }

    private void SetHalfMoveClock(Piece pieceToMove, Piece? capture)
    {
        if (pieceToMove.Type == PieceType.Pawn
            || capture is not null)
        {
            PawnHalfMoveClock = 0;
        }
        else
        {
            PawnHalfMoveClock++;
        }
    }

    private void SetCasteInfo(Piece pieceToMove)
    {
        if ((BlackToMove && !BlackCanCastleKingSide && !BlackCanCastleQueenSide) ||
            (!BlackToMove && !WhiteCanCastleKingSide && !WhiteCanCastleQueenSide))
        {
            return;
        }

        if (pieceToMove.Type == PieceType.King)
        {
            if (BlackToMove)
            {
                BlackCanCastleKingSide = false;
                BlackCanCastleQueenSide = false;
            }
            else
            {
                WhiteCanCastleKingSide = false;
                WhiteCanCastleQueenSide = false;
            }
        }

        if (pieceToMove.Type == PieceType.Rook)
        {
            if (BlackToMove)
            {
                if (pieceToMove.Position.X == 0 && pieceToMove.Position.Y == 7)
                {
                    BlackCanCastleQueenSide = false;
                }
                if (pieceToMove.Position.X == 7 && pieceToMove.Position.Y == 7)
                {
                    BlackCanCastleKingSide = false;
                }
            }
            else
            {
                if (pieceToMove.Position.X == 0 && pieceToMove.Position.Y == 0)
                {
                    WhiteCanCastleQueenSide = false;
                }
                if (pieceToMove.Position.X == 7 && pieceToMove.Position.Y == 0)
                {
                    WhiteCanCastleKingSide = false;
                }
            }
        }
    }

    public void DisplayBoard()
    {
        Console.WriteLine("  a b c d e f g h");
        Console.WriteLine(" +----------------");
        for (int y = 7; y >= 0; y--)
        {
            Console.Write($"{y + 1}|");
            for (int x = 0; x < 8; x++)
            {
                var piece = Pieces[y * 8 + x];
                if (piece == null)
                {
                    Console.Write("  ");
                }
                else
                {
                    char pieceSymbol = GetPieceSymbol(piece);
                    Console.ForegroundColor = piece.IsBlack ? ConsoleColor.DarkGray : ConsoleColor.White;
                    Console.Write($" {pieceSymbol}");
                    Console.ResetColor();
                }
            }
            Console.WriteLine();
        }
    }

    private static char GetPieceSymbol(Piece piece)
    {
        return piece.Type switch
        {
            PieceType.Pawn => 'P',
            PieceType.Knight => 'N',
            PieceType.Bishop => 'B',
            PieceType.Rook => 'R',
            PieceType.Queen => 'Q',
            PieceType.King => 'K',
            _ => ' ',
        };
    }

    public Piece? GetPieceAt(Position pos)
    {
        if (pos.OutOfBounds)
        {
            return null;
        }

        return Pieces[pos.Index()];
    }
}