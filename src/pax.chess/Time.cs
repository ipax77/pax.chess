namespace pax.chess;
public class Time
{
    public DateTime StartTime { get; init; }
    public TimeSpan WhiteTime { get; init; }
    public TimeSpan BlackTime { get; init; }
    public TimeSpan WhiteIncrement { get; init; }
    public TimeSpan BlackIncrement { get; init; }
    public TimeSpan CurrentWhiteTime { get; private set; }
    public TimeSpan CurrentBlackTime { get; private set; }
    public DateTime LastMoveTime { get; private set; }
    public TimeSpan LastMoveDuration { get; private set; }
    public int Moves { get; private set; }

    public Time(TimeSpan whitetime, TimeSpan whiteincrement, TimeSpan blacktime = new TimeSpan(), TimeSpan blackincrement = new TimeSpan())
    {
        WhiteTime = whitetime;
        CurrentWhiteTime = WhiteTime;
        WhiteIncrement = whiteincrement;
        BlackTime = blacktime == TimeSpan.Zero ? whitetime : blacktime;
        CurrentBlackTime = BlackTime;
        BlackIncrement = blackincrement == TimeSpan.Zero ? whiteincrement : blackincrement;
        StartTime = DateTime.UtcNow;
        LastMoveTime = StartTime;
    }

    public bool WhiteMoved()
    {
        CurrentWhiteTime -= (DateTime.UtcNow - LastMoveTime);
        CurrentWhiteTime += WhiteIncrement;
        LastMoveDuration = DateTime.UtcNow - LastMoveTime;
        LastMoveTime = DateTime.UtcNow;

        if (CurrentWhiteTime.TotalMilliseconds < 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool BlackMoved()
    {
        CurrentBlackTime -= (DateTime.UtcNow - LastMoveTime);
        CurrentBlackTime += BlackIncrement;
        LastMoveTime = DateTime.UtcNow;
        if (CurrentBlackTime.TotalMilliseconds < 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
