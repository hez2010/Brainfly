# Brainfly

A Brainf**k JIT compiler built on top of C# and .NET.

Leveraged .NET generic type system and abstract static interface methods to achieve great performance.

## Build

```bash
dotnet build -c Release
```

## Usage

```bash
dotnet run -c Release -- build <file>
dotnet run -c Release -- bundle <file>
dotnet run -c Release -- run <memory_size> <file>
dotnet run -c Release -- bench <memory_size> <file>
```

For example,

```bash
dotnet run -c Release -- build a.bf # produce a self-contained a.cs so that you can run it directly with .NET or build it with .NET NativeAOT, you can pass the memory size in the first argument when you run the generated program
dotnet run -c Release -- run 1024 a.bf # running directly from source file a.bf
dotnet run -c Release -- bench 1024 a.bf # benchmark directly from source file a.bf
```

## Benchmarks

### Mandelbrot

| Name | Time (ms) | Rank | Ratio | Binary size | Description |
| --- | --- | --- | --- | --- | --- |
| Interpreter in C | 4,874.6587 | 5 | 5.59 | N/A | A Brainfuck interpreter written in C |
| GCC | 901.0225 | 3 | 1.03 | **52 KB** | Translate to C, then build with `gcc -O3 -march=native` |
| Clang | 881.7177 | 2 | 1.01 | 56 KB | Translate to C, then build with `clang -O3 -march=native` |
| .NET JIT | 925.1596 | 4 | 1.06 | N/A | Use JIT to generate the type and then instantiate it for running |
| .NET AOT | **872.2287** | 1 | 1.00 | 1732 KB | Use .NET NativeAOT to build the exe that runs the compiled type directly |
