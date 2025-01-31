using System.Runtime.CompilerServices;
using System.Text;

namespace Brainfly;

class Executable
{
    private readonly Type _code;
    private EntryPoint? _entrypoint;

    public Type Code => _code;
    private EntryPoint Entrypoint => _entrypoint ??= (EntryPoint)Delegate.CreateDelegate(typeof(EntryPoint), _code.GetMethod("Run")!);

    delegate int EntryPoint(int address, Span<byte> memory, Stream input, Stream output);

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
            sb.Append(ToString(arg, friendly));
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
