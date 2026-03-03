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

    [TestMethod]
    public void CanDetectCheckmate()
    {
        var pgn = "1. e4 e5 2. Bc4 Bc5 3. Qh5 Nf6 4. Qxf7#";
        var game = PgnSerializer.Parse(pgn);
        // Console.WriteLine(game.CurrentPosition.Board.ToString());
        var result = game.Result;
        Assert.AreEqual(GameResult.WhiteWin, result);
    }

        [TestMethod]
    public void CanDetectCheckmate2()
    {
        var fen = "2r3k1/6pp/p3pp1B/2bn4/2pK3P/3b1PR1/P7/3R4 w - - 2 31";
        var pos = FenSerializer.Parse(fen);
        var game = new ChessGame(pos, new());
        game.Evaluate();
        var result = game.Result;
        Assert.AreEqual(GameResult.BlackWin, result);
        Assert.AreEqual(GameTermination.Checkmate, game.Conclusion?.Termination);
    }
}
