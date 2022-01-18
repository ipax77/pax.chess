namespace pax.chess;

public enum Result
{
    None,
    Draw,
    WhiteWin,
    BlackWin,
}

public enum Termination
{
    None,
    Time,
    Mate,
    NoPawnMoves,
    Repetition,
    Agreed
}

public enum Variant
{
    Standard = 0,
    Chess960 = 1,
    Unknown = 99
}

public enum MoveState
{
    Ok,
    PieceNotFound,
    WrongColor,
    TargetInvalid,
    CastleNotAllowed,
    WouldBeCheck
}

public enum MoveQuality
{
    Unknown = 0,
    Only = 1,
    Best = 10,
    Runner = 20,
    Clubhouse = 30,
    Questionmark = 40,
    Blunder = 50
}
