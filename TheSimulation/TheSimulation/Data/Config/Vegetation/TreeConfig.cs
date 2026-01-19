using System.Windows.Shapes;

namespace TheSimulation;

/// <summary>
/// Represents configuration settings for a tree structure, including maximum count, forest density, and size
/// parameters.
/// </summary>
/// <param name="MaxCount">The maximum number of trees allowed in the configuration. Must be a non-negative integer.</param>
/// <param name="ForestDensity">The density of the forest, expressed as a floating-point value. Higher values indicate a denser forest.</param>
/// <param name="Size">The overall size of the tree structure, specified as a non-negative integer.</param>
/// <param name="AllowRegrowForest">Indicates whether trees are allowed to regrow after being destroyed.</param>
public sealed record TreeConfig
(
    uint MaxCount,
    float ForestDensity,
    uint Size,
    bool AllowRegrowForest
);
