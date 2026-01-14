using System.Windows;

namespace TheSimulation;

public static class WindMapper
{
    public static Vector GetWindVector(WindDirection direction)
    {
        const int correctionDegrees = 90;
        var angleInDegrees = (float)direction + correctionDegrees;

        var angleInRadians = angleInDegrees * (float)Math.PI / 180;

        var x = (float)Math.Cos(angleInRadians);
        var y = (float)Math.Sin(angleInRadians);

        return new(x, y);
    }

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
