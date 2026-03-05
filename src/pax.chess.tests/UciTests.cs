
using pax.chess.Extensions;

namespace pax.chess.tests;

[TestClass]
public sealed class UciTests
{
    [TestMethod]
    public void CanConvertMoves()
    {
        string pv = "e2e4 e7e5 g1f3 b8c6 f1b5 g8f6 e1g1 f6e4 f1e1 e4d6 f3e5 f8e7 b5f1 c6e5 e1e5 e8g8 d2d4 e7f6 e5e1 d6f5 c2c3 d7d5 c1f4 a7a5 b1d2 c7c6 d2f3 g7g6 f1d3 f5g7 d1c2";
        var engineMoves = pv.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var game = new ChessGame();
        foreach (var engineMove in engineMoves)
        {
            var move = Uci.CreateMove(engineMove, game.CurrentPosition);
            Assert.IsNotNull(move);
            game.ApplyMove(move);
        }

        var revMoves = game.Moves.Select(s => Uci.GetUci(s.Move));
        var revPv = string.Join(' ', revMoves);
        Assert.AreEqual(pv, revPv);
    }
}
