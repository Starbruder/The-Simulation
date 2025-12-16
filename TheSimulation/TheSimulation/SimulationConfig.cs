namespace TheSimulation;

public sealed record SimulationConfig
(
    uint MaxTrees = 50_000,
    float TreeDensity = 0.6f,
    uint TreeSize = 7,
    float FireSpreadChancePercent = 75f,
    bool ReplaceWithBurnedDownTree = false,
    bool ShowLightning = false,
    bool PauseDuringFire = true
);
