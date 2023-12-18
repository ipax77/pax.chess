namespace pax.chess;

public sealed record StateInfo
{
    public bool BlackToMove { get; internal set; }
    public bool WhiteCanCastleKingSide { get; internal set; } = true;
    public bool WhiteCanCastleQueenSide { get; internal set; } = true;
    public bool BlackCanCastleKingSide { get; internal set; } = true;
    public bool BlackCanCastleQueenSide { get; internal set; } = true;
    public Position? EnPassantPosition { get; internal set; }
    public int PawnHalfMoveClock { get; internal set; }
    public bool IsCheck { get; internal set; }
    public bool IsCheckMate { get; internal set; }

    public StateInfo() { }
    public StateInfo(StateInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);
        BlackToMove = info.BlackToMove;
        WhiteCanCastleKingSide = info.WhiteCanCastleKingSide;
        WhiteCanCastleQueenSide = info.WhiteCanCastleQueenSide;
        BlackCanCastleKingSide = info.BlackCanCastleKingSide;
        BlackCanCastleQueenSide = info.BlackCanCastleQueenSide;
        EnPassantPosition = info.EnPassantPosition == null ? null : new(info.EnPassantPosition);
        PawnHalfMoveClock = info.PawnHalfMoveClock;
        IsCheck = info.IsCheck;
        IsCheckMate = info.IsCheckMate;
    }

    public void Set(StateInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);
        BlackToMove = info.BlackToMove;
        WhiteCanCastleKingSide = info.WhiteCanCastleKingSide;
        WhiteCanCastleQueenSide = info.WhiteCanCastleQueenSide;
        BlackCanCastleKingSide = info.BlackCanCastleKingSide;
        BlackCanCastleQueenSide = info.BlackCanCastleQueenSide;
        EnPassantPosition = info.EnPassantPosition == null ? null : new(info.EnPassantPosition);
        PawnHalfMoveClock = info.PawnHalfMoveClock;
        IsCheck = info.IsCheck;
        IsCheckMate = info.IsCheckMate;
    }
}
