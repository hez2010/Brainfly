using System.Runtime.CompilerServices;
using System.Text;
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
                        int count = 1;
                        while (i + 1 < code.Length && code[i + 1] == c)
                        {
                            count++;
                            i++;
                        }

                        var offset = (c == '>') ? count : -count;
                        stack.Peek().Add(new MovePointer(offset));
                        break;
                    }
                case '+':
                case '-':
                    {
                        int count = 1;
                        while (i + 1 < code.Length && code[i + 1] == c)
                        {
                            count++;
                            i++;
                        }

                        var delta = (c == '+') ? count : -count;
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
}

class Executable
{
    private readonly Type _code;
    private EntryPoint? _entrypoint;

    public Type Code => _code;
    private EntryPoint Entrypoint => _entrypoint ??= (EntryPoint)Delegate.CreateDelegate(typeof(EntryPoint), _code.GetMethod("Run")!);

    delegate int EntryPoint(int pc, Span<byte> memory, Stream input, Stream output);

    public Executable(Type code)
    {
        if (!code.IsAssignableTo(typeof(IOp))) throw new InvalidProgramException();
        _code = code;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public int Run(Span<byte> memory, Stream input, Stream output)
    {
        return Entrypoint(0, memory, input, output);
    }

    public override string ToString()
    {
        return ToString(_code);
    }

    public string ToFriendlyString()
    {
        return ToString(_code, true);
    }

    private static string ToString(Type t, bool friendly = false)
    {
        if (!t.IsGenericType) return t.Name;
        if (friendly && t.IsAssignableTo(typeof(INum))) return GetNumValue(t);
        var sb = new StringBuilder();
        sb.Append($"{t.Name.AsSpan(0, t.Name.IndexOf('`'))}<");
        var cnt = 0;
        foreach (var arg in t.GetGenericArguments())
        {
            if (cnt > 0) sb.Append(", ");
            sb.Append(ToString(arg));
            cnt++;
        }
        return sb.Append('>').ToString();
    }

    private static string GetNumValue(Type t)
    {
        var hex = t.GetGenericArguments();
        var num = 0;
        for (int i = 0; i < hex.Length; i++)
        {
            num <<= 4;
            num |= (int)hex[i].GetProperty("Value")!.GetValue(null)!;
        }
        return num.ToString();
    }
}
