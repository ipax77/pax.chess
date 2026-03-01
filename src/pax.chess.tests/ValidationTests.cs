using pax.chess.Validation;

namespace pax.chess.tests;

[TestClass]
public sealed class ValidationTests
{
    [TestMethod]
    public void CanIdentifyAttackedSquare()
    {
        var pos = BoardPosition.CreateInitial();
        var from1 = new Square(4, 1);
        var to1 = new Square(4, 3);
        var move1 = new Move(from1, to1);
        var newpos = pos.MakeMove(move1);
        var from2 = new Square(3, 6);
        var to2 = new Square(3, 4);
        var move2 = new Move(from2, to2);
        newpos = newpos.MakeMove(move2);

        var isAttacked = MoveValidator.IsSquareAttacked(to1, PieceColor.White, newpos);
        Assert.IsTrue(isAttacked);
    }

    [TestMethod]
    public void CanValidateMove()
    {
        var pos = BoardPosition.CreateInitial();
        var from1 = new Square(4, 1);
        var to1 = new Square(4, 3);
        var move1 = new Move(from1, to1);
        var moveState = MoveValidator.IsValidMove(move1, pos);
        Assert.AreEqual(MoveState.Ok, moveState);
    }

    [TestMethod]
    public void CanValidateWrongMove()
    {
        var pos = BoardPosition.CreateInitial();
        var from1 = new Square(4, 1);
        var to1 = new Square(4, 4);
        var move1 = new Move(from1, to1);
        var moveState = MoveValidator.IsValidMove(move1, pos);
        Assert.AreEqual(MoveState.TargetInvalid, moveState);
    }

    [TestMethod]
    public void Fen_Checkmate()
    {
        string fen = "2r3k1/6pp/p3pp1B/2bn4/2pK3P/3b1PR1/P7/3R4 w - - 2 31";
        var pos = FenSerializer.Parse(fen);
        var result = MoveValidator.GetGameState(pos);
        Assert.AreEqual(GameState.Checkmate, result);
    }
    
    [TestMethod]
    public void Fen_Stalemate()
    {
        // Classic stalemate - black king has no legal moves but is not in check
        string fen = "5k2/5P2/5K2/8/8/8/8/8 b - - 0 1";
        var pos = FenSerializer.Parse(fen);
        var result = MoveValidator.GetGameState(pos);
        Assert.AreEqual(GameState.Stalemate, result);
    }

    [TestMethod]
    public void Fen_Check()
    {
        // King is in check but has escape moves
        string fen = "4k3/8/4r3/8/8/8/8/4K3 w - - 0 1";
        var pos = FenSerializer.Parse(fen);
        var result = MoveValidator.GetGameState(pos);
        Assert.AreEqual(GameState.Check, result);
    }

    [TestMethod]
    public void Fen_Normal()
    {
        // Starting position - nothing special
        string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        var pos = FenSerializer.Parse(fen);
        var result = MoveValidator.GetGameState(pos);
        Assert.AreEqual(GameState.Normal, result);
    }
}
