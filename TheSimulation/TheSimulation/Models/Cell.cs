namespace TheSimulation.Models;

public readonly record struct Cell(int X, int Y)
{
	public override int GetHashCode() => HashCode.Combine(X, Y);
}
