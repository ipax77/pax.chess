namespace pax.chess;
public sealed record EngineMove
{
    public Position OldPosition { get; init; }
    public Position NewPosition { get; init; }
    public PieceType? Transformation { get; init; }
    public EngineMove(int oldX, int oldY, int newX, int newY, PieceType? transfromation = null)
    {
        OldPosition = new Position(oldX, oldY);
        NewPosition = new Position(newX, newY);
        Transformation = transfromation;
    }

    public EngineMove(Position oldPosition, Position newPosition, PieceType? transformation = null)
    {
        OldPosition = oldPosition;
        NewPosition = newPosition;
        Transformation = transformation;
    }

    internal EngineMove(EngineMove move)
    {
        OldPosition = new(move.OldPosition);
        NewPosition = new(move.NewPosition);
        Transformation = move.Transformation;
    }

    public override string ToString() => Map.GetEngineMoveString(this);
}
