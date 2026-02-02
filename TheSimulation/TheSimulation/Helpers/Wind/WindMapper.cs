using System.Windows;

namespace TheSimulation;

/// <summary>
/// Statische Hilfsklasse zur Umrechnung zwischen verschiedenen Wind-Repräsentationen.
/// Wandelt Richtungen (Enums), Winkel (Grad) und Vektoren ineinander um.
/// </summary>
public static class WindMapper
{
    /// <summary>
    /// Konvertiert eine <see cref="WindDirection"/> in einen Richtungsvektor.
    /// Berücksichtigt eine Korrektur von 90°, um die grafische Ausrichtung im Koordinatensystem anzupassen.
    /// </summary>
    /// <param name="direction">Die gewünschte Windrichtung als <see cref="enum"/>.</param>
    /// <returns>Ein normalisierter Vektor, der in die entsprechende Richtung zeigt.</returns>
    public static Vector GetWindVector(WindDirection direction)
    {
        const int correctionDegrees = 90;
        var angleInDegrees = (float)direction + correctionDegrees;

        var angleInRadians = angleInDegrees * (float)Math.PI / 180;

        var x = (float)Math.Cos(angleInRadians);
        var y = (float)Math.Sin(angleInRadians);

        return new(x, y);
    }

    /// <summary>
    /// Erzeugt einen Windvektor basierend auf einem präzisen Winkel und einer Windstärke.
    /// </summary>
    /// <param name="angleDegrees">Der Windwinkel in Grad (0-360°).</param>
    /// <param name="windStrength">Die Länge (Intensität) des resultierenden Vektors.</param>
    /// <returns>Ein skalierter Vektor, der Richtung und Stärke repräsentiert.</returns>
    public static Vector ConvertWindAngleDegreesToVector(double angleDegrees, double windStrength)
    {
        // in Radiant umrechnen
        var rad = angleDegrees * Math.PI / 180;
        var x = Math.Cos(rad);
        var y = Math.Sin(rad);

        var vector = new Vector(x, y);
        vector.Normalize();
        vector *= windStrength;

        return vector;
    }

    /// <summary>
    /// Berechnet den Winkel in Grad aus einem gegebenen Windvektor.
    /// </summary>
    /// <param name="windVector">Der zu analysierende Vektor.</param>
    /// <returns>Ein Winkel zwischen 0 und 360°.</returns>
    public static double ConvertVectorToWindAngleDegrees(Vector windVector)
    {
        // atan2 liefert den Winkel in Radiant (-π bis π)
        var angleRad = Math.Atan2(windVector.Y, windVector.X);

        var angleDeg = angleRad * (180.0 / Math.PI);

        // auf 0 - 360° normalisieren
        if (angleDeg < 0)
        {
            angleDeg += 360;
        }

        return angleDeg;
    }

    /// <summary>
    /// Rechnet die prozentuale Windstärke (0.0 bis 1.0) grob in die Beaufort-Skala (0-12) um.
    /// </summary>
    /// <param name="windPercentStrength">Die Windstärke als Dezimalwert.</param>
    /// <returns>Die entsprechende Stufe auf der <see cref="BeaufortScale"/>.</returns>
    public static BeaufortScale ConvertWindPercentStrenghToBeaufort(double windPercentStrength)
    {
        // WindStrength ist ein Wert zwischen 0 und 1, daher multiplizieren wir mit 12, um den Bereich 0 bis 12 zu erhalten
        var beaufortScale = (int)(windPercentStrength * 12);
        return (BeaufortScale)Math.Min(beaufortScale, (int)BeaufortScale.Hurricane); // Maximalwert Hurricane (Stufe 11)
    }

    /// <summary>
    /// Erweiterungsmethode, die einen beliebigen Winkel in die nächstgelegene <see cref="WindDirection"/> (Haupt- und Zwischenhimmelsrichtungen) umwandelt.
    /// </summary>
    /// <param name="angle">Der Winkel in Grad.</param>
    /// <returns>Die entsprechende <see cref="WindDirection"/> (z.B. North, NorthEast).</returns>
    public static WindDirection ToWindDirection(this float angle)
    {
        angle = NormalizeWindAngleToMax360Degrees(angle);
        var snapped = SnapToNearest45Degrees(angle);
        return (WindDirection)snapped;
    }

    /// <summary>
    /// Rundet einen Winkel auf den nächsten 45°-Schritt (0, 45, 90, ..., 315).
    /// </summary>
    private static int SnapToNearest45Degrees(float angle)
    {
        return (int)(MathF.Round(angle / 45f) * 45f) % 360;
    }

    /// <summary>
    /// Normalisiert einen Winkel so, dass er immer im Bereich von [0, 360[ Grad liegt.
    /// </summary>
    private static float NormalizeWindAngleToMax360Degrees(float angle)
    {
        angle = (angle % 360 + 360) % 360;
        return angle;
    }
}
