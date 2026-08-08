namespace Epidemia.Core;

public static class CsvExporter
{
    public static void ExportTimeline(string path, DayStats[] timeline)
    {
        using var writer = new StreamWriter(path);
        writer.WriteLine("day,susceptible,infected,recovered,dead,new_infections");

        for (int i = 0; i < timeline.Length; i++)
        {
            var s = timeline[i];
            writer.WriteLine($"{i},{s.Susceptible},{s.Infected},{s.Recovered},{s.Dead},{s.NewInfections}");
        }
    }

    public static void ExportAggregated(string path, List<ReplicaResult> results, int days)
    {
        using var writer = new StreamWriter(path);
        writer.WriteLine("day,s_median,i_median,r_median,d_median,i_p05,i_p25,i_p75,i_p95");

        var sVals = new int[results.Count];
        var iVals = new int[results.Count];
        var rVals = new int[results.Count];
        var dVals = new int[results.Count];

        for (int day = 0; day < days; day++)
        {
            for (int j = 0; j < results.Count; j++)
            {
                sVals[j] = results[j].Timeline[day].Susceptible;
                iVals[j] = results[j].Timeline[day].Infected;
                rVals[j] = results[j].Timeline[day].Recovered;
                dVals[j] = results[j].Timeline[day].Dead;
            }

            Array.Sort(sVals);
            Array.Sort(iVals);
            Array.Sort(rVals);
            Array.Sort(dVals);

            int mid = results.Count / 2;

            writer.WriteLine(string.Join(",",
                day,
                sVals[mid],
                iVals[mid],
                rVals[mid],
                dVals[mid],
                Percentile(iVals, 0.05),
                Percentile(iVals, 0.25),
                Percentile(iVals, 0.75),
                Percentile(iVals, 0.95)));
        }
    }

    public static void ExportBenchmark(
        string path,
        List<(int Threads, double AvgTimeMs, double Speedup, double Efficiency)> data)
    {
        using var writer = new StreamWriter(path);
        writer.WriteLine("threads,avg_time_ms,speedup,efficiency");

        foreach (var (threads, avgMs, speedup, efficiency) in data)
            writer.WriteLine(FormattableString.Invariant($"{threads},{avgMs:F1},{speedup:F3},{efficiency:F3}"));
    }

    private static int Percentile(int[] sorted, double p)
    {
        int idx = Math.Clamp((int)(p * (sorted.Length - 1)), 0, sorted.Length - 1);
        return sorted[idx];
    }
}
