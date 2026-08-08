namespace Epidemia.Core;

public sealed record SimulationConfig
{
    public int Width { get; init; } = 1000;
    public int Height { get; init; } = 1000;
    public int Days { get; init; } = 365;
    public double Beta { get; init; } = 0.25;
    public double Gamma { get; init; } = 1.0 / 7.0;
    public double Mu { get; init; } = 0.005;
    public int Replicas { get; init; } = 30;
    public int InitialInfectedCount { get; init; } = 10;
    public ulong BaseSeed { get; init; } = 42;

    public int Stride => Width + 2;
    public int BufferLength => Stride * (Height + 2);
    public int TotalCells => Width * Height;

    public static SimulationConfig FromArgs(string[] args)
    {
        int width = 1000, height = 1000, days = 365, replicas = 30, initialInfected = 10;
        double beta = 0.25, gamma = 1.0 / 7.0, mu = 0.005;
        ulong baseSeed = 42;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--width": width = int.Parse(args[++i]); break;
                case "--height": height = int.Parse(args[++i]); break;
                case "--days": days = int.Parse(args[++i]); break;
                case "--replicas": replicas = int.Parse(args[++i]); break;
                case "--seed": baseSeed = ulong.Parse(args[++i]); break;
                case "--beta": beta = double.Parse(args[++i]); break;
                case "--gamma": gamma = double.Parse(args[++i]); break;
                case "--mu": mu = double.Parse(args[++i]); break;
                case "--initial-infected": initialInfected = int.Parse(args[++i]); break;
            }
        }

        return new SimulationConfig
        {
            Width = width,
            Height = height,
            Days = days,
            Beta = beta,
            Gamma = gamma,
            Mu = mu,
            Replicas = replicas,
            InitialInfectedCount = initialInfected,
            BaseSeed = baseSeed
        };
    }
}
