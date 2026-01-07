namespace TheSimulation;

/// <summary>
/// <see langword="public"/> record that holds evaluation data for the simulation.
/// <see langword="public"/> properties include total grown trees, total burned trees, active trees, maximum possible trees, runtime, and history of growth and burn events.
/// </summary>
/// <param name="TotalGrownTrees"></param>
/// <param name="TotalBurnedTrees"></param>
/// <param name="ActiveTrees"></param>
/// <param name="MaxTreesPossible"></param>
/// <param name="Runtime"></param>
/// <param name="History"></param>
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
