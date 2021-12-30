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
        public void Fen()
        {
            string fen = "2r3k1/6pp/p3pp1B/2bn4/2pK3P/3b1PR1/P7/3R4 w - - 2 31";
            Game game = new Game(fen);
            Assert.True(game.State.Info.IsCheckMate);
        }

    }
}