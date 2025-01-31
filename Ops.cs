using System.Runtime.CompilerServices;

namespace Brainfly;

interface IOp
{
    abstract static int Run(int index, Span<byte> memory, Stream input, Stream output);
}

struct Stop : IOp
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Run(int index, Span<byte> memory, Stream input, Stream output)
    {
        return index;
    }
}

struct Loop<Body, Next> : IOp
    where Body : IOp
    where Next : IOp
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Run(int index, Span<byte> memory, Stream input, Stream output)
    {
        while (memory.UnsafeAt(index) != 0)
        {
            index = Body.Run(index, memory, input, output);
        }
        return Next.Run(index, memory, input, output);
    }
}

struct AddPointer<Offset, Next> : IOp
    where Offset : INum
    where Next : IOp
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Run(int index, Span<byte> memory, Stream input, Stream output)
    {
        return Next.Run(index + Offset.Value, memory, input, output);
    }
}

struct AddData<Data, Next> : IOp
    where Data : INum
    where Next : IOp
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Run(int index, Span<byte> memory, Stream input, Stream output)
    {
        memory.UnsafeAt(index) += (byte)Data.Value;
        return Next.Run(index, memory, input, output);
    }
}

struct OutputData<Next> : IOp
    where Next : IOp
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Run(int index, Span<byte> memory, Stream input, Stream output)
    {
        output.WriteByte(memory.UnsafeAt(index));
        return Next.Run(index, memory, input, output);
    }
}

struct InputData<Next> : IOp
    where Next : IOp
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Run(int index, Span<byte> memory, Stream input, Stream output)
    {
        var data = input.ReadByte();
        if (data == -1)
        {
            return index;
        }
        memory.UnsafeAt(index) = (byte)data;
        return Next.Run(index, memory, input, output);
    }
}
