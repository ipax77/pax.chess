// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using pax.chess.benchmark;

Console.WriteLine("Hello, World!");

BenchmarkRunner.Run<MoveValidatorBenchmarks>();