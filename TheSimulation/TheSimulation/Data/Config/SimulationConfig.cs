namespace TheSimulation;

public sealed record SimulationConfig
(
    TreeConfig TreeConfig,
    FireConfig FireConfig,
    WindConfig WindConfig,
    PrefillConfig PrefillConfig,
    bool ReplaceWithBurnedDownTree,
    bool ShowLightning
);
