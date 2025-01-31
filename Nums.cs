namespace Brainfly;

using System.Runtime.CompilerServices;

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
