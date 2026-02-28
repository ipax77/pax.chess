namespace pax.chess.tests;

[TestClass]
public sealed class FenTests
{
    [TestMethod]
    public void CanCreateStartingPos()
    {
        string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        var pos = FenSerializer.Parse(fen);

        Assert.AreEqual(PieceType.King, pos.Board[new Square(4, 0).Index]?.Type);
        Assert.AreEqual(PieceColor.White, pos.Board[new Square(4, 0).Index]?.Color);

        Assert.AreEqual(PieceType.King, pos.Board[new Square(4, 7).Index]?.Type);
        Assert.AreEqual(PieceColor.Black, pos.Board[new Square(4, 7).Index]?.Color);

        var posFen = FenSerializer.Serialize(pos);
        Assert.AreEqual(fen, posFen);
    }

    [TestMethod]
    public void EnPassant()
    {
        string fen = "rnbqkb1r/p2p1ppp/2P2n2/4p3/Pp2P3/5N2/1PP2PPP/RNBQKB1R b KQkq a3 0 6";
        var pos = FenSerializer.Parse(fen);
        var square = new Square(0, 3);
        var piece = pos.Board[square.Index];
        Assert.IsNotNull(piece);
        Assert.IsTrue(pos.EnPassantTarget.HasValue);
        Assert.AreEqual(0, pos.EnPassantTarget.Value.File);
        Assert.AreEqual(2, pos.EnPassantTarget.Value.Rank);
        
        var posFen = FenSerializer.Serialize(pos);
        Assert.AreEqual(fen, posFen);
    }
}
