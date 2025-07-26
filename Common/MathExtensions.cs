namespace Common;

public static class MathExtensions
{
    public static int Clamp(this int a, int min, int max)
        => Math.Max(Math.Min(a, max), min);

    public static byte Clamp(this byte a, byte min, byte max)
        => Math.Max(Math.Min(a, max), min);
}
