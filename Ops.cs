using System.Runtime.CompilerServices;

namespace Brainfly;

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
    where Offset : INum
    where Next : IOp
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Run(int address, Span<byte> memory, Stream input, Stream output)
    {
        return Next.Run(address + Offset.Value, memory, input, output);
    }
}

struct AddData<Data, Next> : IOp
    where Data : INum
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
