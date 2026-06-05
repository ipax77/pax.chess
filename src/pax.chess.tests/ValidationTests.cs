using pax.chess.Validation;

namespace pax.chess.tests;

[TestClass]
public sealed class ValidationTests
{
    [TestMethod]
    public void CanIdentifyAttackedSquare()
    {
        var pos = BoardPosition.CreateInitial();
        var from1 = new Square(4, 1);
        var to1 = new Square(4, 3);
        var move1 = new Move(from1, to1);
        var newpos = pos.MakeMove(move1);
        var from2 = new Square(3, 6);
        var to2 = new Square(3, 4);
        var move2 = new Move(from2, to2);
        newpos = newpos.MakeMove(move2);

        var isAttacked = MoveValidator.IsSquareAttacked(to1, PieceColor.White, newpos);
        Assert.IsTrue(isAttacked);
    }

    [TestMethod]
    public void CanValidateMove()
    {
        var pos = BoardPosition.CreateInitial();
        var from1 = new Square(4, 1);
        var to1 = new Square(4, 3);
        var move1 = new Move(from1, to1);
        var moveState = MoveValidator.IsValidMove(move1, pos);
        Assert.AreEqual(MoveState.Ok, moveState);
    }

    [TestMethod]
    public void CanValidateWrongMove()
    {
        var pos = BoardPosition.CreateInitial();
        var from1 = new Square(4, 1);
        var to1 = new Square(4, 4);
        var move1 = new Move(from1, to1);
        var moveState = MoveValidator.IsValidMove(move1, pos);
        Assert.AreEqual(MoveState.TargetInvalid, moveState);
    }

    [TestMethod]
    public void PseudoValidator_ValidPawnDoubleMove()
    {
        var pos = BoardPosition.CreateInitial();
        var move = new Move(new Square(4, 1), new Square(4, 3));

        var moveState = PseudoMoveValidator.IsValidMove(move, pos);

        Assert.AreEqual(MoveState.Ok, moveState);
    }

    [TestMethod]
    public void PseudoValidator_InvalidPawnOverAdvance()
    {
        var pos = BoardPosition.CreateInitial();
        var move = new Move(new Square(4, 1), new Square(4, 4));

        var moveState = PseudoMoveValidator.IsValidMove(move, pos);

        Assert.AreEqual(MoveState.TargetInvalid, moveState);
    }

    [TestMethod]
    public void PseudoValidator_KnightValidAndInvalidTargets()
    {
        var pos = BoardPosition.CreateInitial();
        var validMove = new Move(new Square(1, 0), new Square(2, 2));
        var invalidMove = new Move(new Square(1, 0), new Square(1, 2));

        Assert.AreEqual(MoveState.Ok, PseudoMoveValidator.IsValidMove(validMove, pos));
        Assert.AreEqual(MoveState.TargetInvalid, PseudoMoveValidator.IsValidMove(invalidMove, pos));
    }

    [TestMethod]
    public void PseudoValidator_BishopBlockedPathAndCapture()
    {
        var from = new Square(2, 2);
        var to = new Square(5, 5);
        var blockedPos = CreatePosition(
            PieceColor.White,
            CastlingRights.None,
            null,
            (new Square(4, 0), new Piece(PieceType.King, PieceColor.White)),
            (new Square(4, 7), new Piece(PieceType.King, PieceColor.Black)),
            (from, new Piece(PieceType.Bishop, PieceColor.White)),
            (new Square(3, 3), new Piece(PieceType.Pawn, PieceColor.White)),
            (to, new Piece(PieceType.Pawn, PieceColor.Black)));
        var capturePos = CreatePosition(
            PieceColor.White,
            CastlingRights.None,
            null,
            (new Square(4, 0), new Piece(PieceType.King, PieceColor.White)),
            (new Square(4, 7), new Piece(PieceType.King, PieceColor.Black)),
            (from, new Piece(PieceType.Bishop, PieceColor.White)),
            (to, new Piece(PieceType.Pawn, PieceColor.Black)));
        var move = new Move(from, to, null, MoveType.Capture);

        Assert.AreEqual(MoveState.TargetInvalid, PseudoMoveValidator.IsValidMove(move, blockedPos));
        Assert.AreEqual(MoveState.Ok, PseudoMoveValidator.IsValidMove(move, capturePos));
    }

    [TestMethod]
    public void PseudoValidator_RookBlockedPathAndCapture()
    {
        var from = new Square(0, 0);
        var to = new Square(0, 7);
        var blockedPos = CreatePosition(
            PieceColor.White,
            CastlingRights.None,
            null,
            (new Square(4, 0), new Piece(PieceType.King, PieceColor.White)),
            (new Square(4, 7), new Piece(PieceType.King, PieceColor.Black)),
            (from, new Piece(PieceType.Rook, PieceColor.White)),
            (new Square(0, 3), new Piece(PieceType.Pawn, PieceColor.White)),
            (to, new Piece(PieceType.Pawn, PieceColor.Black)));
        var capturePos = CreatePosition(
            PieceColor.White,
            CastlingRights.None,
            null,
            (new Square(4, 0), new Piece(PieceType.King, PieceColor.White)),
            (new Square(4, 7), new Piece(PieceType.King, PieceColor.Black)),
            (from, new Piece(PieceType.Rook, PieceColor.White)),
            (to, new Piece(PieceType.Pawn, PieceColor.Black)));
        var move = new Move(from, to, null, MoveType.Capture);

        Assert.AreEqual(MoveState.TargetInvalid, PseudoMoveValidator.IsValidMove(move, blockedPos));
        Assert.AreEqual(MoveState.Ok, PseudoMoveValidator.IsValidMove(move, capturePos));
    }

    [TestMethod]
    public void PseudoValidator_QueenBlockedPathAndCapture()
    {
        var from = new Square(3, 3);
        var to = new Square(7, 7);
        var blockedPos = CreatePosition(
            PieceColor.White,
            CastlingRights.None,
            null,
            (new Square(4, 0), new Piece(PieceType.King, PieceColor.White)),
            (new Square(4, 7), new Piece(PieceType.King, PieceColor.Black)),
            (from, new Piece(PieceType.Queen, PieceColor.White)),
            (new Square(5, 5), new Piece(PieceType.Pawn, PieceColor.White)),
            (to, new Piece(PieceType.Pawn, PieceColor.Black)));
        var capturePos = CreatePosition(
            PieceColor.White,
            CastlingRights.None,
            null,
            (new Square(4, 0), new Piece(PieceType.King, PieceColor.White)),
            (new Square(4, 7), new Piece(PieceType.King, PieceColor.Black)),
            (from, new Piece(PieceType.Queen, PieceColor.White)),
            (to, new Piece(PieceType.Pawn, PieceColor.Black)));
        var move = new Move(from, to, null, MoveType.Capture);

        Assert.AreEqual(MoveState.TargetInvalid, PseudoMoveValidator.IsValidMove(move, blockedPos));
        Assert.AreEqual(MoveState.Ok, PseudoMoveValidator.IsValidMove(move, capturePos));
    }

    [TestMethod]
    public void PseudoValidator_WrongColorAndEmptySource()
    {
        var pos = BoardPosition.CreateInitial();

        Assert.AreEqual(
            MoveState.WrongColor,
            PseudoMoveValidator.IsValidMove(new Move(new Square(0, 6), new Square(0, 5)), pos));
        Assert.AreEqual(
            MoveState.PieceNotFound,
            PseudoMoveValidator.IsValidMove(new Move(new Square(0, 2), new Square(0, 3)), pos));
    }

    [TestMethod]
    public void PseudoValidator_CastlingBlockedByMissingRights()
    {
        var pos = CreatePosition(
            PieceColor.White,
            CastlingRights.None,
            null,
            (new Square(4, 0), new Piece(PieceType.King, PieceColor.White)),
            (new Square(7, 0), new Piece(PieceType.Rook, PieceColor.White)),
            (new Square(4, 7), new Piece(PieceType.King, PieceColor.Black)));
        var move = new Move(new Square(4, 0), new Square(6, 0), null, MoveType.CastlingKingSide);

        var moveState = PseudoMoveValidator.IsValidMove(move, pos);

        Assert.AreEqual(MoveState.CastleNotAllowed, moveState);
    }

    [TestMethod]
    public void PseudoValidator_CastlingBlockedByAttackedPath()
    {
        var pos = CreatePosition(
            PieceColor.White,
            CastlingRights.WhiteKingSide,
            null,
            (new Square(4, 0), new Piece(PieceType.King, PieceColor.White)),
            (new Square(7, 0), new Piece(PieceType.Rook, PieceColor.White)),
            (new Square(4, 7), new Piece(PieceType.King, PieceColor.Black)),
            (new Square(5, 7), new Piece(PieceType.Rook, PieceColor.Black)));
        var move = new Move(new Square(4, 0), new Square(6, 0), null, MoveType.CastlingKingSide);

        var moveState = PseudoMoveValidator.IsValidMove(move, pos);

        Assert.AreEqual(MoveState.CastlingPathAttacked, moveState);
    }

    [TestMethod]
    public void PseudoValidator_PinnedMoveWouldBeCheck()
    {
        var pos = CreatePosition(
            PieceColor.White,
            CastlingRights.None,
            null,
            (new Square(4, 0), new Piece(PieceType.King, PieceColor.White)),
            (new Square(6, 7), new Piece(PieceType.King, PieceColor.Black)),
            (new Square(4, 1), new Piece(PieceType.Rook, PieceColor.White)),
            (new Square(4, 7), new Piece(PieceType.Rook, PieceColor.Black)));
        var move = new Move(new Square(4, 1), new Square(5, 1));

        var moveState = PseudoMoveValidator.IsValidMove(move, pos);

        Assert.AreEqual(MoveState.WouldBeCheck, moveState);
    }

    [TestMethod]
    public void PseudoValidator_KingSafetyDetectsEveryAttackShape()
    {
        var kingFrom = new Square(4, 0);
        var kingTo = new Square(4, 1);
        var move = new Move(kingFrom, kingTo);
        var testCases = new[]
        {
            CreatePosition(
                PieceColor.White,
                CastlingRights.None,
                null,
                (kingFrom, new Piece(PieceType.King, PieceColor.White)),
                (new Square(0, 7), new Piece(PieceType.King, PieceColor.Black)),
                (new Square(3, 2), new Piece(PieceType.Pawn, PieceColor.Black))),
            CreatePosition(
                PieceColor.White,
                CastlingRights.None,
                null,
                (kingFrom, new Piece(PieceType.King, PieceColor.White)),
                (new Square(0, 7), new Piece(PieceType.King, PieceColor.Black)),
                (new Square(2, 2), new Piece(PieceType.Knight, PieceColor.Black))),
            CreatePosition(
                PieceColor.White,
                CastlingRights.None,
                null,
                (kingFrom, new Piece(PieceType.King, PieceColor.White)),
                (new Square(3, 2), new Piece(PieceType.King, PieceColor.Black))),
            CreatePosition(
                PieceColor.White,
                CastlingRights.None,
                null,
                (kingFrom, new Piece(PieceType.King, PieceColor.White)),
                (new Square(0, 7), new Piece(PieceType.King, PieceColor.Black)),
                (new Square(4, 7), new Piece(PieceType.Rook, PieceColor.Black))),
            CreatePosition(
                PieceColor.White,
                CastlingRights.None,
                null,
                (kingFrom, new Piece(PieceType.King, PieceColor.White)),
                (new Square(0, 7), new Piece(PieceType.King, PieceColor.Black)),
                (new Square(7, 4), new Piece(PieceType.Bishop, PieceColor.Black))),
            CreatePosition(
                PieceColor.White,
                CastlingRights.None,
                null,
                (kingFrom, new Piece(PieceType.King, PieceColor.White)),
                (new Square(0, 7), new Piece(PieceType.King, PieceColor.Black)),
                (new Square(7, 1), new Piece(PieceType.Queen, PieceColor.Black)))
        };

        foreach (var pos in testCases)
            Assert.AreEqual(MoveState.WouldBeCheck, PseudoMoveValidator.IsValidMove(move, pos));
    }

    [TestMethod]
    public void PseudoValidator_EnPassantExposesKingAttack()
    {
        var pos = FenSerializer.Parse("4k3/8/8/r2pP2K/8/8/8/8 w - d6 0 1");
        var move = new Move(new Square(4, 4), new Square(3, 5), null, MoveType.EnPassant);

        var expected = MoveValidator.IsValidMove(move, pos);
        var actual = PseudoMoveValidator.IsValidMove(move, pos);

        Assert.AreEqual(MoveState.WouldBeCheck, actual);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void PseudoValidator_PromotionCaptureStillBlocksSlidingAttack()
    {
        var pos = FenSerializer.Parse("r4b1K/6P1/8/8/8/8/8/4k3 w - - 0 1");
        var move = new Move(new Square(6, 6), new Square(5, 7), PieceType.Queen, MoveType.Capture | MoveType.Promotion);

        var expected = MoveValidator.IsValidMove(move, pos);
        var actual = PseudoMoveValidator.IsValidMove(move, pos);

        Assert.AreEqual(MoveState.Ok, actual);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void PseudoValidator_MatchesMoveValidatorForRepresentativeMoves()
    {
        var testCases = new[]
        {
            (Position: BoardPosition.CreateInitial(), Move: new Move(new Square(4, 1), new Square(4, 3))),
            (Position: BoardPosition.CreateInitial(), Move: new Move(new Square(4, 1), new Square(4, 4))),
            (Position: BoardPosition.CreateInitial(), Move: new Move(new Square(1, 0), new Square(2, 2))),
            (Position: FenSerializer.Parse("4k3/8/8/8/8/8/4R3/4K3 w - - 0 1"), Move: new Move(new Square(4, 1), new Square(5, 1)))
        };

        foreach (var testCase in testCases)
        {
            var expected = MoveValidator.IsValidMove(testCase.Move, testCase.Position);
            var actual = PseudoMoveValidator.IsValidMove(testCase.Move, testCase.Position);

            Assert.AreEqual(expected, actual);
        }
    }

    [TestMethod]
    public void PseudoValidator_GetGameStateDoesNotMutatePosition()
    {
        var pos = FenSerializer.Parse("r3k2r/ppp2ppp/2n5/3Pp3/8/2N5/PPP2PPP/R3K2R w KQkq e6 0 12");
        string before = FenSerializer.Serialize(pos);

        _ = PseudoMoveValidator.GetGameState(pos);

        Assert.AreEqual(before, FenSerializer.Serialize(pos));
    }

    [TestMethod]
    public void PseudoValidator_GetGameStateMatchesMoveValidator()
    {
        var pinnedPosition = CreatePosition(
            PieceColor.White,
            CastlingRights.None,
            null,
            (new Square(4, 0), new Piece(PieceType.King, PieceColor.White)),
            (new Square(6, 7), new Piece(PieceType.King, PieceColor.Black)),
            (new Square(4, 1), new Piece(PieceType.Rook, PieceColor.White)),
            (new Square(4, 7), new Piece(PieceType.Rook, PieceColor.Black)));
        var castlingAvailablePosition = CreatePosition(
            PieceColor.White,
            CastlingRights.WhiteKingSide,
            null,
            (new Square(4, 0), new Piece(PieceType.King, PieceColor.White)),
            (new Square(7, 0), new Piece(PieceType.Rook, PieceColor.White)),
            (new Square(4, 7), new Piece(PieceType.King, PieceColor.Black)));
        var castlingPathAttackedPosition = CreatePosition(
            PieceColor.White,
            CastlingRights.WhiteKingSide,
            null,
            (new Square(4, 0), new Piece(PieceType.King, PieceColor.White)),
            (new Square(7, 0), new Piece(PieceType.Rook, PieceColor.White)),
            (new Square(4, 7), new Piece(PieceType.King, PieceColor.Black)),
            (new Square(5, 7), new Piece(PieceType.Rook, PieceColor.Black)));
        var enPassantPinnedPosition = FenSerializer.Parse("4r2k/8/8/3pP3/8/8/8/4K3 w - d6 0 1");

        var testCases = new[]
        {
            BoardPosition.CreateInitial(),
            FenSerializer.Parse("2r3k1/6pp/p3pp1B/2bn4/2pK3P/3b1PR1/P7/3R4 w - - 2 31"),
            FenSerializer.Parse("5k2/5P2/5K2/8/8/8/8/8 b - - 0 1"),
            FenSerializer.Parse("4k3/8/4r3/8/8/8/8/4K3 w - - 0 1"),
            pinnedPosition,
            castlingAvailablePosition,
            castlingPathAttackedPosition,
            enPassantPinnedPosition
        };

        foreach (var pos in testCases)
        {
            var expected = MoveValidator.GetGameState(pos);
            var actual = PseudoMoveValidator.GetGameState(pos);

            Assert.AreEqual(expected, actual);
        }
    }

    [TestMethod]
    public void Fen_Checkmate()
    {
        string fen = "2r3k1/6pp/p3pp1B/2bn4/2pK3P/3b1PR1/P7/3R4 w - - 2 31";
        var pos = FenSerializer.Parse(fen);
        var result = MoveValidator.GetGameState(pos);
        Assert.AreEqual(GameState.Checkmate, result);
    }
    
    [TestMethod]
    public void Fen_Stalemate()
    {
        // Classic stalemate - black king has no legal moves but is not in check
        string fen = "5k2/5P2/5K2/8/8/8/8/8 b - - 0 1";
        var pos = FenSerializer.Parse(fen);
        var result = MoveValidator.GetGameState(pos);
        Assert.AreEqual(GameState.Stalemate, result);
    }

    [TestMethod]
    public void Fen_Check()
    {
        // King is in check but has escape moves
        string fen = "4k3/8/4r3/8/8/8/8/4K3 w - - 0 1";
        var pos = FenSerializer.Parse(fen);
        var result = MoveValidator.GetGameState(pos);
        Assert.AreEqual(GameState.Check, result);
    }

    [TestMethod]
    public void Fen_Normal()
    {
        // Starting position - nothing special
        string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        var pos = FenSerializer.Parse(fen);
        var result = MoveValidator.GetGameState(pos);
        Assert.AreEqual(GameState.Normal, result);
    }

    private static BoardPosition CreatePosition(
        PieceColor sideToMove,
        CastlingRights castlingRights,
        Square? enPassantTarget,
        params (Square Square, Piece Piece)[] pieces)
    {
        var board = new Board();

        foreach (var (square, piece) in pieces)
            board[square.Index] = piece;

        return new BoardPosition(
            board,
            sideToMove,
            castlingRights,
            enPassantTarget,
            halfmoveClock: 0,
            fullmoveNumber: 1);
    }
}
