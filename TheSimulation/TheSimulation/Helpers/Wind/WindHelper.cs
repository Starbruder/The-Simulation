using System.Windows;

namespace TheSimulation;

public sealed class WindHelper(WindConfig config)
{
    private readonly WindConfig config = config;
    private readonly RandomHelper randomHelper = new();

    public double CurrentWindAngleDegrees { get; private set; } = (int)config.Direction;

    public double CurrentWindStrength { get; private set; } = config.Strength;

    public double CalculateWindEffect(Cell from, Cell to)
    {
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;

        // Richtungsvektor der Feuerausbreitung
        var spreadDir = new Vector(dx, dy);
        spreadDir.Normalize();

        // Wind normalisieren
        var windVector = GetWindVector();
        if (windVector.Length == 0)
        {
            return 1;
        }

        windVector.Normalize();

        // Skalarprodukt: -1 .. 1
        var alignment = Vector.Multiply(spreadDir, windVector);

        // multipliziere mit Stärke
        var effect = 1 + CurrentWindStrength * alignment;

        // Keine negativen Wahrscheinlichkeiten
        return Math.Max(0.1, effect);
    }

    public Vector GetWindVector()
    {
        if (config.RandomDirection)
        {
            return WindMapper.ConvertWindAngleDegreesToVector(
                CurrentWindAngleDegrees, CurrentWindStrength);
        }

        return WindMapper.GetWindVector(config.Direction);
    }

    public void RandomizeAndUpdateWind()
    {
        if (config.RandomDirection)
        {
            // Windrichtung um ±5° ändern
            RandomizeWindDirection(fluctuation: 5);
        }

        if (config.RandomStrength)
        {
            // Windstärke um ±3% ändern
            RandomizeWindStrengh(fluctuation: 0.03f);
        }
    }

    private void RandomizeWindDirection(uint fluctuation)
    {
        var deltaDirection = (randomHelper.NextDouble() * 2 - 1) * fluctuation;
        CurrentWindAngleDegrees = (CurrentWindAngleDegrees + deltaDirection + 360) % 360;
    }

    private void RandomizeWindStrengh(float fluctuation)
    {
        var deltaStrength = randomHelper.NextDouble(-fluctuation, fluctuation);
        CurrentWindStrength += deltaStrength;

        // Sicherstellen, dass die Windstärke im erlaubten Bereich bleibt
        const uint MaxWindStrength = 1;
        CurrentWindStrength = Math.Clamp(CurrentWindStrength, 0, MaxWindStrength);
    }
}
