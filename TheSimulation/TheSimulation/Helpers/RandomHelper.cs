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

    public Brush NextTreeColor()
    {
        var index = NextInt(0, Colors.TreeColors.Length);
        return Colors.TreeColors[index];
    }

    public Cell NextTree(HashSet<Cell> trees)
    {
        if (trees.Count == 0)
        {
            return new(0, 0);
        }

        var randomIndex = NextInt(0, trees.Count);
        return trees.ElementAt(randomIndex);
    }
}
