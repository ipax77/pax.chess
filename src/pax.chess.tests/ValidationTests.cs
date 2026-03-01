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

}
