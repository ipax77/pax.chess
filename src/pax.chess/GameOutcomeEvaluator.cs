
using pax.chess.Validation;

namespace pax.chess;

internal static class GameOutcomeEvaluator
{
    internal static EvaluationResult? Evaluate(BoardPosition position, IReadOnlyList<MoveInfo> moves, Dictionary<ulong, int> repetition)
    {
        if (position.HalfmoveClock >= 75)
        {
            return new(GameResult.Draw, GameTermination.SeventyFiveMoveRule);
        }

        if (repetition.Count > 0 && repetition.Values.Max() >= 5)
        {
            return new(GameResult.Draw, GameTermination.FivefoldRepetition);
        }

        var gameState = MoveValidator.GetGameState(position);

        if (gameState == GameState.Checkmate)
        {
            var result = position.SideToMove == PieceColor.Black ? GameResult.WhiteWin : GameResult.BlackWin;
            return new(result, GameTermination.Checkmate);
        }

        if (gameState == GameState.Stalemate)
        {
            return new(GameResult.Draw, GameTermination.Stalemate);
        }

        return null;
    }
}

internal sealed record EvaluationResult(
    GameResult Result,
    GameTermination Termination
);