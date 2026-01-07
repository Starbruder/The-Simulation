namespace TheSimulation;

/// <summary>
/// Represents configuration options for wind simulation, including direction and strength settings.
/// </summary>
/// <param name="RandomDirection">Specifies whether the wind direction should be chosen randomly. If <see langword="true"/>, the <paramref
/// name="Direction"/> parameter is ignored.</param>
/// <param name="Direction">The fixed wind direction to use when <paramref name="RandomDirection"/> is <see langword="false"/>.</param>
/// <param name="RandomStrength">Specifies whether the wind strength should be chosen randomly. If <see langword="true"/>, the <paramref
/// name="Strength"/> parameter is ignored.</param>
/// <param name="Strength">The fixed wind strength to use when <paramref name="RandomStrength"/> is <see langword="false"/>.</param>
public sealed record WindConfig
(
    bool RandomDirection,
    WindDirection Direction, // Wird beachtet, wenn RandomDirection == false

    bool RandomStrength,
    double Strength // Wird beachtet, wenn RandomStrength == false
);
