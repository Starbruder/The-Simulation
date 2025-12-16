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

    public Cell GetRandomCell(int columns, int rows)
        => new(NextInt(0, columns), NextInt(0, rows));
}
