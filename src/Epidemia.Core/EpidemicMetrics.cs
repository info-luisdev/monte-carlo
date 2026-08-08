namespace Epidemia.Core;

public static class EpidemicMetrics
{
    public static int FindPeakDay(DayStats[] timeline)
    {
        int peakDay = 0;
        int maxInfected = 0;

        for (int i = 0; i < timeline.Length; i++)
        {
            if (timeline[i].Infected <= maxInfected)
                continue;

            maxInfected = timeline[i].Infected;
            peakDay = i;
        }

        return peakDay;
    }

    public static double CalculateCFR(DayStats stats)
    {
        int totalResolved = stats.Recovered + stats.Dead;
        return totalResolved > 0 ? (double)stats.Dead / totalResolved : 0.0;
    }

    public static double EstimateR0(DayStats[] timeline, double gamma)
    {
        for (int day = 5; day < Math.Min(60, timeline.Length); day++)
        {
            if (timeline[day].Infected < 50 || timeline[day - 1].Infected < 20)
                continue;

            double newInfections = timeline[day].NewInfections;
            double currentInfected = timeline[day - 1].Infected;

            if (currentInfected > 0)
                return newInfections / (gamma * currentInfected);
        }

        return 0;
    }

    public static double[] CalculatePercentiles(List<ReplicaResult> results, int day, double[] percentiles)
    {
        var values = new double[results.Count];
        for (int i = 0; i < results.Count; i++)
            values[i] = results[i].Timeline[day].Infected;

        Array.Sort(values);

        var output = new double[percentiles.Length];
        for (int p = 0; p < percentiles.Length; p++)
        {
            int idx = Math.Clamp((int)(percentiles[p] * (values.Length - 1)), 0, values.Length - 1);
            output[p] = values[idx];
        }

        return output;
    }
}
