﻿using System.Diagnostics;
using System.Text;
using Brainfly;

if (args.Length < 2)
{
    PrintUsage();
}

switch (args[0])
{
    case "build":
        {
            var program = Compiler.Compile(await File.ReadAllTextAsync(args[1]));
            await File.WriteAllTextAsync(Path.GetFileNameWithoutExtension(args[1]) + ".o", program.Code.ToString());
        }
        break;
    case "run":
        {
            var memorySize = int.Parse(args[1]);
            var program = new Executable(Type.GetType(await File.ReadAllTextAsync(args[2]))!);
            Console.OutputEncoding = Encoding.UTF8;
            var output = Console.OpenStandardOutput();
            var input = Console.OpenStandardInput();
            var memory = memorySize < 128 ? stackalloc byte[128] : new byte[memorySize];
            return program.Run(memory, input, output);
        }
    case "bench":
        {
            var memorySize = int.Parse(args[1]);
            var program = new Executable(Type.GetType(await File.ReadAllTextAsync(args[2]))!);
            Console.OutputEncoding = Encoding.UTF8;
            var output = new MemoryStream();
            var input = Console.OpenStandardInput();
            var memory = memorySize < 128 ? stackalloc byte[128] : new byte[memorySize];
            var sw = Stopwatch.StartNew();
            var execTime = new List<long>();
            Console.WriteLine("Warming up...");
            var count = 0;
            do
            {
                Console.Write($"Warmup #{++count}...");
                sw.Restart();
                program.Run(memory, input, output);
                execTime.Add(sw.ElapsedTicks);
                memory.Clear();
                output.Position = 0;
                output.SetLength(0);
                Console.WriteLine($" {TimeSpan.FromTicks(execTime.Last()).TotalNanoseconds} ns");
            } while (count < 5 || (StdDev(execTime[^5..]) > 20000 && count < 30));
            execTime.Clear();
            Console.WriteLine("Benchmarking...");
            count = 0;
            do
            {
                Console.Write($"Run #{++count}...");
                sw.Restart();
                program.Run(memory, input, output);
                execTime.Add(sw.ElapsedTicks);
                memory.Clear();
                Console.WriteLine($" {TimeSpan.FromTicks(execTime.Last()).TotalNanoseconds} ns");
            } while (count < 5 || (StdDev(execTime[^5..]) > 20000 && count < 30));
            RemoveOutliers(execTime);
            Console.WriteLine($"Mean: {TimeSpan.FromTicks((long)execTime.Average()).TotalNanoseconds} ns");
            Console.WriteLine($"StdDev: {TimeSpan.FromTicks((long)StdDev(execTime)).TotalNanoseconds} ns");
        }
        break;
    default:
        PrintUsage();
        break;
}

return 0;

static void PrintUsage()
{
    Console.WriteLine("Usage: Brainfly build <file>");
    Console.WriteLine("       Brainfly run <memory_size> <file>");
    Console.WriteLine("       Brainfly bench <memory_size> <file>");
}

static double StdDev(IEnumerable<long> values)
{
    var mean = values.Average();
    var sum = values.Sum(x => Math.Pow(x - mean, 2));
    return Math.Sqrt(sum / values.Count());
}

static void RemoveOutliers(List<long> values)
{
    var mean = values.Average();
    var stdDev = StdDev(values);
    var count = values.RemoveAll(x => Math.Abs(x - mean) > 2 * stdDev);
    Console.WriteLine($"Removed {count} {(count <= 1 ? "outlier" : "outliers")}.");
}
