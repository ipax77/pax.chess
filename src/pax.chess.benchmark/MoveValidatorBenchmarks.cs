
using BenchmarkDotNet.Attributes;
using pax.chess.Validation;

namespace pax.chess.benchmark;

[MemoryDiagnoser]
public class MoveValidatorBenchmarks
{
    private BoardPosition _startPosition = null!;
    private BoardPosition _checkmatePosition = null!;
    private BoardPosition _middleGamePosition = null!;

    [GlobalSetup]
    public void Setup()
    {
        _startPosition = FenSerializer.Parse("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        _checkmatePosition = FenSerializer.Parse("2r3k1/6pp/p3pp1B/2bn4/2pK3P/3b1PR1/P7/3R4 w - - 2 31");
        _middleGamePosition = FenSerializer.Parse("r1bqk2r/pppp1ppp/2n2n2/2b1p3/2B1P3/2N2N2/PPPP1PPP/R1BQK2R w KQkq - 4 5");
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
}