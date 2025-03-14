namespace pax.chess.Validation;
public static partial class Validate
{
    /// <summary>
    /// Tries to execute the given move
    /// </summary>
    /// <returns>MoveState.Ok if successful.</returns>
    public static MoveState TryExecuteMove(EngineMove engineMove, State state)
    {
        ArgumentNullException.ThrowIfNull(engineMove);
        ArgumentNullException.ThrowIfNull(state);
        Piece? pieceToMove = state.Pieces.SingleOrDefault(s => s.Position == engineMove.OldPosition);

        if (pieceToMove == null)
        {
            return MoveState.PieceNotFound;
        }

        if (!IsMyTurn(pieceToMove, state))
        {
            return MoveState.WrongColor;
        }

        var validPositions = GetMoves(pieceToMove, state);
        if (!validPositions.Contains(engineMove.NewPosition))
        {
            return MoveState.TargetInvalid;
        }

        // Castle
        if (pieceToMove.Type == PieceType.King && Math.Abs(engineMove.OldPosition.X - engineMove.NewPosition.X) > 1
            && !IsValidCastle(pieceToMove, engineMove, state))
        {
            return MoveState.CastleNotAllowed;
        }

        if (WouldBeCheck(pieceToMove, engineMove.NewPosition, engineMove.Transformation, state))
        {
            return MoveState.WouldBeCheck;
        }

        Move move = state.ExecuteMove(engineMove);

        state.Info.IsCheck = state.IsCheck();
        move.StateInfo.IsCheck = state.Info.IsCheck;

        if (move.StateInfo.IsCheck)
        {
            if (IsCheckMate(state))
            {
                state.Info.IsCheckMate = true;
                move.StateInfo.IsCheck = true;
                move.PgnMove += "#";
            }
            else
            {
                move.PgnMove += "+";
            }
        }

        return MoveState.Ok;
    }

    private static bool IsValidCastle(Piece king, EngineMove engineMove, State state)
    {
        if (state.Info.IsCheck)
        {
            return false;
        }

        bool KingSide = engineMove.OldPosition.X < engineMove.NewPosition.X;

        bool valid = (king.IsBlack, KingSide) switch
        {
            (true, true) => state.Info.BlackCanCastleKingSide,
            (true, false) => state.Info.BlackCanCastleQueenSide,
            (false, true) => state.Info.WhiteCanCastleKingSide,
            (false, false) => state.Info.WhiteCanCastleQueenSide,
        };

        if (!valid)
        {
            return false;
        }

        var possibleAttacers = state.Pieces.Where(x => x.IsBlack != king.IsBlack && x.Type != PieceType.King).ToList();

        Position[] checkPositions = (king.IsBlack, KingSide) switch
        {
            (true, true) => [new Position(5, 7), new Position(6, 7)],
            (true, false) => [new Position(2, 7), new Position(3, 7)],
            (false, true) => [new Position(5, 0), new Position(6, 0)],
            (false, false) => [new Position(2, 0), new Position(3, 0)],
        };

        for (int i = 0; i < checkPositions.Length; i++)
        {
            for (int j = 0; j < possibleAttacers.Count; j++)
            {
                var moves = GetMoves(possibleAttacers[j], state);
                if (moves.Contains(checkPositions[i]))
                {
                    return false;
                }
            }
        }
        return true;
    }

    internal static bool WouldBeCheck(Piece piece, Position destination, PieceType? transformation, State state)
    {
        EngineMove testMove = new(piece.Position, destination, transformation);
        state.ExecuteMove(testMove);
        bool isCheck = state.IsCheck(state.Pieces.Single(s => s.IsBlack == piece.IsBlack && s.Type == PieceType.King));
        state.RevertMove();
        if (isCheck)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    internal static bool IsCheckMate(State state)
    {
        if (state.Info.IsCheck)
        {
            var pieces = state.Pieces.Where(x => state.Info.BlackToMove ? x.IsBlack : !x.IsBlack).ToList();
            var king = pieces.First(f => f.Type == PieceType.King);
            var kingmoves = Validate.GetKingMoves(king, state);
            for (int i = 0; i < kingmoves.Count; i++)
            {
                if (!WouldBeCheck(king, kingmoves[i], null, state))
                {
                    return false;
                }
            }
            pieces.Remove(king);
            for (int i = 0; i < pieces.Count; i++)
            {
                var piecemoves = GetMoves(pieces[i], state).ToArray();
                for (int j = 0; j < piecemoves.Length; j++)
                {
                    bool couldPromote = pieces[i].Type == PieceType.Pawn && (piecemoves[j].Y == 0 || piecemoves[j].Y == 7);
                    if (!WouldBeCheck(pieces[i], piecemoves[j], couldPromote ? PieceType.Queen : null, state))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Validates if the color of the piece to move fits the state
    /// </summary>
    public static bool IsMyTurn(Piece piece, State state)
    {
        ArgumentNullException.ThrowIfNull(piece);
        ArgumentNullException.ThrowIfNull(state);

        return (!state.Info.BlackToMove || piece.IsBlack)
         && (state.Info.BlackToMove || !piece.IsBlack);
    }
}
