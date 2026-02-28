
namespace pax.chess;

public sealed class Board
{
    private readonly Piece?[] _squares = new Piece?[64];

    public Piece? this[int index]
    {
        get => _squares[index];
        set => _squares[index] = value;
    }

    public Board Clone()
    {
        var board = new Board();
        Array.Copy(_squares, board._squares, 64);
        return board;
    }
}
