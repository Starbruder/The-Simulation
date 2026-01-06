namespace TheSimulation;

public sealed class EvaluationData
{
    public uint TotalGrownTrees { get; init; }
    public uint TotalBurnedTrees { get; init; }
    public int ActiveTrees { get; init; }
    public int MaxTreesPossible { get; init; }
    public TimeSpan Runtime { get; init; }

    public List<(TimeSpan Time, uint Grown, uint Burned)> History { get; init; } = [];

    public double BurnedPercentage => MaxTreesPossible > 0 ? (double)TotalBurnedTrees / MaxTreesPossible * 100 : 0;
    public double TreeDensityPercentage => MaxTreesPossible > 0 ? (double)ActiveTrees / MaxTreesPossible * 100 : 0;
}
