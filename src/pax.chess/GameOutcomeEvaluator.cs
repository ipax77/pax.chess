
using pax.chess.Validation;

namespace pax.chess;

internal static class GameOutcomeEvaluator
{
    internal static GameResult Evaluate(BoardPosition position, IReadOnlyList<MoveInfo> moves, Dictionary<ulong, int> repetition)
    {
        if (position.HalfmoveClock >= 75)
        {
            return GameResult.Draw;
        }

        if (repetition.Count > 0 && repetition.Values.Max() >= 7)
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