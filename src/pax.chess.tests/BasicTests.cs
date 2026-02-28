namespace pax.chess.tests;

[TestClass]
public sealed class BasicTests
{
    [TestMethod]
    public void CanCreateSquare()
    {
        int rank = 1;
        int file = 1;
        var square = new Square(rank, file);
        var index = rank * 8 + file;
        Assert.AreEqual(index, square.Index);
    }

    [TestMethod]
    public void InitialPosition_HasCorrectPieces()
    {
        var pos = BoardPosition.CreateInitial();

        Assert.AreEqual(PieceType.King, pos.Board[new Square(4, 0).Index]?.Type);
        Assert.AreEqual(PieceColor.White, pos.Board[new Square(4, 0).Index]?.Color);

        Assert.AreEqual(PieceType.King, pos.Board[new Square(4, 7).Index]?.Type);
        Assert.AreEqual(PieceColor.Black, pos.Board[new Square(4, 7).Index]?.Color);
    }

    [TestMethod]
    public void CanMovePiece()
    {
        var pos = BoardPosition.CreateInitial();

        var startSquare = new Square(0, 1);
        var targetSquare = new Square(0, 3);
        var piece = pos.Board[startSquare.Index];
        Assert.IsNotNull(piece);
        Move move = new(startSquare, targetSquare, null);
        var newPos = pos.MakeMove(move);
        piece = newPos.Board[startSquare.Index];
        Assert.IsNull(piece);
        var targetPiece = newPos.Board[targetSquare.Index];
        Assert.IsNotNull(targetPiece);
    }
}
