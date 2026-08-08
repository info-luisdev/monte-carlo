namespace Epidemia.Core;

public sealed record ReplicaResult(
    ulong Seed,
    DayStats[] Timeline,
    List<byte[]>? Frames = null)
{
    public double ElapsedMs { get; init; }
}
