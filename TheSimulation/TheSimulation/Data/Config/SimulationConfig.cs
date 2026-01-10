namespace TheSimulation;

/// <summary>
/// Represents the configuration settings for a fire simulation, including tree, fire, wind, prefill, and visual effects
/// parameters.
/// </summary>
/// <param name="TreeConfig">The configuration settings that define tree properties and behavior within the simulation.</param>
/// <param name="FireConfig">The configuration parameters that control fire behavior, spread, and related properties in the simulation.</param>
/// <param name="EnvironmentConfig">The configuration options specifying the overall environmental configuration for a simulation such as: atmosphere and wind settings.</param>
/// <param name="PrefillConfig">The configuration for pre-filling the simulation environment, such as initial tree or fire placement.</param>
/// <param name="VisualEffectsConfig">The configuration settings for visual effects, including rendering and animation options for the simulation.</param>
/// <param name="TerrainConfig">The configuration settings for terrain properties and behavior within the simulation.</param>
public sealed record SimulationConfig
(
    TreeConfig TreeConfig,
    FireConfig FireConfig,
    EnvironmentConfig EnvironmentConfig,
    PrefillConfig PrefillConfig,
    VisualEffectsConfig VisualEffectsConfig,
    TerrainConfig TerrainConfig
);
