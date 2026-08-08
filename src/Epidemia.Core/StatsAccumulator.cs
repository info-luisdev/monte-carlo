namespace Epidemia.Core;

public sealed class StatsAccumulator
{
    public int Susceptible;
    public int Infected;
    public int Recovered;
    public int Dead;
    public int NewInfections;

    public void Count(byte state)
    {
        switch (state)
        {
            case (byte)CellState.Susceptible: Susceptible++; break;
            case (byte)CellState.Infected: Infected++; break;
            case (byte)CellState.Recovered: Recovered++; break;
            case (byte)CellState.Dead: Dead++; break;
        }
    }

    public void RecordNewInfection() => NewInfections++;

    public void MergeInto(StatsAccumulator target)
    {
        Interlocked.Add(ref target.Susceptible, Susceptible);
        Interlocked.Add(ref target.Infected, Infected);
        Interlocked.Add(ref target.Recovered, Recovered);
        Interlocked.Add(ref target.Dead, Dead);
        Interlocked.Add(ref target.NewInfections, NewInfections);
    }

    public DayStats ToDayStats() => new(Susceptible, Infected, Recovered, Dead, NewInfections);
}
