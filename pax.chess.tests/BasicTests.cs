using System.Linq;
using Xunit;

namespace pax.chess.tests
{
    public class BasicTests
    {
        [Fact]
        public void CheckMate1()
        {
            string pgn = "1. f4 e6 2. g4 Qh4#";
            Game game = Pgn.MapString(pgn);
            Assert.True(game.State.Info.IsCheckMate);
        }

        [Fact]
        public void CheckMate2()
        {
            string pgn = "1. e4 e5 2. Bc4 Bc5 3. Qh5 Nf6 4. Qxf7#";
            Game game = Pgn.MapString(pgn);
            Assert.True(game.State.Info.IsCheckMate);
        }

        [Fact]
        public void Check1()
        {
            string pgn = "1. f4 e6 2. Nc3 Qh4+";
            Game game = Pgn.MapString(pgn);
            Assert.True(game.State.Info.IsCheck);
            Assert.False(game.State.Info.IsCheckMate);
        }

        [Fact]
        public void Check2()
        {
            string pgn = "1. e4 d5 2. exd5 e6 3. dxe6 Qe7 4. Nc3 Bxe6 5. b3 Bxb3+";
            Game game = Pgn.MapString(pgn);
            Assert.True(game.State.Info.IsCheck);
            Assert.False(game.State.Info.IsCheckMate);
        }

        [Fact]
        public void WouldBeCheck1()
        {
            string pgn = "1. e4 d5 2. exd5 e6 3. dxe6 Qe7 4. Nc3 Bxe6 5. b3 Bxb3+";
            Game game = Pgn.MapString(pgn);
            var state = game.Move(new EngineMove(new Position(0, 1), new Position(1, 2)));
            Assert.True(state == MoveState.WouldBeCheck);
        }

        [Fact]
        public void WouldBeCheck2()
        {
            string pgn = "1. e4 d5 2. exd5 e6 3. dxe6 Qe7 4. Nc3 Bxe6 5. b3 Bxb3+ 6. Qe2 Nf6";
            Game game = Pgn.MapString(pgn);
            var state = game.Move(new EngineMove(new Position(4, 1), new Position(1, 4)));
            Assert.True(state == MoveState.WouldBeCheck);
        }

        [Fact]
        public void Castle1()
        {
            string pgn = "1. g4 h5 2. gxh5 Rxh5 3. Nf3 Rh6 4. Bh3 Rg6";
            Game game = Pgn.MapString(pgn);
            var state = game.Move(new EngineMove(new Position(4, 0), new Position(6, 0)));
            Assert.True(state == MoveState.CastleNotAllowed);
        }

        [Fact]
        public void Castle2()
        {
            string pgn = "1. g4 h5 2. gxh5 Rxh5 3. Nf3 Rh6 4. Bh3 Rg6 5. Nd4 Rf6 6. f4 Rxf4";
            Game game = Pgn.MapString(pgn);
            var state = game.Move(new EngineMove(new Position(4, 0), new Position(6, 0)));
            Assert.True(state == MoveState.CastleNotAllowed);
        }

        [Fact]
        public void Castle3()
        {
            string pgn = "1. c4 c6 2. Qc2 d6 3. Nf3 Bg4 4. Nc3 Na6 5. e3 c5 6. Be2 Qc7 7. Qa4+";
            Game game = Pgn.MapString(pgn);
            var state = game.Move(new EngineMove(new Position(4, 7), new Position(2, 7)));
            Assert.True(state == MoveState.CastleNotAllowed);
        }

        [Fact]
        public void PawnMoves1()
        {
            string pgn = "1. c4 c6 2. Qc2 d6 3. Nf3 Bg4 4. Nc3 Na6 5. e3 c5 6. Be2 Qc7 7. Qa4+ Bd7 8. Qb3 O-O-O 9. O-O Nf6 10. Ng5 Ng8 11. Nge4 Nh6 12. Ng3 Nf5 13. Nh5 Nh6 14. Nf4 Ng8 15. Nh3 Nb4 16. Bf3 Na6 17. Be2 Nb8 18. Nd1 Be8";
            Game game = Pgn.MapString(pgn);
            Assert.True(game.State.Info.PawnHalfMoveClock == 26);
        }

        [Fact]
        public void PawnMoves2()
        {
            string pgn = "1. c4 c6 2. Qc2 d6 3. Nf3 Bg4 4. Nc3 Na6 5. e3 c5 6. Be2 Qc7 7. Qa4+ Bd7 8. Qb3 O-O-O 9. O-O Nf6 10. Ng5 Ng8 11. Nge4 Nh6 12. Ng3 Nf5 13. Nh5 Nh6 14. Nf4 Ng8 15. Nh3 Nb4 16. Bf3 Na6 17. Be2 Nb8 18. Nd1 Be8 19. Ng5 Bd7 20. Nxf7";
            Game game = Pgn.MapString(pgn);
            Assert.True(game.State.Info.PawnHalfMoveClock == 0);
        }

        [Fact]
        public void Fen_Checkmate()
        {
            string fen = "2r3k1/6pp/p3pp1B/2bn4/2pK3P/3b1PR1/P7/3R4 w - - 2 31";
            Game game = new Game(fen);
            Assert.True(game.State.Info.IsCheckMate);
        }

        [Fact]
        public void Fen_FindPiece()
        {
            string fen = "rnbqkbnr/pp2pppp/3p4/2p5/8/1P3P2/P1PPP1PP/RNBQKBNR w KQkq c5 0 2";
            Game game = new Game(fen);
            var pawn = game.State.Pieces.FirstOrDefault(p => p.IsBlack && p.Position == new Position(2, 4));
            Assert.NotNull(pawn);
        }

        [Fact]
        public void Fen_NotCheckmate()
        {
            string fen = "rnbqkbnr/pp2pppp/3p4/2p5/8/1P3P2/P1PPP1PP/RNBQKBNR w KQkq - 0 2";
            Game game = new Game(fen);
            Assert.False(game.State.Info.IsCheckMate);
        }

        [Fact]
        public void Fen_InitialPosition()
        {
            string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            Game game = new Game(fen);
            Assert.Equal(32, game.State.Pieces.Count);
        }

        [Fact]
        public void Fen_EnPassant()
        {
            string fen = "rnbqkb1r/p2p1ppp/2P2n2/4p3/Pp2P3/5N2/1PP2PPP/RNBQKB1R b KQkq a3 0 6";
            Game game = new Game(fen);
            var pawn = game.State.Pieces.FirstOrDefault(p => !p.IsBlack && p.Position == new Position(0, 3));
            Assert.NotNull(pawn);
        }
    }
}