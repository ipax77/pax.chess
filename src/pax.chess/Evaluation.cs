using System.Globalization;

namespace pax.chess;
public record Evaluation
{
    public int Score { get; init; }
    public int Mate { get; init; }
    public bool IsBlack { get; init; }

    public override string ToString() => Mate != 0 ? $"#M{Mate}" : ((double)Score / 100.0).ToString("N2", CultureInfo.InvariantCulture);

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