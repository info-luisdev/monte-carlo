namespace Epidemia.Core;

public static class Simulator
{
    private const byte S = (byte)CellState.Susceptible;
    private const byte I = (byte)CellState.Infected;
    private const byte R = (byte)CellState.Recovered;
    private const byte D = (byte)CellState.Dead;

    public static ReplicaResult RunReplica(
        SimulationConfig config, ulong replicaSeed, int threadCount, int frameInterval = 0)
    {
        byte[] current = Grid.Create(config);
        byte[] next = new byte[config.BufferLength];
        var timeline = new DayStats[config.Days];
        List<byte[]>? frames = frameInterval > 0 ? [] : null;
        int stride = config.Stride;

        frames?.Add(Grid.CaptureFrame(current, config));

        for (int day = 0; day < config.Days; day++)
        {
            timeline[day] = threadCount <= 1
                ? StepSequential(current, next, config, replicaSeed, day, stride)
                : StepParallel(current, next, config, replicaSeed, day, stride, threadCount);

            (current, next) = (next, current);

            if (frames != null && (day + 1) % frameInterval == 0)
                frames.Add(Grid.CaptureFrame(current, config));
        }

        return new ReplicaResult(replicaSeed, timeline, frames);
    }

    private static DayStats StepSequential(
        byte[] current, byte[] next, SimulationConfig config,
        ulong seed, int day, int stride)
    {
        var stats = new StatsAccumulator();

        for (int y = 0; y < config.Height; y++)
        for (int x = 0; x < config.Width; x++)
        {
            int idx = (y + 1) * stride + (x + 1);
            UpdateCell(current, next, idx, x, y, day, stride, config, seed, stats);
        }

        return stats.ToDayStats();
    }

    private static DayStats StepParallel(
        byte[] current, byte[] next, SimulationConfig config,
        ulong seed, int day, int stride, int threadCount)
    {
        var globalStats = new StatsAccumulator();
        var options = new ParallelOptions { MaxDegreeOfParallelism = threadCount };

        Parallel.For(0, threadCount, options,
            () => new StatsAccumulator(),
            (block, _, local) =>
            {
                int yStart = block * config.Height / threadCount;
                int yEnd = (block + 1) * config.Height / threadCount;

                for (int y = yStart; y < yEnd; y++)
                for (int x = 0; x < config.Width; x++)
                {
                    int idx = (y + 1) * stride + (x + 1);
                    UpdateCell(current, next, idx, x, y, day, stride, config, seed, local);
                }

                return local;
            },
            local => local.MergeInto(globalStats));

        return globalStats.ToDayStats();
    }

    private static void UpdateCell(
        byte[] current, byte[] next, int idx, int x, int y,
        int day, int stride, SimulationConfig config, ulong seed,
        StatsAccumulator stats)
    {
        byte state = current[idx];

        switch (state)
        {
            case S:
                int k = Grid.CountInfectedNeighbors(current, idx, stride);
                if (k > 0)
                {
                    double p = 1.0 - Math.Pow(1.0 - config.Beta, k);
                    if (DeterministicRng.Uniform(seed, x, y, day, DeterministicRng.StreamContagion) < p)
                    {
                        next[idx] = I;
                        stats.RecordNewInfection();
                        stats.Count(I);
                        return;
                    }
                }
                next[idx] = S;
                stats.Count(S);
                break;

            case I:
                if (DeterministicRng.Uniform(seed, x, y, day, DeterministicRng.StreamDeath) < config.Mu)
                    next[idx] = D;
                else if (DeterministicRng.Uniform(seed, x, y, day, DeterministicRng.StreamRecovery) < config.Gamma)
                    next[idx] = R;
                else
                    next[idx] = I;
                stats.Count(next[idx]);
                break;

            default:
                next[idx] = state;
                stats.Count(state);
                break;
        }
    }
}
