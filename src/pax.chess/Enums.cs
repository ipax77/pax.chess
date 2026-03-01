
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
    None = 0,
    Pawn = 1,
    Knight = 2,
    Bishop = 3,
    Rook = 4,
    Queen = 5,
    King = 6
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
    Ongoing,
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