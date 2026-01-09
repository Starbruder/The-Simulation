namespace TheSimulation;

/// <summary>
/// <see langword="sealed"/> record that represents the configuration settings for the atmosphere in a simulation, including air humidity and temperature.
/// </summary>
/// <param name="AirHumidityPercentage">The global air humidity level in the simulation, represented as a percentage (0 to 1), which can affect fire behavior and spread.</param>
/// <param name="AirTemperatureCelsius">The ambient temperature in degrees Celsius, influencing fire dynamics and environmental conditions within the simulation.</param>
public sealed record AtmosphereConfig
(
    float AirHumidityPercentage, // Normalized relative humidity factor (0..1), not meteorological rH
    float AirTemperatureCelsius // z.B. 15 = kühl, 30 = heiß, 40 = extrem
);
