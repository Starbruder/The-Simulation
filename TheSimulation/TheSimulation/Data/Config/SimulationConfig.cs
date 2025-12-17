namespace TheSimulation;

public sealed record SimulationConfig
(
    uint MaxTrees,
    float TreeDensity,
    uint TreeSize,
    float FireSpreadChancePercent,
    bool ReplaceWithBurnedDownTree,
    bool ShowLightning,
    bool PauseDuringFire,
    WindDirection WindDirection,
    float WindStrength
);
