namespace pax.chess;
public record EngineMove
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
    public override string ToString() => Map.GetEngineMoveString(this);
}

public record EngineMoveNum : EngineMove
{
    public int HalfMoveNumber { get; init; }

    public EngineMoveNum(int num, int oldX, int oldY, int newX, int newY, PieceType? transfromation = null) : base(oldX, oldY, newX, newY, transfromation)
    {
        HalfMoveNumber = num;
    }
}