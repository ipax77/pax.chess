
namespace pax.chess;
// ──────────────────────────────────────────────
// Display contract – implement this in any UI layer
// (WinForms, WPF, Blazor, console, …)
// ──────────────────────────────────────────────
public interface IChessClockDisplay
{
    /// <summary>Called whenever either clock value changes.</summary>
    void OnClockUpdated(TimeSpan whiteTime, TimeSpan blackTime, PieceColor? activeColor);

    /// <summary>Called when a player's time reaches zero.</summary>
    void OnTimeout(PieceColor timedOutColor);
}

// ──────────────────────────────────────────────
// Event args (keeps the event surface clean)
// ──────────────────────────────────────────────
public sealed class ClockUpdatedEventArgs(TimeSpan whiteTime, TimeSpan blackTime, PieceColor? activeColor) : EventArgs
{
    public TimeSpan WhiteTime { get; } = whiteTime;
    public TimeSpan BlackTime { get; } = blackTime;
    public PieceColor? ActiveColor { get; } = activeColor;
}

public sealed class TimeoutEventArgs(PieceColor color) : EventArgs
{
    public PieceColor Color { get; } = color;
}

// ──────────────────────────────────────────────
// Enhanced ChessClock
// ──────────────────────────────────────────────
public sealed class ChessClock : IDisposable
{
    // ── State ────────────────────────────────
    public TimeSpan WhiteTime { get; private set; }
    public TimeSpan BlackTime { get; private set; }
    public TimeSpan Increment { get; }

    private PieceColor? _activeColor;   // whose clock is currently ticking
    private DateTime? _lastMoveTime;
    private Timer? _tickTimer;
    private bool _disposed;

    // ── Display wiring ───────────────────────
    private IChessClockDisplay? _display;

    // ── Events (alternative to IChessClockDisplay) ──
    public event EventHandler<ClockUpdatedEventArgs>? ClockUpdated;
    public event EventHandler<TimeoutEventArgs>? Timeout;

    // ── Tick interval for live UI updates ───
    private static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(100);

    // ─────────────────────────────────────────
    public ChessClock(TimeSpan initialTime, TimeSpan increment, IChessClockDisplay? display = null)
    {
        WhiteTime = initialTime;
        BlackTime = initialTime;
        Increment = increment;
        _display = display;
    }

    // ── Attach / detach a display at any time ─
    public void AttachDisplay(IChessClockDisplay display) => _display = display;
    public void DetachDisplay() => _display = null;

    // ─────────────────────────────────────────
    public void Start(PieceColor firstMover = PieceColor.White)
    {
        _lastMoveTime = DateTime.UtcNow;
        _activeColor = firstMover;
        StartTicking();
        RaiseClockUpdated();
    }

    public void ApplyMove(PieceColor color)
    {
        if (_lastMoveTime is null)
        {
            Start(color);
            return;
        }

        var now = DateTime.UtcNow;
        var elapsed = now - _lastMoveTime.Value;
        _lastMoveTime = now;

        // Deduct time from the player who just moved
        if (color == PieceColor.White)
            WhiteTime = Clamp(WhiteTime - elapsed + Increment);
        else
            BlackTime = Clamp(BlackTime - elapsed + Increment);

        // Switch active color
        _activeColor = color == PieceColor.White ? PieceColor.Black : PieceColor.White;

        CheckTimeout(color);
        RaiseClockUpdated();
    }

    public void SetTime(PieceColor color, TimeSpan time)
    {
        if (color == PieceColor.White) WhiteTime = time;
        else BlackTime = time;
        RaiseClockUpdated();
    }

    public void Pause()
    {
        StopTicking();
        _lastMoveTime = null;   // clock is paused; resume via Start()
    }

    public bool HasTimedOut(PieceColor color) =>
        color == PieceColor.White ? WhiteTime <= TimeSpan.Zero : BlackTime <= TimeSpan.Zero;

    public PieceColor? TimedOutColor =>
        WhiteTime <= TimeSpan.Zero ? PieceColor.White :
        BlackTime <= TimeSpan.Zero ? PieceColor.Black : null;

    // ── Internals ────────────────────────────
    private void StartTicking()
    {
        _tickTimer?.Dispose();
        _tickTimer = new Timer(_ => Tick(), null, TickInterval, TickInterval);
    }

    private void StopTicking()
    {
        _tickTimer?.Dispose();
        _tickTimer = null;
    }

    /// <summary>
    /// Called ~10× per second to push live time updates to the display
    /// without waiting for a move to be applied.
    /// </summary>
    private void Tick()
    {
        if (_lastMoveTime is null || _activeColor is null) return;

        var elapsed = DateTime.UtcNow - _lastMoveTime.Value;

        // Compute display times without mutating state
        var displayWhite = _activeColor == PieceColor.White
            ? Clamp(WhiteTime - elapsed)
            : WhiteTime;

        var displayBlack = _activeColor == PieceColor.Black
            ? Clamp(BlackTime - elapsed)
            : BlackTime;

        // Fire events / display callback on the tick
        ClockUpdated?.Invoke(this, new ClockUpdatedEventArgs(displayWhite, displayBlack, _activeColor));
        _display?.OnClockUpdated(displayWhite, displayBlack, _activeColor);

        // Detect live timeout (clock ran out between moves)
        if (_activeColor == PieceColor.White && displayWhite <= TimeSpan.Zero)
        {
            StopTicking();
            CheckTimeout(PieceColor.White);
        }
        else if (_activeColor == PieceColor.Black && displayBlack <= TimeSpan.Zero)
        {
            StopTicking();
            CheckTimeout(PieceColor.Black);
        }
    }

    private void RaiseClockUpdated()
    {
        ClockUpdated?.Invoke(this, new ClockUpdatedEventArgs(WhiteTime, BlackTime, _activeColor));
        _display?.OnClockUpdated(WhiteTime, BlackTime, _activeColor);
    }

    private void CheckTimeout(PieceColor color)
    {
        if (!HasTimedOut(color)) return;
        Timeout?.Invoke(this, new TimeoutEventArgs(color));
        _display?.OnTimeout(color);
    }

    private static TimeSpan Clamp(TimeSpan t) =>
        TimeSpan.FromTicks(Math.Max(0, t.Ticks));

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        StopTicking();
    }
}

// ───────────────────────────
// Example: console display 
// ───────────────────────────
public sealed class ChessClockConsoleDisplay : IChessClockDisplay
{
    public void OnClockUpdated(TimeSpan whiteTime, TimeSpan blackTime, PieceColor? activeColor)
    {
        Console.SetCursorPosition(0, 0);
        Console.WriteLine($"  White {(activeColor == PieceColor.White ? "▶" : " ")}  {Format(whiteTime)}");
        Console.WriteLine($"  Black {(activeColor == PieceColor.Black ? "▶" : " ")}  {Format(blackTime)}");
    }

    public void OnTimeout(PieceColor timedOutColor) =>
        Console.WriteLine($"\n⚠  {timedOutColor} has timed out!");

    private static string Format(TimeSpan t) =>
        $"{(int)t.TotalMinutes:D2}:{t.Seconds:D2}.{t.Milliseconds / 100}";
}