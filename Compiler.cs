using static Brainfly.Compiler.Instruction;

namespace Brainfly;

class Compiler
{
    internal abstract record Instruction
    {
        internal record MovePointer(int Offset) : Instruction;
        internal record AddData(int Delta) : Instruction;
        internal record Output : Instruction;
        internal record Input : Instruction;
        internal record LoopBody(List<Instruction> Body) : Instruction;
    }

    private static Type GetHex(int hex)
    {
        return hex switch
        {
            0 => typeof(Hex0),
            1 => typeof(Hex1),
            2 => typeof(Hex2),
            3 => typeof(Hex3),
            4 => typeof(Hex4),
            5 => typeof(Hex5),
            6 => typeof(Hex6),
            7 => typeof(Hex7),
            8 => typeof(Hex8),
            9 => typeof(Hex9),
            10 => typeof(HexA),
            11 => typeof(HexB),
            12 => typeof(HexC),
            13 => typeof(HexD),
            14 => typeof(HexE),
            15 => typeof(HexF),
            _ => throw new ArgumentOutOfRangeException(nameof(hex)),
        };
    }

    internal static Type GetNum(int num)
    {
        var hex0 = num & 0xF;
        var hex1 = (num >>> 4) & 0xF;
        var hex2 = (num >>> 8) & 0xF;
        var hex3 = (num >>> 12) & 0xF;
        var hex4 = (num >>> 16) & 0xF;
        var hex5 = (num >>> 20) & 0xF;
        var hex6 = (num >>> 24) & 0xF;
        var hex7 = (num >>> 28) & 0xF;
        return typeof(Int<,,,,,,,>).MakeGenericType(GetHex(hex7), GetHex(hex6), GetHex(hex5), GetHex(hex4), GetHex(hex3), GetHex(hex2), GetHex(hex1), GetHex(hex0));
    }

    public static Executable Compile(ReadOnlySpan<char> code)
    {
        return new Executable(Emit(Import(code)));
    }

    private static List<Instruction> Import(ReadOnlySpan<char> code)
    {
        var stack = new Stack<List<Instruction>>();
        stack.Push([]);

        for (int i = 0; i < code.Length; i++)
        {
            var c = code[i];
            switch (c)
            {
                case '>':
                case '<':
                    {
                        int offset = 0;
                        while (i < code.Length && code[i] is '>' or '<')
                        {
                            offset += (code[i] == '>') ? 1 : -1;
                            i++;
                        }
                        i--;
                        stack.Peek().Add(new MovePointer(offset));
                        break;
                    }
                case '+':
                case '-':
                    {
                        int delta = 0;
                        while (i < code.Length && code[i] == c)
                        {
                            delta += (c == '+') ? 1 : -1;
                            i++;
                        }
                        i--;
                        stack.Peek().Add(new AddData(delta));
                        break;
                    }
                case '.':
                    stack.Peek().Add(new Output());
                    break;
                case ',':
                    stack.Peek().Add(new Input());
                    break;
                case '[':
                    stack.Push([]);
                    break;
                case ']':
                    if (stack.Count == 1) throw new InvalidProgramException("Mismatched ']' — no matching '['.");
                    var loopBody = stack.Pop();
                    var loopInstruction = new LoopBody(loopBody);
                    stack.Peek().Add(loopInstruction);
                    break;

                default:
                    break;
            }
        }

        if (stack.Count != 1)
            throw new InvalidProgramException("Mismatched '[' — missing ']'.");

        return stack.Pop();
    }

    private static Type Emit(List<Instruction> instructions)
    {
        var code = typeof(Stop);
        for (int i = instructions.Count - 1; i >= 0; i--)
        {
            code = instructions[i] switch
            {
                MovePointer mp => typeof(AddPointer<,>).MakeGenericType(GetNum(mp.Offset), code),
                AddData ad => typeof(AddData<,>).MakeGenericType(GetNum(ad.Delta), code),
                Output => typeof(OutputData<>).MakeGenericType(code),
                Input => typeof(InputData<>).MakeGenericType(code),
                LoopBody loop => typeof(Loop<,>).MakeGenericType(Emit(loop.Body), code),
                _ => throw new InvalidProgramException("Illegal instruction."),
            };
        }

        return code;
    }

    internal static string GetDependencySource()
    {
        return """
        static class SpanExtensions
        {
            internal static ref T UnsafeAt<T>(this Span<T> span, int address)
            {
                return ref Unsafe.Add(ref MemoryMarshal.GetReference(span), address);
            }
        }
        interface IHex
        {
            abstract static int Value { get; }
        }
        interface INum<T>
        {
            abstract static T Value { get; }
        }
        struct Hex0 : IHex
        {
            public static int Value => 0;
        }
        struct Hex1 : IHex
        {
            public static int Value => 1;
        }
        struct Hex2 : IHex
        {
            public static int Value => 2;
        }
        struct Hex3 : IHex
        {
            public static int Value => 3;
        }
        struct Hex4 : IHex
        {
            public static int Value => 4;
        }
        struct Hex5 : IHex
        {
            public static int Value => 5;
        }
        struct Hex6 : IHex
        {
            public static int Value => 6;
        }
        struct Hex7 : IHex
        {
            public static int Value => 7;
        }
        struct Hex8 : IHex
        {
            public static int Value => 8;
        }
        struct Hex9 : IHex
        {
            public static int Value => 9;
        }
        struct HexA : IHex
        {
            public static int Value => 10;
        }
        struct HexB : IHex
        {
            public static int Value => 11;
        }
        struct HexC : IHex
        {
            public static int Value => 12;
        }
        struct HexD : IHex
        {
            public static int Value => 13;
        }
        struct HexE : IHex
        {
            public static int Value => 14;
        }
        struct HexF : IHex
        {
            public static int Value => 15;
        }
        struct Int<H7, H6, H5, H4, H3, H2, H1, H0> : INum<int>
            where H7 : IHex
            where H6 : IHex
            where H5 : IHex
            where H4 : IHex
            where H3 : IHex
            where H2 : IHex
            where H1 : IHex
            where H0 : IHex
        {
            public static int Value
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => H7.Value << 28 | H6.Value << 24 | H5.Value << 20 | H4.Value << 16 | H3.Value << 12 | H2.Value << 8 | H1.Value << 4 | H0.Value;
            }
        }
        interface IOp
        {
            abstract static int Run(int address, Span<byte> memory, Stream input, Stream output);
        }
        struct Stop : IOp
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int Run(int address, Span<byte> memory, Stream input, Stream output)
            {
                return address;
            }
        }
        struct Loop<Body, Next> : IOp
            where Body : IOp
            where Next : IOp
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int Run(int address, Span<byte> memory, Stream input, Stream output)
            {
                while (memory.UnsafeAt(address) != 0)
                {
                    address = Body.Run(address, memory, input, output);
                }
                return Next.Run(address, memory, input, output);
            }
        }
        struct AddPointer<Offset, Next> : IOp
            where Offset : INum<int>
            where Next : IOp
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int Run(int address, Span<byte> memory, Stream input, Stream output)
            {
                return Next.Run(address + Offset.Value, memory, input, output);
            }
        }
        struct AddData<Data, Next> : IOp
            where Data : INum<int>
            where Next : IOp
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int Run(int address, Span<byte> memory, Stream input, Stream output)
            {
                memory.UnsafeAt(address) += (byte)Data.Value;
                return Next.Run(address, memory, input, output);
            }
        }
        struct OutputData<Next> : IOp
            where Next : IOp
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int Run(int address, Span<byte> memory, Stream input, Stream output)
            {
                output.WriteByte(memory.UnsafeAt(address));
                return Next.Run(address, memory, input, output);
            }
        }
        struct InputData<Next> : IOp
            where Next : IOp
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int Run(int address, Span<byte> memory, Stream input, Stream output)
            {
                var data = input.ReadByte();
                if (data == -1)
                {
                    return address;
                }
                memory.UnsafeAt(address) = (byte)data;
                return Next.Run(address, memory, input, output);
            }
        }
        """;
    }
}
