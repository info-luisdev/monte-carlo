using System.Diagnostics;
using Epidemia.Core;

namespace Epidemia.Paralelo;

internal static class Program
{
    private static void Main(string[] args)
    {
        var config = SimulationConfig.FromArgs(args);
        int threadCount = ParseThreadCount(args);
        var results = new List<ReplicaResult>();

        Console.WriteLine("=== Simulación Monte-Carlo SIRD — Versión Paralela ===");
        Console.WriteLine($"Grilla: {config.Width}×{config.Height} | Días: {config.Days} | Réplicas: {config.Replicas}");
        Console.WriteLine($"β={config.Beta:F4}  γ={config.Gamma:F4}  μ={config.Mu:F4}");
        Console.WriteLine($"Hilos: {threadCount}");
        Console.WriteLine();

        var totalSw = Stopwatch.StartNew();

        for (int r = 0; r < config.Replicas; r++)
        {
            ulong seed = config.BaseSeed + (ulong)r;
            var sw = Stopwatch.StartNew();
            var result = Simulator.RunReplica(config, seed, threadCount);
            sw.Stop();

            double ms = sw.Elapsed.TotalMilliseconds;
            int peakDay = EpidemicMetrics.FindPeakDay(result.Timeline);
            int peakInf = result.Timeline[peakDay].Infected;

            Console.WriteLine($"  Réplica {r + 1,3}: {ms,9:F1} ms | Pico día {peakDay,3} ({peakInf:N0} infectados)");
            results.Add(result with { ElapsedMs = ms });
        }

        totalSw.Stop();

        PrintSummary(results, config, totalSw.Elapsed);
        ExportResults(results, config);
    }

    private static int ParseThreadCount(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--threads")
                return int.Parse(args[i + 1]);
        }

        return Environment.ProcessorCount;
    }

    private static void PrintSummary(List<ReplicaResult> results, SimulationConfig config, TimeSpan elapsed)
    {
        var lastStats = results[0].Timeline[^1];
        double cfr = EpidemicMetrics.CalculateCFR(lastStats);
        double r0 = EpidemicMetrics.EstimateR0(results[0].Timeline, config.Gamma);
        double avgMs = results.Average(r => r.ElapsedMs);

        Console.WriteLine();
        Console.WriteLine($"Tiempo total: {elapsed.TotalSeconds:F2} s ({avgMs:F1} ms/réplica promedio)");
        Console.WriteLine($"R₀ estimado:  {r0:F2}");
        Console.WriteLine($"CFR:          {cfr:P2}");
    }

    private static void ExportResults(List<ReplicaResult> results, SimulationConfig config)
    {
        string outputDir = "resultados";
        Directory.CreateDirectory(outputDir);

        CsvExporter.ExportAggregated(Path.Combine(outputDir, "paralelo_stats.csv"), results, config.Days);
        CsvExporter.ExportTimeline(Path.Combine(outputDir, "paralelo_replica0.csv"), results[0].Timeline);

        Console.WriteLine($"\nCSV exportado a: {outputDir}/");
    }
}
