using System.Globalization;
using System.Text.Json.Serialization;

namespace pax.chess;
public record Evaluation
{
    public int Score { get; init; }
    public int Mate { get; init; }
    public bool IsBlack { get; init; }
    public MoveQuality MoveQuality { get; set; }

    public double ChartScore() => (Score, Mate) switch
    {
        (_, > 0) => 20.0,
        (_, < 0) => -20.0,
        ( >= 0, 0) => Math.Min(20, Math.Round((double)Score / 100.0, 2)),
        ( < 0, 0) => Math.Max(-20, Math.Round((double)Score / 100.0, 2)),
    };

    public override string ToString() => Mate != 0 ? $"#M{Mate}" : ((double)Score / 100.0).ToString("N2", CultureInfo.InvariantCulture);

    [JsonConstructor]
    public Evaluation()
    {

    }

    public Evaluation(int score, int mate, bool isBlack)
    {
        //Score = score;
        //Mate = mate;
        IsBlack = isBlack;
        Score = isBlack ? score * -1 : score;
        Mate = isBlack ? mate * -1 : mate;
    }

    public Evaluation(Evaluation evaluation)
    {
        Score = evaluation.Score;
        Mate = evaluation.Mate;
        IsBlack = evaluation.IsBlack;
    }
}