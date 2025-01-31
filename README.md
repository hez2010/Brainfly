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
dotnet run -c Release -- run <memory_size> <file>
dotnet run -c Release -- bench <memory_size> <file>
```

For example,

```bash
dotnet run -c Release -- build a.bf # produce a.bfo
dotnet run -c Release -- run 1024 a.bfo # running directly from source file a.bf is also supported
dotnet run -c Release -- bench 1024 a.bfo # running directly from source file a.bf is also supported
```

You can also copy the compiled type and its type dependencies to a separate project, and run it directly, or even build it with NativeAOT:

```cs
static void Main()
{
    // A hello world program
    AddData<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex8>,Loop<AddPointer<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex1>, AddData<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex4>, Loop<AddPointer<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex1>, AddData<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex2>, AddPointer<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex1>, AddData<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex3>, AddPointer<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex1>, AddData<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex3>, AddPointer<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex1>, AddData<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex1>, AddPointer<Int<HexF, HexF, HexF, HexF, HexF, HexF, HexF, HexC>, AddData<Int<HexF, HexF, HexF, HexF, HexF, HexF, HexF, HexF>, Stop>>>>>>>>>>, AddPointer<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex1>, AddData<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex1>, AddPointer<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex1>, AddData<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex1>, AddPointer<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex1>, AddData<Int<HexF, HexF, HexF, HexF, HexF, HexF, HexF, HexF>, AddPointer<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex2>, AddData<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex1>, Loop<AddPointer<Int<HexF, HexF, HexF, HexF, HexF, HexF, HexF, HexF>, Stop>, AddPointer<Int<HexF, HexF, HexF, HexF, HexF, HexF, HexF, HexF>, AddData<Int<HexF, HexF, HexF, HexF, HexF, HexF, HexF, HexF>, Stop>>>>>>>>>>>>>>, AddPointer<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex2>, OutputData<AddPointer<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex1>, AddData<Int<HexF, HexF, HexF, HexF, HexF, HexF, HexF, HexD>, OutputData<AddData<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex7>, OutputData<OutputData<AddData<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex3>, OutputData<AddPointer<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex2>, OutputData<AddPointer<Int<HexF, HexF, HexF, HexF, HexF, HexF, HexF, HexF>, AddData<Int<HexF, HexF, HexF, HexF, HexF, HexF, HexF, HexF>, OutputData<AddPointer<Int<HexF, HexF, HexF, HexF, HexF, HexF, HexF, HexF>, OutputData<AddData<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex3>, OutputData<AddData<Int<HexF, HexF, HexF, HexF, HexF, HexF, HexF, HexA>, OutputData<AddData<Int<HexF, HexF, HexF, HexF, HexF, HexF, HexF, Hex8>, OutputData<AddPointer<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex2>, AddData<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex1>, OutputData<AddPointer<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex1>, AddData<Int<Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex0, Hex2>, OutputData<Stop>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        .Run(0, stackalloc byte[16], Console.OpenStandardInput(), Console.OpenStandardOutput());
}

// Pulling other type dependencies here
```

## Benchmarks

### Mandelbrot

| Name | Time (ms) | Rank | Ratio |
| --- | --- | --- | --- |
| Interpreter in C | 4,874.6587 | 5 | 5.59 |
| GCC | 901.0225 | 3 | 1.03 |
| Clang | 881.7177 | 2 | 1.01 |
| .NET JIT | 925.1596 | 4 | 1.06 |
| .NET AOT | **872.2287** | 1 | 1.00 |
