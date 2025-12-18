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

    public static WindDirection ToWindDirection(this float angle)
    {
        angle = NormalizeWindAngleToMax360Degrees(angle);
        var snapped = SnapToNearest45Degrees(angle);
        return (WindDirection)snapped;
    }

    private static int SnapToNearest45Degrees(float angle)
    {
        return (int)(MathF.Round(angle / 45f) * 45f) % 360;
    }

    private static float NormalizeWindAngleToMax360Degrees(float angle)
    {
        angle = (angle % 360 + 360) % 360;
        return angle;
    }
}
