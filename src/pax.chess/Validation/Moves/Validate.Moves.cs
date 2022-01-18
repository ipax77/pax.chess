namespace pax.chess.Validation;
public static partial class Validate
{
    /// <summary>
    /// Returns all technically possible moves for the given piece and state
    /// </summary>
    /// <remarks>
    /// <para>
    /// There is no check whether it would be check or checkmate 
    /// </para>
    /// </remarks>       
    public static IReadOnlyCollection<Position> GetMoves(Piece piece, State state)
    {
        if (piece == null)
        {
            throw new ArgumentNullException(nameof(piece));
        }
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }
        return piece.Type switch
        {
            PieceType.Pawn => GetPawnMoves(piece, state),
            PieceType.Knight => GetKnightMoves(piece, state.Pieces),
            PieceType.Bishop => GetBishopMoves(piece, state.Pieces),
            PieceType.Rook => GetRookMoves(piece, state.Pieces),
            PieceType.Queen => GetQueenMoves(piece, state.Pieces),
            PieceType.King => GetKingMoves(piece, state),
            _ => throw new ArgumentOutOfRangeException($"unknown piece type {piece.Type}")
        };
    }
}
