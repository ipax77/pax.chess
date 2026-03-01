namespace pax.chess.tests;

[TestClass]
public sealed class PgnTests
{
    [TestMethod]
    public void CanParseSimplePgn()
    {
        string pgn = "1. f4 e6 2. g4 Qh4#";
        var game = PgnSerializer.Parse(pgn);

        var moves = game.Moves;
        Assert.HasCount(4, moves);
    }

    [TestMethod]
    public void CanParseLichess()
    {
        var pgn = @"[Event ""rated blitz game""]
[Site ""https://lichess.org/SZf3wQqU""]
[Date ""2026.02.27""]
[Round ""-""]
[White ""pax77""]
[Black ""Trollernot""]
[Result ""1-0""]
[GameId ""SZf3wQqU""]
[UTCDate ""2026.02.27""]
[UTCTime ""12:13:04""]
[WhiteElo ""2062""]
[BlackElo ""1999""]
[WhiteRatingDiff ""+4""]
[BlackRatingDiff ""-4""]
[Variant ""Standard""]
[TimeControl ""180+2""]
[ECO ""B23""]
[Opening ""Sicilian Defense: Closed, Chameleon Variation""]
[Termination ""Normal""]

1. e4 c5 2. Ne2 Nc6 3. Nbc3 Nf6 4. g3 d6 5. Bg2 g6 6. d3 Bg7 7. h3 O-O 8. O-O a6 9. f4 b5 10. Be3 Bb7 11. Qd2 Qb6 12. Rae1 a5 13. Kh2 a4 14. Nd5 Nxd5 15. exd5 Nd4 16. c3 Nxe2 17. Rxe2 Rfc8 18. f5 b4 19. Bh6 bxc3 20. bxc3 a3 21. Bxg7 Kxg7 22. Qg5 Re8 23. f6+ Kg8 24. fxe7 f5 25. Rxf5 1-0
";
        var game = PgnSerializer.Parse(pgn);

        var moves = game.Moves;
        Assert.HasCount(49, moves);
    }

    [TestMethod]
    public void CanParseLichessCommented()
    {
        var pgn = @"[Event ""rated blitz game""]
[Site ""https://lichess.org/SZf3wQqU""]
[Date ""2026.02.27""]
[Round ""-""]
[White ""pax77""]
[Black ""Trollernot""]
[Result ""1-0""]
[GameId ""SZf3wQqU""]
[UTCDate ""2026.02.27""]
[UTCTime ""12:13:04""]
[WhiteElo ""2062""]
[BlackElo ""1999""]
[WhiteRatingDiff ""+4""]
[BlackRatingDiff ""-4""]
[Variant ""Standard""]
[TimeControl ""180+2""]
[ECO ""B23""]
[Opening ""Sicilian Defense: Closed, Chameleon Variation""]
[Termination ""Normal""]
[Annotator ""lichess.org""]

1. e4 { [%eval 0.18] [%clk 0:03:00] } 1... c5 { [%eval 0.25] [%clk 0:03:00] } 2. Ne2 { [%eval 0.0] [%clk 0:03:01] } 2... Nc6 { [%eval 0.32] [%clk 0:03:00] } 3. Nbc3 { [%eval 0.19] [%clk 0:03:01] } { B23 Sicilian Defense: Closed, Chameleon Variation } 3... Nf6 { [%eval 0.41] [%clk 0:03:01] } 4. g3 { [%eval 0.0] [%clk 0:03:01] } 4... d6 { [%eval 0.07] [%clk 0:02:58] } 5. Bg2 { [%eval 0.0] [%clk 0:03:02] } 5... g6 { [%eval 0.0] [%clk 0:03:00] } 6. d3 { [%eval 0.0] [%clk 0:03:01] } 6... Bg7 { [%eval -0.02] [%clk 0:03:01] } 7. h3 { [%eval 0.0] [%clk 0:03:02] } 7... O-O { [%eval 0.0] [%clk 0:03:02] } 8. O-O { [%eval 0.11] [%clk 0:03:01] } 8... a6 { [%eval 0.13] [%clk 0:03:03] } 9. f4 { [%eval 0.09] [%clk 0:03:02] } 9... b5? { (0.09 → 1.74) Mistake. Nd7 was best. } { [%eval 1.74] [%clk 0:03:04] } (9... Nd7 10. a4 e6 11. Be3 Nd4 12. Bd2 b6 13. Nxd4 cxd4 14. Ne2 Re8 15. f5) 10. Be3?? { (1.74 → -0.30) Blunder. e5 was best. } { [%eval -0.3] [%clk 0:02:56] } (10. e5 dxe5 11. Bxc6 Bxh3 12. fxe5 Nd7 13. e6 Bxf1 14. Bxa8 Bh3 15. Nf4 Qxa8) 10... Bb7 { [%eval -0.18] [%clk 0:03:04] } 11. Qd2 { [%eval -0.38] [%clk 0:02:56] } 11... Qb6 { [%eval 0.07] [%clk 0:02:57] } 12. Rae1 { [%eval 0.05] [%clk 0:02:54] } 12... a5 { [%eval 0.49] [%clk 0:02:52] } 13. Kh2?! { (0.49 → -0.11) Inaccuracy. g4 was best. } { [%eval -0.11] [%clk 0:02:52] } (13. g4 b4 14. Nd5 Qd8 15. c3 e6 16. Nxf6+ Bxf6 17. f5 Ne5 18. Bh6) 13... a4 { [%eval 0.0] [%clk 0:02:50] } 14. Nd5 { [%eval 0.0] [%clk 0:02:38] } 14... Nxd5 { [%eval 0.0] [%clk 0:02:44] } 15. exd5 { [%eval 0.04] [%clk 0:02:40] } 15... Nd4 { [%eval -0.05] [%clk 0:02:32] } 16. c3 { [%eval -0.19] [%clk 0:02:37] } 16... Nxe2 { [%eval 0.28] [%clk 0:02:26] } 17. Rxe2 { [%eval 0.01] [%clk 0:02:39] } 17... Rfc8?? { (0.01 → 2.00) Blunder. e6 was best. } { [%eval 2.0] [%clk 0:02:07] } (17... e6 18. dxe6 fxe6 19. Bxb7 Qxb7 20. d4 Qc6 21. Bf2 a3 22. Rxe6 axb2 23. Qxb2) 18. f5 { [%eval 1.73] [%clk 0:02:37] } 18... b4 { [%eval 2.33] [%clk 0:02:07] } 19. Bh6 { [%eval 2.45] [%clk 0:02:10] } 19... bxc3 { [%eval 2.59] [%clk 0:01:50] } 20. bxc3 { [%eval 2.64] [%clk 0:02:11] } 20... a3? { (2.64 → 4.44) Mistake. Be5 was best. } { [%eval 4.44] [%clk 0:01:48] } (20... Be5 21. fxg6 hxg6 22. Qg5 Qd8 23. Be4 e6 24. dxe6 Qxg5 25. exf7+ Kh7 26. Bxg5) 21. Bxg7 { [%eval 4.13] [%clk 0:02:04] } 21... Kxg7 { [%eval 4.9] [%clk 0:01:49] } 22. Qg5?! { (4.90 → 3.70) Inaccuracy. Rxe7 was best. } { [%eval 3.7] [%clk 0:02:04] } (22. Rxe7 Qb2 23. Qg5 Bxd5 24. f6+ Kh8 25. Qxd5 Rf8 26. h4 Qd2 27. Qg5 Qxa2) 22... Re8?! { (3.70 → 5.04) Inaccuracy. h6 was best. } { [%eval 5.04] [%clk 0:01:05] } (22... h6 23. Qxe7 Rc7 24. Qh4 Rd7 25. Re7 Qc7 26. f6+ Kh7 27. Rfe1 Rf8 28. Rxd7) 23. f6+ { [%eval 5.21] [%clk 0:02:00] } 23... Kg8 { [%eval 5.48] [%clk 0:00:53] } 24. fxe7 { [%eval 5.05] [%clk 0:01:54] } 24... f5 { [%eval 5.47] [%clk 0:00:35] } 25. Rxf5 { [%eval 4.89] [%clk 0:01:51] } { Black resigns. } 1-0
";
        var game = PgnSerializer.Parse(pgn);

        var moves = game.Moves;
        Assert.HasCount(49, moves);
    }

    [TestMethod]
    public void CanSerializeSimplePgn()
    {
        string pgn = "1. f4 e6 2. g4 Qh4#";
        var game = PgnSerializer.Parse(pgn);
        game.Metadata.White = "WhitePlayer";
        game.Metadata.Black = "BlackPlayer";
        
        var serialized = PgnSerializer.Serialize(game);
        
        Assert.Contains("[White \"WhitePlayer\"]", serialized);
        Assert.Contains("[Black \"BlackPlayer\"]", serialized);
        Assert.Contains("1. f4 e6 2. g4 Qh4#", serialized);
        
        var reParsed = PgnSerializer.Parse(serialized);
        Assert.HasCount(game.Moves.Count, reParsed.Moves);
        for (int i = 0; i < game.Moves.Count; i++)
        {
            Assert.AreEqual(game.Moves[i].From, reParsed.Moves[i].From);
            Assert.AreEqual(game.Moves[i].To, reParsed.Moves[i].To);
        }
    }
}
