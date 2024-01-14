namespace pax.chess;

public class MoveEventArgs : EventArgs
{
    public BoardMove Move { get; set; } = null!;
    public bool Reverted { get; set; }
}
