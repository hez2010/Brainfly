﻿using System.Diagnostics;
using System.Text;
using Brainfly;
using ZstdSharp;

if (args.Length < 2)
{
    PrintUsage();
    return 0;
}

switch (args[0])
{
    case "build":
        {
            var program = Compiler.Compile(await File.ReadAllTextAsync(args[1]));
            using var compressor = new Compressor();
            await File.WriteAllBytesAsync(Path.GetFileNameWithoutExtension(args[1]) + ".bft", Encoding.UTF8.GetBytes(program.ToString()));
            await File.WriteAllBytesAsync(Path.GetFileNameWithoutExtension(args[1]) + ".bftf", Encoding.UTF8.GetBytes(program.ToFriendlyString()));
            await File.WriteAllBytesAsync(Path.GetFileNameWithoutExtension(args[1]) + ".bfo", compressor.Wrap(Encoding.UTF8.GetBytes(program.Code.ToString())).ToArray());
        }
        break;
    case "run":
        {
            var memorySize = int.Parse(args[1]);
            var extName = Path.GetExtension(args[2]);
            Executable program;
            if (string.Equals(extName, ".bfo", StringComparison.OrdinalIgnoreCase))
            {
                using var decompressor = new Decompressor();
                program = new Executable(Type.GetType(Encoding.UTF8.GetString(decompressor.Unwrap(await File.ReadAllBytesAsync(args[2]))))!);
            }
            else
            {
                program = Compiler.Compile(await File.ReadAllTextAsync(args[2]));
            }
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            await using var output = Console.OpenStandardOutput();
            await using var input = Console.OpenStandardInput();
            var memory = memorySize < 128 ? stackalloc byte[128] : new byte[memorySize];
            return program.Run(memory, input, output);
        }
    case "bench":
        {
            var memorySize = int.Parse(args[1]);
            var extName = Path.GetExtension(args[2]);
            Executable program;
            if (string.Equals(extName, ".bfo", StringComparison.OrdinalIgnoreCase))
            {
                using var decompressor = new Decompressor();
                program = new Executable(Type.GetType(Encoding.UTF8.GetString(decompressor.Unwrap(await File.ReadAllBytesAsync(args[2]))))!);
            }
            else
            {
                program = Compiler.Compile(await File.ReadAllTextAsync(args[2]));
            }
            Console.OutputEncoding = Encoding.UTF8;
            await using var output = new MemoryStream();
            await using var input = new MemoryStream();
            var memory = memorySize < 128 ? stackalloc byte[128] : new byte[memorySize];
            var sw = Stopwatch.StartNew();
            var execTimeControl = Stopwatch.StartNew();
            var execTime = new List<long>();
            Console.WriteLine("Warming up...");
            var count = 0;
            do
            {
                sw.Restart();
                program.Run(memory, input, output);
                execTime.Add(sw.ElapsedTicks);
                Thread.Sleep(10);
                memory.Clear();
                output.Position = 0;
                output.SetLength(0);
            } while (++count < 10 || execTimeControl.Elapsed < TimeSpan.FromSeconds(10) || (HasAnyOutlier(execTime[^5..]) && execTimeControl.Elapsed < TimeSpan.FromSeconds(60)));
            execTime.Clear();
            Console.WriteLine("Benchmarking...");
            execTimeControl.Restart();
            count = 0;
            do
            {
                sw.Restart();
                program.Run(memory, input, output);
                execTime.Add(sw.ElapsedTicks);
                memory.Clear();
                output.Position = 0;
                output.SetLength(0);
            } while (++count < 10 || execTimeControl.Elapsed < TimeSpan.FromSeconds(10) || (HasAnyOutlier(execTime[^5..]) && execTimeControl.Elapsed < TimeSpan.FromSeconds(60)));
            Console.WriteLine($"Executed {execTime.Count} ops");
            RemoveOutliers(execTime);
            var mean = ToFriendlyTime(execTime.Average() * TimeSpan.NanosecondsPerTick);
            Console.WriteLine($"Mean: {mean.Value} {mean.Unit}");
            var stdDev = ToFriendlyTime(StdDev(execTime) * TimeSpan.NanosecondsPerTick);
            Console.WriteLine($"StdDev: {stdDev.Value} {stdDev.Unit}");
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
    Console.WriteLine($"Removed {count} {(count <= 1 ? "outlier" : "outliers")}");
}

static bool HasAnyOutlier(IEnumerable<long> values)
{
    var mean = values.Average();
    var stdDev = StdDev(values);
    var count = values.Count(x => Math.Abs(x - mean) > 2 * stdDev);
    return count > 0;
}

static (double Value, string Unit) ToFriendlyTime(double nanoseconds)
{
    if (nanoseconds < 1000)
    {
        return (nanoseconds, "ns");
    }
    var microseconds = nanoseconds / 1000;
    if (microseconds < 1000)
    {
        return (microseconds, "μs");
    }
    var milliseconds = microseconds / 1000;
    if (milliseconds < 1000)
    {
        return (milliseconds, "ms");
    }
    var seconds = milliseconds / 1000;
    if (seconds < 60)
    {
        return (seconds, "s");
    }
    var minutes = seconds / 60;
    if (minutes < 60)
    {
        return (minutes, "m");
    }
    var hours = minutes / 60;
    if (hours < 24)
    {
        return (hours, "h");
    }
    var days = hours / 24;
    return (days, "d");
}
