namespace pax.chess.sample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //ChessBoard board = new();

            //board.Move(new("e2"), new("e4"));
            //board.Move(new("a7"), new("a6"));
            //board.Move(new("e4"), new("e5"));
            //board.Move(new("d7"), new("d5"));
            //board.Move(new("e5"), new("d6"));
            //board.Move(new("b8"), new("c6"));

            //board.DisplayBoard();

            //var pgn = PgnParser.MovesToPgn(board.Moves.ToList());

            //Console.WriteLine(pgn);

            var lichesspgn = @"[Event ""Rated Blitz game""]
[Site ""https://lichess.org/ZpHJD9nv""]
[Date ""2024.01.10""]
[White ""Bobbert_Simpson""]
[Black ""pax77""]
[Result ""0-1""]
[UTCDate ""2024.01.10""]
[UTCTime ""05:06:01""]
[WhiteElo ""2104""]
[BlackElo ""2047""]
[WhiteRatingDiff ""-8""]
[BlackRatingDiff ""+7""]
[Variant ""Standard""]
[TimeControl ""180+2""]
[ECO ""C77""]
[Opening ""Ruy Lopez: Morphy Defense, Anderssen Variation""]
[Termination ""Normal""]
[Annotator ""lichess.org""]

1. e4 e5 2. Nf3 Nc6 3. Bb5 a6 4. Ba4 Nf6 5. d3 { C77 Ruy Lopez: Morphy Defense, Anderssen Variation } d6 6. O-O Be7 7. c3 O-O 8. Bg5 Bg4 9. Nbd2 Kh8 10. Qb3 b5 11. Bxb5 axb5 12. Qxb5 Na7 13. Qc4 Be6 14. Qa4 h6 15. Bxf6 Bxf6 16. d4 exd4 17. cxd4 Bd7 18. Qc4 Bb5 19. Qd3 { White resigns. } 0-1";

            var chesscompgn = @"[Event ""Live Chess""]
[Site ""Chess.com""]
[Date ""2021.12.20""]
[Round ""?""]
[White ""pax_77""]
[Black ""Kamil-Babayev""]
[Result ""0-1""]
[ECO ""D40""]
[WhiteElo ""1569""]
[BlackElo ""1622""]
[TimeControl ""180+2""]
[EndTime ""20:44:49 PST""]
[Termination ""Kamil-Babayev won by resignation""]

1. d4 d5 2. c4 e6 3. Nc3 Nf6 4. Nf3 c5 5. Bg5 Be7 6. e3 Nc6 7. a3 O-O 8. h4 b6
9. Bd3 dxc4 10. Bxc4 cxd4 11. exd4 Bb7 12. Qd3 Nd5 13. Rd1 Nxc3 14. bxc3 Bxg5
15. hxg5 g6 16. Qc2 Na5 17. Ba2 Bxf3 18. gxf3 Qxg5 19. Ke2 Qb5+ 20. Ke3 Nc4+ 21.
Bxc4 Qxc4 22. Rh6 f5 23. Rg1 Kg7 24. Rgh1 Rh8 25. f4 Rac8 26. Kd2 Qd5 27. Qa4
Qc6 28. Qb4 Rc7 29. R6h3 h5 30. Re1 Rhc8 31. Rhe3 Kf7 32. Qb3 Re7 33. Kd3 a6 34.
Re5 b5 35. R1e3 Qc4+ 36. Qxc4 Rxc4 37. Kc2 a5 38. Rxb5 Rec7 39. Rxa5 Rxd4 40.
Ra6 Rxf4 41. Raxe6 Rxf2+ 42. Kb3 Rb7+ 43. Kc4 Rf4+ 44. Kc5 Rc7+ 45. Kd6 Ra7 46.
R6e5 Rxa3 47. Re7+ Kf6 48. R7e6+ Kg5 49. Rg3+ Rg4 50. Rd3 Ra6+ 51. Ke7 Rxe6+ 52.
Kxe6 Re4+ 53. Kd5 h4 54. c4 Rxc4 55. Kxc4 Kg4 56. Kd4 h3 57. Ke3 h2 58. Rd1 Kg3
59. Ke2 Kg2 60. Ke3 h1=Q 61. Rd2+ Kg3 62. Kd4 Qe4+ 63. Kc5 Qe3+ 0-1";

            var moves = PgnParser.GetPgnMoves(lichesspgn);

            var pgnBoard = ChessBoard.FromPgn(lichesspgn);

            var pgn1 = pgnBoard.GetPgn();

            pgnBoard.DisplayBoard();
            Console.WriteLine(pgn1);
        }
    }
}
