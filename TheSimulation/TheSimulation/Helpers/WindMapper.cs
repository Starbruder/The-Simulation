using System.Windows;

namespace TheSimulation;

public static class WindMapper
{
    public static Vector GetWindVector(WindDirection direction) =>
        direction switch
        {
            WindDirection.North => new(0, -1),
            WindDirection.South => new(0, 1),
            WindDirection.East => new(1, 0),
            WindDirection.West => new(-1, 0),
            _ => new(0, 0)
        };
}
