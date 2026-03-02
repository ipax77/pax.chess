
using pax.chess.Validation;

namespace pax.chess;

internal static class GameOutcomeEvaluator
{
    internal static GameResult Evaluate(BoardPosition position, IReadOnlyList<MoveInfo> moves)
    {
        if (position.HalfmoveClock >= 50)
        {
            return GameResult.Draw;
        }
        var gameState = MoveValidator.GetGameState(position);

        if (gameState == GameState.Checkmate)
        {
            return position.SideToMove == PieceColor.Black ? GameResult.WhiteWin : GameResult.BlackWin;
        }

        if (gameState == GameState.Stalemate)
        {
            return GameResult.Draw;
        }

        return GameResult.Ongoing;
    }
}