
using pax.chess.Extensions;

namespace pax.chess.tests;

[TestClass]
public class HashTests
{
    [TestMethod]
    public void CanCreateHashes()
    {
        var game = new ChessGame();
        game.ActivatePositionHashing();

        var startSquare = new Square(0, 1);
        var targetSquare = new Square(0, 3);
        Move move = new(startSquare, targetSquare, null);
        var result = game.TryApplyMove(move);
        Assert.AreEqual(MoveState.Ok, result);
        Assert.AreEqual(1, game.GetCurrentRepetitions());
    }

    [TestMethod]
    public void CanDetectRepetition()
    {
        var positionHasher = new ZobristHasher();
        var game = PgnSerializer.Parse("1. Nc3 Nc6 2. Nb1 Nb8 3. Nc3 Nc6 4. Nb1 Nb8 5. Nc3 Nc6", positionHasher);
        Assert.AreEqual(3, game.GetCurrentRepetitions());
    }

    [TestMethod]
    public void CanUpdateHash()
    {
        var game = new ChessGame();
        game.ActivatePositionHashing();
        var positionHasher = new ZobristHasher();

        var move1 = Uci.CreateMove("e2e4")!;
        var move2 = Uci.CreateMove("e7e5")!;
        var move3 = Uci.CreateMove("g1e2")!;
        var move4 = Uci.CreateMove("g8f6")!;
        game.ApplyMove(move1);
        game.ApplyMove(move2);
        game.ApplyMove(move3);
        game.ApplyMove(move4);
        var incrementalHash = game.CurrentHash;
        var posHash = positionHasher.Compute(game.CurrentPosition);
        Assert.AreEqual(posHash, incrementalHash);
    }

    [TestMethod]
    public void CanReComputeWithCastling()
    {
        var positionHasher = new ZobristHasher();
        var game = PgnSerializer.Parse("1. e4 e5 2. Nf3 Nc6 3. Bc4 Bc5 4. O-O O-O", positionHasher);
        var incrementalHash = game.CurrentHash;
        var posHash = positionHasher.Compute(game.CurrentPosition);
        Assert.AreEqual(posHash, incrementalHash);
    }

    [TestMethod]
    public void CanReComputeWithEnPassant()
    {
        var positionHasher = new ZobristHasher();
        var game = PgnSerializer.Parse("1. e4 c5 2. e5 d5 3. exd6", positionHasher);
        var incrementalHash = game.CurrentHash;
        var posHash = positionHasher.Compute(game.CurrentPosition);
        Assert.AreEqual(posHash, incrementalHash);
    }
}