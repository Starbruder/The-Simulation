using System.Windows;

namespace TheSimulation;

/// <summary>
/// <see cref="SimulationSnapshot"/> ist ein <see langword="public"/> <see langword="record"/>, das einen bestimmten Zeitpunkt in der Simulation darstellt.
/// Einen History-Eintrag mit Zeit, gewachsenen und verbrannten Bäumen, um die Charts darzustellen in der Auswertung.
/// </summary>
/// <param name="Time"></param>
/// <param name="Grown"></param>
/// <param name="Burned"></param>
/// <param name="WindSpeed"></param>
/// <param name="WindDirection"></param>
public sealed record SimulationSnapshot
(
    TimeSpan Time,
    uint Grown,
    uint Burned,
    double WindSpeed,
    double WindDirection
);
