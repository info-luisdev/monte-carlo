using System.Diagnostics;
using Epidemia.Core;

namespace Epidemia.Benchmark;

internal static class Program
{
    private const int BenchmarkReplicas = 5;
    private const int FrameInterval = 5;

    private static void Main(string[] args)
    {
        var config = SimulationConfig.FromArgs(args);
        int[] threadCounts = [1, 2, 4, 8];

        Console.WriteLine("=== Benchmark de Strong Scaling — Simulación SIRD ===");
        Console.WriteLine($"Grilla: {config.Width}×{config.Height} | Días: {config.Days} | Réplicas por prueba: {BenchmarkReplicas}");
        Console.WriteLine();

        Warmup(config);

        var results = RunScalingExperiments(config, threadCounts);
        PrintTable(results);

        string outputDir = "resultados";
        Directory.CreateDirectory(outputDir);

        ExportResults(outputDir, results);
        GenerateAnimation(outputDir, config);

        Console.WriteLine();
        Console.WriteLine($"Archivos generados en: {outputDir}/");
        Console.WriteLine("  - benchmark_times.csv");
        Console.WriteLine("  - speedup.svg");
        Console.WriteLine("  - epidemia_animacion.gif");
    }

    private static void Warmup(SimulationConfig config)
    {
        Console.Write("Calentamiento... ");
        Simulator.RunReplica(config with { Days = 30 }, config.BaseSeed, 1);
        Console.WriteLine("OK");
        Console.WriteLine();
    }

    private static List<(int Threads, double AvgTimeMs, double Speedup, double Efficiency)> RunScalingExperiments(
        SimulationConfig config, int[] threadCounts)
    {
        var results = new List<(int Threads, double AvgTimeMs, double Speedup, double Efficiency)>();
        double baselineMs = 0;

        foreach (int tc in threadCounts)
        {
            Console.Write($"  {tc,2} hilo(s): ");
            var sw = Stopwatch.StartNew();

            for (int r = 0; r < BenchmarkReplicas; r++)
                Simulator.RunReplica(config, config.BaseSeed + (ulong)r, tc);

            sw.Stop();
            double avgMs = sw.Elapsed.TotalMilliseconds / BenchmarkReplicas;

            if (tc == 1) baselineMs = avgMs;

            double speedup = baselineMs / avgMs;
            double efficiency = speedup / tc;

            results.Add((tc, avgMs, speedup, efficiency));
            Console.WriteLine($"{avgMs,10:F1} ms/réplica | Speed-up: {speedup,5:F2}× | Eficiencia: {efficiency:P0}");
        }

        return results;
    }

    private static void PrintTable(
        List<(int Threads, double AvgTimeMs, double Speedup, double Efficiency)> results)
    {
        Console.WriteLine();
        Console.WriteLine("  Hilos | Tiempo (ms) | Speed-up | Eficiencia");
        Console.WriteLine("  ------+-------------+----------+-----------");

        foreach (var (threads, avgMs, speedup, efficiency) in results)
            Console.WriteLine($"  {threads,5} | {avgMs,11:F1} | {speedup,7:F2}× | {efficiency,9:P0}");
    }

    private static void ExportResults(
        string outputDir,
        List<(int Threads, double AvgTimeMs, double Speedup, double Efficiency)> results)
    {
        CsvExporter.ExportBenchmark(Path.Combine(outputDir, "benchmark_times.csv"), results);

        SpeedupChartGenerator.GenerateSvg(
            Path.Combine(outputDir, "speedup.svg"),
            results.Select(r => (r.Threads, r.Speedup)).ToList());
    }

    private static void GenerateAnimation(string outputDir, SimulationConfig config)
    {
        Console.WriteLine();
        Console.Write("Generando animación side-by-side... ");

        var seqResult = Simulator.RunReplica(config, config.BaseSeed, 1, FrameInterval);
        var parResult = Simulator.RunReplica(
            config, config.BaseSeed, Environment.ProcessorCount, FrameInterval);

        AnimationGenerator.Generate(
            Path.Combine(outputDir, "epidemia_animacion.gif"),
            seqResult.Frames!,
            parResult.Frames!,
            config.Width,
            config.Height);

        Console.WriteLine("OK");
    }
}
