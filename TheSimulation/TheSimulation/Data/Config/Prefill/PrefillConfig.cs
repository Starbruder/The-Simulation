namespace TheSimulation;

public sealed record PrefillConfig
(
    bool ShouldPrefillMap,
    double Density // neu: 0..1, z.B. 0.7 = 70% der möglichen Bäume
);
