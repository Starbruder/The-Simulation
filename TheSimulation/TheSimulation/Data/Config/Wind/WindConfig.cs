namespace TheSimulation;

public sealed record WindConfig
(
    bool RandomDirection,
    WindDirection Direction,
    float Strength
);
