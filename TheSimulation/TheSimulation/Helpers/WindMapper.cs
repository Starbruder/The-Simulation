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

    public static BeaufortScale ConvertWindPercentStrenghToBeaufort(double windPercentStrength)
    {
        // WindStrength ist ein Wert zwischen 0 und 1, daher multiplizieren wir mit 12, um den Bereich 0 bis 12 zu erhalten
        var beaufortScale = (int)(windPercentStrength * 12);
        return (BeaufortScale)Math.Min(beaufortScale, (int)BeaufortScale.Hurricane); // Maximalwert Hurricane (Stufe 11)
    }

    private static BeaufortScale ConvertWindKmPerHourToBeaufortScale(double windSpeed)
    {
        // Windgeschwindigkeit in m/s bestimmt die Beaufort-Stufe
        if (windSpeed < 0.3) return BeaufortScale.Calm;
        if (windSpeed < 1.6) return BeaufortScale.LightAir;
        if (windSpeed < 3.4) return BeaufortScale.LightBreeze;
        if (windSpeed < 5.5) return BeaufortScale.GentleBreeze;
        if (windSpeed < 8.0) return BeaufortScale.ModerateBreeze;
        if (windSpeed < 10.8) return BeaufortScale.FreshBreeze;
        if (windSpeed < 13.9) return BeaufortScale.StrongBreeze;
        if (windSpeed < 17.2) return BeaufortScale.StrongWind;
        if (windSpeed < 20.8) return BeaufortScale.SevereWind;
        if (windSpeed < 24.5) return BeaufortScale.Storm;
        if (windSpeed < 28.5) return BeaufortScale.ViolentStorm;
        if (windSpeed < 32.7) return BeaufortScale.Hurricane;
        return BeaufortScale.Hurricane;
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
