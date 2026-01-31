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

    //public Brush NextTreeColor()
    //{
    //    var index = NextInt(0, ColorsData.TreeGridColorBrushes.Length);
    //    return ColorsData.TreeGridColorBrushes[index];
    //}

    public uint NextTreeColorUint()
    {
        var index = NextInt(0, ColorsData.TreeGridColors.Length);
        return ColorsData.TreeGridColors[index];
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
