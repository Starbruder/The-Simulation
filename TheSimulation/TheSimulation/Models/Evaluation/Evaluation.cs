namespace TheSimulation;

/// <summary>
/// <see langword="public"/> <see langword="record"/> that holds evaluation data for the simulation.
/// <see langword="public"/> properties include total grown trees, total burned trees, active trees, maximum possible trees, runtime, and history of growth and burn events.
/// </summary>
/// <param name="TotalGrownTrees"></param>
/// <param name="TotalBurnedTrees"></param>
/// <param name="MaxTreesPossible"></param>
/// <param name="AirHumidityPercentage"></param>
/// <param name="AirTemperatureCelsius"></param>
/// <param name="Runtime"></param>
/// <param name="History"></param>
/// <param name="FireEvents"></param>
public sealed record Evaluation
(
    uint TotalGrownTrees,
    uint TotalBurnedTrees,
    int MaxTreesPossible,
    float AirHumidityPercentage,
    float AirTemperatureCelsius,
    TimeSpan Runtime,
    List<(TimeSpan Time, uint Grown, uint Burned, double WindSpeed)> History,
    List<FireEvent> FireEvents
);
