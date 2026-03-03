using Microsoft.VisualStudio.TestTools.UnitTesting;
using pax.chess;

namespace pax.chess.tests;

[TestClass]
public sealed class AnalysisBoardTests
{
    [TestMethod]
    public void TestInitialState()
    {
        var board = new AnalysisBoard();
        Assert.IsNotNull(board.Root);
        Assert.AreEqual(board.Root, board.CurrentNode);
        Assert.IsNull(board.Root.Parent);
        Assert.IsNull(board.Root.Move);
        Assert.IsEmpty(board.Root.Variations);
    }

    [TestMethod]
    public void TestApplyMove()
    {
        var board = new AnalysisBoard();
        var e2e4 = new Move(new Square(4, 1), new Square(4, 3));
        
        var state = board.TryApplyMove(e2e4);
        
        Assert.AreEqual(MoveState.Ok, state);
        Assert.HasCount(1, board.Root.Variations);
        Assert.AreEqual(board.Root.Variations[0], board.CurrentNode);
        Assert.AreEqual(e2e4, board.CurrentNode.Move);
        Assert.AreEqual(board.Root, board.CurrentNode.Parent);
    }

    [TestMethod]
    public void TestVariations()
    {
        var board = new AnalysisBoard();
        var e2e4 = new Move(new Square(4, 1), new Square(4, 3));
        var d2d4 = new Move(new Square(3, 1), new Square(3, 3));

        board.TryApplyMove(e2e4);
        board.MoveBackward();
        board.TryApplyMove(d2d4);

        Assert.HasCount(2, board.Root.Variations);
        Assert.AreEqual(d2d4, board.Root.Variations[1].Move);
        Assert.AreEqual(board.Root.Variations[1], board.CurrentNode);
    }

    [TestMethod]
    public void TestNavigation()
    {
        var board = new AnalysisBoard();
        var e2e4 = new Move(new Square(4, 1), new Square(4, 3));
        var e7e5 = new Move(new Square(4, 6), new Square(4, 4));

        board.TryApplyMove(e2e4);
        board.TryApplyMove(e7e5);

        Assert.AreEqual(e7e5, board.CurrentNode.Move);

        Assert.IsTrue(board.MoveBackward());
        Assert.AreEqual(e2e4, board.CurrentNode.Move);

        Assert.IsTrue(board.MoveBackward());
        Assert.AreEqual(board.Root, board.CurrentNode);

        Assert.IsFalse(board.MoveBackward());

        Assert.IsTrue(board.MoveForward());
        Assert.AreEqual(e2e4, board.CurrentNode.Move);

        Assert.IsTrue(board.MoveForward());
        Assert.AreEqual(e7e5, board.CurrentNode.Move);

        Assert.IsFalse(board.MoveForward());
    }

    [TestMethod]
    public void TestPath()
    {
        var board = new AnalysisBoard();
        var e2e4 = new Move(new Square(4, 1), new Square(4, 3));
        var e7e5 = new Move(new Square(4, 6), new Square(4, 4));

        board.TryApplyMove(e2e4);
        board.TryApplyMove(e7e5);

        var path = board.GetPath();
        Assert.HasCount(2, path);
        Assert.AreEqual(e2e4, path[0].Move);
        Assert.AreEqual(e7e5, path[1].Move);
    }
    
    [TestMethod]
    public void TestSwitchToExistingVariation()
    {
        var board = new AnalysisBoard();
        var e2e4 = new Move(new Square(4, 1), new Square(4, 3));
        
        board.TryApplyMove(e2e4);
        var firstNode = board.CurrentNode;
        
        board.MoveBackward();
        board.TryApplyMove(e2e4);
        
        Assert.AreEqual(firstNode, board.CurrentNode);
        Assert.HasCount(1, board.Root.Variations);
    }

    [TestMethod]
    public void TestDeleteVariation()
    {
        var board = new AnalysisBoard();
        var e2e4 = new Move(new Square(4, 1), new Square(4, 3));
        var d2d4 = new Move(new Square(3, 1), new Square(3, 3));

        board.TryApplyMove(e2e4);
        var e2e4Node = board.CurrentNode;
        board.MoveBackward();
        board.TryApplyMove(d2d4);

        Assert.HasCount(2, board.Root.Variations);

        Assert.IsTrue(board.DeleteVariation(e2e4Node));
        Assert.HasCount(1, board.Root.Variations);
        Assert.AreEqual(d2d4, board.Root.Variations[0].Move);
    }

    [TestMethod]
    public void TestDeleteCurrentVariation()
    {
        var board = new AnalysisBoard();
        var e2e4 = new Move(new Square(4, 1), new Square(4, 3));
        var e7e5 = new Move(new Square(4, 6), new Square(4, 4));

        board.TryApplyMove(e2e4);
        board.TryApplyMove(e7e5);

        Assert.AreEqual(e7e5, board.CurrentNode.Move);

        Assert.IsTrue(board.DeleteCurrentVariation());
        Assert.AreEqual(e2e4, board.CurrentNode.Move);
        Assert.IsEmpty(board.CurrentNode.Variations);
    }
}
