
using BenchmarkDotNet.Attributes;
using pax.chess.Validation;

namespace pax.chess.benchmark;

[MemoryDiagnoser]
public class MoveValidatorBenchmarks
{
    private BoardPosition _startPosition = null!;
    private BoardPosition _checkmatePosition = null!;
    private BoardPosition _middleGamePosition = null!;
    private BoardPosition _pinnedPosition = null!;
    private BoardPosition _castlingPathAttackedPosition = null!;
    private Move _legalOpeningPawnMove = null!;
    private Move _illegalPawnOverAdvanceMove = null!;
    private Move _legalKnightMove = null!;
    private Move _pinnedMove = null!;
    private Move _castlingPathAttackedMove = null!;

    [GlobalSetup]
    public void Setup()
    {
        _startPosition = FenSerializer.Parse("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        _checkmatePosition = FenSerializer.Parse("2r3k1/6pp/p3pp1B/2bn4/2pK3P/3b1PR1/P7/3R4 w - - 2 31");
        _middleGamePosition = FenSerializer.Parse("r1bqk2r/pppp1ppp/2n2n2/2b1p3/2B1P3/2N2N2/PPPP1PPP/R1BQK2R w KQkq - 4 5");
        _pinnedPosition = CreatePosition(
            PieceColor.White,
            CastlingRights.None,
            null,
            (new Square(4, 0), new Piece(PieceType.King, PieceColor.White)),
            (new Square(6, 7), new Piece(PieceType.King, PieceColor.Black)),
            (new Square(4, 1), new Piece(PieceType.Rook, PieceColor.White)),
            (new Square(4, 7), new Piece(PieceType.Rook, PieceColor.Black)));
        _castlingPathAttackedPosition = CreatePosition(
            PieceColor.White,
            CastlingRights.WhiteKingSide,
            null,
            (new Square(4, 0), new Piece(PieceType.King, PieceColor.White)),
            (new Square(7, 0), new Piece(PieceType.Rook, PieceColor.White)),
            (new Square(4, 7), new Piece(PieceType.King, PieceColor.Black)),
            (new Square(5, 7), new Piece(PieceType.Rook, PieceColor.Black)));
        _legalOpeningPawnMove = new Move(new Square(4, 1), new Square(4, 3));
        _illegalPawnOverAdvanceMove = new Move(new Square(4, 1), new Square(4, 4));
        _legalKnightMove = new Move(new Square(1, 0), new Square(2, 2));
        _pinnedMove = new Move(new Square(4, 1), new Square(5, 1));
        _castlingPathAttackedMove = new Move(
            new Square(4, 0),
            new Square(6, 0),
            null,
            MoveType.CastlingKingSide);
    }

    // --- GetGameState ---

    [Benchmark]
    public GameState GetGameState_StartPosition()
        => MoveValidator.GetGameState(_startPosition);

    [Benchmark]
    public GameState GetGameState_Checkmate()
        => MoveValidator.GetGameState(_checkmatePosition);

    [Benchmark]
    public GameState GetGameState_MiddleGame()
        => MoveValidator.GetGameState(_middleGamePosition);

    // --- GetValidMoves ---

    [Benchmark]
    public IReadOnlyCollection<Move> GetValidMoves_Knight()
    {
        var square = new Square(1, 0); // b1 knight in start position
        return MoveValidator.GetValidMoves(square, _startPosition, out _);
    }

    [Benchmark]
    public IReadOnlyCollection<Move> GetValidMoves_Queen_MiddleGame()
    {
        var square = new Square(3, 0); // d1 queen
        return MoveValidator.GetValidMoves(square, _middleGamePosition, out _);
    }

    // --- IsSquareAttacked ---

    [Benchmark]
    public bool IsSquareAttacked_KingSafe()
    {
        var kingSquare = _startPosition.Board.GetKingSquare(PieceColor.White);
        return MoveValidator.IsSquareAttacked(kingSquare, PieceColor.White, _startPosition);
    }

    [Benchmark]
    public bool IsSquareAttacked_Checkmate()
    {
        var kingSquare = _checkmatePosition.Board.GetKingSquare(PieceColor.White);
        return MoveValidator.IsSquareAttacked(kingSquare, PieceColor.White, _checkmatePosition);
    }

    // --- IsValidMove: MoveValidator vs PseudoMoveValidator ---

    [Benchmark(Baseline = true)]
    public MoveState MoveValidator_IsValidMove_LegalOpeningPawn()
        => MoveValidator.IsValidMove(_legalOpeningPawnMove, _startPosition);

    [Benchmark]
    public MoveState PseudoMoveValidator_IsValidMove_LegalOpeningPawn()
        => PseudoMoveValidator.IsValidMove(_legalOpeningPawnMove, _startPosition);

    [Benchmark]
    public MoveState MoveValidator_IsValidMove_IllegalPawnOverAdvance()
        => MoveValidator.IsValidMove(_illegalPawnOverAdvanceMove, _startPosition);

    [Benchmark]
    public MoveState PseudoMoveValidator_IsValidMove_IllegalPawnOverAdvance()
        => PseudoMoveValidator.IsValidMove(_illegalPawnOverAdvanceMove, _startPosition);

    [Benchmark]
    public MoveState MoveValidator_IsValidMove_LegalKnight()
        => MoveValidator.IsValidMove(_legalKnightMove, _startPosition);

    [Benchmark]
    public MoveState PseudoMoveValidator_IsValidMove_LegalKnight()
        => PseudoMoveValidator.IsValidMove(_legalKnightMove, _startPosition);

    [Benchmark]
    public MoveState MoveValidator_IsValidMove_PinnedMove()
        => MoveValidator.IsValidMove(_pinnedMove, _pinnedPosition);

    [Benchmark]
    public MoveState PseudoMoveValidator_IsValidMove_PinnedMove()
        => PseudoMoveValidator.IsValidMove(_pinnedMove, _pinnedPosition);

    [Benchmark]
    public MoveState MoveValidator_IsValidMove_CastlingPathAttacked()
        => MoveValidator.IsValidMove(_castlingPathAttackedMove, _castlingPathAttackedPosition);

    [Benchmark]
    public MoveState PseudoMoveValidator_IsValidMove_CastlingPathAttacked()
        => PseudoMoveValidator.IsValidMove(_castlingPathAttackedMove, _castlingPathAttackedPosition);

    private static BoardPosition CreatePosition(
        PieceColor sideToMove,
        CastlingRights castlingRights,
        Square? enPassantTarget,
        params (Square Square, Piece Piece)[] pieces)
    {
        var board = new Board();

        foreach (var (square, piece) in pieces)
            board[square.Index] = piece;

        return new BoardPosition(
            board,
            sideToMove,
            castlingRights,
            enPassantTarget,
            halfmoveClock: 0,
            fullmoveNumber: 1);
    }
}
