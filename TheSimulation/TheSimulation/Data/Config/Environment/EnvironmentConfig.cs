namespace TheSimulation;

/// <summary>
/// <see langword="sealed"/> record that encapsulates the overall environmental configuration for a simulation, including atmosphere and wind settings.
/// </summary>
/// <param name="AtmosphereConfig"></param>
/// <param name="WindConfig"></param>
public sealed record EnvironmentConfig
(
    AtmosphereConfig AtmosphereConfig,
    WindConfig WindConfig
);
