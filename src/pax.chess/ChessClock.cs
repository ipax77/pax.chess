
using System;

namespace pax.chess;

public sealed class ChessClock(TimeSpan initialTime, TimeSpan increment)
{
    public TimeSpan WhiteTime { get; private set; } = initialTime;
    public TimeSpan BlackTime { get; private set; } = initialTime;
    public TimeSpan Increment { get; } = increment;

    private DateTime? _lastMoveTime;

    public void Start()
    {
        _lastMoveTime = DateTime.UtcNow;
    }

    public void ApplyMove(PieceColor color)
    {
        if (_lastMoveTime == null)
        {
            Start();
            return;
        }

        var now = DateTime.UtcNow;
        var elapsed = now - _lastMoveTime.Value;
        _lastMoveTime = now;

        if (color == PieceColor.White)
            WhiteTime = TimeSpan.FromTicks(Math.Max(0, (WhiteTime - elapsed + Increment).Ticks));
        else
            BlackTime = TimeSpan.FromTicks(Math.Max(0, (BlackTime - elapsed + Increment).Ticks));
    }

    public void SetTime(PieceColor color, TimeSpan time)
    {
        if (color == PieceColor.White) WhiteTime = time;
        else BlackTime = time;
    }

    public bool HasTimedOut(PieceColor color) =>
    color == PieceColor.White ? WhiteTime <= TimeSpan.Zero : BlackTime <= TimeSpan.Zero;

    public PieceColor? TimedOutColor =>
        WhiteTime <= TimeSpan.Zero ? PieceColor.White :
        BlackTime <= TimeSpan.Zero ? PieceColor.Black : null;
}
