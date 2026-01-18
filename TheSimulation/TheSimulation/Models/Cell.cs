namespace TheSimulation;

/// <summary>
/// Represents an immutable two-dimensional cell defined by its X and Y coordinates.
/// </summary>
/// <param name="X">The horizontal coordinate of the cell.</param>
/// <param name="Y">The vertical coordinate of the cell.</param>
public readonly record struct Cell(int X, int Y) : IEquatable<Cell>
{
    /// <summary>
    /// Returns a string representation of the cell in the format "(X, Y)".
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() => HashCode.Combine(X, Y);
}
