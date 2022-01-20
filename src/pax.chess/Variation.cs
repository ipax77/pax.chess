using System.Diagnostics.CodeAnalysis;
namespace pax.chess;

[SuppressMessage(
    "Usage", "CA1002:Do not expose generic lists",
    Justification = "I'd argue with performance ..")]
public record Variation
{
    public int StartMove { get; init; }
    public int RootStartMove { get; init; }
    public List<Move> Moves { get; init; } = new List<Move>();
    public Variation? RootVariation { get; set; }
    public List<Variation> ChildVariations { get; init; } = new List<Variation>();
    public Evaluation? Evaluation { get; set; }
    public int Pv { get; set; }
    public Variation(int startMove, Variation? rootVariation = null, int rootStartMove = 0)
    {
        StartMove = startMove;
        RootVariation = rootVariation;
        RootStartMove = rootStartMove;
    }

    public Variation? Find(Variation? variation)
    {
        if (variation == null)
        {
            return null;
        }

        if (this == variation)
        {
            return this;
        }

        foreach (var child in ChildVariations)
        {
            var found = child.Find(variation);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }
}
