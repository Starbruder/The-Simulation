namespace TheSimulation;

public sealed record SimulationConfig
(
    uint MaxTrees = 50_000,
    float TreeDensity = 0.6f,
    uint TreeSize = 7,
	bool ReplaceWithBurnedDownTree = false,
	bool ShowLightning = false,
	bool PauseDuringFire = true
);
