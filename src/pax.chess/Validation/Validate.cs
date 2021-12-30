namespace pax.chess.Validation;
public partial class Validate
{
    public static MoveState TryExecuteMove(EngineMove engineMove, State state, PieceType? transformation = null)
    {
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

    public static bool IsValidCastle(Piece king, EngineMove engineMove, State state)
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
            (true, true) => new Position[2] { new Position(5, 7), new Position(6, 7) },
            (true, false) => new Position[2] { new Position(2, 7), new Position(3, 7) },
            (false, true) => new Position[2] { new Position(5, 0), new Position(6, 0) },
            (false, false) => new Position[2] { new Position(2, 0), new Position(3, 0) },
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

    //public static bool IsCheck(State state)
    //{
    //    var possibleAttacers = state.Pieces.Where(x => x.IsBlack == state.Info.BlackToMove && x.Type != PieceType.King).ToList();
    //    var king = state.Pieces.Single(s => s.Type == PieceType.King && s.IsBlack != state.Info.BlackToMove);

    //    for (int i = 0; i < possibleAttacers.Count; i++)
    //    {
    //        var moves = GetMoves(possibleAttacers[i], state);
    //        if (moves.Contains(king.Position))
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    public static bool WouldBeCheck(Piece piece, Position destination, PieceType? transformation, State state)
    {
        EngineMove testMove = new EngineMove(piece.Position, destination, transformation);
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

    public static bool IsCheckMate(State state)
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
                var piecemoves = Validate.GetMoves(pieces[i], state, PieceType.Queen);
                for (int j = 0; j < piecemoves.Count; j++)
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

    public static bool IsMyTurn(Piece piece, State state)
    {
        if ((state.Info.BlackToMove && !piece.IsBlack)
         || (!state.Info.BlackToMove && piece.IsBlack)
        )
        {
            return false;
        }
        return true;
    }
}
