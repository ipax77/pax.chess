
namespace pax.chess;

[Flags]
public enum MoveType
{
    None = 0,
    Capture = 1,
    EnPassant = 2,
    CastlingKingSide = 4,
    CastlingQueenSide = 8,
    Promotion = 16
}

public enum PieceType
{
    Pawn = 0,
    Knight = 1,
    Bishop = 2,
    Rook = 3,
    Queen = 4,
    King = 5
}

public enum PieceColor { White, Black }

[Flags]
public enum CastlingRights
{
    None = 0,
    WhiteKingSide = 1,
    WhiteQueenSide = 2,
    BlackKingSide = 4,
    BlackQueenSide = 8
}

public enum GameResult
{
    WhiteWin,
    BlackWin,
    Draw
}

public enum MoveState
{
    Ok,
    PieceNotFound,
    WrongColor,
    NoValidMoves,
    TargetInvalid,
    CastleNotAllowed,
    CastlingPathAttacked,
    WouldBeCheck,
}

public enum GameState
{
    Normal,
    Check,
    Checkmate,
    Stalemate,
}

public enum GameTermination
{
    Checkmate,
    Stalemate,
    Resignation,
    Timeout,
    DrawByAgreement,
    SeventyFiveMoveRule,
    FivefoldRepetition,
    NoMaterial,
}