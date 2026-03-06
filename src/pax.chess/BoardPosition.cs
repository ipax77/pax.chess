
namespace pax.chess;

public sealed class BoardPosition(
    Board board,
    PieceColor sideToMove,
    CastlingRights castlingRights,
    Square? enPassantTarget,
    int halfmoveClock,
    int fullmoveNumber)
{
    public Board Board { get; } = board;
    public PieceColor SideToMove { get; } = sideToMove;
    public CastlingRights CastlingRights { get; } = castlingRights;
    public Square? EnPassantTarget { get; } = enPassantTarget;
    public int HalfmoveClock { get; } = halfmoveClock;
    public int FullmoveNumber { get; } = fullmoveNumber;

    public BoardPosition MakeMove(Move move)
    {
        ArgumentNullException.ThrowIfNull(move);

        var newBoard = Board.Clone();

        var movingPiece = newBoard[move.From.Index]
            ?? throw new InvalidOperationException("No piece on source square.");

        var targetPiece = newBoard[move.To.Index];

        bool isCapture = targetPiece is not null;

        // 1. Handle special moves first
        HandleCastling(newBoard, move, movingPiece);
        HandleEnPassant(newBoard, move, movingPiece);

        // 2. Move piece
        newBoard[move.From.Index] = null;

        var placedPiece = move.Promotion is not null
            ? new Piece(move.Promotion.Value, movingPiece.Color)
            : movingPiece;

        newBoard[move.To.Index] = placedPiece;

        // 3. Compute new state
        var newCastlingRights = UpdateCastlingRights(move, movingPiece);
        var newEnPassantTarget = ComputeEnPassantTarget(move, movingPiece);
        var newHalfmoveClock = ComputeHalfmoveClock(movingPiece, isCapture);
        var newFullmoveNumber = ComputeFullmoveNumber();



        return new BoardPosition(
            newBoard,
            SideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White,
            newCastlingRights,
            newEnPassantTarget,
            newHalfmoveClock,
            newFullmoveNumber
        );
    }

    private static void HandleCastling(Board board, Move move, Piece piece)
    {
        if (piece.Type != PieceType.King)
            return;

        int fileDelta = move.To.File - move.From.File;

        if (Math.Abs(fileDelta) != 2)
            return;

        bool kingSide = fileDelta > 0;

        int rank = move.From.Rank;

        var rookFrom = kingSide
            ? new Square(7, rank)
            : new Square(0, rank);

        var rookTo = kingSide
            ? new Square(5, rank)
            : new Square(3, rank);

        var rook = board[rookFrom.Index]
            ?? throw new InvalidOperationException("Missing rook for castling.");

        board[rookFrom.Index] = null;
        board[rookTo.Index] = rook;
    }

    private void HandleEnPassant(Board board, Move move, Piece piece)
    {
        if (piece.Type != PieceType.Pawn)
            return;

        if (EnPassantTarget is null)
            return;

        if (move.To != EnPassantTarget)
            return;

        int direction = piece.Color == PieceColor.White ? -1 : 1;

        var capturedSquare = new Square(
            move.To.File,
            move.To.Rank + direction);

        board[capturedSquare.Index] = null;
    }

    private static Square? ComputeEnPassantTarget(Move move, Piece piece)
    {
        if (piece.Type != PieceType.Pawn)
            return null;

        int delta = move.To.Rank - move.From.Rank;

        if (Math.Abs(delta) != 2)
            return null;

        int midRank = (move.From.Rank + move.To.Rank) / 2;

        return new Square(move.From.File, midRank);
    }

    private CastlingRights UpdateCastlingRights(Move move, Piece piece)
    {
        var rights = CastlingRights;

        if (piece.Type == PieceType.King)
        {
            rights &= piece.Color == PieceColor.White
                ? ~(CastlingRights.WhiteKingSide | CastlingRights.WhiteQueenSide)
                : ~(CastlingRights.BlackKingSide | CastlingRights.BlackQueenSide);
        }

        if (piece.Type == PieceType.Rook)
            rights = RemoveRookRight(move.From, piece.Color, rights);

        // rook capture
        var captured = Board[move.To.Index];
        if (captured?.Type == PieceType.Rook)
            rights = RemoveRookRight(move.To, captured.Value.Color, rights);

        return rights;
    }

    private static CastlingRights RemoveRookRight(Square from, PieceColor color, CastlingRights rights)
    {
        if (color == PieceColor.White)
        {
            if (from.File == 0)
                rights &= ~CastlingRights.WhiteQueenSide;
            else if (from.File == 7)
                rights &= ~CastlingRights.WhiteKingSide;
        }
        else
        {
            if (from.File == 0)
                rights &= ~CastlingRights.BlackQueenSide;
            else if (from.File == 7)
                rights &= ~CastlingRights.BlackKingSide;
        }

        return rights;
    }

    private int ComputeHalfmoveClock(Piece piece, bool isCapture)
    {
        if (piece.Type == PieceType.Pawn || isCapture)
            return 0;

        return HalfmoveClock + 1;
    }

    private int ComputeFullmoveNumber()
    {
        return SideToMove == PieceColor.Black
            ? FullmoveNumber + 1
            : FullmoveNumber;
    }

    public BoardPosition Clone()
    {
        return new BoardPosition(
            Board.Clone(),
            SideToMove,
            CastlingRights,
            EnPassantTarget,
            HalfmoveClock,
            FullmoveNumber
        );
    }

    public static BoardPosition CreateInitial()
    {
        var board = new Board();

        // White pieces
        board[new Square(0, 0).Index] = new Piece(PieceType.Rook, PieceColor.White);
        board[new Square(1, 0).Index] = new Piece(PieceType.Knight, PieceColor.White);
        board[new Square(2, 0).Index] = new Piece(PieceType.Bishop, PieceColor.White);
        board[new Square(3, 0).Index] = new Piece(PieceType.Queen, PieceColor.White);
        board[new Square(4, 0).Index] = new Piece(PieceType.King, PieceColor.White);
        board[new Square(5, 0).Index] = new Piece(PieceType.Bishop, PieceColor.White);
        board[new Square(6, 0).Index] = new Piece(PieceType.Knight, PieceColor.White);
        board[new Square(7, 0).Index] = new Piece(PieceType.Rook, PieceColor.White);

        for (int file = 0; file < 8; file++)
            board[new Square(file, 1).Index] = new Piece(PieceType.Pawn, PieceColor.White);

        // Black pieces
        board[new Square(0, 7).Index] = new Piece(PieceType.Rook, PieceColor.Black);
        board[new Square(1, 7).Index] = new Piece(PieceType.Knight, PieceColor.Black);
        board[new Square(2, 7).Index] = new Piece(PieceType.Bishop, PieceColor.Black);
        board[new Square(3, 7).Index] = new Piece(PieceType.Queen, PieceColor.Black);
        board[new Square(4, 7).Index] = new Piece(PieceType.King, PieceColor.Black);
        board[new Square(5, 7).Index] = new Piece(PieceType.Bishop, PieceColor.Black);
        board[new Square(6, 7).Index] = new Piece(PieceType.Knight, PieceColor.Black);
        board[new Square(7, 7).Index] = new Piece(PieceType.Rook, PieceColor.Black);

        for (int file = 0; file < 8; file++)
            board[new Square(file, 6).Index] = new Piece(PieceType.Pawn, PieceColor.Black);

        return new BoardPosition(
            board,
            PieceColor.White,
            CastlingRights.WhiteKingSide |
            CastlingRights.WhiteQueenSide |
            CastlingRights.BlackKingSide |
            CastlingRights.BlackQueenSide,
            enPassantTarget: null,
            halfmoveClock: 0,
            fullmoveNumber: 1
        );
    }
}
