using pax.chess.Validation;

namespace pax.chess.mstests
{
    [TestClass]
    public class BasicTests
    {
        [TestMethod]
        public void CreateTest()
        {
            ChessBoard board1 = new();
            ChessBoard board2 = new("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            Assert.IsTrue(board1.Pieces.SequenceEqual(board2.Pieces));
        }

        [TestMethod]
        public void MoveTest()
        {
            ChessBoard board = new();

            var moveState = Validate.ValidateBoardMove(board, new(0, 1), new(0, 2));
            Assert.AreEqual(MoveState.Ok, moveState);
        }

        [TestMethod]
        public void PieceNotFoundTest()
        {
            ChessBoard board = new();

            var moveState = Validate.ValidateBoardMove(board, new Position(1, 4), new Position(1, 2));
            Assert.AreEqual(MoveState.PieceNotFound, moveState);
        }

        [TestMethod]
        public void WrongColorTest()
        {
            ChessBoard board = new();
            var moveState = Validate.ValidateBoardMove(board, new Position(0, 6), new Position(0, 5));
            Assert.AreEqual(MoveState.WrongColor, moveState);
        }

        [TestMethod]
        public void TargetInvalidTest()
        {
            ChessBoard board = new();

            var moveState = Validate.ValidateBoardMove(board, new Position(0, 1), new Position(0, 4));
            Assert.AreEqual(MoveState.TargetInvalid, moveState);
        }

        [TestMethod]
        public void OutOfBoundsTest()
        {
            ChessBoard board = new();

            var moveState = Validate.ValidateBoardMove(board, new Position(-1, 1), new Position(0, 2));
            Assert.AreEqual(MoveState.OutOfBounds, moveState);
        }

        [TestMethod]
        public void InvalidInputTest()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action act = () => Validate.ValidateBoardMove(null, new Position(0, 1), new Position(0, 2));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            Assert.ThrowsException<ArgumentNullException>(act);
        }

        [TestMethod]
        public void EnPassantMoveTest()
        {
            ChessBoard board = new("rnbqkbnr/ppp1p1pp/8/3pPp2/8/8/PPP2PPP/RNBQKBNR w KQkq d6 0 4");

            var moveState = Validate.ValidateBoardMove(board, new Position(4, 4), new Position(3, 5));
            Assert.AreEqual(MoveState.Ok, moveState);
        }

        [TestMethod]
        public void WouldBeCheck_PutKingInCheck_Test()
        {
            // Arrange
            ChessBoard board = new("rnbqkbnr/ppp2ppp/8/4P3/4p3/8/PPP2PPP/RNBQKBNR w KQkq - 0 4");

            // Act
            bool result = Validate.WouldBeCheck(board, new Position(4, 0), new Position(3, 1));

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void WouldBeCheck_NotPutKingInCheck_Test()
        {
            // Arrange
            ChessBoard board = new("rnbQkbnr/ppp2ppp/8/4P3/4p3/8/PPP2PPP/RNB1KBNR b KQkq - 0 4");

            // Act
            bool result = Validate.WouldBeCheck(board, new Position(4, 7), new Position(3, 7));

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void WouldBeCheck_CanCaste_Test()
        {
            // Arrange
            ChessBoard board = new("r1bqk1nr/pppnbp2/8/4P1P1/2B1p3/8/PPP2PP1/RNBQK2R w KQkq - 1 9");

            // Act
            bool result = Validate.WouldBeCheck(board, new Position(4, 0), new Position(6, 0));

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void WouldBeCheck_CannotCaste_Test1()
        {
            // Arrange
            ChessBoard board = new("r1bqk3/pppnnp2/5b2/1N2P1r1/4p3/1B6/PPP2P2/R1BQK2R w KQq - 0 14");

            // Act
            bool result = Validate.WouldBeCheck(board, new Position(4, 0), new Position(6, 0));

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void WouldBeCheck_CannotCaste_Test2()
        {
            // Arrange
            ChessBoard board = new("r1bqk3/pppnnp2/5b2/4P3/4pr2/1BN5/PPP5/R1BQK2R w KQq - 0 16");

            // Act
            bool result = Validate.WouldBeCheck(board, new Position(4, 0), new Position(6, 0));

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void WouldBeCheck_CannotCaste_Test3()
        {
            // Arrange
            ChessBoard board = new("r2qkbnr/pb3ppp/1pn5/4p3/Q3p2B/2N2N2/PP2BPPP/R3K2R w KQkq - 0 10");

            // Act
            bool result = Validate.WouldBeCheck(board, new Position(4, 0), new Position(2, 0));

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void WouldBeCheck_CannotCaste_Test4()
        {
            // Arrange
            ChessBoard board = new("r3kbnr/pb3ppp/1p6/4p3/Q2np2N/2N5/PP2BPPP/R3K2R b KQkq - 2 12");

            // Act
            bool result = Validate.WouldBeCheck(board, new Position(4, 7), new Position(2, 7));

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsCheckTest()
        {
            // Arrange
            ChessBoard board = new("r1bqkbnr/pppQ1ppp/2n5/4p3/4P3/8/PPPP1PPP/RNB1KBNR b KQkq - 0 3");

            // Act
            bool result = Validate.IsCheck(board);

            // Assert
            Assert.IsTrue(result);
        }

        [DataTestMethod]
        [DataRow("1k6/8/8/8/8/8/PPP5/1K3r2 w - - 0 1")]
        [DataRow("r1b2q2/3k1p2/pP2p3/3pB1Q1/B2P4/P3P3/5PPP/1R3K2 b - - 0 29")]
        [DataRow("kbK5/pP6/p7/8/8/8/8/8 b - - 0 2")]
        [DataRow("r1bqkb1r/pppp1Qpp/2n2n2/4p3/2B1P3/8/PPPP1PPP/RNB1K1NR b KQkq - 0 4")]
        public void IsCheckMateTest(string fen)
        {
            // Arrange
            ChessBoard board = new ChessBoard(fen);

            // Act and Assert
            Assert.IsTrue(board.IsCheck);
            Assert.IsTrue(board.IsCheckMate);
        }

        [TestMethod]
        public void EnPassantTest()
        {
            // Arrange
            ChessBoard board = new();

            // Act
            board.Move(new(4, 1), new(4, 3));
            board.Move(new(0, 6), new(0, 5));
            board.Move(new(4, 3), new(4, 4));
            var result = board.Move(new(3, 6), new(3, 4));

            // Assert
            Assert.AreEqual(MoveState.Ok, result);
            Assert.AreEqual(new Position(3, 5), board.EnPassantPosition);
        }

        [TestMethod]
        public void EnPassantCaptureTest()
        {
            // Arrange
            ChessBoard board = new("rnbqkbnr/1pp1pppp/p7/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 3");

            // Act
            var result = board.Move(new(4, 4), new(3, 5));

            // Assert
            Assert.AreEqual(MoveState.Ok, result);
            Assert.AreEqual(null, board.EnPassantPosition);
            var capture = board.GetPieceAt(new(3, 4));
            Assert.AreEqual(null, capture);
        }

        [TestMethod]
        public void EnPassantCheckTest()
        {
            // Arrange
            ChessBoard board = new("r1b2q2/3k1p2/p3p3/1pPpB1Q1/B2P4/P3P3/5PPP/1R3K2 w - b6 0 29");

            // Act
            var result = board.Move(new(2, 4), new(1, 5));
            var isCheck = Validate.IsCheck(board);

            // Assert
            Assert.AreEqual(MoveState.Ok, result);
            Assert.IsTrue(isCheck);
        }

        [TestMethod]
        public void NotUniqueMoveTest1()
        {
            // Arrange
            ChessBoard board = new("r1bq1rk1/ppppbppp/2n2n2/4p3/4P3/2P5/PPNPNPPP/R1BQKB1R w KQ - 3 6");

            // Act
            var result = board.Move(new(2, 1), new(3, 3));
            var move = board.Moves.LastOrDefault();

            // Assert
            Assert.AreEqual(MoveState.Ok, result);
            Assert.IsNotNull(move);
            Assert.IsTrue(move.IsNotUnique);
        }

        [TestMethod]
        public void NotUniqueMoveTest2()
        {
            // Arrange
            ChessBoard board = new("rn1qkbnr/pppbpppp/8/3p4/2P1P3/8/PP1P1PPP/RNBQKBNR w KQkq - 1 3");

            // Act
            var result = board.Move(new(2, 3), new(3, 4));
            var move = board.Moves.LastOrDefault();

            // Assert
            Assert.AreEqual(MoveState.Ok, result);
            Assert.IsNotNull(move);
            Assert.IsTrue(move.IsNotUnique);
        }

        [TestMethod]
        public void UniqueMoveTest()
        {
            // Arrange
            ChessBoard board = new("1k4q1/8/8/8/6P1/8/6q1/1K6 b - - 0 1");

            // Act
            var result = board.Move(new(6, 7), new(6, 5));
            var move = board.Moves.LastOrDefault();

            // Assert
            Assert.AreEqual(MoveState.Ok, result);
            Assert.IsNotNull(move);
            Assert.IsFalse(move.IsNotUnique);
        }

        [TestMethod]
        public void RevertMoveTest()
        {
            // Arrange
            ChessBoard board1 = new("r1bqkb1r/pppp1ppp/2n2n2/4p2Q/2B1P3/8/PPPP1PPP/RNB1K1NR w KQkq - 4 4");
            ChessBoard board2 = new("r1bqkb1r/pppp1ppp/2n2n2/4p2Q/2B1P3/8/PPPP1PPP/RNB1K1NR w KQkq - 4 4");
            // Act
            var result = board1.Move(new(7, 4), new(5, 6));

            // Assert
            Assert.AreEqual(MoveState.Ok, result);
            Assert.IsTrue(board1.IsCheckMate);

            board1.RevertMove();
            Assert.IsTrue(board1.Pieces.SequenceEqual(board2.Pieces));
        }

        [TestMethod]
        public void RevertEnPassantMoveTest()
        {
            // Arrange
            ChessBoard board1 = new("rnbqkbnr/1pp1pppp/p7/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 3");
            ChessBoard board2 = new("rnbqkbnr/1pp1pppp/p7/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 3");
            // Act
            var result = board1.Move(new(4, 4), new(3, 5));

            // Assert
            Assert.AreEqual(MoveState.Ok, result);

            board1.RevertMove();
            Assert.IsTrue(board1.Pieces.SequenceEqual(board2.Pieces));
        }
    }
}