namespace Epidemia.Core;

public readonly record struct DayStats(
    int Susceptible,
    int Infected,
    int Recovered,
    int Dead,
    int NewInfections);
