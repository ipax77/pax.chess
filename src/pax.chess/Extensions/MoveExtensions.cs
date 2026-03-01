
namespace pax.chess.Extensions;

public static class Uci
{
    public static Move CreateMove(string notation)
    {
        // e2e4
        // e4

        var startSquare = new Square(1, 4);
        var targetSquare = new Square(3, 4);
        return new Move(startSquare, targetSquare);
    }
}