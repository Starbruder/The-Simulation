namespace TheSimulation;

public sealed record WindConfig
(
    bool RandomDirection,
    WindDirection Direction, // Wird beachtet, wenn RandomDirection == false
	double Strength
);
