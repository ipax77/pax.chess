namespace pax.chess;

public record Variation
{
    public int StartMove { get; init; }
    public int RootStartMove { get; init; }
    public List<Move> Moves { get; init; } = new List<Move>();
    public Variation? RootVariation { get; set; }
    public List<Variation> ChildVariations { get; set; } = new List<Variation>();
    public Evaluation? Evaluation { get; set; }
    public Variation(int startMove, Variation? rootVariation = null, int rootStartMove = 0)
    {
        StartMove = startMove;
        RootVariation = rootVariation;
        RootStartMove = rootStartMove;
    }
}
