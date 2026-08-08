namespace Epidemia.Core;

public static class DeterministicRng
{
    public const int StreamContagion = 0;
    public const int StreamDeath = 1;
    public const int StreamRecovery = 2;

    public static double Uniform(ulong seed, int x, int y, int day, int stream)
    {
        ulong h = seed;
        h ^= (ulong)(uint)x * 0x9E3779B97F4A7C15UL;
        h = Avalanche(h);
        h ^= (ulong)(uint)y * 0x517CC1B727220A95UL;
        h = Avalanche(h);
        h ^= (ulong)(uint)day * 0x6C62272E07BB0142UL;
        h = Avalanche(h);
        h ^= (ulong)(uint)stream * 0x94D049BB133111EBUL;
        h = Avalanche(h);
        return (h >> 11) * (1.0 / (1UL << 53));
    }

    private static ulong Avalanche(ulong h)
    {
        h ^= h >> 30;
        h *= 0xBF58476D1CE4E5B9UL;
        h ^= h >> 27;
        h *= 0x94D049BB133111EBUL;
        h ^= h >> 31;
        return h;
    }
}
