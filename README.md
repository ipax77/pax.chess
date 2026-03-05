# pax.chess

C# chess library for .NET 10 with:

- move validation and game-state evaluation
- PGN parsing/serialization
- FEN parsing/serialization
- UCI move conversion helpers
- move-tree analysis with variations
- optional game clock and timeout handling

Sample project: [pax.BlazorChess](https://github.com/ipax77/pax.BlazorChess)

## Requirements

- .NET SDK 10.0+

## Installation

```bash
dotnet add package pax.chess
```

## Quick Start

```csharp
using pax.chess;

var game = new ChessGame();
var move = new Move(new Square(4, 1), new Square(4, 3)); // e2e4

var state = game.TryApplyMove(move);
if (state == MoveState.Ok)
{
    Console.WriteLine(game.CurrentPosition.SideToMove); // Black
}
```

## PGN

### Parse PGN

```csharp
using pax.chess;

string pgn = "1. e4 e5 2. Bc4 Bc5 3. Qh5 Nf6 4. Qxf7#";
var game = PgnSerializer.Parse(pgn);
game.Evaluate();

Console.WriteLine(game.Result); // WhiteWin
Console.WriteLine(game.Conclusion?.Termination); // Checkmate
```

### Serialize PGN

```csharp
using pax.chess;

var game = PgnSerializer.Parse("1. f4 e6 2. g4 Qh4#");
game.Metadata.White = "WhitePlayer";
game.Metadata.Black = "BlackPlayer";

string serialized = PgnSerializer.Serialize(game);
Console.WriteLine(serialized);
```

## FEN

```csharp
using pax.chess;

string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
BoardPosition position = FenSerializer.Parse(fen);
string roundtrip = FenSerializer.Serialize(position);
```

## UCI Helpers

```csharp
using pax.chess;
using pax.chess.Extensions;

var game = new ChessGame();
var move = Uci.CreateMove("e2e4", game.CurrentPosition);

if (move is not null)
{
    game.ApplyMove(move); // Use ApplyMove for pre-validated engine moves
}

string uci = Uci.GetUci(game.Moves[0].Move); // "e2e4"
```

## Analysis Board (Variations)

```csharp
using pax.chess;

var board = new AnalysisBoard();
board.TryApplyMove(new Move(new Square(4, 1), new Square(4, 3))); // e2e4
board.MoveBackward();
board.TryApplyMove(new Move(new Square(3, 1), new Square(3, 3))); // d2d4 as second variation

Console.WriteLine(board.Root.Variations.Count); // 2
```

## Development

```bash
dotnet test
```
