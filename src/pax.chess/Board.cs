
namespace pax.chess;

public sealed class Board
{
    private readonly Piece?[] _squares = new Piece?[64];
    private readonly List<(Square Square, Piece Piece)> _whitePieces = new(16);
    private readonly List<(Square Square, Piece Piece)> _blackPieces = new(16);

    public Square? WhiteKingSquare { get; private set; }
    public Square? BlackKingSquare { get; private set; }

    public Piece? this[int index]
    {
        get => _squares[index];
        set
        {
            var square = new Square(index);

            if (_squares[index].HasValue)
            {
                var existing = _squares[index]!.Value;
                var list = GetList(existing.Color);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Square.Index == index)
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }

                if (existing.Type == PieceType.King)
                    ClearKingSquare(existing.Color);
            }

            _squares[index] = value;

            if (value.HasValue)
            {
                GetList(value.Value.Color).Add((square, value.Value));

                if (value.Value.Type == PieceType.King)
                    SetKingSquare(value.Value.Color, square);
            }
        }
    }

    public Square GetKingSquare(PieceColor color)
    {
        var square = color == PieceColor.White ? WhiteKingSquare : BlackKingSquare;
        return square ?? throw new InvalidOperationException($"{color} king is not on the board.");
    }

    public IReadOnlyList<(Square Square, Piece Piece)> GetPieces(PieceColor color)
        => color == PieceColor.White ? _whitePieces : _blackPieces;

    private List<(Square Square, Piece Piece)> GetList(PieceColor color)
        => color == PieceColor.White ? _whitePieces : _blackPieces;

    private void SetKingSquare(PieceColor color, Square square)
    {
        if (color == PieceColor.White) WhiteKingSquare = square;
        else BlackKingSquare = square;
    }

    private void ClearKingSquare(PieceColor color)
    {
        if (color == PieceColor.White) WhiteKingSquare = null;
        else BlackKingSquare = null;
    }

    public Board Clone()
    {
        var board = new Board();
        Array.Copy(_squares, board._squares, 64);
        board._whitePieces.Clear();
        board._whitePieces.AddRange(_whitePieces);
        board._blackPieces.Clear();
        board._blackPieces.AddRange(_blackPieces);
        board.WhiteKingSquare = WhiteKingSquare;
        board.BlackKingSquare = BlackKingSquare;
        return board;
    }
}
