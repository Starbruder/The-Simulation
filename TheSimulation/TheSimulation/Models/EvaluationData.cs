namespace TheSimulation;

public sealed record EvaluationData
(
    uint TotalGrownTrees,
    uint TotalBurnedTrees,
    int ActiveTrees,
    int MaxTreesPossible,
    TimeSpan Runtime,
    List<(TimeSpan Time, uint Grown, uint Burned)> History
)
{
    public double BurnedPercentage => MaxTreesPossible > 0
        ? (double)TotalBurnedTrees / MaxTreesPossible * 100
        : 0;

    public double TreeDensityPercentage => MaxTreesPossible > 0
        ? (double)ActiveTrees / MaxTreesPossible * 100
        : 0;
}
