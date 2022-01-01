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

    public EngineMove(EngineMove move)
    {
        OldPosition = new(move.OldPosition);
        NewPosition = new(move.NewPosition);
        Transformation = move.Transformation;
    }

    public override string ToString() => Map.GetEngineMoveString(this);
}

public record EngineMoveNum : EngineMove
{
    public int HalfMoveNumber { get; init; }

    public EngineMoveNum(int num, EngineMove move) : base(move)
    {
        HalfMoveNumber = num;
    }
}