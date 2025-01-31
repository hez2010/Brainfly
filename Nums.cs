namespace Brainfly;

using System.Runtime.CompilerServices;

interface IHex
{
    abstract static int Value { get; }
}

interface INum
{
    abstract static int Value { get; }
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

struct Int<Hex7, Hex6, Hex5, Hex4, Hex3, Hex2, Hex1, Hex0> : INum
    where Hex7 : IHex
    where Hex6 : IHex
    where Hex5 : IHex
    where Hex4 : IHex
    where Hex3 : IHex
    where Hex2 : IHex
    where Hex1 : IHex
    where Hex0 : IHex
{
    public static int Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Hex7.Value << 28 | Hex6.Value << 24 | Hex5.Value << 20 | Hex4.Value << 16 | Hex3.Value << 12 | Hex2.Value << 8 | Hex1.Value << 4 | Hex0.Value;
    }
}
