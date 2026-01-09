namespace TheSimulation;

/// <summary>
/// Represents the configuration settings for a fire simulation, including tree, fire, wind, prefill, and visual effects
/// parameters.
/// </summary>
/// <param name="TreeConfig">The configuration settings that define tree properties and behavior within the simulation.</param>
/// <param name="FireConfig">The configuration parameters that control fire behavior, spread, and related properties in the simulation.</param>
/// <param name="WindConfig">The configuration options specifying wind direction, strength, and its influence on the simulation.</param>
/// <param name="PrefillConfig">The configuration for pre-filling the simulation environment, such as initial tree or fire placement.</param>
/// <param name="VisualEffectsConfig">The configuration settings for visual effects, including rendering and animation options for the simulation.</param>
/// <param name="ReplaceWithBurnedDownTree">Indicates whether trees should be replaced with burned-down versions after being consumed by fire. Set to <see
/// langword="true"/> to enable replacement; otherwise, <see langword="false"/>.</param>
/// <param name="AirHumidityPercentage">The global air humidity level in the simulation, represented as a percentage (0 to 1), which can affect fire behavior and spread.</param>
public sealed record SimulationConfig
(
    TreeConfig TreeConfig,
    FireConfig FireConfig,
    WindConfig WindConfig,
    PrefillConfig PrefillConfig,
    VisualEffectsConfig VisualEffectsConfig,
    bool ReplaceWithBurnedDownTree,
    double AirHumidityPercentage // Normalized relative humidity factor (0..1), not meteorological rH
);
