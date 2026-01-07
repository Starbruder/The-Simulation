namespace TheSimulation;

/// <summary>
/// Represents configuration options for pre-filling a map with objects, such as trees, based on specified density and
/// activation settings.
/// </summary>
/// <param name="ShouldPrefillMap">Indicates whether the map should be pre-filled with objects. Set to <see langword="true"/> to enable pre-filling;
/// otherwise, <see langword="false"/>.</param>
/// <param name="Density">The proportion of available space to fill, specified as a value between 0 and 1. For example, 0.7 fills 70% of
/// possible locations. Must be in the range 0 to 1 inclusive.</param>
public sealed record PrefillConfig
(
    bool ShouldPrefillMap,
    double Density // neu: 0..1, z.B. 0.7 = 70% der möglichen Bäume
);
