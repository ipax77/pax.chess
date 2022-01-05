namespace pax.chess;

public record Variation
{
    public Move StartMove { get; init; }
    public List<Move> Moves { get; init; }
    public Variation(Move startMove, Move newMove)
    {
        StartMove = startMove;
        Moves = new List<Move>() { newMove };
    }
}
