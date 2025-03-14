# Introduction

C# dotnet chess library with pgn/fen import/export and move validation.

# Getting started
## Prerequisites
dotnet

## Installation
You can install it with the Package Manager in your IDE or alternatively using the command line:

```bash
dotnet add package pax.chess
```
## Usage

Sample Project: [pax.BlazorChess](https://github.com/ipax77/pax.BlazorChess)

```csharp
Game game = new Game();
EngineMove move = new EngineMove(new Position(4, 2), new Position(5, 3));
var state = game.Move(move);
var pgn = Pgn.MapPieces(game.State);
```
```csharp
string pgn = "1. e4 e5 2. Bc4 Bc5 3. Qh5 Nf6 4. Qxf7#";
Game game = Pgn.MapString(pgn);
Assert.True(game.State.Info.IsCheckMate);

```
```csharp
string pgn = "1. e4 d5 2. exd5 e6 3. dxe6 Qe7 4. Nc3 Bxe6 5. b3 Bxb3+";
Game game = Pgn.MapString(pgn);
var state = game.Move(new EngineMove(new Position(0, 1), new Position(1, 2)));
Assert.True(state == MoveState.WouldBeCheck);
```
```csharp
string fen = "2r3k1/6pp/p3pp1B/2bn4/2pK3P/3b1PR1/P7/3R4 w - - 2 31";
Game game = new Game(fen);
Assert.True(game.State.Info.IsCheckMate);

```
```csharp
string pgn = "1. g4 h5 2. gxh5 Rxh5 3. Nf3 Rh6 4. Bh3 Rg6";
Game game = Pgn.MapString(pgn);
var state = game.Move(new EngineMove(new Position(4, 0), new Position(6, 0)));
Assert.True(state == MoveState.CastleNotAllowed);
```