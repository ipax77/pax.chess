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

var state = game.ApplyMove(move);
if (state == MoveState.Ok)
{
    Console.WriteLine(game.CurrentPosition.SideToMove); // Black
}
```

### Play UCI Moves

Use `Play` for concise UCI coordinate moves and the serializer helpers for the current position or full game.

```csharp
using pax.chess;

var game = new ChessGame();

game.Play("e2e4");
game.Play("e7e5");
game.Play("g1f3");

Console.WriteLine(game.ToFen());
Console.WriteLine(game.ToPgn());
```

`Play` returns a `MoveResult` with the parsed move, SAN, state, and optional error text.

```csharp
var result = game.Play("b8c6");

if (result.IsOk)
{
    Console.WriteLine(result.San); // Nc6
}
else
{
    Console.WriteLine(result.Error);
}
```

`Play` accepts UCI notation such as `e2e4` and `e7e8q`. Use `PgnSerializer.Parse(...)` for SAN/PGN text.

### ChessGameOptions

Use `ChessGameOptions.Engine` when moves come from an engine or another trusted source and are already validated. This skips move validation and post-move evaluation to reduce CPU work in engine-style pipelines.

```csharp
using pax.chess;

var game = new ChessGame(ChessGameOptions.Engine);
var move = new Move(new Square(4, 1), new Square(4, 3)); // e2e4

game.ApplyMove(move);
game.Evaluate(); // Evaluate explicitly when you need Result or Conclusion
```

The default `new ChessGame()` path keeps validation and evaluation enabled for normal application code.

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
using pax.chess.Analyze;

var board = new AnalysisBoard();
board.TryApplyMove(new Move(new Square(4, 1), new Square(4, 3))); // e2e4
board.MoveBackward();
board.TryApplyMove(new Move(new Square(3, 1), new Square(3, 3))); // d2d4 as second variation

Console.WriteLine(board.Root.Variations.Count); // 2
```

## Changelog

### v0.7.0

- Targets .NET 10.
- Replaces the old game model with `ChessGame` and `ChessGameOptions`.
- Adds `ChessGame.Play(...)`, `ToFen()`, and `ToPgn()` convenience helpers.
- Adds `PgnSerializer` and `FenSerializer` for PGN/FEN parsing and serialization.
- Adds UCI conversion helpers.
- Adds `AnalysisBoard` support for move trees and variations.
- Adds optional `ChessClock` support with timeout handling.
- Adds Zobrist hashing and repetition tracking.
- Adds the `PseudoMoveValidator` implementation and benchmark project.

## Migrating from v0.6.6

v0.7.0 is a breaking API update from the current live v0.6.6 package. The main type mappings are:

| v0.6.6 | v0.7.0 |
| --- | --- |
| `Game` | `ChessGame` |
| `EngineMove` | `Move` |
| `Position` | `Square` |
| `Pgn.MapString(...)` | `PgnSerializer.Parse(...)` |
| `Pgn.MapPieces(...)` | `PgnSerializer.Serialize(...)` |
| `Fen` helpers | `FenSerializer.Parse(...)` / `FenSerializer.Serialize(...)` |
| `game.Move(...)` | `game.ApplyMove(...)` |

Old DB mapping types are no longer part of the documented core library surface.

For pre-validated engine output, create games with `ChessGameOptions.Engine` to skip validation and automatic evaluation:

```csharp
using pax.chess;

var game = new ChessGame(ChessGameOptions.Engine);
var engineMove = new Move(new Square(4, 1), new Square(4, 3)); // e2e4

game.ApplyMove(engineMove);
```
