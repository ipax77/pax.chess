namespace pax.chess;

public record StateInfo
{
    public bool BlackToMove { get; internal set; } = false;
    public bool WhiteCanCastleKingSide { get; internal set; } = true;
    public bool WhiteCanCastleQueenSide { get; internal set; } = true;
    public bool BlackCanCastleKingSide { get; internal set; } = true;
    public bool BlackCanCastleQueenSide { get; internal set; } = true;
    public Position? EnPassantPosition { get; internal set; } = null;
    public int PawnHalfMoveClock { get; internal set; } = 0;
    public bool IsCheck { get; internal set; } = false;
    public bool IsCheckMate { get; internal set; } = false;

    public StateInfo() { }
    public StateInfo(StateInfo info)
    {
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
