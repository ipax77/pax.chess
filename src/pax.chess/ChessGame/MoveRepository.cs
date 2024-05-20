
namespace pax.chess;

internal partial class MoveRepository
{
    internal List<BoardMove> Moves { get; set; } = [];
    internal Piece?[] Pieces { get; set; } = new Piece[64];
    public MoveCursor MoveCursor { get; private set; } = new();
    public string StartPosFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";


    public bool BlackToMove { get; protected set; }
    public bool WhiteCanCastleKingSide { get; protected set; } = true;
    public bool WhiteCanCastleQueenSide { get; protected set; } = true;
    public bool BlackCanCastleKingSide { get; protected set; } = true;
    public bool BlackCanCastleQueenSide { get; protected set; } = true;
    public Position? EnPassantPosition { get; protected set; }
    public int PawnHalfMoveClock { get; protected set; }
    public bool IsCheck { get; protected set; }
    public bool IsCheckMate { get; protected set; }
    public int HalfMove { get; protected set; }
    public int MoveNumber { get; protected set; } = 1;
    public Result Result { get; set; }

    internal void GotToMove(int moveNumber, MoveVariation? variation)
    {
        if (variation is null)
        {
            if (moveNumber <= 0)
            {
                SetFen(StartPosFen);
            }
            else
            {
                
            }
        }
    }

    internal void MoveForward()
    {

    }

    internal void MoveBackward()
    {

    }

    internal void RevertMove(BoardMove move)
    {
        var pieceToRevert = GetPieceAt(move.ToPosition);

        ArgumentNullException.ThrowIfNull(pieceToRevert);

        Pieces[move.ToPosition.Index()] = null;
        Pieces[move.FromPosition.Index()] = pieceToRevert;
        pieceToRevert.Position = move.FromPosition;

        // castle
        if (pieceToRevert.Type == PieceType.King && Math.Abs(move.FromPosition.X - move.ToPosition.X) > 1)
        {
            bool kingSideCastle = move.FromPosition.X - move.ToPosition.X < 0;
            Position rookTo = new(kingSideCastle ? (byte)7 : (byte)0, pieceToRevert.Position.Y);
            Position rookFrom = new(kingSideCastle ? (byte)5 : (byte)3, pieceToRevert.Position.Y);

            var rook = GetPieceAt(rookFrom);
            ArgumentNullException.ThrowIfNull(rook);
            Pieces[rookFrom.Index()] = null;
            Pieces[rookTo.Index()] = rook;
            rook.Position = rookTo;
        }
        else
        {
            // revert promotion
            pieceToRevert.Type = move.PieceType;
            if (move.Capture != PieceType.None)
            {
                Position restorePos = move.ToPosition;
                if (move.EnPassantCapture)
                {
                    restorePos = GetEnPassantTargetPosition(pieceToRevert.IsBlack,
                                                            move.ToPosition);
                }
                Pieces[restorePos.Index()] = new(move.Capture, !pieceToRevert.IsBlack, restorePos.X, restorePos.Y);
            }
        }
    }

    internal void ExecuteMove(BoardMove move)
    {
        var pieceToMove = GetPieceAt(move.FromPosition);

        if (pieceToMove is null)
        {
            return;
        }

        var capture = GetPieceAt(move.ToPosition);

        Pieces[move.FromPosition.Index()] = null;
        Pieces[move.ToPosition.Index()] = pieceToMove;
        pieceToMove.Position = move.ToPosition;

        // castle
        if (pieceToMove.Type == PieceType.King && Math.Abs(move.FromPosition.X - move.ToPosition.X) > 1)
        {
            bool kingSideCastle = move.FromPosition.X - move.ToPosition.X < 0;
            Position rookFrom = new(kingSideCastle ? (byte)7 : (byte)0, pieceToMove.Position.Y);
            Position rookTo = new(kingSideCastle ? (byte)5 : (byte)3, pieceToMove.Position.Y);

            var rook = GetPieceAt(rookFrom);
            ArgumentNullException.ThrowIfNull(rook);
            Pieces[rookFrom.Index()] = null;
            Pieces[rookTo.Index()] = rook;
            rook.Position = rookTo;
        }
        else
        {
            HandlePromotion(pieceToMove, move.FromPosition, move.Transformation);
            HandleEnPassant(pieceToMove,
                            capture,
                            move.ToPosition,
                            move.FromPosition.Y - move.ToPosition.X > 0);
        }
    }

    private static bool HandlePromotion(Piece pieceToMove,
                                        Position to,
                                        PieceType transformation)
    {
        if (pieceToMove.Type != PieceType.Pawn)
        {
            return false;
        }

        if ((pieceToMove.IsBlack && to.Y != 0)
            || (!pieceToMove.IsBlack && to.Y != 7))
        {
            return false;
        }

        if (transformation == PieceType.None)
        {
            // throw new ArgumentOutOfRangeException(nameof(transformation));
            transformation = PieceType.Queen;
        }

        pieceToMove.Type = transformation;
        return true;
    }

    private (bool, bool) HandleEnPassant(Piece pieceToMove, Piece? capture, Position to, bool blackToMove)
    {
        if (pieceToMove.Type == PieceType.Pawn && Math.Abs(pieceToMove.Position.Y - to.Y) > 1)
        {
            var pieceLeft = GetPieceAt(new(to.X - 1, to.Y));
            var pieceRight = GetPieceAt(new(to.X + 1, to.Y));

            if ((pieceLeft is not null && pieceLeft.IsBlack != pieceToMove.IsBlack)
                || (pieceRight is not null && pieceRight.IsBlack != pieceToMove.IsBlack))
            {
                EnPassantPosition = GetEnPassantTargetPosition(blackToMove, to);
                return (true, false);
            }
        }

        if (pieceToMove.Type == PieceType.Pawn
            && EnPassantPosition is not null
            && to.Y != pieceToMove.Position.Y
            && capture is null)
        {
            Pieces[new Position(EnPassantPosition.X, blackToMove ?
                EnPassantPosition.Y + 1 : EnPassantPosition.Y - 1).Index()] = null;
            EnPassantPosition = null;
            return (false, true);
        }
        EnPassantPosition = null;
        return (false, false);
    }

    private static Position GetEnPassantTargetPosition(bool isBlack, Position to)
    {
        return isBlack ? new Position(to.X, to.Y + 1) : new Position(to.X, to.Y - 1);
    }


    public Piece? GetPieceAt(Position pos)
    {
        if (pos.OutOfBounds)
        {
            return null;
        }

        return Pieces[pos.Index()];
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

        if (int.TryParse(gameInfos[5], out int moves))
        {
            MoveNumber = moves;
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
}

public record MoveCursor
{
    public BoardMove? CurrentMove { get; set; }
    public MoveVariation? CurrentVariation { get; set; }
}