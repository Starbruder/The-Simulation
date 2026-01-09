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
/// <param name="AirHumidityPercentage">The global air humidity level in the simulation, represented as a percentage (0 to 1), which can affect fire behavior and spread.</param>
/// <param name="TemperatureCelsius">The ambient temperature in degrees Celsius, influencing fire dynamics and environmental conditions within the simulation.</param>
public sealed record SimulationConfig
(
    TreeConfig TreeConfig,
    FireConfig FireConfig,
    WindConfig WindConfig,
    PrefillConfig PrefillConfig,
    VisualEffectsConfig VisualEffectsConfig,
    float AirHumidityPercentage, // Normalized relative humidity factor (0..1), not meteorological rH
    float AirTemperatureCelsius // z.B. 15 = kühl, 30 = heiß, 40 = extrem
);
