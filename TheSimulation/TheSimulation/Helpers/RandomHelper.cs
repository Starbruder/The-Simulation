using System.Windows.Media;

namespace TheSimulation;

public sealed class RandomHelper
{
    private readonly Random random = new();

    public int NextInt(int minValue, int maxValue)
        => random.Next(minValue, maxValue);

    public double NextDouble()
        => random.NextDouble();

    public double NextDouble(float minValue, float maxValue)
        => random.NextDouble() * (maxValue - minValue) + minValue;

    public Cell NextCell(int columns, int rows)
        => new(NextInt(0, columns), NextInt(0, rows));

    /// <summary>
    /// Returns a randomly selected brush representing a tree color from the predefined set.
    /// </summary>
    /// <remarks>
    /// The returned brush is frozen to improve performance and ensure thread safety.
    /// Each call may return a different color, selected at random from the available tree colors.
    /// </remarks>
    /// <returns>
    /// A frozen <see cref="Brush"/> instance representing a randomly chosen tree color.
    /// The returned brush is immutable and can be safely shared across threads.
    /// </returns>
    public Brush NextTreeColor()
    {
        var index = NextInt(0, ColorsData.TreeColors.Length);
        var frozenTreeColor = ColorsData.TreeColors[index];
        if (frozenTreeColor.CanFreeze)
        {
            frozenTreeColor.Freeze(); // Macht den Brush schreibgeschützt und performant
        }
        return frozenTreeColor;
    }

    public Cell NextCell(HashSet<Cell> cell)
    {
        if (cell.Count == 0)
        {
            throw new InvalidOperationException("No free growable cells left.");
        }

        var randomIndex = NextInt(0, cell.Count);
        return cell.ElementAt(randomIndex);
    }
}
