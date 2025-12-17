namespace TheSimulation;

public sealed record SimulationConfig
(
    TreeConfig TreeConfig,
    FireConfig FireConfig,
    WindConfig WindConfig,
    bool ReplaceWithBurnedDownTree,
    bool ShowLightning
);
