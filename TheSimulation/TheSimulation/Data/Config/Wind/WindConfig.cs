namespace TheSimulation;

public sealed record WindConfig
(
    bool RandomDirection,
    WindDirection Direction, // Wird beachtet, wenn RandomDirection == false
    bool RandomStrength,
    double Strength
);
