using pax.chess.Analyze;

namespace pax.chess.tests;

[TestClass]
public sealed class AnalysisBoardTests
{
    [TestMethod]
    public void TestInitialState()
    {
        var board = new AnalysisBoard(new());
        Assert.IsNotNull(board.Root);
        Assert.AreEqual(board.Root, board.CurrentNode);
        Assert.IsNull(board.Root.Parent);
        Assert.IsNull(board.Root.Move);
        Assert.IsEmpty(board.Root.Variations);
    }

    [TestMethod]
    public void TestApplyMove()
    {
        var board = new AnalysisBoard(new());
        var e2e4 = new Move(new Square(4, 1), new Square(4, 3));

        board.AddVariation(e2e4);

        Assert.HasCount(1, board.Root.Children);
        Assert.AreEqual(board.Root.Children.First(), board.CurrentNode);
        Assert.AreEqual(e2e4, board.CurrentNode.Move);
        Assert.AreEqual(board.Root, board.CurrentNode.Parent);
    }



    [TestMethod]
    public void TestNavigation()
    {
        var board = new AnalysisBoard(new());
        var e2e4 = new Move(new Square(4, 1), new Square(4, 3));
        var e7e5 = new Move(new Square(4, 6), new Square(4, 4));

        board.AddVariation(e2e4);
        board.AddVariation(e7e5);

        Assert.AreEqual(e7e5, board.CurrentNode.Move);

        board.MoveBackward();
        Assert.AreEqual(e2e4, board.CurrentNode.Move);

        board.MoveBackward();
        Assert.AreEqual(board.Root, board.CurrentNode);

        board.MoveBackward();

        board.MoveForward();
        Assert.AreEqual(e2e4, board.CurrentNode.Move);

        board.MoveForward();
        Assert.AreEqual(e7e5, board.CurrentNode.Move);
    }
}