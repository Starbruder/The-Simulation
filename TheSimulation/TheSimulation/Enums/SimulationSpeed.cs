namespace TheSimulation;

/// <summary>
/// Specifies the available simulation speed levels, represented as the number of milliseconds per simulation tick.
/// </summary>
/// <remarks>Use this enumeration to control how quickly the simulation advances. Lower values correspond to
/// faster simulation speeds. The values are intended to provide standard speed presets for typical simulation
/// scenarios.</remarks>
public enum SimulationSpeed
{
    Slow = 240, // ×0.5
    Normal = 120, // ×1
    Fast = 60, // ×2
    Ultra = 3 // ×40
}
