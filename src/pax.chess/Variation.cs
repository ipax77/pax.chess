namespace pax.chess;

public record Variation
{
    public int HalfMoveNumber { get; init; }
    public int Evaluation { get; init; }
    public List<Move> Moves { get; init; }

    public Variation(int halfMoveNumber, int eval, List<Move> moves)
    {
        HalfMoveNumber = halfMoveNumber;
        Evaluation = eval;
        Moves = moves;
    }
}
