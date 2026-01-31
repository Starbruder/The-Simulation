namespace TheSimulation;

/// <summary>
/// Represents configuration settings for fire behavior, including spread chance and pause behavior.
/// </summary>
/// <param name="SpreadChancePercent">The probability, expressed as a percentage from 0 to 100, that fire will spread during each evaluation. Must be
/// between 0 and 100 inclusive.</param>
/// <param name="PauseDuringFire">Indicates whether the simulation should pause while fire is active. Set to <see langword="true"/> to pause;
/// otherwise, <see langword="false"/>.</param>
/// /// <paramref name="LightningStrikeChancePercent"/> will determine the frequency of strikes.
/// <see langword="default"/> is 0,15 %.</param>
/// <param name="EnableLightningStrikes">Indicates whether lightning strikes are enabled in the simulation. Set to <see langword="true"/> to enable;
public sealed record FireConfig
(
    double SpreadChancePercent,
    bool PauseDuringFire,
    double LightningStrikeChancePercent,
    bool EnableLightningStrikes
);
