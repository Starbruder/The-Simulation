namespace TheSimulation;

/// <summary>
/// <see cref="SimulationSnapshot"/> ist ein <see langword="public"/> <see langword="record"/>, das einen bestimmten Zeitpunkt in der Simulation darstellt.
/// </summary>
/// <param name="Time"></param>
/// <param name="Grown"></param>
/// <param name="Burned"></param>
/// <param name="WindSpeed"></param>
public sealed record SimulationSnapshot
(
    TimeSpan Time,
    uint Grown,
    uint Burned,
    double WindSpeed
);
