namespace pax.chess;

public enum Result : byte
{
    None,
    Draw,
    WhiteWin,
    BlackWin,
}

public enum Termination : byte
{
    None,
    Time,
    Mate,
    NoPawnMoves,
    Repetition,
    Agreed
}

public enum Variant : byte
{
    Standard = 0,
    Chess960 = 1,
    Unknown = 99
}

public enum MoveState : byte
{
    Ok,
    PieceNotFound,
    WrongColor,
    TargetInvalid,
    CastleNotAllowed,
    WouldBeCheck
}

public enum MoveQuality : byte
{
    Unknown = 0,
    Only = 1,
    Best = 10,
    Runner = 20,
    Clubhouse = 30,
    Questionmark = 40,
    Blunder = 50
}
